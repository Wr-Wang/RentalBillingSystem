namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Billing;

/// <summary>
/// 会计领域服务 — 凭证生成、科目映射、应收余额拆分等核心会计规则的实现。
/// 本服务为纯领域逻辑，不依赖任何基础设施（数据库、外部服务），
/// 所有输入均通过参数传入，返回值均为领域对象。
/// </summary>
public class AccountingDomainService : IAccountingDomainService
{
    /// <summary>
    /// 从已确认的收款生成记账凭证（抵应收 / 转预收分账逻辑）。
    /// </summary>
    /// <inheritdoc/>
    public Voucher GenerateVoucherFromReceipt(Receipt receipt, decimal receivableBalance, IReadOnlyDictionary<string, Guid> subjectMap)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(subjectMap);

        // 1. 验证必需科目
        if (!subjectMap.TryGetValue("1001", out var subject1001))
            throw new ArgumentException("科目映射缺少必需科目「1001-库存现金」", nameof(subjectMap));
        if (!subjectMap.TryGetValue("1122", out var subject1122))
            throw new ArgumentException("科目映射缺少必需科目「1122-应收账款」", nameof(subjectMap));

        subjectMap.TryGetValue("2203", out var subject2203);

        // 2. AR 余额拆分逻辑
        var safeArBalance = Math.Max(0, receivableBalance);
        var offset = Math.Min(receipt.Amount, safeArBalance);   // 冲应收部分
        var overflow = receipt.Amount - offset;                  // 溢出进预收部分

        // 3. 生成凭证编号及日期
        var now = DateTime.UtcNow;
        var voucherNo = $"PZ-{now:yyyyMMdd}-{receipt.Id:N}"[..Math.Min(32, 52)];
        var voucherDate = DateOnly.FromDateTime(now);
        var period = voucherDate.ToString("yyyy-MM");

        // 4. 创建凭证
        var voucher = new Voucher(voucherNo, voucherDate, $"收款确认：{receipt.ReceiptNo}");
        voucher.SetSource(receipt.Id, "Receipt");
        voucher.SetContract(receipt.ContractId);
        voucher.SetPeriod(period);

        // 5. 借 1001 库存现金 / 银行存款（全额）
        voucher.AddEntry(subject1001, "Debit", receipt.Amount, $"收款 {receipt.ReceiptNo}");

        // 6. 贷 1122 应收账款（≤ 余额冲应收）
        if (offset > 0)
            voucher.AddEntry(subject1122, "Credit", offset, "冲应收");

        // 7. 贷 2203 预收账款（溢出部分）
        if (overflow > 0 && subject2203 != Guid.Empty)
            voucher.AddEntry(subject2203, "Credit", overflow, "溢出进预收");

        // 8. 过账校验（至少一条分录、借贷平衡）
        voucher.Post();

        return voucher;
    }

    /// <summary>
    /// 生成一次性费用凭证（如押金、一次性服务费等）。
    /// </summary>
    /// <inheritdoc/>
    public Voucher GenerateOneTimeVoucher(string voucherNo, DateOnly voucherDate, Guid contractId, string period, Guid feeCodeId, decimal amount, IReadOnlyDictionary<string, Guid> subjectMap)
    {
        ArgumentNullException.ThrowIfNull(subjectMap);

        if (string.IsNullOrWhiteSpace(voucherNo))
            throw new ArgumentException("凭证编号不能为空", nameof(voucherNo));
        if (amount <= 0)
            throw new ArgumentException("一次性费用金额必须大于零", nameof(amount));

        // 1. 创建凭证
        var voucher = new Voucher(voucherNo, voucherDate);
        voucher.SetContract(contractId);
        voucher.SetPeriod(period);

        // 2. 判别业务类型：押金（DEPOSIT）或普通一次性费用
        var depositArId = subjectMap.GetValueOrDefault("112202", Guid.Empty);
        var depositLiabilityId = subjectMap.GetValueOrDefault("2241", Guid.Empty);

        if (depositArId != Guid.Empty && depositLiabilityId != Guid.Empty)
        {
            // —— 押金：借 112202（应收押金）/ 贷 2241（其他应付款-押金） ——
            voucher.AddEntry(depositArId, "Debit", amount, "押金-应收");
            voucher.AddEntry(depositLiabilityId, "Credit", amount, "押金-应付款");
        }
        else
        {
            // —— 普通一次性费用：借 1122（应收账款）/ 贷 6001（主营业务收入） ——
            if (!subjectMap.TryGetValue("1122", out var arId))
                throw new ArgumentException("科目映射缺少必需科目「1122-应收账款」", nameof(subjectMap));

            var revenueId = subjectMap.GetValueOrDefault("6001", subjectMap.GetValueOrDefault("6051", Guid.Empty));
            if (revenueId == Guid.Empty)
                throw new ArgumentException("科目映射缺少收入科目「6001-主营业务收入」或「6051-其他业务收入」", nameof(subjectMap));

            voucher.AddEntry(arId, "Debit", amount, "应收-一次性费用");
            voucher.AddEntry(revenueId, "Credit", amount, "收入-一次性费用");
        }

        // 3. 过账校验（至少一条分录、借贷平衡）
        voucher.Post();

        return voucher;
    }

    /// <summary>
    /// 生成补充费用凭证（费用调价后的补差凭证）。
    /// </summary>
    /// <inheritdoc/>
    public Voucher GenerateSupplementaryVoucher(string voucherNo, DateOnly voucherDate, Guid contractId, string period, decimal amount, IReadOnlyDictionary<string, Guid> subjectMap)
    {
        ArgumentNullException.ThrowIfNull(subjectMap);

        if (string.IsNullOrWhiteSpace(voucherNo))
            throw new ArgumentException("凭证编号不能为空", nameof(voucherNo));
        if (amount == 0)
            throw new ArgumentException("补充金额不能为零", nameof(amount));

        // 1. 验证必需科目
        if (!subjectMap.TryGetValue("1122", out var arId))
            throw new ArgumentException("科目映射缺少必需科目「1122-应收账款」", nameof(subjectMap));

        var revenueId = subjectMap.GetValueOrDefault("6001", subjectMap.GetValueOrDefault("6051", Guid.Empty));
        if (revenueId == Guid.Empty)
            throw new ArgumentException("科目映射缺少收入科目「6001-主营业务收入」或「6051-其他业务收入」", nameof(subjectMap));

        // 2. 创建凭证
        var voucher = new Voucher(voucherNo, voucherDate, $"调价补差（{period}）");
        voucher.SetContract(contractId);
        voucher.SetPeriod(period);

        // 3. 根据金额正负确定借贷方向
        if (amount > 0)
        {
            // 正数补差（应收增加）：借 1122 / 贷 6001（或 6051）
            voucher.AddEntry(arId, "Debit", amount, "补差-应收增加");
            voucher.AddEntry(revenueId, "Credit", amount, "补差-收入增加");
        }
        else
        {
            // 负数补差（应收冲减）：贷 1122 / 借 6001（或 6051）
            var absAmount = Math.Abs(amount);
            voucher.AddEntry(arId, "Credit", absAmount, "补差-应收冲减");
            voucher.AddEntry(revenueId, "Debit", absAmount, "补差-收入冲减");
        }

        // 4. 过账校验（至少一条分录、借贷平衡）
        voucher.Post();

        return voucher;
    }
}
