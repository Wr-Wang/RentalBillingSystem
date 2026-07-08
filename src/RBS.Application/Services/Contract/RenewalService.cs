using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Common;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using System.Data;

namespace RBS.Application.Services.Contract;

/// <summary>
/// 续签应用服务实现
/// </summary>
public class RenewalService : IRenewalService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly IApprovalService _approvalService;
    private readonly ISqlLoader _sql;
    private readonly IServiceProvider _serviceProvider;

    public RenewalService(IUnitOfWork uow, IDbConnectionFactory db, IApprovalService approvalService, ISqlLoader sql, IServiceProvider serviceProvider)
    {
        _uow = uow;
        _db = db;
        _approvalService = approvalService;
        _sql = sql;
        _serviceProvider = serviceProvider;
    }

    public async Task<RenewalPreviewDto> PreviewAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        var contract = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Lease.Select.Contract.RenewalPreview"), new { Id = contractId });

        if (contract == null)
            throw new KeyNotFoundException("合同不存在");

        var dto = new RenewalPreviewDto
        {
            OldContract = new RenewalOldContractDto
            {
                Id = contract.Id, ContractNo = contract.ContractNo,
                RentAmount = contract.RentAmount, DepositAmount = contract.DepositAmount,
                StartDate = FormatDate(contract.StartDate), EndDate = FormatDate(contract.EndDate),
                PaymentCycle = contract.PaymentCycle, Status = contract.Status,
                RoomFullCode = contract.RoomFullCode, RenewalCount = contract.RenewalCount ?? 0
            }
        };

        // 查租客
        var tenants = await conn.QueryAsync<RenewalInheritedTenantDto>(
            _sql.Get("Lease.Select.ContractTenant.WithNameByContract"), new { Id = contractId });
        dto.Tenants = tenants.ToList();

        // 查费用配置
        var fees = await conn.QueryAsync<RenewalInheritedFeeDto>(
            _sql.Get("Lease.Select.ContractFeeConfig.ActiveWithFeeName"), new { Id = contractId });
        dto.FeeConfigs = fees.ToList();

        // 欠费检查
        var outstanding = await conn.QuerySingleAsync<decimal>(
            _sql.Get("Billing.Select.ReceivablePlan.OutstandingByContract"),
            new { Id = contractId });
        dto.Checks.PaymentStatus = new PaymentStatusDto
        {
            Passed = outstanding <= 0,
            OutstandingAmount = outstanding
        };

        // 并发检查
        var hasPending = await conn.QuerySingleAsync<int>(
            _sql.Get("Approval.Select.Request.PendingByContractId"),
            new { Id = contractId });
        dto.Checks.ConcurrentApprovals = new ConcurrentApprovalsDto
        {
            HasPending = hasPending > 0,
            BlockedMessage = hasPending > 0 ? "该合同存在待审批的申请，请处理完成后再提交续签" : null
        };

        // 重复续签检查：是否已有合同指向本合同（PreviousContractId = 本合同Id）
        var renewedInfo = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Lease.Select.Contract.ByPreviousContractId"),
            new { Id = contractId });
        if (renewedInfo != null)
        {
            dto.Checks.ConcurrentApprovals.AlreadyRenewed = true;
            dto.Checks.ConcurrentApprovals.RenewedContractId = renewedInfo.Id;
            dto.Checks.ConcurrentApprovals.RenewedContractNo = renewedInfo.ContractNo;
            dto.Checks.ConcurrentApprovals.BlockedMessage = $"该合同已被续签，新合同号为 {renewedInfo.ContractNo}，不可再次续签";
        }

        // 市场参考价（mock数据，后续对接真实数据源）
        dto.Checks.MarketPrice = new MarketPriceInfoDto
        {
            MinPrice = contract.RentAmount * 0.9m,
            MaxPrice = contract.RentAmount * 1.2m,
            AveragePrice = contract.RentAmount * 1.05m,
            SourceDescription = "同小区近3个月成交均价（模拟数据）"
        };

        // 默认续签信息
        var endDate = DateOnly.FromDateTime(DateTime.Today);
        if (contract.EndDate is DateOnly ed) endDate = ed;
        var suggestedStart = endDate.AddDays(1);
        var suggestedEnd = new DateOnly(suggestedStart.Year + 1, suggestedStart.Month, suggestedStart.Day).AddDays(-1);

        dto.DefaultRenewalInfo = new RenewalDefaultsDto
        {
            SuggestedStartDate = suggestedStart.ToString("yyyy-MM-dd"),
            SuggestedEndDate = suggestedEnd.ToString("yyyy-MM-dd"),
            CurrentRentAmount = contract.RentAmount
        };

        return dto;
    }

    /// <summary>统一格式化日期（处理 Dapper dynamic 返回 DateTime 的场景）</summary>
    private static string FormatDate(object? dateValue)
    {
        if (dateValue == null) return "";
        if (dateValue is DateTime dt) return dt.ToString("yyyy-MM-dd");
        if (dateValue is DateOnly d) return d.ToString("yyyy-MM-dd");
        return dateValue.ToString() ?? "";
    }

    public async Task<RenewalSubmitResultDto> SubmitAsync(SubmitRenewalRequest request, Guid userId, CancellationToken ct = default)
    {
        // 1. 统一并发检查
        await EnsureNoPendingApprovalAsync(request.ContractId, ct);

        // 2. 加载旧合同
        var oldContract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct)
            ?? throw new KeyNotFoundException("合同不存在");

        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("只有生效中或已到期的合同可以续签");

        // 3. 重复续签检查：是否已有续签合同指向本合同
        using (var checkConn = _db.CreateConnection())
        {
            checkConn.Open();
            var existing = await checkConn.QuerySingleOrDefaultAsync<dynamic>(
                _sql.Get("Lease.Select.Contract.ByPreviousContractId"),
                new { Id = request.ContractId });
            if (existing != null)
                throw new InvalidOperationException($"该合同已被续签（新合同号：{existing.ContractNo}），不可再次续签");
        }

        // 4. 欠费检查
        using (var conn = _db.CreateConnection())
        {
            conn.Open();
            var outstanding = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Billing.Select.ReceivablePlan.OutstandingByContract"),
                new { Id = request.ContractId });

            if (outstanding > 0)
                throw new InvalidOperationException($"该合同有未结清欠费 ¥{outstanding:N2}，请先处理后再续签");
        }

        // 4.5 押金守卫：NEW 模式必须指定新押金金额
        if (request.DepositHandling == "NEW" && (!request.NewDepositAmount.HasValue || request.NewDepositAmount.Value <= 0))
            throw new InvalidOperationException("重新收取押金时，新押金金额必须大于 0");

        // 5. 生成新合同号：剥离已有 -R{n} 后缀，基于原始号 + 续签次数
        var baseNo = oldContract.ContractNo.Split("-R").First();
        var newContractNo = $"{baseNo}-R{oldContract.RenewalCount + 1}";

        // 5. 创建 RenewalRequest
        using var rentConn = _db.CreateConnection(); rentConn.Open();
        var oldRentAmount = await rentConn.QuerySingleOrDefaultAsync<decimal>(
            _sql.Get("Contract.Select.FeeConfig.AmountByCode"),
            new { Cid = oldContract.Id, Code = "RENT" });
        var oldDepositAmount = await rentConn.QuerySingleOrDefaultAsync<decimal>(
            _sql.Get("Contract.Select.DepositConfig.AmountByContract"),
            new { Cid = oldContract.Id });

        var renewal = new RenewalRequest(
            oldContract.Id, newContractNo, oldRentAmount,
            request.NewRentAmount, DateOnly.FromDateTime(DateTime.Parse(request.NewEndDate, System.Globalization.CultureInfo.InvariantCulture)), oldContract.CompanyId);

        renewal.SetDepositInfo(
            request.DepositHandling, oldDepositAmount, request.NewDepositAmount);
        renewal.SetMarketPrice(request.MarketReferencePrice);
        renewal.SetPaymentStatusCheck(true);
        renewal.SetRemark(request.Remark);
        // 注意：AddAsync 内部会调用 SetCreated 覆盖 CreatedBy/CreatedAt
        // 所以这里不手动设置，由仓储统一处理
        await _uow.RenewalRequests.AddAsync(renewal, ct);

        // 6. 提交审批
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_RENEW", ct);
        if (approvalType == null)
            throw new InvalidOperationException("未配置续签审批类型，请联系管理员");

        // 找审批级别配置
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var typeLevels = levels.Where(l => l.ApprovalTypeId == approvalType.Id).ToList();
        var maxLevel = typeLevels.Count > 0 ? typeLevels.Max(l => l.Level) : 0;

        var approvalRequest = new ApprovalRequest(
            approvalType.Id,
            $"合同续签 - {oldContract.ContractNo}",
            renewal.Id,
            "ContractRenewal",
            oldContract.CompanyId,
            maxLevel);  // 直接传入 maxLevel，不再用反射

        // 设置创建人和合同ID
        approvalRequest.SetCreated(userId, ChinaTime.Now, null, null);
        approvalRequest.SetContractId(oldContract.Id);
        approvalRequest.AddRecord(userId, "Submitted",
            $"续签：月租 ¥{renewal.PreviousRent:N2} → ¥{request.NewRentAmount:N2}，" +
            $"到期日：{request.NewEndDate}，" +
            $"押金：¥{renewal.OldDepositAmount:N2} → ¥{(request.DepositHandling == "NEW" ? (request.NewDepositAmount ?? renewal.OldDepositAmount) : renewal.OldDepositAmount):N2}（{(request.DepositHandling == "TRANSFER" ? "原押金延续" : "重新收取")}）");

        approvalRequest.Submit();
        await _uow.ApprovalRequests.AddAsync(approvalRequest, ct);

        // [事件] 提交后通知第一级审批人
        if (approvalRequest.Status == "Pending")
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalSubmittedEvent>>();
            await handler.HandleAsync(
                new ApprovalSubmittedEvent(approvalRequest.Id, approvalRequest.ApprovalTypeId,
                    approvalRequest.TargetEntityId, approvalRequest.TargetEntityType, approvalRequest.Title),
                ct);
        }

        // 7. 提交审批（写入审批记录）
        var firstRecord = approvalRequest.Records.First();
        using (var updateConn = _db.CreateConnection())
        {
            updateConn.Open();
            await updateConn.ExecuteAsync(
                _sql.Get("Lease.Insert.RenewalRequest.ApprovalRecord"),
                new
                {
                    firstRecord.Id, ApprovalRequestId = approvalRequest.Id,
                    firstRecord.Level, firstRecord.ApproverId, firstRecord.Action,
                    Comment = firstRecord.Comment ?? "", firstRecord.CreatedBy, firstRecord.CreatedAt
                });

            // 更新状态
            await updateConn.ExecuteAsync(
                _sql.Get("Lease.Update.ApprovalRequest.StatusAndContract"),
                new { approvalRequest.Status, ContractId = oldContract.Id, approvalRequest.Id });

            // 更新 RenewalRequest 状态
            await updateConn.ExecuteAsync(
                _sql.Get("Lease.Update.RenewalRequest.SetPendingApproval"),
                new { renewal.Id });
        }

        return new RenewalSubmitResultDto
        {
            RenewalRequestId = renewal.Id,
            ApprovalRequestId = approvalRequest.Id,
            Status = approvalRequest.Status,
            Message = "续签申请已提交，等待审批"
        };
    }

    public async Task ExecuteRenewalAsync(Guid renewalRequestId, CancellationToken ct = default)
    {
        var renewal = await _uow.RenewalRequests.GetByIdAsync(renewalRequestId, ct)
            ?? throw new KeyNotFoundException("续签请求不存在");

        if (renewal.Status != "PendingApproval")
            throw new InvalidOperationException($"续签请求状态为 {renewal.Status}，无法执行（仅 PendingApproval 可执行）");

        var oldContract = await _uow.Contracts.GetByIdAsync(renewal.OldContractId, ct)
            ?? throw new KeyNotFoundException("原合同不存在");

        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("原合同状态不允许续签");

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var now = ChinaTime.Now;
            var newId = Guid.NewGuid();

            // 1. 乐观锁：原子性地标记 RenewalRequest 防止重复执行
            var locked = await conn.ExecuteAsync(
                _sql.Get("Lease.Update.RenewalRequest.LockExecuting"),
                new { renewal.Id, Now = now }, tx);
            if (locked == 0)
                throw new InvalidOperationException("续签请求已被其他操作处理，请刷新后重试");

            // 2. 处理旧合同状态（区分 Active 和 Expired）
            int affected;
            if (oldContract.Status == "Expired")
            {
                // 已到期合同 → 标记 Renewed（终态）
                affected = await conn.ExecuteAsync(
                    _sql.Get("Lease.Update.Contract.RenewedGuard"),
                    new { Id = renewal.OldContractId }, tx);
            }
            else
            {
                // 未到期合同 → 继续保持 Active，只校验状态未被修改
                affected = await conn.ExecuteAsync(
                    _sql.Get("Lease.Update.Contract.ActiveGuard"),
                    new { Id = renewal.OldContractId }, tx);
            }
            if (affected == 0)
                throw new InvalidOperationException("原合同状态已被修改，续签执行失败");

            // 3. 创建新合同
            var depositAmount = renewal.DepositHandling == "TRANSFER"
                ? renewal.OldDepositAmount
                : (renewal.NewDepositAmount ?? renewal.OldDepositAmount);

            var startDate = oldContract.EndDate.AddDays(1);

            await conn.ExecuteAsync(
                _sql.Get("Lease.Insert.Contract.FromRenewal"),
                new
                {
                    Id = newId, ContractNo = renewal.ContractNo,
                    RoomId = oldContract.RoomId,
                    StartDate = startDate,
                    EndDate = renewal.NewEndDate, PaymentCycle = oldContract.PaymentCycle,
                    CompanyId = oldContract.CompanyId,
                    PreviousContractId = oldContract.Id,
                    RenewalCount = oldContract.RenewalCount + 1,
                    OriginalContractId = oldContract.OriginalContractId ?? oldContract.Id,
                    MarketPrice = renewal.MarketReferencePrice,
                    CreatedBy = renewal.CreatedBy, CreatedAt = now
                }, tx);

            // 3. 复制租客
            var tenants = await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.ContractTenant.PrimaryByContract"),
                new { Id = renewal.OldContractId }, tx);

            foreach (var t in tenants)
            {
                await conn.ExecuteAsync(
                    _sql.Get("Lease.Insert.ContractTenant.Default"),
                    new { Id = Guid.NewGuid(), ContractId = newId, t.TenantId, t.IsPrimary, CreatedBy = renewal.CreatedBy, CreatedAt = now }, tx);
            }

            // 4. 复制费用配置（生效日期设为新合同起租日）
            var fees = await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.ContractFeeConfig.ActiveByContract"),
                new { Id = renewal.OldContractId }, tx);

            foreach (var f in fees)
            {
                await conn.ExecuteAsync(
                    _sql.Get("Lease.Insert.ContractFeeConfig.CopyFromRenewal"),
                    new { Id = Guid.NewGuid(), ContractId = newId, f.FeeCodeId, f.BillingMode, f.Amount, f.Unit, f.UnitPrice,
                        EffectiveDate = startDate.ToString("yyyy-MM-dd"), CreatedBy = renewal.CreatedBy, CreatedAt = now }, tx);
            }

            // 4.5 原合同费用配置到期（在复制之后执行，避免 SELECT 查不到数据）
            var oldEndDate = oldContract.EndDate.ToString("yyyy-MM-dd");
            await conn.ExecuteAsync(
                _sql.Get("Lease.Update.ContractFeeConfig.ExpireByOldContract"),
                new { p0 = oldEndDate, p1 = renewal.OldContractId }, tx);

            // 5. 押金处理
            if (renewal.DepositHandling == "TRANSFER")
            {
                // 原押金转移
                await conn.ExecuteAsync(
                    _sql.Get("Lease.Insert.DepositLog.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = renewal.OldContractId,
                        Amount = -renewal.OldDepositAmount, Balance = 0m,
                        Action = "TransferOut", Remark = "续签押金转出新合同",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);

                await conn.ExecuteAsync(
                    _sql.Get("Lease.Insert.DepositLog.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = newId,
                        Amount = renewal.OldDepositAmount, Balance = renewal.OldDepositAmount,
                        Action = "TransferIn", Remark = "续签押金转入",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);
            }
            else if (renewal.DepositHandling == "NEW")
            {
                var diff = (renewal.NewDepositAmount ?? 0) - renewal.OldDepositAmount;

                // 退旧押金
                await conn.ExecuteAsync(
                    _sql.Get("Lease.Insert.DepositLog.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = renewal.OldContractId,
                        Amount = -renewal.OldDepositAmount, Balance = 0m,
                        Action = "Refund", Remark = $"续签退押金，新押金 ¥{depositAmount:N2}",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);

                // 收新押金
                await conn.ExecuteAsync(
                    _sql.Get("Lease.Insert.DepositLog.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = newId,
                        Amount = depositAmount, Balance = depositAmount,
                        Action = "Collection", Remark = "续签新收押金",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);
            }

            // 6. 更新 RenewalRequest
            await conn.ExecuteAsync(
                _sql.Get("Lease.Update.RenewalRequest.Complete"),
                new { NewContractId = newId, renewal.Id, Now = now }, tx);

            tx.Commit();

            // 6.5 ★ 为新合同押金生成一次性 JE（Commit 后执行，不影响续签主流程）
            try
            {
                if (depositAmount > 0)
                {
                    var depositFeeConfigId = await conn.QuerySingleOrDefaultAsync<Guid>(
                        _sql.Get("Contract.Select.FeeConfig.IdByContractAndCode"),
                        new { Cid = newId, Code = "DEPOSIT" });
                    if (depositFeeConfigId != Guid.Empty)
                    {
                        var journalGen = _serviceProvider.GetRequiredService<IJournalGenerationService>();
                        await journalGen.GenerateOneTimeAsync(newId, depositFeeConfigId, ct);
                    }
                }
            }
            catch
            {
                // JE 生成失败不影响续签完成，可后续通过 Job 重试
            }

            // 7. 写 ChangeHistory（续签完成后记录）
            try
            {
                using var histConn = _db.CreateConnection();
                histConn.Open();

                var oldDeposit = renewal.OldDepositAmount;
                var newDeposit = depositAmount;
                var opName = renewal.CreatedBy != Guid.Empty
                    ? (histConn.QuerySingleOrDefault<string>(
                        _sql.Get("Identity.Select.User.DisplayName"), new { Id = renewal.CreatedBy })
                       ?? "")
                    : "";

                if (renewal.DepositHandling == "TRANSFER")
                {
                    // 押金延续记录
                    await histConn.ExecuteAsync(
                        _sql.Get("Contract.Insert.ChangeHistory.Default"),
                        new
                        {
                            Id = Guid.NewGuid(), ContractId = oldContract.Id,
                            ChangeType = "DEPOSIT_TRANSFER", Title = "续签押金延续",
                            Detail = $"续签押金延续：¥{oldDeposit:F2}",
                            OldValue = oldDeposit, NewValue = oldDeposit,
                            EffectiveDate = startDate.ToString("yyyy-MM-dd"),
                            OperatorId = renewal.CreatedBy, OperatorName = opName
                        });
                }
                else if (renewal.DepositHandling == "NEW")
                {
                    var diff = (renewal.NewDepositAmount ?? 0) - oldDeposit;
                    var detail = diff > 0
                        ? $"续签押金调整：¥{oldDeposit:F2} → ¥{newDeposit:F2}（上调 ¥{diff:F2}）"
                        : diff < 0
                            ? $"续签押金调整：¥{oldDeposit:F2} → ¥{newDeposit:F2}（下调 ¥{Math.Abs(diff):F2}）"
                            : $"续签押金调整：¥{oldDeposit:F2} → ¥{newDeposit:F2}";

                    await histConn.ExecuteAsync(
                        _sql.Get("Contract.Insert.ChangeHistory.Default"),
                        new
                        {
                            Id = Guid.NewGuid(), ContractId = oldContract.Id,
                            ChangeType = "DEPOSIT_ADJUST", Title = "续签押金调整",
                            Detail = detail,
                            OldValue = oldDeposit, NewValue = newDeposit,
                            EffectiveDate = startDate.ToString("yyyy-MM-dd"),
                            OperatorId = renewal.CreatedBy, OperatorName = opName
                        });
                }

                // 续签整体摘要（写在新合同上）
                var depositHandlingLabel = renewal.DepositHandling == "NEW" ? "重新收取" : "原押金延续";
                await histConn.ExecuteAsync(
                    _sql.Get("Contract.Insert.ChangeHistory.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = newId,
                        ChangeType = "RENEWAL", Title = "合同续签完成",
                        Detail = $"续签完成：月租 ¥{renewal.PreviousRent:F2} → ¥{renewal.NewRent:F2}，押金 ¥{oldDeposit:F2} → ¥{newDeposit:F2}（{depositHandlingLabel}）",
                        OldValue = renewal.PreviousRent, NewValue = renewal.NewRent,
                        EffectiveDate = startDate.ToString("yyyy-MM-dd"),
                        OperatorId = renewal.CreatedBy, OperatorName = opName
                    });
            }
            catch
            {
                // ChangeHistory 写入失败不应影响主流程
            }
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<List<RenewalHistoryDto>> GetHistoryAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<RenewalHistoryDto>(
            _sql.Get("Lease.Select.RenewalRequest.History"),
            new { Id = contractId });
        return rows.ToList();
    }

    public async Task<List<RenewalChainNodeDto>> GetRenewalChainAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        // 先找到原始合同（OriginalContractId 或自身）
        var rootId = await conn.QuerySingleAsync<Guid?>(
            _sql.Get("Lease.Select.Contract.RootId"),
            new { Id = contractId });

        if (rootId == null)
            return new List<RenewalChainNodeDto>();

        // 获取整条链
        var chain = await conn.QueryAsync<RenewalChainNodeDto>(
            _sql.Get("Lease.Select.Contract.RenewalChain"),
            new { RootId = rootId, TargetId = contractId });
        return chain.ToList();
    }

    public async Task<ContractOperationsDto> GetAllowedOperationsAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract == null) throw new KeyNotFoundException("合同不存在");

        var dto = new ContractOperationsDto();

        if (contract.Status == "Renewed")
        {
            // 已续签合同完全只读
            dto.CanModifyRent = false;
            dto.CanTerminate = false;
            dto.CanRenew = false;
            dto.CanSuspend = false;
            dto.CanResume = false;
            dto.CanAdjustFee = false;
            return dto;
        }

        // 检查是否有待审批流
        using var conn = _db.CreateConnection();
        conn.Open();
        var pendingType = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.ApprovalType.PendingByContract"),
            new { Id = contractId });

        if (pendingType != null)
        {
            dto.PendingApprovalType = pendingType;
            dto.CanModifyRent = false;
            dto.CanTerminate = false;
            dto.CanRenew = false;
            dto.CanAdjustFee = false;
        }

        // 根据合同状态限制
        if (contract.Status != "Active")
        {
            dto.CanSuspend = false;
            dto.CanResume = contract.Status == "Suspended";
        }

        return dto;
    }

    public async Task<RejectedRenewalDto?> GetLastRejectedAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        return await conn.QuerySingleOrDefaultAsync<RejectedRenewalDto>(
            _sql.Get("Lease.Select.RenewalRequest.LastRejected"),
            new { Id = contractId });
    }

    public async Task EnsureNoPendingApprovalAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var hasPending = await conn.QuerySingleAsync<int>(
            _sql.Get("Approval.Select.Request.PendingByContractId"),
            new { Id = contractId });

        if (hasPending > 0)
            throw new InvalidOperationException("该合同存在待审批的申请，请处理完成后再提交");
    }
}
