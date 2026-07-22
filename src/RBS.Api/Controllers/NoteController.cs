using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Billing;
using RBS.Infrastructure.PdfGeneration;

namespace RBS.Api.Controllers;

/// <summary>
/// DemandNote PDF 演示控制器 — 生成模拟的 Demand Note PDF 用于预览和导出
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NoteController : ControllerBase
{
    /// <summary>
    /// 预览 DemandNote PDF（浏览器内联显示）
    /// </summary>
    [HttpGet("dnpreview")]
    public IActionResult DnPreview()
    {
        var pdf = GenerateDemandNotePdf();
        return File(pdf, "application/pdf", "DemandNote_Preview.pdf");
    }

    /// <summary>
    /// 导出 DemandNote PDF（触发下载）
    /// </summary>
    [HttpGet("dndownload")]
    public IActionResult DnDownload()
    {
        var pdf = GenerateDemandNotePdf();
        return File(pdf, "application/pdf", $"DemandNote_{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// 预览 ReminderNote PDF（浏览器内联显示）
    /// </summary>
    [HttpGet("rnpreview")]
    public IActionResult RnPreview()
    {
        var pdf = GenerateReminderNotePdf();
        return File(pdf, "application/pdf", "ReminderNote_Preview.pdf");
    }

    /// <summary>
    /// 导出 ReminderNote PDF（触发下载）
    /// </summary>
    [HttpGet("rndownload")]
    public IActionResult RnDownload()
    {
        var pdf = GenerateReminderNotePdf();
        return File(pdf, "application/pdf", $"ReminderNote_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private byte[] GenerateDemandNotePdf()
    {
        // ===== Mock 数据（从 BillingBatchProcess Program.cs 移植） =====
        var note = new DemandNote
        {
            DnNo = "DN-2026-0001",
            PropertyAc = "PM-1001",
            DnSerialNo = 1,
            IssueDate = DateTime.Now.ToString("dd/MM/yyyy"),
            PpsStatus = "N",
            BillAddr = "FLAT A, 1/F, TOWER 1\n100 TEST ROAD\nKOWLOON",
            EnPropAddress = "FLAT A, 1/F, TOWER 1\n100 TEST ROAD\nKOWLOON",
            ZhPropAddress = "九龙测试道100号\n1座1楼A室",
            EnEnquiry = "Please pay by the due date.",
            ZhEnquiry = "请于到期日前付款。",
            BankAc = "123-456-789",
            OpenBal = 1000.00m,
            CloseBal = 3500.00m,
            FpsQrContent = "00020101021226300011HKHKD2005001000123456789MockFPSData",
            Barcode = "123456789000000350000PM-1001DN-2026-0001",
            BarcodeTitle = "123-456-789-000000350000-PM-1001-DN-2026-0001",
        };

        for (int i = 0; i < 16; i++)
        {
            note.Charges.Add(new DemandNoteCharges
            {
                ChargeCode = "C" + (i + 1).ToString("D4"),
                ChargeDesc = "Charge Item " + (i + 1),
                ChargeChiDesc = "收费项目",
                BilledDate = DateTime.Now.AddMonths(-1).ToString("MM/yyyy"),
                EffDate = DateTime.Now.ToString("dd/MM/yyyy"),
                Amount = 100m + (i * 50m),
                TotalSum = 100m + (i * 50m),
            });
        }

        var generator = new NotePdf();
        return generator.DemandNote(new List<DemandNote> { note });
    }

    private byte[] GenerateReminderNotePdf()
    {
        var note = new ReminderNote
        {
            RnNo = "RN-2026-0001",
            PropertyAc = "PM-1001",
            RnSerialNo = 1,
            IssueDate = DateTime.Now.ToString("dd/MM/yyyy"),
            PpsStatus = "N",
            BillAddr = "FLAT A, 1/F, TOWER 1\n100 TEST ROAD\nKOWLOON",
            EnPropAddress = "FLAT A, 1/F, TOWER 1\n100 TEST ROAD\nKOWLOON",
            ZhPropAddress = "九龙测试道100号\n1座1楼A室",
            EnEnquiry = "Please pay by the due date.",
            ZhEnquiry = "请于到期日前付款。",
            BankAc = "123-456-789",
            OpenBal = 1000.00m,
            CloseBal = 3500.00m,
            FpsQrContent = "00020101021226300011HKHKD2005001000123456789MockFPSData",
            Barcode = "123456789000000350000PM-1001RN-2026-0001",
            BarcodeTitle = "123-456-789-000000350000-PM-1001-RN-2026-0001",
        };

        for (int i = 0; i < 14; i++)
        {
            note.Charges.Add(new DemandNoteCharges
            {
                ChargeCode = "C" + (i + 1).ToString("D4"),
                ChargeDesc = "Charge Item " + (i + 1),
                ChargeChiDesc = "收费项目",
                BilledDate = DateTime.Now.AddMonths(-1).ToString("MM/yyyy"),
                EffDate = DateTime.Now.ToString("dd/MM/yyyy"),
                Amount = 100m + (i * 50m),
                TotalSum = 100m + (i * 50m),
            });
        }

        var generator = new NotePdf();
        return generator.ReminderNote(new List<ReminderNote> { note });
    }
}
