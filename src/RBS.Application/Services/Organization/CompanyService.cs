using Dapper;
using RBS.Application.Common;
using RBS.Core.Entities.Property;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

public class CompanyService : ICompanyService
{
    private readonly IDbConnectionFactory _db;
    private readonly IUnitOfWork _uow;

    public CompanyService(IDbConnectionFactory db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public async Task<PagedResult<CompanyDto>> GetPagedAsync(CompanyQuery query, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        var where = new List<string>();
        var parms = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            where.Add("(Name LIKE @Name OR Code LIKE @Name)");
            parms.Add("@Name", $"%{query.Name}%");
        }
        if (query.IsActive.HasValue)
        {
            where.Add("IsActive = @IsActive");
            parms.Add("@IsActive", query.IsActive.Value);
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var offset = (query.Page - 1) * query.PageSize;
        parms.Add("@Offset", offset);
        parms.Add("@PageSize", query.PageSize);

        var total = await conn.QuerySingleAsync<int>(
            $"SELECT COUNT(*) FROM Companies {whereClause}", parms);

        var items = await conn.QueryAsync<Company>(
            $"SELECT * FROM Companies {whereClause} ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            parms);

        return new PagedResult<CompanyDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)query.PageSize) : 0
        };
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var company = await _db.GetByIdAsync<Company>("Companies", id);
        return company == null ? null : MapToDto(company);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default)
    {
        var company = new Company(request.Name);
        ApplyCompanyFields(company, request);
        await _db.InsertAsync("Companies", company);
        return MapToDto(company);
    }

    public async Task UpdateAsync(Guid id, CreateCompanyRequest request, CancellationToken ct = default)
    {
        var company = await _db.GetByIdAsync<Company>("Companies", id)
            ?? throw new KeyNotFoundException("公司不存在");

        company.Rename(request.Name);
        company.SetCode(request.Code);
        company.SetContactPerson(request.ContactPerson);
        company.SetPhone(request.ContactPhone);
        company.SetAddress(request.Address);
        company.SetIdInfo(request.IdType, request.IdNumber);
        company.SetBankInfo(request.BankName, request.BankAccount, request.BankAccountName);
        company.SetSettlement(request.SettlementCycle, request.SettlementDay, request.CommissionRate);
        company.SetRemark(request.Remark);
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) company.Activate();
            else company.Deactivate();
        }
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(@"
            UPDATE Companies SET Name=@Name,Code=@Code,ContactPerson=@ContactPerson,Phone=@Phone,
                Address=@Address,IdType=@IdType,IdNumber=@IdNumber,
                BankName=@BankName,BankAccount=@BankAccount,BankAccountName=@BankAccountName,
                SettlementCycle=@SettlementCycle,SettlementDay=@SettlementDay,
                CommissionRate=@CommissionRate,Remark=@Remark,IsActive=@IsActive,
                UpdatedBy=@UpdatedBy,UpdatedAt=@UpdatedAt
            WHERE Id=@Id", company);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync("UPDATE Companies SET IsActive=0 WHERE Id=@Id", new { Id = id });
    }

    public async Task<CompanyStatsDto?> GetStatsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var company = await conn.QuerySingleOrDefaultAsync<Company>("SELECT * FROM Companies WHERE Id=@Id", new { Id = id });
        if (company == null) return null;

        var units = (await conn.QueryAsync<HousingUnit>("SELECT * FROM HousingUnits WHERE CompanyId=@Id", new { Id = id })).ToList();
        var totalRooms = units.Count;
        var rentedRooms = units.Count(u => u.IsRented);

        return new CompanyStatsDto
        {
            BuildingCount = units.Select(u => u.BuildingName).Distinct().Count(),
            RoomCount = totalRooms,
            OccupancyRate = totalRooms > 0 ? Math.Round((decimal)rentedRooms / totalRooms * 100, 2) : 0,
            CollectionRate = 0
        };
    }

    private static void ApplyCompanyFields(Company company, CreateCompanyRequest request)
    {
        if (!string.IsNullOrEmpty(request.Code)) company.SetCode(request.Code);
        if (!string.IsNullOrEmpty(request.ContactPerson)) company.SetContactPerson(request.ContactPerson);
        if (!string.IsNullOrEmpty(request.ContactPhone)) company.SetPhone(request.ContactPhone);
        if (!string.IsNullOrEmpty(request.Address)) company.SetAddress(request.Address);
        if (!string.IsNullOrEmpty(request.IdType) || !string.IsNullOrEmpty(request.IdNumber))
            company.SetIdInfo(request.IdType, request.IdNumber);
        if (!string.IsNullOrEmpty(request.BankName) || !string.IsNullOrEmpty(request.BankAccount) || !string.IsNullOrEmpty(request.BankAccountName))
            company.SetBankInfo(request.BankName, request.BankAccount, request.BankAccountName);
        if (!string.IsNullOrEmpty(request.SettlementCycle) || request.SettlementDay.HasValue || request.CommissionRate.HasValue)
            company.SetSettlement(request.SettlementCycle, request.SettlementDay, request.CommissionRate);
        if (!string.IsNullOrEmpty(request.Remark))
            company.SetRemark(request.Remark);
    }

    private static CompanyDto MapToDto(Company company) => new()
    {
        Id = company.Id, Name = company.Name, Code = company.Code,
        ContactPerson = company.ContactPerson, ContactPhone = company.Phone,
        Address = company.Address, IdType = company.IdType, IdNumber = company.IdNumber,
        BankName = company.BankName, BankAccount = company.BankAccount,
        BankAccountName = company.BankAccountName, SettlementCycle = company.SettlementCycle,
        SettlementDay = company.SettlementDay, CommissionRate = company.CommissionRate,
        Remark = company.Remark, IsActive = company.IsActive, CreatedAt = company.CreatedAt
    };
}
