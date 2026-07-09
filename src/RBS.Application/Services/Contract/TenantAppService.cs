using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Common;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Contract;

public class TenantAppService : ITenantAppService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public TenantAppService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    public async Task<PagedResult<TenantDto>> GetPagedAsync(Guid companyId, string? keyword, int page, int pageSize, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();

        var parms = new DynamicParameters();
        parms.Add("@CompanyId", companyId);
        var where = "WHERE CompanyId = @CompanyId";

        if (!string.IsNullOrEmpty(keyword))
        {
            where += " AND (Name LIKE @K OR Phone LIKE @K OR IdCard LIKE @K)";
            parms.Add("@K", $"%{keyword}%");
        }

        var total = await conn.QuerySingleAsync<int>(
            $"SELECT COUNT(1) FROM Tenants {where}", parms);

        var offset = (page - 1) * pageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", pageSize);
        var rows = await conn.QueryAsync<TenantDto>(
            $"SELECT Id, Name, IdentityNo AS IdCard, Phone, Email, IsActive, Wechat, EmergencyContact, EmergencyPhone, Address, Remarks AS Remark FROM Tenants {where} ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", parms);

        var list = rows.ToList();
        foreach (var t in list)
        {
            t.ContractCount = await conn.QuerySingleAsync<int>(
                _sql.Get("Rental.Select.Tenant.ContractCount"), new { TenantId = t.Id });
            t.CurrentContractNo = await conn.QuerySingleOrDefaultAsync<string>(
                _sql.Get("Rental.Select.Tenant.ActiveContractNo"), new { TenantId = t.Id });
        }

        return new PagedResult<TenantDto>
        {
            Items = list, Total = total, Page = page, PageSize = pageSize,
            TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var tenant = await conn.QuerySingleOrDefaultAsync<TenantDto>(
            "SELECT Id, Name, IdentityNo AS IdCard, Phone, Email, IsActive, Wechat, EmergencyContact, EmergencyPhone, Address, Remarks AS Remark FROM Tenants WHERE Id=@Id",
            new { Id = id });
        if (tenant == null) return null;

        tenant.ContractCount = await conn.QuerySingleAsync<int>(
            _sql.Get("Rental.Select.Tenant.ContractCount"), new { TenantId = id });
        tenant.CurrentContractNo = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Rental.Select.Tenant.ActiveContractNo"), new { TenantId = id });
        return tenant;
    }

    public async Task<TenantDto> CreateAsync(CreateTenantRequest request, CancellationToken ct)
    {
        var tenant = new Tenant(request.Name, request.CompanyId);
        tenant.SetPhone(request.Phone);
        tenant.SetIdCard(request.IdCard);
        tenant.SetEmail(request.Email);
        tenant.SetWechat(request.Wechat);
        tenant.SetEmergency(request.EmergencyContact, request.EmergencyPhone);
        tenant.SetAddress(request.Address);
        tenant.SetRemark(request.Remark);
        tenant.SetCreated(Guid.Empty, ChinaTime.Now, null, null);

        await _uow.Tenants.AddAsync(tenant, ct);
        await _uow.CommitAsync(ct);

        return await GetByIdAsync(tenant.Id, ct) ?? new TenantDto { Id = tenant.Id, Name = tenant.Name };
    }

    public async Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct)
    {
        var tenant = await _uow.Tenants.GetByIdAsync(id, ct);
        if (tenant == null) throw new KeyNotFoundException("租客不存在");

        if (!string.IsNullOrEmpty(request.Name)) tenant.Rename(request.Name);
        tenant.SetPhone(request.Phone);
        tenant.SetIdCard(request.IdCard);
        tenant.SetEmail(request.Email);
        tenant.SetWechat(request.Wechat);
        tenant.SetEmergency(request.EmergencyContact, request.EmergencyPhone);
        tenant.SetAddress(request.Address);
        tenant.SetRemark(request.Remark);

        await _uow.Tenants.UpdateAsync(tenant, ct);
        await _uow.CommitAsync(ct);

        return await GetByIdAsync(id, ct) ?? new TenantDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var tenant = await _uow.Tenants.GetByIdAsync(id, ct);
        if (tenant == null) throw new KeyNotFoundException("租客不存在");

        using var conn = _db.CreateConnection(); conn.Open();
        var contractCount = await conn.QuerySingleAsync<int>(
            _sql.Get("Rental.Select.Tenant.ContractCount"), new { TenantId = id });

        if (contractCount > 0)
        {
            tenant.Deactivate();
            await _uow.Tenants.UpdateAsync(tenant, ct);
        }
        else
        {
            await _uow.Tenants.DeleteAsync(tenant, ct);
        }
        await _uow.CommitAsync(ct);
    }

    public async Task<bool> IsPhoneUniqueAsync(Guid companyId, string phone, Guid? excludeId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var count = await conn.QuerySingleAsync<int>(
            _sql.Get("Rental.Select.Tenant.CheckPhoneUnique"),
            new { CompanyId = companyId, Phone = phone, ExcludeId = excludeId });
        return count == 0;
    }

    public async Task<bool> IsIdCardUniqueAsync(Guid companyId, string idCard, Guid? excludeId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var count = await conn.QuerySingleAsync<int>(
            _sql.Get("Rental.Select.Tenant.CheckIdCardUnique"),
            new { CompanyId = companyId, IdCard = idCard, ExcludeId = excludeId });
        return count == 0;
    }
}
