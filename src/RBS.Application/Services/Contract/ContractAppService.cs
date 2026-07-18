using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;
using System.Data;
using ContractEntity = RBS.Core.Entities.Contract.Contract;

namespace RBS.Application.Services.Contract;

/// <summary>
/// 合同管理应用服务实现 — 基于 Dapper 直接 SQL 访问数据，手动关联租客与续签状态
/// 查询使用 IDbConnectionFactory 创建独立连接，写操作用 IUnitOfWork 工作单元
/// </summary>
public class ContractAppService : IContractService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IUnitOfWork _uow;
    private readonly IContractDomainService _contractDomain;
    private readonly IReceivableGenerationService _receivableGen;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ContractAppService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql,
        IContractDomainService contractDomain,
        IReceivableGenerationService receivableGen)
    {
        _uow = uow; _db = db; _sql = sql; _contractDomain = contractDomain;
        _receivableGen = receivableGen;
    }

    /// <summary>
    /// 获取指定公司的合同列表（含主租客信息）
    /// 优化：使用 Dapper 多结果集查询，减少数据库往返
    /// </summary>
    public async Task<List<ContractDto>> GetListAsync(Guid companyId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<ContractDto>(
            _sql.Get("Lease.Select.Contract.ListByCompany"),
            new { Id = companyId });
        var list = rows.ToList();

        if (list.Count > 0)
        {
            var ids = list.Select(x => x.Id).ToList();
            var tenants = await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.ContractTenant.PrimaryByIds"),
                new { Ids = ids });
            var tenantLookup = tenants.Cast<IDictionary<string, object>>()
                .GroupBy(d => (Guid)d["ContractId"])
                .ToDictionary(g => g.Key, g => g.First());
            foreach (var item in list)
            {
                if (tenantLookup.TryGetValue(item.Id, out var t))
                    item.Tenants = new List<ContractTenantDto> { new ContractTenantDto { ContractId = item.Id, TenantId = t.ContainsKey("TenantId") && t["TenantId"] is Guid gt ? gt : Guid.Empty, TenantName = t.ContainsKey("TenantName") ? t["TenantName"] as string ?? "" : "", TenantPhone = t.ContainsKey("TenantPhone") ? t["TenantPhone"] as string ?? "" : "" } };
            }
        }
        return list;
    }

    
    /// <summary>
    /// 根据租客 ID 获取其关联的所有合同（含主租客信息）
    /// </summary>
    public async Task<List<ContractDto>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<ContractDto>(
            _sql.Get("Lease.Select.Contract.ByTenantId"),
            new { TenantId = tenantId });
        var list = rows.ToList();
        if (list.Count > 0)
        {
            var ids = list.Select(x => x.Id).ToList();
            var tenants = await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.ContractTenant.PrimaryByIds"),
                new { Ids = ids });
            var lookup = tenants.Cast<IDictionary<string, object>>().GroupBy(d => (Guid)d["ContractId"]).ToDictionary(g => g.Key, g => g.First());
            foreach (var item in list)
                if (lookup.TryGetValue(item.Id, out var t))
                    item.Tenants = new List<ContractTenantDto> { new ContractTenantDto { ContractId = item.Id, TenantId = t.ContainsKey("TenantId") && t["TenantId"] is Guid gt ? gt : Guid.Empty, TenantName = t.ContainsKey("TenantName") ? t["TenantName"] as string ?? "" : "", TenantPhone = t.ContainsKey("TenantPhone") ? t["TenantPhone"] as string ?? "" : "" } };
        }
        return list;
    }

    /// <summary>
    /// 分页查询合同列表
    /// 优化：无关键词时 COUNT 无需 JOIN HousingUnits；
    /// 租客 + 续签状态使用 Task.WhenAll 并行查询减少等待时间
    /// </summary>
    public async Task<PagedResult<ContractDto>> GetPagedListAsync(Guid companyId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null, Guid? roomId = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var where = new List<string> { "c.CompanyId = @CompanyId" };
        var parms = new DynamicParameters();
        parms.Add("@CompanyId", companyId);

        var hasKeyword = !string.IsNullOrEmpty(keyword);
        var hasStatus = !string.IsNullOrEmpty(status);
        var hasRoomId = roomId.HasValue && roomId.Value != Guid.Empty;

        if (hasKeyword) { where.Add("(c.ContractNo LIKE @Keyword OR r.FullCode LIKE @Keyword)"); parms.Add("@Keyword", $"%{keyword}%"); }
        if (hasStatus) { where.Add("c.Status = @Status"); parms.Add("@Status", status); }
        if (hasRoomId) { where.Add("c.RoomId = @RoomId"); parms.Add("@RoomId", roomId!.Value); }

        var w = "WHERE " + string.Join(" AND ", where);
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset); parms.Add("@PageSize", pageSize);

        // 优化：无关键词时 COUNT 无需 JOIN HousingUnits
        var total = await conn.QuerySingleAsync<int>(
            hasKeyword
                ? $"SELECT COUNT(*) FROM Contracts c LEFT JOIN HousingUnits r ON r.Id = c.RoomId {w}"
                : $"SELECT COUNT(*) FROM Contracts c {w}", parms);

        // 优化：用 OUTER APPLY 替代 CORRELATED EXISTS（减少子查询重复评估）
        var joinClause = hasKeyword
            ? "FROM Contracts c LEFT JOIN HousingUnits r ON r.Id = c.RoomId"
            : "FROM Contracts c LEFT JOIN HousingUnits r ON r.Id = c.RoomId";

        var rows = await conn.QueryAsync<ContractDto>($@"
SELECT c.Id, c.ContractNo, c.RoomId, r.FullCode AS RoomFullCode,
       c.StartDate, c.EndDate, c.PaymentCycle, c.Status, c.CompanyId,
       CASE WHEN prev.ContractId IS NOT NULL THEN 1 ELSE 0 END AS HasRenewalContract,
       c.AutoRenew
{joinClause}
OUTER APPLY (SELECT TOP 1 1 AS ContractId FROM Contracts prev WHERE prev.PreviousContractId = c.Id) prev
{w}
ORDER BY c.CreatedAt DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", parms);
        var list = rows.ToList();

        if (list.Count > 0)
        {
            var ids = list.Select(x => x.Id).ToList();

            // 优化：租客 + 续签状态并行查询，减少等待时间
            var tenantsTask = conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.ContractTenant.PrimaryByIds"),
                new { Ids = ids });
            var renewalsTask = conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.Contract.RenewalStatusByIds"),
                new { Ids = ids });

            await Task.WhenAll(tenantsTask, renewalsTask);

            var tenants = tenantsTask.Result;
            var renewals = renewalsTask.Result.ToList();

            var tenantLookup = tenants.Cast<IDictionary<string, object>>()
                .GroupBy(d => (Guid)d["ContractId"])
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var item in list)
            {
                if (tenantLookup.TryGetValue(item.Id, out var t))
                    item.Tenants = new List<ContractTenantDto> { new ContractTenantDto
                    {
                        ContractId = item.Id,
                        TenantId = t.TryGetValue("TenantId", out var g) && g is Guid gt ? gt : Guid.Empty,
                        TenantName = t.TryGetValue("TenantName", out var n) ? n as string ?? "" : "",
                        TenantPhone = t.TryGetValue("TenantPhone", out var p) ? p as string ?? "" : ""
                    } };
                var r = renewals.Where(x => (Guid?)x.OldContractId == item.Id).ToList();
                item.HasPendingRenewal = r.Any(x => (string?)x.Status == "PendingApproval");
                item.HasRejectedRenewal = r.Any(x => (string?)x.Status == "Rejected");
            }
        }
        return new PagedResult<ContractDto> { Items = list, Total = (int)total, Page = page, PageSize = pageSize, TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 };
    }

    /// <summary>
    /// 根据 ID 获取合同详情（含租客列表、费用配置）
    /// 使用 QueryMultipleAsync 一次查询多结果集
    /// </summary>
    public async Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Lease.Select.Contract.DetailMulti"),
            new { Id = id });
        var dto = await multi.ReadSingleOrDefaultAsync<ContractDto>();
        if (dto == null) return null;
        dto.Tenants = (await multi.ReadAsync<ContractTenantDto>()).ToList();
        dto.FeeConfigs = (await multi.ReadAsync<ContractFeeConfigDto>()).ToList();
        return dto;
    }

    /// <summary>
    /// 获取合同租客列表（含电话、身份证、邮箱等详细信息）
    /// </summary>
    public async Task<List<ContractTenantInfoDto>> GetTenantsAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var tenants = (await conn.QueryAsync<ContractTenantInfoDto>(
            _sql.Get("Lease.Select.ContractTenant.WithDetailByContract"),
            new { Id = contractId })).ToList();
        return tenants;
    }

    /// <summary>
    /// 创建新合同（仅写入主表，不含租客关联和费用配置）
    /// </summary>
    public async Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken ct = default)
    {
        var contractNo = request.ContractNo ?? "";
        var contract = new ContractEntity(contractNo, request.RoomId, request.CompanyId);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.Contract.Default"), contract);
        return (await GetByIdAsync(contract.Id, ct))!;
    }

    /// <summary>
    /// 激活合同 — 通过领域服务校验房间状态并触发状态机变更
    /// </summary>
    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("合同不存在");

        // 校验房间是否已有生效合同（应用层负责数据查询，领域服务只做状态变更）
        var hasActive = await _uow.Contracts.HasActiveForHousingUnitAsync(contract.RoomId, ct);
        if (hasActive)
            throw new InvalidOperationException("该房屋单元已有生效合同");

        await _contractDomain.ActivateContractAsync(contract, ct);
        await _uow.Contracts.UpdateAsync(contract, ct);
        await _uow.CommitAsync(ct);
    }

    /// <summary>
    /// 终止合同 — 通过领域服务校验状态机并记录终止原因
    /// </summary>
    public async Task TerminateAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("合同不存在");
        await _contractDomain.TerminateContractAsync(contract, reason, ct);
        await _uow.Contracts.UpdateAsync(contract, ct);
        await _uow.CommitAsync(ct);
    }

    /// <summary>
    /// 暂停合同 — 通过领域服务校验状态机并记录暂停时间
    /// </summary>
    public async Task SuspendAsync(Guid id, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("合同不存在");
        await _contractDomain.SuspendContractAsync(contract, ct);
        await _uow.Contracts.UpdateAsync(contract, ct);
        await _uow.CommitAsync(ct);
    }

    /// <summary>
    /// 恢复合同 — 通过领域服务校验状态机并记录恢复时间
    /// </summary>
    public async Task ResumeAsync(Guid id, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("合同不存在");
        await _contractDomain.ResumeContractAsync(contract, ct);
        await _uow.Contracts.UpdateAsync(contract, ct);
        await _uow.CommitAsync(ct);
    }

    public async Task SubmitContractCreateRequestStatusAsync(Guid requestId, CancellationToken ct = default)
    {
        await _uow.ExecuteSqlRawAsync(_sql.Get("ContractCreate.Update.Request.Submit"),
            new { Id = requestId }, ct);
    }

    public async Task SetApprovalRequestContractIdAsync(Guid approvalRequestId, Guid contractId, CancellationToken ct = default)
    {
        await _uow.ExecuteSqlRawAsync(_sql.Get("Approval.Update.Request.SetContractId"),
            new { Id = approvalRequestId, ContractId = contractId }, ct);
    }

    /// <summary>
    /// 检查合同是否有待审批的调价申请，有则抛出 InvalidOperationException
    /// </summary>
    public async Task EnsureNoPendingForContractAsync(Guid contractId, CancellationToken ct = default)
    {
        var pending = await _uow.ExecuteSqlRawAsync(
            _sql.Get("Approval.Select.Request.HasPendingByContract"),
            new { ContractId = contractId, TargetEntityType = "ContractFeeAdjust" }, ct);
        if (pending > 0)
            throw new InvalidOperationException("该合同已有待审批的调价申请");
    }

    /// <summary>
    /// 直接执行合同创建（从审批跳过或直接创建时调用）
    /// </summary>
    public async Task<Guid> ExecuteContractCreationAsync(Guid requestId, Guid userId, CancellationToken ct = default)
    {
        var request = await _uow.ContractCreateRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new InvalidOperationException("请求不存在");

        var allTenants = await _uow.ContractCreateRequestTenants.GetAllAsync(ct);
        var tenants = allTenants.Where(t => t.RequestId == requestId).ToList();
        var allFees = await _uow.ContractCreateRequestFees.GetAllAsync(ct);
        var feeList = allFees.Where(f => f.RequestId == requestId).ToList();
        var now = ChinaTime.Now;
        var contractId = Guid.NewGuid();

        await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Insert.Contract.Default"),
            new
            {
                Id = contractId, ContractNo = request.ContractNo, RoomId = request.RoomId,
                StartDate = request.StartDate, EndDate = request.EndDate,
                PaymentCycle = request.PaymentCycle, Status = "Active", CompanyId = request.CompanyId,
                CreatedBy = userId, CreatedAt = now
            }, ct);
        foreach (var t in tenants)
            await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Insert.ContractTenant.Default"),
                new { ContractId = contractId, t.TenantId, t.IsPrimary,
                    CreatedBy = userId, CreatedAt = now }, ct);
        foreach (var f in feeList)
            await _uow.ExecuteSqlRawAsync(_sql.Get("Lease.Insert.ContractFeeConfig.Default"),
                new
                {
                    Id = Guid.NewGuid(), ContractId = contractId, f.FeeCodeId,
                    BillingMode = f.BillingMode, Amount = f.Amount,
                    EffectiveDate = f.EffectiveDate ?? request.StartDate.ToString("yyyy-MM-dd"),
                    CreatedBy = userId, CreatedAt = now
                }, ct);

        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract != null) { contract.Activate(); await _uow.CommitAsync(ct); }
        try { await _receivableGen.GenerateForActivationAsync(contractId, ct); } catch { }
        return contractId;
    }

    /// <summary>
    /// 执行费用调价（无审批配置时直接执行）
    /// </summary>
    public async Task<object> FeeAdjustAsync(Guid contractId, FeeAdjustRequest request, Guid userId, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract == null) throw new KeyNotFoundException("合同不存在");

        // 校验所有调价项的生效日期在合同起止日期范围内
        foreach (var item in request.Items)
        {
            var effDate = item.EffectiveDate ?? "";
            if (!string.IsNullOrEmpty(effDate))
                ContractEntity.ValidateFeeEffectiveDate(DateOnly.Parse(effDate), contract.StartDate, contract.EndDate, item.FeeName);
        }

        // 找审批类型
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_FEE_CHANGE", ct);

        if (approvalType == null)
        {
            using var innerConn = _db.CreateConnection(); innerConn.Open();
            using var innerTx = innerConn.BeginTransaction();
            try
            {
                foreach (var item in request.Items)
                {
                    var effDate = item.EffectiveDate ?? "";
                    if (string.IsNullOrEmpty(effDate)) continue;

                    var current = await innerConn.QuerySingleOrDefaultAsync(
                        _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
                        new { ContractId = contractId, FeeCodeId = item.FeeCodeId }, innerTx);

                    var overlap = await innerConn.QuerySingleAsync<int>(
                        _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
                        new
                        {
                            ContractId = contractId, FeeCodeId = item.FeeCodeId,
                            EffectiveDate = effDate, ExpiryDate = (string?)null,
                            ExcludeId = current != null ? (Guid)((dynamic)current).Id : (Guid?)null
                        }, innerTx);
                    if (overlap > 0)
                        throw new InvalidOperationException("费用项 " + item.FeeName + " 的生效日期与已有记录冲突");

                    var expiryDate = DateTime.Parse(effDate).AddDays(-1).ToString("yyyy-MM-dd");
                    if (current != null)
                    {
                        await innerConn.ExecuteAsync(_sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                            new { Id = (Guid)((dynamic)current).Id, ExpiryDate = expiryDate }, innerTx);
                        await innerConn.ExecuteAsync(_sql.Get("Contract.Update.ContractFeeConfig.ExpireByCodeId"),
                            new { ExpiryDate = expiryDate, ContractId = contractId, FeeCodeId = item.FeeCodeId }, innerTx);
                    }
                    await innerConn.ExecuteAsync(_sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                        new
                        {
                            Id = Guid.NewGuid(), ContractId = contractId, FeeCodeId = item.FeeCodeId,
                            BillingMode = item.BillingMode ?? "FixedAmount", Amount = item.NewAmount,
                            EffectiveDate = effDate, CreatedBy = userId, Now = ChinaTime.Now
                        }, innerTx);
                }
                innerTx.Commit();
            }
            catch (Exception ex)
            {
                innerTx.Rollback();
                throw new InvalidOperationException("[Tx] " + ex.Message);
            }

            // 以下为 Commit 后的操作（独立 try，失败不影响调价）
            try
            {
                foreach (var item in request.Items)
                {
                    var effDateStr = item.EffectiveDate ?? "";
                    if (!string.IsNullOrEmpty(effDateStr))
                    {
                        using (var hisConn = _db.CreateConnection())
                        {
                            hisConn.Open();
                            var operatorName = await hisConn.QuerySingleOrDefaultAsync<string>(
                                _sql.Get("Contract.Select.User.DisplayNameById"), new { Id = userId });
                            await hisConn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
                                new
                                {
                                    Id = Guid.NewGuid(), ContractId = contractId, ChangeType = "FEE_ADJUST",
                                    Title = "费用调价",
                                    Detail = item.FeeName + ": " + item.OldAmount.ToString("F2") + " -> " + item.NewAmount.ToString("F2") + "，生效 " + effDateStr,
                                    OldValue = item.OldAmount, NewValue = item.NewAmount,
                                    EffectiveDate = effDateStr, OperatorId = userId,
                                    OperatorName = operatorName ?? ""
                                });
                        }
                    }
                }

                using var journalConn = _db.CreateConnection();
                journalConn.Open();
                foreach (var item in request.Items)
                {
                    var effDateStr = item.EffectiveDate ?? "";
                    if (!string.IsNullOrEmpty(effDateStr))
                    {
                        var diffAmt = Math.Round(item.NewAmount - item.OldAmount, 2);
                        if (diffAmt != 0)
                        {
                            var subj = await journalConn.QuerySingleOrDefaultAsync(
                                _sql.Get("Accounting.Select.Subject.ByCode"), new { Code = "1122" });
                            var sId = subj != null ? (Guid)((dynamic)subj).Id : Guid.Empty;
                            var effM = effDateStr[..7];
                            await journalConn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                                new
                                {
                                    Id = Guid.NewGuid(), CoId = contract.CompanyId, CId = contractId,
                                    FId = item.FeeCodeId, FConfigId = (Guid?)null, SubjId = sId,
                                    Period = effM, Amt = Math.Abs(diffAmt),
                                    Due = DateOnly.FromDateTime(ChinaTime.Now).AddDays(30),
                                    EntryType = "Supplementary", BilledAt = ChinaTime.Now,
                                    DNId = (Guid?)null, ParentId = (Guid?)null,
                                    Summary = $"{item.FeeName}调价补差", CBy = userId
                                });
                        }
                    }
                }
            }
            catch { /* TODO: 补差 JE 失败不阻断调价 */ }
        }

        return new { contractId };
    }
}
