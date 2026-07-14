namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 抄表读数实体（领域实体，继承 AuditableEntity）
/// —— 记录水、电、气等按表计量（Metered）模式下的仪表读数数据。
/// 用于计算周期性用量并生成对应费用，是 Metered 计费方式的核心数据来源。
/// 状态流转：Draft（草稿，初始状态） -> Confirmed（已确认，不可修改）。
/// 用量 Usage = CurrentReading - PreviousReading，为只读计算属性。
/// </summary>
public class MeterReading : AuditableEntity
{
    /// <summary>
    /// 关联的合同费用配置 ID（ContractFeeConfig 的主键）。
    /// 标识该抄表记录属于哪个合同的哪个费用科目配置（如水费、电费）。
    /// </summary>
    public Guid ContractFeeConfigId { get; private set; }

    /// <summary>抄表年份，例如 2026 表示 2026 年</summary>
    public int Year { get; private set; }

    /// <summary>抄表月份，取值范围 1~12</summary>
    public int Month { get; private set; }

    /// <summary>上期读数（上次抄表的仪表数值），用于计算本期用量</summary>
    public decimal PreviousReading { get; private set; }

    /// <summary>本期读数（本次抄表的仪表数值），必须 >= PreviousReading</summary>
    public decimal CurrentReading { get; private set; }

    /// <summary>
    /// 本期用量（只读计算属性）。
    /// 计算公式：Usage = CurrentReading - PreviousReading。
    /// 单位与对应 FeeCode 的 Unit 一致（如吨、度、立方米）。
    /// </summary>
    public decimal Usage => CurrentReading - PreviousReading;

    /// <summary>
    /// 抄表记录状态。
    /// "Draft"（草稿，默认）—— 可修改，尚未用于生成账单；
    /// "Confirmed"（已确认）—— 不可修改，已用于生成账单或确认无误。
    /// </summary>
    public string Status { get; private set; } = "Draft";

    /// <summary>私有无参构造函数，供 EF Core 延迟加载使用</summary>
    private MeterReading() { }

    /// <summary>
    /// 创建抄表读数记录。
    /// 新创建的记录默认为 Draft（草稿）状态，确认后方可用于账单生成。
    /// </summary>
    /// <param name="contractFeeConfigId">关联的合同费用配置 ID</param>
    /// <param name="year">抄表年份，例如 2026</param>
    /// <param name="month">抄表月份，取值范围 1~12</param>
    /// <param name="previous">上期读数（上次抄表数值），建议 >= 0</param>
    /// <param name="current">本期读数（本次抄表数值），应 >= previous</param>
    public MeterReading(Guid contractFeeConfigId, int year, int month, decimal previous, decimal current)
    {
        ContractFeeConfigId = contractFeeConfigId;
        Year = year;
        Month = month;
        PreviousReading = previous;
        CurrentReading = current;
    }
}
