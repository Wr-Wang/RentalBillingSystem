namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Billing;

/// <summary>
/// 抄表记录仓储接口。
/// 定义抄表记录聚合根的特有查询方法，继承泛型 CRUD 操作。
/// 提供最新读数的查询、历史记录的查询以及检查指定月份是否已有读数等业务方法。
/// 用于水电费等按用量计费的费用项。
/// </summary>
public interface IMeterReadingRepository : IRepository<MeterReading>
{
    /// <summary>
    /// 获取指定费用配置下的最新抄表读数。
    /// 用于计算本期用量（最新读数 - 上期读数）。
    /// </summary>
    /// <param name="contractFeeConfigId">合同费用配置 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>最新抄表记录，不存在时返回 null</returns>
    Task<MeterReading?> GetLatestReadingAsync(Guid contractFeeConfigId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定费用配置在指定年月的抄表历史记录。
    /// </summary>
    /// <param name="contractFeeConfigId">合同费用配置 ID</param>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>抄表记录列表</returns>
    Task<List<MeterReading>> GetHistoryAsync(Guid contractFeeConfigId, int year, int month, CancellationToken ct = default);

    /// <summary>
    /// 检查指定费用配置在指定月份是否已有抄表读数。
    /// 用于防止同一月份重复抄表。
    /// </summary>
    /// <param name="contractFeeConfigId">合同费用配置 ID</param>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>已存在读数时返回 true，否则 false</returns>
    Task<bool> ReadingExistsAsync(Guid contractFeeConfigId, int year, int month, CancellationToken ct = default);

    /// <summary>
    /// 确认抄表记录（仅变更状态为 Confirmed）。
    /// </summary>
    /// <param name="id">抄表记录 ID</param>
    /// <param name="ct">取消令牌</param>
    Task ConfirmAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 确认抄表记录并写入当前读数。
    /// </summary>
    /// <param name="id">抄表记录 ID</param>
    /// <param name="currentReading">当前读数</param>
    /// <param name="ct">取消令牌</param>
    Task ConfirmWithReadingAsync(Guid id, decimal currentReading, CancellationToken ct = default);

    /// <summary>
    /// 按公司、年月查询抄表记录（含 FeeCodeId）。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>抄表记录列表</returns>
    Task<IEnumerable<dynamic>> GetByCompanyMonthAsync(Guid companyId, int year, int month, CancellationToken ct = default);
}
