namespace RBS.Core.Entities.Billing;

/// <summary>
/// 共享接口 — 使 DemandNote 和 ReminderNote 可以使用统一的 Charge 渲染
/// </summary>
public interface INoteEntity
{
    List<DemandNoteCharges> Charges { get; }
    decimal? OpenBal { get; }
    decimal? CloseBal { get; }
    string? PaidByAutopay { get; }
}

/// <summary>
/// DemandNote 实体 — 缴款通知书
/// （从源项目 BillingBatchProcess.Model 移植）
/// </summary>
public class DemandNote : INoteEntity
{
    public List<DemandNoteCharges> Charges { get; set; } = new();

    public string? DnNo { get; set; }
    public string? PropertyAc { get; set; }
    public int DnSerialNo { get; set; }
    public string? IssueDate { get; set; }
    public string? PpsStatus { get; set; }
    public string? PpsNo { get; set; }
    public string? BillName { get; set; }
    public string? BillName1 { get; set; }
    public string? BillName2 { get; set; }
    public string? BillAddress1 { get; set; }
    public string? BillAddress2 { get; set; }
    public string? BillAddress3 { get; set; }
    public string? BillAddress4 { get; set; }
    public string? BillAddress5 { get; set; }
    public string? FpsQrContent { get; set; }
    public string? EnEnquiry { get; set; }
    public string? ZhEnquiry { get; set; }
    public string? PaidByAutopay { get; set; }
    public decimal? OpenBal { get; set; }
    public decimal? CloseBal { get; set; }
    public string? EnPropAddress { get; set; }
    public string? ZhPropAddress { get; set; }
    public string? BillAddr { get; set; }
    public string? BankAc { get; set; }
    public string? Barcode { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BankCode { get; set; }
    public string? DeliveryFlag { get; set; }
    public string? OptIn { get; set; }
    public string? DnDistribute { get; set; }
}

/// <summary>
/// DemandNote 收费明细实体
/// </summary>
public class DemandNoteCharges
{
    public int PageNo { get; set; }
    public int PrintOrder { get; set; }
    public double RowSum { get; set; }
    public string? ChargeCode { get; set; }
    public string? ChargeDesc { get; set; }
    public string? ChargeChiDesc { get; set; }
    public string? BilledDate { get; set; }
    public string? EffDate { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalSum { get; set; }
    public double LastMonthBf { get; set; }
    public double PaymentReceived { get; set; }
    public DateTime PaymentReceivedTime { get; set; }
}

/// <summary>
/// ReminderNote 实体 — 催缴通知书
/// </summary>
public class ReminderNote : INoteEntity
{
    public List<DemandNoteCharges> Charges { get; set; } = new();

    public string? RnNo { get; set; }
    public string? PropertyAc { get; set; }
    public int RnSerialNo { get; set; }
    public string? IssueDate { get; set; }
    public string? PpsStatus { get; set; }
    public string? PpsNo { get; set; }
    public string? BillName { get; set; }
    public string? BillName1 { get; set; }
    public string? BillName2 { get; set; }
    public string? BillAddress1 { get; set; }
    public string? BillAddress2 { get; set; }
    public string? BillAddress3 { get; set; }
    public string? BillAddress4 { get; set; }
    public string? BillAddress5 { get; set; }
    public string? FpsQrContent { get; set; }
    public string? EnEnquiry { get; set; }
    public string? ZhEnquiry { get; set; }
    public string? PaidByAutopay { get; set; }
    public decimal? OpenBal { get; set; }
    public decimal? CloseBal { get; set; }
    public string? EnPropAddress { get; set; }
    public string? ZhPropAddress { get; set; }
    public string? BillAddr { get; set; }
    public string? BankAc { get; set; }
    public string? Barcode { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BankCode { get; set; }
    public string? DeliveryFlag { get; set; }
    public string? OptIn { get; set; }
    public string? DnDistribute { get; set; }
}

/// <summary>
/// ReminderNote 收费明细（同 DemandNoteCharges，为类型区分保留）
/// </summary>
public class ReminderNoteCharges : DemandNoteCharges
{
}
