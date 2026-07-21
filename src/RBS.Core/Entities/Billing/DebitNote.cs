namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 账单聚合根（继承 AuditableEntity）
/// —— 出账时由 BillJob 生成，汇总一个合同在指定账期的所有应收费用。
/// 包含快照字段（出账时定格，后续合同信息变更不影响历史账单）。
/// 生命周期状态流转：Draft(草稿) → Published(已发布) → Cancelled(已作废)。
/// </summary>
public class DebitNote : AuditableEntity
{
    /// <summary>账单编号，系统自动生成，同一公司内唯一</summary>
    public string NoteNo { get; private set; } = string.Empty;

    /// <summary>关联的合同 ID</summary>
    public Guid ContractId { get; private set; }

    /// <summary>合同编号（快照字段，出账后不再随合同变更）</summary>
    public string? ContractNo { get; private set; }

    /// <summary>所属账期，格式如 "2026-07"</summary>
    public string? Period { get; private set; }

    /// <summary>所属公司 ID</summary>
    public Guid CompanyId { get; private set; }

    // === 快照字段（出账时定格，后续变更不影响历史） ===
    /// <summary>房产全编码（快照字段），如 "B1-3A-01"</summary>
    public string? RoomFullCode { get; private set; }

    /// <summary>租户名称（快照字段），出账时合同中的租户名字</summary>
    public string? TenantName { get; private set; }

    /// <summary>地址（快照字段），出账时房屋单元地址</summary>
    public string? BuildingAddress { get; private set; }

    /// <summary>公司名称（快照字段），出账时所属公司名，用于 PDF 水印</summary>
    public string? CompanyName { get; private set; }

    /// <summary>上月结余（快照字段），出账时定格计算的合同前期欠款</summary>
    public decimal PreviousBalance { get; private set; }

    /// <summary>账单总金额，单位：元。所有明细项金额之和。</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>已收金额合计，单位：元</summary>
    public decimal TotalReceived { get; private set; }

    /// <summary>预付金额合计，单位：元</summary>
    public decimal TotalPrepaid { get; private set; }

    /// <summary>应付余额 = TotalAmount - TotalReceived - TotalPrepaid</summary>
    public decimal BalanceDue { get; private set; }

    /// <summary>
    /// 账单状态：Draft（草稿）| Published（已发布）| Cancelled（已作废）
    /// </summary>
    public string Status { get; private set; } = "Draft";

    /// <summary>是否历史账单（补录/调整时生成的旧账期账单）</summary>
    public bool IsHistorical { get; private set; }

    /// <summary>付款到期日，由出账时计算设定</summary>
    public DateOnly? DueDate { get; private set; }

    /// <summary>出账时间，MarkGenerated 时记录</summary>
    public DateTime? GeneratedAt { get; private set; }

    /// <summary>作废时间，Cancel 时记录</summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>作废操作人 ID</summary>
    public Guid? CancelledBy { get; private set; }

    /// <summary>作废原因</summary>
    public string? CancelReason { get; private set; }

    /// <summary>出账任务日志 ID，关联到 <see cref="BillJob"/> 的执行记录</summary>
    public Guid? BillJobTaskLogId { get; private set; }

    private readonly List<DebitNoteItem> _items = new();
    private readonly List<DebitNoteReceipt> _receipts = new();

    /// <summary>账单明细行集合，包含各费用项目的应收信息</summary>
    public IReadOnlyCollection<DebitNoteItem> Items => _items.AsReadOnly();

    /// <summary>收款快照集合，出账时定格写入（PDF 用）</summary>
    public IReadOnlyCollection<DebitNoteReceipt> Receipts => _receipts.AsReadOnly();

    /// <summary>私有无参构造函数，供 EF Core / Dapper 延迟加载使用</summary>
    private DebitNote() : base() { }

    /// <summary>
    /// 创建新账单实例。
    /// 新账单默认状态为 Draft（草稿），尚未出账发布。
    /// </summary>
    /// <param name="noteNo">账单编号，不能为空</param>
    /// <param name="contractId">关联的合同 ID</param>
    /// <param name="period">所属账期，格式如 "2026-07"</param>
    /// <param name="id">可选主键，不传则自动生成</param>
    /// <exception cref="ArgumentException">noteNo 为空时抛出</exception>
    public DebitNote(string noteNo, Guid contractId, string period, Guid? id = null) : base()
    {
        if (string.IsNullOrWhiteSpace(noteNo))
            throw new ArgumentException("账单编号不能为空", nameof(noteNo));
        if (id.HasValue) Id = id.Value;
        NoteNo = noteNo;
        ContractId = contractId;
        Period = period;
    }

    /// <summary>设置账单总金额</summary>
    /// <param name="total">总金额，单位：元</param>
    public void SetTotalAmount(decimal total) { TotalAmount = total; }

    /// <summary>设置合同快照字段（出账时定格，避免后续合同变更影响历史账单）</summary>
    /// <param name="contractNo">合同编号</param>
    /// <param name="roomCode">房产全编码</param>
    /// <param name="tenantName">租户名称</param>
    /// <param name="buildingAddress">地址</param>
    /// <param name="companyName">公司名称</param>
    /// <param name="previousBalance">上月结余</param>
    public void SetSnapshot(string? contractNo, string? roomCode, string? tenantName,
        string? buildingAddress = null, string? companyName = null, decimal previousBalance = 0)
    {
        ContractNo = contractNo;
        RoomFullCode = roomCode;
        TenantName = tenantName;
        BuildingAddress = buildingAddress;
        CompanyName = companyName;
        PreviousBalance = previousBalance;
    }

    /// <summary>设置收款汇总信息，同时计算应付余额</summary>
    /// <param name="received">已收金额合计</param>
    /// <param name="prepaid">预付金额合计</param>
    public void SetPaymentSummary(decimal received, decimal prepaid)
    {
        TotalReceived = received;
        TotalPrepaid = prepaid;
        BalanceDue = TotalAmount - TotalReceived - TotalPrepaid;
    }

    /// <summary>将账单标记为已发布（出账完成）</summary>
    /// <param name="taskLogId">出账任务日志 ID</param>
    /// <param name="isHistorical">是否为历史账单</param>
    /// <param name="dueDate">付款到期日</param>
    public void MarkGenerated(Guid taskLogId, bool isHistorical = false, DateOnly? dueDate = null)
    {
        Status = "Published";
        GeneratedAt = RBS.Core.Common.ChinaTime.Now;
        BillJobTaskLogId = taskLogId;
        IsHistorical = isHistorical;
        DueDate = dueDate;
    }

    /// <summary>作废账单</summary>
    /// <param name="userId">操作人 ID</param>
    /// <param name="reason">作废原因</param>
    public void Cancel(Guid userId, string reason)
    {
        Status = "Cancelled";
        CancelledAt = RBS.Core.Common.ChinaTime.Now;
        CancelledBy = userId;
        CancelReason = reason;
    }

    /// <summary>加载账单明细行，替换现有明细集合</summary>
    /// <param name="items">明细行集合</param>
    public void LoadItems(IEnumerable<DebitNoteItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    /// <summary>加载收款快照集合</summary>
    /// <param name="receipts">收款快照集合</param>
    public void LoadReceipts(IEnumerable<DebitNoteReceipt> receipts)
    {
        _receipts.Clear();
        _receipts.AddRange(receipts);
    }
}
