using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.SystemConfig;

/// <summary>
/// 自动续签策略配置 — 定义合同到期时自动续签的规则（AuditableEntity）
/// 包含租金调整方式、续签期限规则及逾期处理策略。
/// 每家公司可维护独立的自动续签策略
/// </summary>
public class AutoRenewConfig : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 所属公司标识，每家公司可设置独立的自动续签策略
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 提前提醒天数，合同到期前多少天开始执行自动续签检查。
    /// 默认值为 7 天
    /// </summary>
    public int AdvanceDays { get; private set; } = 7;

    /// <summary>
    /// 租金规则：Same=维持原价, Percentage=按比例上浮, MarketPrice=跟随市场价
    /// 默认值为 "Same"
    /// </summary>
    public string RentRule { get; private set; } = "Same";

    /// <summary>
    /// 租金上浮比例（可选），当 RentRule=Percentage 时生效。
    /// 如 0.1 表示上浮 10%
    /// </summary>
    public decimal? RentIncreasePercent { get; private set; }

    /// <summary>
    /// 续签期限规则：Same=与原合同一致, FixedMonths=固定月数
    /// 默认值为 "Same"
    /// </summary>
    public string TermRule { get; private set; } = "Same";

    /// <summary>
    /// 固定续签月数（可选），当 TermRule=FixedMonths 时生效
    /// </summary>
    public int? TermMonths { get; private set; }

    /// <summary>
    /// 逾期处理策略：Block=阻止续签, WarnAndContinue=警告后继续, Skip=跳过自动续签
    /// 默认值为 "Block"
    /// </summary>
    public string OverdueAction { get; private set; } = "Block";

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private AutoRenewConfig() { }

    /// <summary>
    /// 创建自动续签策略配置实例，使用系统默认值初始化
    /// </summary>
    /// <param name="companyId">所属公司标识</param>
    public AutoRenewConfig(Guid companyId)
    {
        CompanyId = companyId;
    }

    /// <summary>
    /// 更新自动续签策略的全部参数
    /// </summary>
    /// <param name="rentRule">租金规则（Same/Percentage/MarketPrice）</param>
    /// <param name="rentIncreasePercent">租金上浮比例（可选）</param>
    /// <param name="termRule">续签期限规则（Same/FixedMonths）</param>
    /// <param name="termMonths">固定续签月数（可选）</param>
    /// <param name="advanceDays">提前提醒天数</param>
    /// <param name="overdueAction">逾期处理策略（Block/WarnAndContinue/Skip）</param>
    public void Update(string rentRule, decimal? rentIncreasePercent, string termRule, int? termMonths,
        int advanceDays, string overdueAction)
    {
        RentRule = rentRule;
        RentIncreasePercent = rentIncreasePercent;
        TermRule = termRule;
        TermMonths = termMonths;
        AdvanceDays = advanceDays;
        OverdueAction = overdueAction;
    }
}
