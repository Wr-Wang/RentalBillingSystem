using RBS.Core.Entities.SystemConfig;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 自动续签策略配置服务接口
/// </summary>
public interface IAutoRenewConfigService
{
    Task<AutoRenewConfig?> GetByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<AutoRenewConfig> SaveAsync(AutoRenewConfig config, CancellationToken ct = default);
}
