namespace RBS.Core.Entities.Accounting;

using RBS.Core.Entities.Base;

/// <summary>
/// 凭证实体（聚合根）
/// 表示会计系统中的一张记账凭证，包含多笔分录（借方/贷方），
/// 遵循 Draft（草稿）→ Posted（已过账）→ Audited（已审核）的状态流转
/// </summary>
public class Voucher : AggregateRoot
{
    private readonly List<JournalEntry> _entries = new();

    /// <summary>
    /// 凭证编号，由系统根据规则自动生成（如 "PZ-2026-0001"）
    /// </summary>
    public string VoucherNo { get; private set; } = string.Empty;

    /// <summary>
    /// 凭证日期
    /// </summary>
    public DateOnly VoucherDate { get; private set; }

    /// <summary>
    /// 凭证摘要说明
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 凭证当前状态（草稿/已过账/已审核）
    /// </summary>
    public VoucherStatus Status { get; private set; } = VoucherStatus.Draft;

    /// <summary>
    /// 来源业务实体标识（如合同、收款单等）
    /// </summary>
    public Guid? SourceEntityId { get; private set; }

    /// <summary>
    /// 来源业务实体类型名称
    /// </summary>
    public string? SourceEntityType { get; private set; }

    /// <summary>
    /// 乐观并发控制行版本号
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 凭证分录集合（只读）
    /// </summary>
    public IReadOnlyCollection<JournalEntry> Entries => _entries.AsReadOnly();

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Voucher() { }

    /// <summary>
    /// 创建凭证实例，初始状态为草稿（Draft）
    /// </summary>
    /// <param name="voucherNo">凭证编号</param>
    /// <param name="voucherDate">凭证日期</param>
    /// <param name="description">摘要说明（可选）</param>
    /// <exception cref="ArgumentException">当凭证编号为空时抛出</exception>
    public Voucher(string voucherNo, DateOnly voucherDate, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(voucherNo))
            throw new ArgumentException("凭证编号不能为空", nameof(voucherNo));

