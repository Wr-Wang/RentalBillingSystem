using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;
using System.Data;
using ContractEntity = RBS.Core.Entities.Contract.Contract;

namespace RBS.Application.Services.Contract;

public class ContractAppService : IContractService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IUnitOfWork _uow;
    public ContractAppService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql) { _uow = uow; _db = db; _sql = sql; }

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
            var tenants = await conn.QueryAsync<dynamic>(@"SELECT ct.ContractId, ct.TenantId, t.Name AS TenantName, t.Phone AS TenantPhone FROM ContractTenants ct INNER JOIN Tenants t ON t.Id = ct.TenantId WHERE ct.ContractId IN @Ids AND ct.IsPrimary = 1", new { Ids = ids });
            var lookup = tenants.Cast<IDictionary<string, object>>().GroupBy(d => (Guid)d["ContractId"]).ToDictionary(g => g.Key, g => g.First());
            foreach (var item in list)
                if (lookup.TryGetValue(item.Id, out var t))
                    item.Tenants = new List<ContractTenantDto> { new ContractTenantDto { ContractId = item.Id, TenantId = t.ContainsKey("TenantId") && t["TenantId"] is Guid gt ? gt : Guid.Empty, TenantName = t.ContainsKey("TenantName") ? t["TenantName"] as string ?? "" : "", TenantPhone = t.ContainsKey("TenantPhone") ? t["TenantPhone"] as string ?? "" : "" } };
        }
        return list;
    }

    public async Task<PagedResult<ContractDto>> GetPagedListAsync(Guid companyId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null, CancellationToken ct = default)
    {
        // 动态 WHERE 无法预置到 SqlMaps.xml，此处保留内联
        using var conn = _db.CreateConnection(); conn.Open();
        var where = new List<string> { "c.CompanyId = @CompanyId" };
        var parms = new DynamicParameters();
        parms.Add("@CompanyId", companyId);
        if (!string.IsNullOrEmpty(keyword)) { where.Add("(c.ContractNo LIKE @Keyword OR r.FullCode LIKE @Keyword)"); parms.Add("@Keyword", $"%{keyword}%"); }
        if (!string.IsNullOrEmpty(status)) { where.Add("c.Status = @Status"); parms.Add("@Status", status); }
        var w = "WHERE " + string.Join(" AND ", where);
        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset); parms.Add("@PageSize", pageSize);

        var total = await conn.QuerySingleAsync<int>($"SELECT COUNT(*) FROM Contracts c LEFT JOIN HousingUnits r ON r.Id = c.RoomId {w}", parms);
        var rows = await conn.QueryAsync<ContractDto>($@"SELECT c.Id, c.ContractNo, c.RoomId, r.FullCode AS RoomFullCode, c.RentAmount, c.DepositAmount, c.StartDate, c.EndDate, c.PaymentCycle, c.Status, c.CompanyId, CASE WHEN EXISTS (SELECT 1 FROM Contracts r WHERE r.PreviousContractId = c.Id) THEN 1 ELSE 0 END AS HasRenewalContract, c.AutoRenew FROM Contracts c LEFT JOIN HousingUnits r ON r.Id = c.RoomId {w} ORDER BY c.CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", parms);
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

            // 续签状态：是否有 Pending/Rejected 的续签请求
            var renewals = (await conn.QueryAsync<dynamic>(
                _sql.Get("Lease.Select.Contract.RenewalStatusByIds"),
                new { Ids = ids })).ToList();

            foreach (var item in list)
            {
                if (tenantLookup.TryGetValue(item.Id, out var t))
                    item.Tenants = new List<ContractTenantDto> { new ContractTenantDto { ContractId = item.Id, TenantId = t.ContainsKey("TenantId") && t["TenantId"] is Guid gt ? gt : Guid.Empty, TenantName = t.ContainsKey("TenantName") ? t["TenantName"] as string ?? "" : "", TenantPhone = t.ContainsKey("TenantPhone") ? t["TenantPhone"] as string ?? "" : "" } };
                var r = renewals.Where(x => x.OldContractId == item.Id).ToList();
                item.HasPendingRenewal = r.Any(x => x.Status == "PendingApproval");
                item.HasRejectedRenewal = r.Any(x => x.Status == "Rejected");
            }
        }
        return new PagedResult<ContractDto> { Items = list, Total = (int)total, Page = page, PageSize = pageSize, TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 };
    }

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

    public async Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken ct = default)
    {
        var contractNo = request.ContractNo ?? "";
        var contract = new ContractEntity(contractNo, request.RoomId, request.CompanyId);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Lease.Insert.Contract.Default"), contract);
        return (await GetByIdAsync(contract.Id, ct))!;
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Lease.Update.Contract.Status"), new { Id = id, Status = "Active" }); }

    public async Task TerminateAsync(Guid id, string reason, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Lease.Update.Contract.Terminate"), new { Id = id, Reason = reason }); }

    public async Task SuspendAsync(Guid id, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Lease.Update.Contract.Status"), new { Id = id, Status = "Suspended" }); }

    public async Task ResumeAsync(Guid id, CancellationToken ct = default)
        { using var conn = _db.CreateConnection(); conn.Open(); await conn.ExecuteAsync(_sql.Get("Lease.Update.Contract.Status"), new { Id = id, Status = "Active" }); }

    public async Task AdjustRentAsync(Guid id, decimal newAmount, CancellationToken ct = default)
    {
        if (newAmount < 0) throw new ArgumentException("租金不能为负数");
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(_sql.Get("Lease.Update.Contract.RentAmount"), new { Id = id, NewAmount = newAmount });
    }
}
