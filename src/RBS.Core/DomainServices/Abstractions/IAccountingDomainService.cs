namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Billing;

/// <summary>
/// 会计领域服务接口 — 封装凭证生成、科目映射、借贷平衡等核心会计规则。
/// 将从收款确认、一次性费用、补充费用等业务操作中的会计规则抽象到领域层，
/// 确保科目编码映射、借贷方向、应收余额拆分等规则集中在单一职责的服务中。
/// </summary>
public interface IAccountingDomainService
{
    /// <summary>
    /// 从已确认的收款生成记账凭证（抵应收 / 转预收的分账逻辑）。
    /// 核心规则：
    /// 1. 拆分行：offset = Min(收款金额, 应收余额) 冲应收账款；
    ///    overflow = 收款金额 - offset 转入预收账款。
    /// 2. 借贷：借 1001（库存现金/银行存款）全额；
    ///    贷 1122（应收账款）冲抵部分；
    ///    贷 2203（预收账款）溢出部分。
    /// </summary>
    /// <param name="receipt">已确认的收款聚合根</param>
    /// <param name="receivableBalance">当前合同应收账款余额（按合同维度查询）</param>
    /// <param name="subjectMap">会计科目映射字典 (code → subjectId)，至少包含 "1001"、"1122" 键</param>
    /// <returns>已过账的凭证（含分录）</returns>
    /// <exception cref="ArgumentNullException">receipt 或 subjectMap 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">subjectMap 缺少必需科目 "1001" 或 "1122" 时抛出</exception>
    Voucher GenerateVoucherFromReceipt(Receipt receipt, decimal receivableBalance, IReadOnlyDictionary<string, Guid> subjectMap);

    /// <summary>
    /// 生成一次性费用凭证（如押金、一次性服务费等）。
    /// 核心规则：
    /// - 押金（DEPOSIT）：借 112202（应收押金） / 贷 2241（其他应付款-押金）
    /// - 其他一次性费用：借 1122（应收账款） / 贷 6001（主营业务收入），
    ///   当 6001 不存在时回退到 6051（其他业务收入）
    /// </summary>
    /// <param name="voucherNo">凭证编号（由调用方按规则生成，如 "OT-20260714-xxxx"）</param>
    /// <param name="voucherDate">凭证日期</param>
    /// <param name="contractId">关联合同标识</param>
    /// <param name="period">会计期间，格式 "yyyy-MM"</param>
    /// <param name="feeCodeId">费用代码标识（用于判别是否为押金）</param>
    /// <param name="amount">费用金额（必须大于零）</param>
    /// <param name="subjectMap">会计科目映射字典 (code → subjectId)，需包含对应业务场景的科目编码</param>
    /// <returns>已过账的凭证（含分录）</returns>
    /// <exception cref="ArgumentException">amount 小于等于 0 或 subjectMap 缺少必需科目时抛出</exception>
    Voucher GenerateOneTimeVoucher(string voucherNo, DateOnly voucherDate, Guid contractId, string period, Guid feeCodeId, decimal amount, IReadOnlyDictionary<string, Guid> subjectMap);

    /// <summary>
    /// 生成补充费用凭证（费用调价后的补差凭证）。
    /// 核心规则：
    /// - 正数补差（应收增加）：借 1122（应收账款） / 贷 6001（主营业务收入）
    /// - 负数补差（应收冲减）：借方向与贷方互换（贷 1122 / 借 6001）
    /// </summary>
    /// <param name="voucherNo">凭证编号（由调用方按规则生成，如 "SUP-20260714-xxxx"）</param>
    /// <param name="voucherDate">凭证日期</param>
    /// <param name="contractId">关联合同标识</param>
    /// <param name="period">会计期间，格式 "yyyy-MM"</param>
    /// <param name="amount">补充金额（正数为应收增加，负数为冲减）</param>
    /// <param name="subjectMap">会计科目映射字典 (code → subjectId)，需包含 "1122" 和 "6001"（或 "6051"）</param>
    /// <returns>已过账的凭证（含分录）</returns>
    /// <exception cref="ArgumentException">amount 为 0 或 subjectMap 缺少必需科目时抛出</exception>
    Voucher GenerateSupplementaryVoucher(string voucherNo, DateOnly voucherDate, Guid contractId, string period, decimal amount, IReadOnlyDictionary<string, Guid> subjectMap);
}
