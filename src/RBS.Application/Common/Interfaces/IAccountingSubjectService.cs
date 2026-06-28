using RBS.Application.DTOs.Accounting;

namespace RBS.Application.Common.Interfaces;

public interface IAccountingSubjectService
{
    Task<List<AccountingSubjectDto>> GetTreeAsync(CancellationToken ct = default);
    Task<AccountingSubjectDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AccountingSubjectDto> CreateAsync(CreateAccountingSubjectRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateAccountingSubjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
