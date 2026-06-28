using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Accounting;
using RBS.Core.Entities.Accounting;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Accounting;

public class AccountingSubjectService : IAccountingSubjectService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public AccountingSubjectService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }
    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<AccountingSubjectDto>> GetTreeAsync(CancellationToken ct = default)
    {
        var all = await _uow.AccountingSubjects.GetAllAsync(ct);
        var dtos = all.Select(MapToDto).ToList();
        return BuildTree(dtos, null);
    }

    public async Task<AccountingSubjectDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.AccountingSubjects.GetByIdAsync(id, ct);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<AccountingSubjectDto> CreateAsync(CreateAccountingSubjectRequest request, CancellationToken ct = default)
    {
        var entity = new AccountingSubject(request.Code, request.Name, CompanyId);
        entity.SetParentCode(request.ParentCode);
        entity.SetDirection(request.Direction);
        await _uow.AccountingSubjects.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateAccountingSubjectRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.AccountingSubjects.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("会计科目不存在");
        if (request.Name != null) entity.Rename(request.Name);
        if (request.Direction != null) entity.SetDirection(request.Direction);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) entity.Activate(); else entity.Deactivate(); }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.AccountingSubjects.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("会计科目不存在");
        await _uow.AccountingSubjects.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static AccountingSubjectDto MapToDto(AccountingSubject s) => new()
    { Id = s.Id, Code = s.Code, Name = s.Name, ParentCode = s.ParentCode, Level = s.Level, Direction = s.Direction, IsLeaf = s.IsLeaf, IsActive = s.IsActive };

    private static List<AccountingSubjectDto> BuildTree(List<AccountingSubjectDto> flat, string? parentCode)
    {
        return flat.Where(s => s.ParentCode == parentCode).OrderBy(s => s.Code).Select(s =>
        {
            s.Children = BuildTree(flat, s.Code);
            return s;
        }).ToList();
    }
}
