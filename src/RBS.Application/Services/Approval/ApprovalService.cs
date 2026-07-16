using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Common;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.DomainServices;

namespace RBS.Application.Services.Approval;

/// <summary>
/// 审批应用服务实现
/// 写操作（Approve/Reject/Cancel）使用原始 SQL + 显式事务，
/// 读操作使用 Dapper 仓储。
/// </summary>
public class ApprovalService : IApprovalService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlLoader _sql;
    private readonly IApprovalDomainService _approvalDomain;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uow">工作单元</param>
    /// <param name="tenantService">租户服务，获取默认公司 ID</param>
    /// <param name="currentUserService">当前用户服务，获取操作用户 ID</param>
    /// <param name="connectionFactory">数据库连接工厂</param>
    /// <param name="serviceProvider">服务提供者（延迟获取事件处理器）</param>
    /// <param name="sql">SQL 加载器</param>
    /// <param name="approvalDomain">审批领域服务</param>
    public ApprovalService(
        IUnitOfWork uow,
        ITenantService tenantService,
        ICurrentUserService currentUserService,
        IDbConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ISqlLoader sql,
        IApprovalDomainService approvalDomain)
    {
        _uow = uow;
        _tenantService = tenantService;
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
        _serviceProvider = serviceProvider;
        _sql = sql;
        _approvalDomain = approvalDomain;
    }

    // =====================================================================
    // 写操作：SubmitAsync
    // 包含并发守卫（同一业务实体不能有两个待审批） + 事件分发
    // =====================================================================

    /// <summary>
    /// 提交审批请求 — 含并发守卫，自动提交后通知第一级审批人
    /// </summary>
    public async Task<ApprovalRequestDto> SubmitAsync(SubmitApprovalRequest request, CancellationToken ct = default)
    {
        // ===== 并发守卫：同一业务实体+同类型不能有两个待审批 =====
        using (var guardConn = _connectionFactory.CreateConnection())
        {
            guardConn.Open();
            var pending = await guardConn.QuerySingleAsync<int>(
                _sql.Get("Approval.Select.Request.PendingCount"),
                new { Id = request.TargetEntityId, Type = request.TargetEntityType });
            if (pending > 0)
                throw new InvalidOperationException("该业务已有待审批的申请，请处理完成后再提交");
        }

        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var typeLevels = levels.Where(l => l.ApprovalTypeId == request.ApprovalTypeId).ToList();
        var maxLevel = typeLevels.Count > 0 ? typeLevels.Max(l => l.LevelNo) : 0;

        var entity = new ApprovalRequest(
            request.ApprovalTypeId,
            request.Title,
            request.TargetEntityId,
            request.TargetEntityType,
            _tenantService.DefaultCompanyId,
            maxLevel);

        entity.SetCreated(_currentUserService.UserId, ChinaTime.Now, null, null);
        entity.AddRecord(_currentUserService.UserId, "Submitted", request.Description);
        await _uow.ApprovalRequests.AddAsync(entity, ct);

        // [领域] 通过领域服务执行提交状态变迁（Draft → Pending，若0级则自动 Approved）
        _approvalDomain.SubmitRequest(entity);

        var record = entity.Records.First();
        using (var updateConn = _connectionFactory.CreateConnection())
        {
            updateConn.Open();
            // 插入审批记录（Submitted），与状态更新共用同一连接
            await Dapper.SqlMapper.ExecuteAsync(updateConn,
                _sql.Get("Approval.Insert.Record.Default"),
                new { record.Id, ApprovalRequestId = entity.Id, Level = record.LevelNo, record.ApproverId, record.Action, Comment = record.Comment ?? "", record.CreatedBy, record.CreatedAt });

            await Dapper.SqlMapper.ExecuteAsync(updateConn,
                _sql.Get("Approval.Update.Request.SetStatus"),
                new { entity.Status, Id = entity.Id });
        }

        // [事件] 提交后通知第一级审批人
        if (entity.Status == "Pending")
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalSubmittedEvent>>();
            await handler.HandleAsync(
                new ApprovalSubmittedEvent(entity.Id, entity.ApprovalTypeId,
                    entity.TargetEntityId, entity.TargetEntityType, entity.Title),
                ct);
        }

        return await MapToDtoAsync(entity, ct);
    }

    // =====================================================================
    // 写操作：ApproveAsync / RejectAsync / CancelAsync 使用原始 SQL
    // =====================================================================

    /// <summary>
    /// 审批通过 — 终审时触发业务回调事件，非终审则推进到下一级
    /// </summary>
    public async Task<ApprovalRequestDto> ApproveAsync(Guid id, string? comment, CancellationToken ct = default)
    {
        // [读] 加载实体，验证状态
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var userId = _currentUserService.UserId;
        var now = ChinaTime.Now;

        // [领域] 通过领域服务执行审批流转
        var result = await _approvalDomain.ApproveAsync(entity, userId, comment, ct);

        // [写] 原始 SQL + 显式事务
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 状态变迁：终审设 Status，非终审进 CurrentLevel+1
            string updateSql;
            if (result.IsCompleted)
            {
                updateSql = _sql.Get("Approval.Update.Request.ToApproved");
            }
            else
            {
                updateSql = _sql.Get("Approval.Update.Request.AdvanceLevel");
            }

            var rows = await _uow.ExecuteSqlRawAsync(updateSql,
                new object[] { userId, now, id }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入审批记录（读取领域服务在聚合根上添加的记录）
            var newRecord = entity.LatestRecord!;
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { newRecord.Id, id, newRecord.LevelNo, userId, result.Action,
                    comment ?? "", userId, now }, ct);

            await tx.CommitAsync(ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [事件] 终审时手动分发领域事件
        if (result.IsCompleted)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalCompletedEvent>>();
            await handler.HandleAsync(
                new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType, result.Action),
                ct);
        }
        else
        {
            // 非终审：通知下一级审批人
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalLevelAdvancedEvent>>();
            await handler.HandleAsync(
                new ApprovalLevelAdvancedEvent(id, result.NextLevel!.Value),
                ct);
        }

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 审批驳回 — 驳回即为终审，触发审批完成事件
    /// </summary>
    public async Task<ApprovalRequestDto> RejectAsync(Guid id, string comment, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var userId = _currentUserService.UserId;
        var now = ChinaTime.Now;

        // [领域] 通过领域服务执行审批驳回
        var result = await _approvalDomain.RejectAsync(entity, userId, comment, ct);

        // [写] 原始 SQL + 显式事务
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var rows = await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Update.Request.ToRejected"),
                new object[] { userId, now, id }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入审批记录（读取领域服务在聚合根上添加的记录）
            var newRecord = entity.LatestRecord!;
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { newRecord.Id, id, newRecord.LevelNo, userId, result.Action,
                    comment, userId, now }, ct);

            await tx.CommitAsync(ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [事件] 驳回即为终审
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<IEventHandler<ApprovalCompletedEvent>>();
        await handler.HandleAsync(
            new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType, result.Action),
            ct);

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 撤回审批请求 — 仅提交人可操作，关联导入批次时同步更新批次状态
    /// </summary>
    public async Task<ApprovalRequestDto> CancelAsync(Guid id, string? reason = null, CancellationToken ct = default)
    {
        // [读] 加载实体（含 Records，需校验提交人）
        var entity = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("只能撤回待审批的请求");

        var submitRecord = entity.Records.FirstOrDefault(r => r.Action == "Submitted");
        if (submitRecord == null || submitRecord.ApproverId != _currentUserService.UserId)
            throw new InvalidOperationException("仅提交人可以撤回");

        var now = ChinaTime.Now;
        var userId = _currentUserService.UserId;

        // [写] 原始 SQL + 显式事务（同时更新审批状态和关联的导入批次）
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 更新审批状态
            var rows = await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Update.Request.ToCancelled"),
                new object[] { userId, now, id }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入"撤回"记录
            var recordId = Guid.NewGuid();
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { recordId, id, entity.CurrentLevel, userId, "Cancelled",
                    reason ?? "提交人撤回", userId, now }, ct);

            // 如果关联的是导入批次，同时更新批次状态
            if (entity.TargetEntityType == "Import")
            {
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Approval.Update.ImportBatch.Cancelled"),
                    new object[] { userId, now, entity.TargetEntityId }, ct);
            }

            await tx.CommitAsync(ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    // =====================================================================
    // 读操作
    // =====================================================================

    /// <summary>
    /// 获取当前用户可审批的待审批列表
    /// </summary>
    public async Task<List<ApprovalRequestDto>> GetPendingAsync(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var list = await _uow.ApprovalRequests.GetPendingByApproverAsync(userId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var item in list)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    /// <summary>
    /// 获取当前用户提交的审批请求
    /// </summary>
    public async Task<List<ApprovalRequestDto>> GetMyRequestsAsync(CancellationToken ct = default)
    {
        var list = await _uow.ApprovalRequests.GetByApproverAsync(_currentUserService.UserId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var item in list)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    /// <summary>
    /// 根据 ID 获取审批请求详情（含审批记录）
    /// </summary>
    public async Task<ApprovalRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(id, ct);
        if (entity == null) return null;
        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 分页获取当前用户的审批历史记录
    /// </summary>
    public async Task<PagedResult<ApprovalRequestDto>> GetHistoryAsync(
        ApprovalHistoryQuery query, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var result = await _uow.ApprovalRequests.GetHistoryAsync(
            userId, query.Keyword, query.Status, query.Page, query.PageSize, ct);

        var items = new List<ApprovalRequestDto>();
        foreach (var entity in result.Items)
            items.Add(await MapToDtoAsync(entity, ct));

        return new PagedResult<ApprovalRequestDto>
        {
            Items = items,
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }

    /// <summary>
    /// 获取指定业务实体的最近一次被驳回的审批数据（用于重新提交预填）
    /// </summary>
    public async Task<LastRejectedApprovalDto?> GetLastRejectedAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        return await conn.QuerySingleOrDefaultAsync<LastRejectedApprovalDto>(
            _sql.Get("Approval.Select.Request.LastRejected"),
            new { Id = targetEntityId, Type = targetEntityType });
    }

    /// <summary>
    /// 获取审批业务详情（新旧对比数据）
    /// 优先从 ApprovalBizData 结构化数据构建，无结构化数据时回退 Description 正则解析
    /// 按业务类型分发：RENT_ADJUST / FEE_ADJUST / TERMINATE / ContractRenewal / ContractActivation
    /// </summary>
    public async Task<ApprovalBizDetailDto?> GetBizDetailAsync(Guid id, CancellationToken ct = default)
    {
        var approval = await _uow.ApprovalRequests.GetByIdAsync(id, ct);
        if (approval == null) return null;

        // 优先从 ApprovalBizData 表读取结构化业务数据（新审批）
        var bizData = await _uow.ApprovalBizData.GetByApprovalRequestIdAsync(id, ct);
        if (bizData != null)
        {
            try
            {
                return await BuildBizDetailFromStructuredData(bizData, approval, ct);
            }
            catch (Exception ex)
            {
                // ★ 定位无业务数据的根因
                System.Diagnostics.Debug.WriteLine($"[BizDetail] BuildBizDetailFromStructuredData failed for approval {id}: {ex}");
            }
        }

        // Fallback: 旧审批无结构化数据时，保留原有逻辑
        var fallback = BuildBizDetailFromDescription(approval);
        if (fallback != null) return fallback;

        // 最终 fallback：至少返回标题
        return new ApprovalBizDetailDto
        {
            Title = approval.Title ?? "",
            BizType = approval.TargetEntityType,
            Fields = new List<BizFieldDto>()
        };
    }

    private async Task<ApprovalBizDetailDto?> BuildBizDetailFromStructuredData(
        ApprovalBizData bizData, ApprovalRequest approval, CancellationToken ct)
    {
        var dto = new ApprovalBizDetailDto
        {
            Title = approval.Title ?? "",
            EffectiveDate = bizData.EffectiveDate?.ToString("yyyy-MM-dd")
        };

        switch (bizData.ChangeType)
        {
            case "RENT_ADJUST":
                var diff = (bizData.NewAmount ?? 0) - (bizData.OldAmount ?? 0);
                var pct = bizData.OldAmount > 0 ? diff / bizData.OldAmount.Value * 100 : 0;
                dto.BizType = "RENT_ADJUST";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "调整前月租", OldValue = $"¥{bizData.OldAmount:N2}" },
                    new() { Label = "调整后月租", NewValue = $"¥{bizData.NewAmount:N2}", IsChanged = true },
                    new() { Label = "调整差额",   NewValue = $"{(diff >= 0 ? "+" : "")}¥{diff:N2} ({(pct >= 0 ? "+" : "")}{pct:F1}%)", IsChanged = true },
                    new() { Label = "生效日期",   NewValue = bizData.EffectiveDate?.ToString("yyyy-MM-dd"), IsChanged = true },
                    new() { Label = "调整原因",   NewValue = bizData.Reason },
                };
                break;

            case "FEE_ADJUST":
                var feeItems = await _uow.ApprovalFeeItems.GetByApprovalRequestIdAsync(approval.Id, ct);
                dto.BizType = "FEE_ADJUST";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "调价项目数", NewValue = $"{feeItems.Count} 项", IsChanged = true },
                    new() { Label = "生效日期",   NewValue = bizData.EffectiveDate?.ToString("yyyy-MM-dd"), IsChanged = true },
                };

                // 逐项查询当前活跃配置，补充原数据信息
                dto.FeeItems = new List<BizFeeItemDto>();
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    foreach (var item in feeItems)
                    {
                        var oldConfig = await conn.QuerySingleOrDefaultAsync<dynamic>(
                            _sql.Get("Lease.Select.ContractFeeConfig.FullCurrentByContractAndFee"),
                            new { ContractId = item.ContractId, FeeCodeId = item.FeeCodeId });

                        // 旧配置无 ChargeType 时直接从 FeeCodes 表查询
                        var chargeType = oldConfig?.ChargeType as string;
                        if (string.IsNullOrEmpty(chargeType))
                        {
                            var feeCodeInfo = await conn.QuerySingleOrDefaultAsync<dynamic>(
                                _sql.Get("FeeCode.Select.FeeCode.ChargeTypeById"), new { Id = item.FeeCodeId });
                            chargeType = feeCodeInfo?.ChargeType as string;
                        }

                        dto.FeeItems.Add(new BizFeeItemDto
                        {
                            FeeName = item.FeeName,
                            OldAmount = item.OldAmount,
                            NewAmount = item.NewAmount,
                            BillingMode = item.BillingMode,
                            Unit = item.Unit,
                            EffectiveDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd"),
                            OldEffectiveDate = oldConfig?.EffectiveDate is DateTime oldEd
                                ? oldEd.ToString("yyyy-MM-dd") : oldConfig?.EffectiveDate as string,
                            OldExpiryDate = oldConfig?.ExpiryDate is DateTime oldXd
                                ? oldXd.ToString("yyyy-MM-dd") : oldConfig?.ExpiryDate as string,
                            OldBillingMode = oldConfig?.BillingMode as string,
                            OldUnit = oldConfig?.Unit as string,
                            ChargeType = chargeType,
                        });
                    }
                }
                break;

            case "RECEIVABLE_GENERATE":
                dto.BizType = "RECEIVABLE_GENERATE";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "合同号",   NewValue = bizData.ContractNo },
                    new() { Label = "应收合计", NewValue = bizData.NewAmount.HasValue ? $"¥{bizData.NewAmount:N2}" : "-", IsChanged = true },
                    new() { Label = "账期范围", NewValue = bizData.Reason },
                };

                // 加载费用明细（带 ChargeType 区分周期/一次性）
                using (var conn = _connectionFactory.CreateConnection())
                {
                    conn.Open();
                    var items = conn.Query<dynamic>(
                        _sql.Get("ReceivableGenerate.Select.Items.WithChargeTypeByRequestId"),
                        new { RequestId = approval.TargetEntityId }).ToList();
                    if (items.Count > 0)
                    {
                        dto.FeeItems = items.Select(i => new BizFeeItemDto
                        {
                            FeeName = (string)i.FeeName,
                            NewAmount = (decimal)i.Amount,
                            EffectiveDate = (string)i.Period,
                            ChargeType = (string)(i.ChargeType ?? "Recurring"),
                        }).ToList();
                    }
                }
                break;

            case "TERMINATE":
                dto.BizType = "TERMINATE";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "终止类型", NewValue = bizData.TerminateType == "EARLY" ? "提前解约" : "到期终止" },
                    new() { Label = "实际搬离日", NewValue = bizData.ActualEndDate?.ToString("yyyy-MM-dd") },
                    new() { Label = "押金处理", NewValue = bizData.DepositReturn switch
                    {
                        "FULL" => "全额退还",
                        "DEDUCT" => "扣款后退还",
                        "LAST_RENT" => "抵扣最后月租",
                        _ => bizData.DepositReturn
                    }},
                    new() { Label = "终止原因", NewValue = bizData.Reason },
                };
                break;
        }
        return dto.Fields.Count > 0 ? dto : null;
    }
    /// <summary>旧审批回调：保留原有正则解析 + ContractRenewal/ChangeRequest 分支</summary>
    private ApprovalBizDetailDto? BuildBizDetailFromDescription(ApprovalRequest approval)
    {
        var desc = approval.Description;
        var dto = new ApprovalBizDetailDto { Title = approval.Title ?? "" };

        // 优先按 TargetEntityType 分发（不依赖 Description）
        if (approval.TargetEntityType == "ContractRenewal" && approval.TargetEntityId != Guid.Empty)
        {
            var renewal = _uow.RenewalRequests.GetByIdAsync(approval.TargetEntityId, CancellationToken.None).GetAwaiter().GetResult();
            if (renewal != null)
            {
                var oldContract = _uow.Contracts.GetByIdAsync(renewal.OldContractId, CancellationToken.None).GetAwaiter().GetResult();
                dto.Fields.Add(new BizFieldDto { Label = "原合同号", OldValue = oldContract?.ContractNo, NewValue = renewal.ContractNo, IsChanged = true });
                dto.Fields.Add(new BizFieldDto { Label = "月租金", OldValue = $"¥{renewal.PreviousRent:N2}", NewValue = $"¥{renewal.NewRent:N2}", IsChanged = true });
                dto.Fields.Add(new BizFieldDto { Label = "到期日", OldValue = oldContract?.EndDate?.ToString("yyyy-MM-dd") ?? "不限", NewValue = renewal.NewEndDate.ToString("yyyy-MM-dd"), IsChanged = true });
                var oldDeposit = renewal.OldDepositAmount;
                var newDeposit = renewal.DepositHandling == "NEW" ? (renewal.NewDepositAmount ?? oldDeposit) : oldDeposit;
                dto.Fields.Add(new BizFieldDto { Label = "押金", OldValue = $"¥{oldDeposit:N2}", NewValue = $"¥{newDeposit:N2}", IsChanged = newDeposit != oldDeposit });
                dto.Fields.Add(new BizFieldDto { Label = "押金处理方式", OldValue = null, NewValue = renewal.DepositHandling == "TRANSFER" ? "原押金延续" : "重新收取", IsChanged = false });
                if (!string.IsNullOrEmpty(renewal.Remark))
                    dto.Fields.Add(new BizFieldDto { Label = "备注", OldValue = null, NewValue = renewal.Remark, IsChanged = true });
            }
            return dto.Fields.Count > 0 ? dto : null;
        }

        // ContractActivation 类型：新建合同审批
        if (approval.TargetEntityType == "ContractActivation" && approval.TargetEntityId != Guid.Empty)
        {
            var req = _uow.ContractCreateRequests.GetByIdAsync(approval.TargetEntityId, CancellationToken.None).GetAwaiter().GetResult();
            if (req != null)
            {
                using var conn = _connectionFactory.CreateConnection(); conn.Open();
                var room = conn.QuerySingleOrDefault(_sql.Get("ContractCreate.Select.Room.FullCode"), new { Id = req.RoomId });
                var tenants = conn.Query(_sql.Get("ContractCreate.Select.Tenants.NamesByRequest"), new { Id = req.Id }).ToList();
                var fees = conn.Query(_sql.Get("ContractCreate.Select.Fees.NamesByRequest"), new { Id = req.Id }).ToList();

                dto.BizType = "ContractActivation";
                dto.Fields.Add(new BizFieldDto { Label = "合同编号", NewValue = req.ContractNo });
                dto.Fields.Add(new BizFieldDto { Label = "房屋", NewValue = room?.FullCode ?? "" });
                dto.Fields.Add(new BizFieldDto { Label = "起租日期", NewValue = req.StartDate.ToString("yyyy-MM-dd") });
                dto.Fields.Add(new BizFieldDto { Label = "到期日期", NewValue = req.EndDate?.ToString("yyyy-MM-dd") ?? "不限" });
                dto.Fields.Add(new BizFieldDto { Label = "付款周期", NewValue = req.PaymentCycle switch { "Monthly" => "月付", "Quarterly" => "季付", "HalfYearly" => "半年付", "Yearly" => "年付", _ => req.PaymentCycle } });
                dto.Fields.Add(new BizFieldDto { Label = "租客", NewValue = string.Join("、", tenants.Select(t => (string)t.Name)) });
                var feeItems = fees.Select(f => new BizFeeItemDto
                {
                    FeeName = f.Name, NewAmount = f.Amount,
                    ChargeType = f.ChargeType, BillingMode = f.BillingMode,
                    EffectiveDate = f.EffectiveDate
                }).ToList();
                dto.FeeItems = feeItems;
                var onceFees = fees.Where(f => f.ChargeType == "OneTime").ToList();
                var recFees = fees.Where(f => f.ChargeType == "Recurring").ToList();
                if (onceFees.Count > 0)
                {
                    var t = onceFees.Sum(f => (decimal)f.Amount);
                    dto.Fields.Add(new BizFieldDto { Label = $"一次性费用（{onceFees.Count} 项）", NewValue = $"¥{t:N2}" });
                }
                if (recFees.Count > 0)
                {
                    var t = recFees.Sum(f => (decimal)f.Amount);
                    dto.Fields.Add(new BizFieldDto { Label = $"周期性费用（{recFees.Count} 项）", NewValue = $"¥{t:N2}/月" });
                }
                var grandTotal = fees.Sum(f => (decimal)f.Amount);
                dto.Fields.Add(new BizFieldDto { Label = $"费用合计（{fees.Count} 项）", NewValue = $"¥{grandTotal:N2}" });
            }
            return dto.Fields.Count > 0 ? dto : null;
        }

        // Contract 类型：正则解析（需要 Description）
        if (!string.IsNullOrEmpty(desc) && approval.TargetEntityType == "Contract" && approval.Title?.StartsWith("[合同终止]") == false)
        {
            var match = System.Text.RegularExpressions.Regex.Match(desc, @"→\s*¥([\d,]+)");
            if (match.Success)
            {
                var newAmount = match.Groups[1].Value;
                var contract = _uow.Contracts.GetByIdAsync(approval.TargetEntityId, CancellationToken.None).GetAwaiter().GetResult();
                dto.Fields.Add(new BizFieldDto { Label = "月租金", OldValue = newAmount, NewValue = $"¥{newAmount}", IsChanged = true });

                var dateMatch = System.Text.RegularExpressions.Regex.Match(desc, @"生效日期[：:](\S+)");
                if (dateMatch.Success)
                    dto.Fields.Add(new BizFieldDto { Label = "生效日期", OldValue = null, NewValue = dateMatch.Groups[1].Value, IsChanged = true });

                var reasonMatch = System.Text.RegularExpressions.Regex.Match(desc, @"调整原因[：:](\S+)");
                if (reasonMatch.Success)
                    dto.Fields.Add(new BizFieldDto { Label = "调整原因", OldValue = null, NewValue = reasonMatch.Groups[1].Value, IsChanged = true });
            }
        }
        else if (!string.IsNullOrEmpty(desc) && approval.TargetEntityType == "Contract" && approval.Title?.StartsWith("[合同终止]") == true)
        {
            var contract = _uow.Contracts.GetByIdAsync(approval.TargetEntityId, CancellationToken.None).GetAwaiter().GetResult();
            dto.Fields.Add(new BizFieldDto { Label = "合同号", OldValue = contract?.ContractNo, NewValue = null, IsChanged = false });
            dto.Fields.Add(new BizFieldDto { Label = "终止原因", OldValue = null, NewValue = approval.Description, IsChanged = true });
        }

        return dto.Fields.Count > 0 ? dto : null;
    }




    private async Task<ApprovalRequestDto> MapToDtoAsync(ApprovalRequest entity, CancellationToken ct)
    {
        string? typeName = null;
        if (entity.ApprovalTypeId != Guid.Empty)
        {
            var type = await _uow.ApprovalTypes.GetByIdAsync(entity.ApprovalTypeId, ct);
            typeName = type?.Name;
        }

        var approverIds = entity.Records.Select(r => r.ApproverId).Distinct().ToList();
        if (entity.CreatedBy != Guid.Empty && !approverIds.Contains(entity.CreatedBy))
            approverIds.Add(entity.CreatedBy);
        var userDict = new Dictionary<Guid, (string Name, string Account)>();
        if (approverIds.Count > 0)
        {
            foreach (var uid in approverIds)
            {
                var user = await _uow.Users.GetByIdAsync(uid, ct);
                if (user != null) userDict[uid] = (user.DisplayName, user.Username);
            }
        }

        var allUsersWithRoles = await _uow.Users.GetAllWithRolesAsync(ct);
        var roleUserMap = new Dictionary<Guid, List<(string Name, string Account)>>();
        foreach (var u in allUsersWithRoles)
        {
            foreach (var ur in u.Roles)
            {
                if (!roleUserMap.ContainsKey(ur.RoleId))
                    roleUserMap[ur.RoleId] = new();
                roleUserMap[ur.RoleId].Add((u.DisplayName, u.Username));
            }
        }

        string? currentLevelName = null;
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var currentLevelConfig = levels.FirstOrDefault(l =>
            l.ApprovalTypeId == entity.ApprovalTypeId && l.LevelNo == entity.CurrentLevel);
        if (currentLevelConfig != null)
        {
            var role = await _uow.Roles.GetByIdAsync(currentLevelConfig.ApproverRoleId, ct);
            if (role != null) currentLevelName = $"{role.Name}审批";
        }

        var lastRecord = entity.Records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

        var levelChain = new List<ApprovalLevelStatusDto>();
        var submitRecord = entity.Records.FirstOrDefault(r => r.Action == "Submitted");
        var submitterId = submitRecord?.ApproverId;
        if (submitterId == null || submitterId.Value == Guid.Empty)
            submitterId = entity.CreatedBy != Guid.Empty ? entity.CreatedBy : null;

        if (submitterId.HasValue)
        {
            var submitterInfo = userDict.GetValueOrDefault(submitterId.Value);
            levelChain.Add(new ApprovalLevelStatusDto
            {
                Level = 0,
                RoleName = "提交审批",
                Status = "submitted",
                ApproverName = submitterInfo.Name,
                ApproverAccount = submitterInfo.Account
            });
        }

        var allLevelConfigs = levels
            .Where(l => l.ApprovalTypeId == entity.ApprovalTypeId)
            .OrderBy(l => l.LevelNo).ToList();

        foreach (var lc in allLevelConfigs)
        {
            var role = await _uow.Roles.GetByIdAsync(lc.ApproverRoleId, ct);
            var roleName = role?.Name ?? "未知角色";
            var approvedRecord = entity.Records.FirstOrDefault(r => r.LevelNo == lc.LevelNo && r.Action != "Submitted");

            string status;
            if (approvedRecord != null)
                status = approvedRecord.Action == "Rejected" ? "rejected" : "completed";
            else if (lc.LevelNo == entity.CurrentLevel && entity.Status == "Pending")
                status = "current";
            else if (entity.Status is "Approved" or "Rejected" || lc.LevelNo < entity.CurrentLevel)
                status = "skipped";
            else
                status = "pending";

            string? expectedNames = null;
            if (approvedRecord == null && roleUserMap.TryGetValue(lc.ApproverRoleId, out var usersWithRole))
            {
                expectedNames = string.Join("、", usersWithRole.Select(u => $"{u.Name}({u.Account})"));
            }

            levelChain.Add(new ApprovalLevelStatusDto
            {
                Level = lc.LevelNo,
                RoleName = roleName,
                Status = status,
                ApproverName = approvedRecord != null
                    ? userDict.GetValueOrDefault(approvedRecord.ApproverId).Name : expectedNames,
                ApproverAccount = approvedRecord != null
                    ? userDict.GetValueOrDefault(approvedRecord.ApproverId).Account : null
            });
        }

        return new ApprovalRequestDto
        {
            Id = entity.Id,
            ApprovalTypeId = entity.ApprovalTypeId,
            Title = entity.Title,
            Description = entity.Description,
            TargetEntityId = entity.TargetEntityId,
            TargetEntityType = entity.TargetEntityType,
            Status = entity.Status,
            CurrentLevel = entity.CurrentLevel,
            MaxLevel = entity.MaxLevel,
            ApprovalTypeName = typeName,
            SubmitterName = entity.CreatedBy != Guid.Empty
                ? userDict.GetValueOrDefault(entity.CreatedBy).Name : null,
            CurrentLevelName = currentLevelName,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            LevelChain = levelChain,
            Records = entity.Records.OrderBy(r => r.CreatedAt).Select(r =>
            {
                var info = userDict.GetValueOrDefault(r.ApproverId);
                return new ApprovalRecordDto
                {
                    Id = r.Id,
                    Level = r.LevelNo,
                    ApproverId = r.ApproverId,
                    ApproverName = info.Name ?? r.ApproverId.ToString(),
                    ApproverAccount = info.Account ?? "",
                    Action = r.Action,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                };
            }).ToList()
        };
    }
}
