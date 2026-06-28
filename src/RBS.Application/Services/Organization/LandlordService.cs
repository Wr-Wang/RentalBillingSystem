using System.Linq.Expressions;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Organization;
using RBS.Core.Entities.Organization;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Organization;

/// <summary>
/// 房东管理应用服务
/// </summary>
public class LandlordService : ILandlordService
{
    private readonly IUnitOfWork _uow;

    public LandlordService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<LandlordDto>> GetPagedAsync(LandlordQuery query, CancellationToken ct = default)
    {
        // 构建查询条件
        var predicate = BuildPredicate(query);

        var paged = await _uow.Landlords.GetPagedAsync(
            query.Page,
            query.PageSize,
            predicate,
            q => q.OrderByDescending(l => l.CreatedAt),
            ct);

        return new PagedResult<LandlordDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages
        };
    }

    public async Task<LandlordDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var landlord = await _uow.Landlords.GetByIdAsync(id, ct);
        return landlord == null ? null : MapToDto(landlord);
    }

    public async Task<LandlordDto> CreateAsync(CreateLandlordRequest request, CancellationToken ct = default)
    {
        var landlord = new Landlord(request.Name);

        if (!string.IsNullOrEmpty(request.Code))
            landlord.SetCode(request.Code);
        if (!string.IsNullOrEmpty(request.ContactPerson))
            landlord.SetContactPerson(request.ContactPerson);
        if (!string.IsNullOrEmpty(request.ContactPhone))
            landlord.SetPhone(request.ContactPhone);
        if (!string.IsNullOrEmpty(request.Address))
            landlord.SetAddress(request.Address);
        if (!string.IsNullOrEmpty(request.IdType) || !string.IsNullOrEmpty(request.IdNumber))
            landlord.SetIdInfo(request.IdType, request.IdNumber);
        if (!string.IsNullOrEmpty(request.BankName) || !string.IsNullOrEmpty(request.BankAccount) || !string.IsNullOrEmpty(request.BankAccountName))
            landlord.SetBankInfo(request.BankName, request.BankAccount, request.BankAccountName);
        if (!string.IsNullOrEmpty(request.SettlementCycle) || request.SettlementDay.HasValue || request.CommissionRate.HasValue)
            landlord.SetSettlement(request.SettlementCycle, request.SettlementDay, request.CommissionRate);
        if (!string.IsNullOrEmpty(request.Remark))
            landlord.SetRemark(request.Remark);

        await _uow.Landlords.AddAsync(landlord, ct);
        await _uow.CommitAsync(ct);

        return await GetByIdAsync(landlord.Id, ct)
            ?? throw new InvalidOperationException("创建房东失败");
    }

    public async Task UpdateAsync(Guid id, CreateLandlordRequest request, CancellationToken ct = default)
    {
        var landlord = await _uow.Landlords.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("房东不存在");

        landlord.Rename(request.Name);
        landlord.SetCode(request.Code);
        landlord.SetContactPerson(request.ContactPerson);
        landlord.SetPhone(request.ContactPhone);
        landlord.SetAddress(request.Address);
        landlord.SetIdInfo(request.IdType, request.IdNumber);
        landlord.SetBankInfo(request.BankName, request.BankAccount, request.BankAccountName);
        landlord.SetSettlement(request.SettlementCycle, request.SettlementDay, request.CommissionRate);
        landlord.SetRemark(request.Remark);

        // 显式传入 IsActive 时更新状态（用于启用/停用切换）
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                landlord.Activate();
            else
                landlord.Deactivate();
        }

        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var landlord = await _uow.Landlords.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("房东不存在");

        landlord.Deactivate();
        await _uow.CommitAsync(ct);
    }

    public async Task<LandlordStatsDto?> GetStatsAsync(Guid id, CancellationToken ct = default)
    {
        var landlord = await _uow.Landlords.GetByIdAsync(id, ct);
        if (landlord == null) return null;

        // 查询该房东名下的楼宇和房间统计
        var buildings = await _uow.Buildings.GetByLandlordIdAsync(id, ct);
        var totalRooms = buildings.Sum(b => b.Floors.Sum(f => f.Rooms.Count));
        var rentedRooms = buildings.Sum(b => b.Floors.Sum(f => f.Rooms.Count(r => r.IsRented)));

        return new LandlordStatsDto
        {
            BuildingCount = buildings.Count,
            RoomCount = totalRooms,
            OccupancyRate = totalRooms > 0 ? Math.Round((decimal)rentedRooms / totalRooms * 100, 2) : 0,
            CollectionRate = 0 // 收租率需要账单数据，暂时置 0
        };
    }

    private static LandlordDto MapToDto(Landlord landlord)
    {
        return new LandlordDto
        {
            Id = landlord.Id,
            Name = landlord.Name,
            Code = landlord.Code,
            ContactPerson = landlord.ContactPerson,
            ContactPhone = landlord.Phone,
            Address = landlord.Address,
            IdType = landlord.IdType,
            IdNumber = landlord.IdNumber,
            BankName = landlord.BankName,
            BankAccount = landlord.BankAccount,
            BankAccountName = landlord.BankAccountName,
            SettlementCycle = landlord.SettlementCycle,
            SettlementDay = landlord.SettlementDay,
            CommissionRate = landlord.CommissionRate,
            Remark = landlord.Remark,
            IsActive = landlord.IsActive,
            CreatedAt = landlord.CreatedAt
        };
    }

    private static Expression<Func<Landlord, bool>>? BuildPredicate(LandlordQuery query)
    {
        // 无筛选条件时返回 null（查询全部）
        var hasNameFilter = !string.IsNullOrWhiteSpace(query.Name);
        var hasActiveFilter = query.IsActive.HasValue;
        if (!hasNameFilter && !hasActiveFilter)
            return null;

        Expression<Func<Landlord, bool>>? predicate = null;

        if (hasNameFilter)
        {
            var name = query.Name!;
            predicate = l => l.Name.Contains(name) || (l.Code != null && l.Code.Contains(name));
        }

        if (hasActiveFilter)
        {
            var isActive = query.IsActive!.Value;
            Expression<Func<Landlord, bool>> activePredicate = l => l.IsActive == isActive;
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