        VoucherNo = voucherNo.Trim();
        VoucherDate = voucherDate;
        Description = description?.Trim();
        Status = VoucherStatus.Draft;
    }

    /// <summary>
    /// 添加一条分录到凭证
    /// </summary>
    /// <param name="accountingSubjectId">会计科目标识</param>
    /// <param name="direction">借贷方向：Debit（借方）或 Credit（贷方）</param>
    /// <param name="amount">金额</param>
    /// <param name="summary">分录摘要（可选）</param>
    /// <exception cref="InvalidOperationException">当凭证已过账或已审核时抛出</exception>
    /// <exception cref="ArgumentException">当方向无效或金额不大于零时抛出</exception>
    public void AddEntry(Guid accountingSubjectId, string direction, decimal amount, string? summary = null)
    {
        if (Status != VoucherStatus.Draft)
            throw new InvalidOperationException($"凭证状态为「{Status.DisplayName}」，仅草稿状态下可添加分录");

        if (direction != "Debit" && direction != "Credit")
            throw new ArgumentException("分录方向必须为 Debit（借方）或 Credit（贷方）", nameof(direction));

        if (amount <= 0)
            throw new ArgumentException("分录金额必须大于零", nameof(amount));

        var entry = new JournalEntry(Id, accountingSubjectId, direction, amount);
        entry.SetSummary(summary);
        _entries.Add(entry);
    }

    /// <summary>
    /// 从凭证中移除指定分录
    /// </summary>
    /// <param name="entry">要移除的分录实体</param>
    /// <exception cref="InvalidOperationException">当凭证已过账或已审核，或分录不属于本凭证时抛出</exception>
    /// <exception cref="ArgumentNullException">当分录参数为空时抛出</exception>
    public void RemoveEntry(JournalEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (Status != VoucherStatus.Draft)
            throw new InvalidOperationException($"凭证状态为「{Status.DisplayName}」，仅草稿状态下可移除分录");

        if (!_entries.Remove(entry))
            throw new InvalidOperationException("该分录不属于本凭证，无法移除");
    }

    /// <summary>
    /// 过账：将草稿凭证转为已过账状态
    /// 过账前自动校验借方金额合计等于贷方金额合计
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不允许过账或借贷不平衡时抛出</exception>
    public void Post()
    {
        if (Status != VoucherStatus.Draft)
            throw new InvalidOperationException($"凭证状态为「{Status.DisplayName}」，仅草稿状态可执行过账操作");

        if (_entries.Count == 0)
            throw new InvalidOperationException("凭证至少需要一条分录才能过账");

        if (!IsDebitCreditBalanced())
            throw new InvalidOperationException(
                $"借贷不平衡：借方合计 {GetTotalDebit():F2}，贷方合计 {GetTotalCredit():F2}，借方与贷方必须相等");

        Status = VoucherStatus.Posted;
    }

    /// <summary>
    /// 审核：将已过账凭证转为已审核状态
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不允许审核时抛出</exception>
    public void Audit()
    {
        if (Status != VoucherStatus.Posted)
            throw new InvalidOperationException($"凭证状态为「{Status.DisplayName}」，仅已过账状态可执行审核操作");

        Status = VoucherStatus.Audited;
    }

    /// <summary>
    /// 反过账：将已过账凭证退回草稿状态
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不允许反过账时抛出</exception>
    public void Unpost()
    {
        if (Status != VoucherStatus.Posted)
            throw new InvalidOperationException($"凭证状态为「{Status.DisplayName}」，仅已过账状态可执行反过账操作");

        Status = VoucherStatus.Draft;
    }

    /// <summary>
    /// 设置凭证来源业务实体
    /// </summary>
    /// <param name="sourceEntityId">来源实体标识</param>
    /// <param name="sourceEntityType">来源实体类型名称</param>
    public void SetSource(Guid? sourceEntityId, string? sourceEntityType)
    {
        SourceEntityId = sourceEntityId;
        SourceEntityType = sourceEntityType?.Trim();
    }

    /// <summary>
    /// 设置凭证摘要
    /// </summary>
    /// <param name="description">摘要内容</param>
    /// <exception cref="InvalidOperationException">当凭证已过账或已审核时抛出</exception>
    public void SetDescription(string? description)
    {
        if (Status != VoucherStatus.Draft)
            throw new InvalidOperationException($"凭证状态为「{Status.DisplayName}」，仅草稿状态可修改摘要");

        Description = description?.Trim();
    }

    /// <summary>
    /// 校验借方合计与贷方合计是否平衡
    /// </summary>
    public bool IsDebitCreditBalanced()
    {
        return GetTotalDebit() == GetTotalCredit();
    }

    /// <summary>
    /// 获取借方合计金额
    /// </summary>
    public decimal GetTotalDebit()
    {
        return _entries.Where(e => e.Direction == "Debit").Sum(e => e.Amount);
    }

    /// <summary>
    /// 获取贷方合计金额
    /// </summary>
    public decimal GetTotalCredit()
    {
        return _entries.Where(e => e.Direction == "Credit").Sum(e => e.Amount);
    }
}

/// <summary>
/// 凭证状态值对象
/// 定义凭证的生命周期状态：Draft（草稿）→ Posted（已过账）→ Audited（已审核）
/// </summary>
public sealed class VoucherStatus : ValueObject
{
    /// <summary>草稿 — 可编辑分录，未过账</summary>
    public static readonly VoucherStatus Draft = new("Draft", "草稿");

    /// <summary>已过账 — 借贷已平衡，不可编辑分录</summary>
    public static readonly VoucherStatus Posted = new("Posted", "已过账");

    /// <summary>已审核 — 最终状态，不可变更</summary>
    public static readonly VoucherStatus Audited = new("Audited", "已审核");

    /// <summary>状态代码</summary>
    public string Code { get; }

    /// <summary>状态显示名称</summary>
    public string DisplayName { get; }

    private VoucherStatus() { Code = "Draft"; DisplayName = "草稿"; }

    private VoucherStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>
    /// 从状态代码解析为 VoucherStatus 实例
    /// </summary>
    public static VoucherStatus FromCode(string code) =>
        _all.FirstOrDefault(s => s.Code == code) ?? Draft;

    /// <summary>
    /// 判断当前状态是否可以转换到目标状态
    /// </summary>
    public bool CanTransitionTo(VoucherStatus target) =>
        (_validTransitions.TryGetValue(this, out var allowed) && allowed.Contains(target));

    private static readonly Dictionary<VoucherStatus, VoucherStatus[]> _validTransitions = new()
    {
        [Draft] = new[] { Posted },
        [Posted] = new[] { Audited, Draft },
    };

    private static readonly VoucherStatus[] _all = { Draft, Posted, Audited };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
