using System.Linq.Expressions;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 公司管理应用服务
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _uow;

    public CompanyService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<CompanyDto>> GetPagedAsync(CompanyQuery query, CancellationToken ct = default)
    {
        // 构建查询条件
        var predicate = BuildPredicate(query);

        var paged = await _uow.Companies.GetPagedAsync(
            query.Page,
            query.PageSize,
            predicate,
            q => q.OrderByDescending(l => l.CreatedAt),
            ct);

        return new PagedResult<CompanyDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages
        };
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var company = await _uow.Companies.GetByIdAsync(id, ct);
        return company == null ? null : MapToDto(company);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default)
    {
        var company = new Company(request.Name);

        if (!string.IsNullOrEmpty(request.Code))
            company.SetCode(request.Code);
        if (!string.IsNullOrEmpty(request.ContactPerson))
            company.SetContactPerson(request.ContactPerson);
        if (!string.IsNullOrEmpty(request.ContactPhone))
            company.SetPhone(request.ContactPhone);
        if (!string.IsNullOrEmpty(request.Address))
            company.SetAddress(request.Address);
        if (!string.IsNullOrEmpty(request.IdType) || !string.IsNullOrEmpty(request.IdNumber))
            company.SetIdInfo(request.IdType, request.IdNumber);
        if (!string.IsNullOrEmpty(request.BankName) || !string.IsNullOrEmpty(request.BankAccount) || !string.IsNullOrEmpty(request.BankAccountName))
            company.SetBankInfo(request.BankName, request.BankAccount, request.BankAccountName);
        if (!string.IsNullOrEmpty(request.SettlementCycle) || request.SettlementDay.HasValue || request.CommissionRate.HasValue)
            company.SetSettlement(request.SettlementCycle, request.SettlementDay, request.CommissionRate);
        if (!string.IsNullOrEmpty(request.Remark))
            company.SetRemark(request.Remark);

        await _uow.Companies.AddAsync(company, ct);
        await _uow.CommitAsync(ct);

        return await GetByIdAsync(company.Id, ct)
            ?? throw new InvalidOperationException("创建公司失败");
    }

    public async Task UpdateAsync(Guid id, CreateCompanyRequest request, CancellationToken ct = default)
    {
        var company = await _uow.Companies.GetByIdAsync(id, ct)
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

        // 显式传入 IsActive 时更新状态（用于启用/停用切换）
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                company.Activate();
            else
                company.Deactivate();
        }

        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var company = await _uow.Companies.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("公司不存在");

        company.Deactivate();
        await _uow.CommitAsync(ct);
    }

    public async Task<CompanyStatsDto?> GetStatsAsync(Guid id, CancellationToken ct = default)
    {
        var company = await _uow.Companies.GetByIdAsync(id, ct);
        if (company == null) return null;

        // 查询该公司名下的楼宇和房间统计
        var buildings = await _uow.Buildings.GetByCompanyIdAsync(id, ct);
        var totalRooms = buildings.Sum(b => b.Floors.Sum(f => f.Rooms.Count));
        var rentedRooms = buildings.Sum(b => b.Floors.Sum(f => f.Rooms.Count(r => r.IsRented)));

        return new CompanyStatsDto
        {
            BuildingCount = buildings.Count,
            RoomCount = totalRooms,
            OccupancyRate = totalRooms > 0 ? Math.Round((decimal)rentedRooms / totalRooms * 100, 2) : 0,
            CollectionRate = 0 // 收租率需要账单数据，暂时置 0
        };
    }

    private static CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Code = company.Code,
            ContactPerson = company.ContactPerson,
            ContactPhone = company.Phone,
            Address = company.Address,
            IdType = company.IdType,
            IdNumber = company.IdNumber,
            BankName = company.BankName,
            BankAccount = company.BankAccount,
            BankAccountName = company.BankAccountName,
            SettlementCycle = company.SettlementCycle,
            SettlementDay = company.SettlementDay,
            CommissionRate = company.CommissionRate,
            Remark = company.Remark,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt
        };
    }

    private static Expression<Func<Company, bool>>? BuildPredicate(CompanyQuery query)
    {
        // 无筛选条件时返回 null（查询全部）
        var hasNameFilter = !string.IsNullOrWhiteSpace(query.Name);
        var hasActiveFilter = query.IsActive.HasValue;
        if (!hasNameFilter && !hasActiveFilter)
            return null;

        Expression<Func<Company, bool>>? predicate = null;

        if (hasNameFilter)
        {
            var name = query.Name!;
            predicate = l => l.Name.Contains(name) || (l.Code != null && l.Code.Contains(name));
        }

        if (hasActiveFilter)
        {
            var isActive = query.IsActive!.Value;
            Expression<Func<Company, bool>> activePredicate = l => l.IsActive == isActive;
            predicate = predicate == null ? activePredicate : Combine(predicate, activePredicate);
        }

        return predicate;
    }

    private static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(left, param),
            Expression.Invoke(right, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
