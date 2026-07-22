#pragma warning disable CA1416 // System.Drawing 仅 Windows 支持，本应用仅在 Windows 部署

using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using RBS.Core.Entities.Billing;
using PdfRectangle = iTextSharp.text.Rectangle;

namespace RBS.Infrastructure.PdfGeneration;

/// <summary>
/// 缴款/催缴通知书 PDF 生成器 — 基于 iTextSharp
/// （从 BillingBatchProcess NotePdf 移植，返回 byte[]）
/// </summary>
public class NotePdf
{
    private static readonly BaseFont SimHei;
    private static readonly BaseFont Helvetica;
    private static readonly BaseColor ForegroundColor = new(102, 102, 102);

    static NotePdf()
    {
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Fonts", "SimHei.ttf");
        SimHei = BaseFont.CreateFont(fontPath, "Identity-H", embedded: false);
        Helvetica = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
    }

    /// <summary>生成 DemandNote（缴款通知书）PDF</summary>
    public byte[] DemandNote(List<DemandNote> list)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 10f, 10f, 84f, 10f);
        var writer = PdfWriter.GetInstance(document, ms);
        document.Open();

        foreach (var entity in list)
        {
            if (entity == null || entity.Charges == null || entity.Charges.Count == 0)
                continue;

            int pageSize = 13;
            int totalPage = (int)Math.Ceiling((double)entity.Charges.Count / pageSize);

            for (int page = 0; page < totalPage; page++)
            {
                if (page > 0) document.NewPage();

                document.Add(SetBaseTable(entity.BillAddr, entity.DnNo, entity.PropertyAc, entity.IssueDate, entity.PpsStatus, entity.PpsNo));
                document.Add(PropAddr(entity.ZhPropAddress, entity.EnPropAddress));
                document.Add(ChargeTable(page, pageSize, totalPage, entity));
                document.Add(DateDesc(0));
                document.Add(Presentation());
                document.Add(Cheques(entity.ZhEnquiry));
                var qrImage = GenQrCode(entity.FpsQrContent, 282f);
                if (qrImage != null)
                    document.Add(qrImage);
                document.Add(ForEnquiry(entity.EnEnquiry));
                document.Add(Bank(entity.BankAc, entity.DnNo, entity.CloseBal, entity.PropertyAc));
                document.Add(Cheque());
                if (!string.IsNullOrWhiteSpace(entity.Barcode))
                {
                    document.Add(GenBatCode(entity.Barcode, 58f));
                    document.Add(BatCodeTitle(entity.BarcodeTitle));
                }
                AddTextWatermark(writer, "繳款通知書", "DEMAND NOTE");
            }
        }
        document.Close();
        return ms.ToArray();
    }

    /// <summary>生成 ReminderNote（催缴通知书）PDF</summary>
    public byte[] ReminderNote(List<ReminderNote> list)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 10f, 10f, 84f, 10f);
        var writer = PdfWriter.GetInstance(document, ms);
        document.Open();

        foreach (var entity in list)
        {
            if (entity == null || entity.Charges == null || entity.Charges.Count == 0)
                continue;

            int pageSize = 11;
            int totalPage = (int)Math.Ceiling((double)entity.Charges.Count / pageSize);

            for (int page = 0; page < totalPage; page++)
            {
                if (page > 0) document.NewPage();

                document.Add(SetBaseTable(entity.BillAddr, entity.RnNo, entity.PropertyAc, entity.IssueDate, entity.PpsStatus, entity.PpsNo));
                document.Add(PropAddr(entity.ZhPropAddress, entity.EnPropAddress));
                document.Add(ChargeTable(page, pageSize, totalPage, entity));
                document.Add(FirstTimePrintedItems());
                document.Add(DateDesc(1));
                document.Add(Presentation());
                document.Add(Cheques(entity.ZhEnquiry));
                var qrImage = GenQrCode(entity.FpsQrContent, 260f);
                if (qrImage != null)
                    document.Add(qrImage);
                document.Add(ForEnquiry(entity.EnEnquiry));
                document.Add(Bank(entity.BankAc, entity.RnNo, entity.CloseBal, entity.PropertyAc));
                document.Add(Cheque());
                if (!string.IsNullOrWhiteSpace(entity.Barcode))
                {
                    document.Add(GenBatCode(entity.Barcode, 36f));
                    document.Add(BatCodeTitle(entity.BarcodeTitle));
                }
                AddTextWatermark(writer, "最後通知", "FINAL DEMAND");
            }
        }
        document.Close();
        return ms.ToArray();
    }

    // ===================================================================
    // SetBaseTable — bill address + DN info
    // ===================================================================
    private PdfPTable SetBaseTable(string? billAddr, string? dnNo, string? propertyAc, string? issueDate, string? ppsStatus, string? ppsNo)
    {
        var table = new PdfPTable(19) { HorizontalAlignment = Element.ALIGN_CENTER, TotalWidth = 560f };
        table.LockedWidth = true;
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;

        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER, PaddingLeft = 5f };
        cellModel.Colspan = 10;
        table.AddCell(MakeCell("", cellModel, SetAddrs(billAddr)));

        cellModel.Colspan = 9;
        table.AddCell(MakeCell("", cellModel, SetDnNo(dnNo, propertyAc, issueDate, ppsStatus, ppsNo)));
        return table;
    }

    private PdfPTable SetAddrs(string? billAddr)
    {
        var table = new PdfPTable(1);
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;

        var font = IsIncludeChar(billAddr) ? SimHei : Helvetica;
        var phrase = new Phrase(billAddr ?? "", new iTextSharp.text.Font(font, 10f));
        var cell = new PdfPCell(phrase)
        {
            FixedHeight = 90f, Border = PdfRectangle.NO_BORDER,
            PaddingTop = 11.5f, PaddingLeft = 35f,
            BackgroundColor = new BaseColor(238, 238, 238)
        };
        table.AddCell(cell);

        var emptyCell = new PdfPCell(new Phrase(" ")) { FixedHeight = 1f, Border = PdfRectangle.NO_BORDER };
        table.AddCell(emptyCell);
        return table;
    }

    private PdfPTable SetDnNo(string? dnNo, string? propertyAc, string? issueDate, string? ppsStatus, string? ppsNo)
    {
        var table = new PdfPTable(9);
        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER, PaddingLeft = 18f, FixedHeight = 22f };

        // Demand Note No.
        cellModel.Colspan = 4;
        cellModel.VerticalAlignment = Element.ALIGN_BOTTOM;
        table.AddCell(MakeCell("繳款通知書編號\n", "Demand Note No.:", cellModel, 9f, 9f, 1.1f));
        cellModel.Colspan = 5;
        table.AddCell(MakeCell(dnNo ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));

        // Property A/C Code
        cellModel.Colspan = 4;
        cellModel.PaddingBottom = 0.5f;
        cellModel.FixedHeight = 34f;
        table.AddCell(MakeCell("物業單位號碼\n", "Property A/C Code:", cellModel, 9f, 9f, 1.1f));
        cellModel.Colspan = 5;
        table.AddCell(MakeCell(propertyAc ?? "", cellModel, fontSize: 12f, baseFont: Helvetica));

        // Issue Date
        cellModel.Colspan = 4;
        cellModel.FixedHeight = 22f;
        table.AddCell(MakeCell("發出日期\n", "Date Of Issue:", cellModel, 9f, 9f, 1.1f));
        cellModel.Colspan = 5;
        table.AddCell(MakeCell(issueDate ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));

        // PPS
        cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER, PaddingLeft = 18f, PaddingTop = 8f };
        cellModel.Colspan = 4;
        cellModel.FixedHeight = 34f;
        cellModel.VerticalAlignment = Element.ALIGN_BOTTOM;
        if (ppsStatus == "Y")
        {
            table.AddCell(MakeCell("賬單繳款號碼(繳費靈)\n", "Bill Payment No.:(PPS)", cellModel, 9f, 9f, 1.1f));
            cellModel.Colspan = 5;
            table.AddCell(MakeCell(ppsNo ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));
        }
        else
        {
            table.AddCell(MakeCell("\n", " ", cellModel));
            cellModel.Colspan = 5;
            table.AddCell(MakeCell(" ", cellModel));
        }
        return table;
    }

    // ===================================================================
    // PropAddr
    // ===================================================================
    private PdfPTable PropAddr(string? zhPropAddress, string? enPropAddress)
    {
        var table = new PdfPTable(1) { TotalWidth = 560f };
        table.LockedWidth = true;

        var cellModel = new PdfPCell { VerticalAlignment = Element.ALIGN_BOTTOM, Border = PdfRectangle.NO_BORDER, FixedHeight = 15f };
        table.AddCell(MakeCell("物业地址         " + (zhPropAddress ?? ""), cellModel));
        table.AddCell(MakeCell("Property Address:  " + (enPropAddress ?? ""), cellModel, fontSize: 9f, baseFont: Helvetica));
        return table;
    }

    // ===================================================================
    // Charge table
    // ===================================================================
    private PdfPTable ChargeTable(int page, int pageSize, int totalPage, INoteEntity entity)
    {
        var table = new PdfPTable(20) { TotalWidth = 560f };
        table.LockedWidth = true;

        var cellModel = new PdfPCell
        {
            VerticalAlignment = Element.ALIGN_BOTTOM,
            Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER,
            BorderWidth = 1f, BorderWidthBottom = 0f, BorderColor = ForegroundColor
        };

        // Header row
        cellModel.Colspan = 15;
        cellModel.FixedHeight = 28f;
        table.AddCell(MakeCell("賬項說明\n", "Description", cellModel, 9f, 9f, 1.3f));
        cellModel.Colspan = 2;
        cellModel.BorderWidthLeft = 0f;
        table.AddCell(MakeCell("日期\n", "Date", cellModel, 9f, 9f, 1.3f));
        cellModel.Colspan = 3;
        table.AddCell(MakeCell("款項\n", "Amount", cellModel, 9f, 9f, 1.3f));

        // B/F row
        cellModel = new PdfPCell
        {
            VerticalAlignment = Element.ALIGN_MIDDLE,
            Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER,
            BorderWidth = 1f, BorderWidthBottom = 0f, BorderColor = ForegroundColor, FixedHeight = 14f
        };

        if (page == 0)
        {
            cellModel.Colspan = 15; cellModel.BorderWidthLeft = 1f; cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(MakeCell("", cellModel, ChargeDetails("LAST MONTH B/F", "上月結餘", " ")));
            cellModel.Colspan = 2; cellModel.BorderWidthLeft = 0f;
            table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
            cellModel.Colspan = 3; cellModel.HorizontalAlignment = Element.ALIGN_RIGHT; cellModel.PaddingRight = 3f;
            table.AddCell(MakeCell(entity.OpenBal?.ToString("N") ?? " ", cellModel, fontSize: 9f, baseFont: Helvetica));
            cellModel.PaddingRight = 2f;
        }
        else
        {
            cellModel.Colspan = 15; cellModel.BorderWidthLeft = 1f; cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(MakeCell("", cellModel, ChargeDetails("............CONT'D", " ", " ")));
            cellModel.Colspan = 2; cellModel.BorderWidthLeft = 0f;
            table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
            cellModel.Colspan = 3; cellModel.HorizontalAlignment = Element.ALIGN_RIGHT; cellModel.PaddingRight = 3f;
            table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
            cellModel.PaddingRight = 2f;
        }

        // Charge item rows
        cellModel = new PdfPCell
        {
            VerticalAlignment = Element.ALIGN_BOTTOM,
            Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER,
            BorderWidth = 1f, BorderWidthBottom = 0f, BorderColor = ForegroundColor
        };
        cellModel.FixedHeight = 12f;

        int emptyRows = 14;
        int start = page * pageSize;
        if (entity.Charges != null)
        {
            for (int i = start; i < entity.Charges.Count; i++)
            {
                if (pageSize <= 0) break;

                var charge = entity.Charges[i];
                string amount = (!string.IsNullOrWhiteSpace(charge.ChargeDesc) || !string.IsNullOrWhiteSpace(charge.ChargeChiDesc)
                    || !string.IsNullOrWhiteSpace(charge.BilledDate) || !string.IsNullOrWhiteSpace(charge.EffDate) || charge.Amount != 0m)
                    ? charge.Amount.ToString("N") : " ";
                string desc = TruncateString(charge.ChargeDesc, 47);
                string chi = TruncateString(charge.ChargeChiDesc, 15);

                cellModel.Colspan = 15; cellModel.BorderWidthLeft = 1f; cellModel.BorderWidthBottom = 0f;
                cellModel.BorderWidthTop = 0f; cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(MakeCell("", cellModel, ChargeDetails(desc, chi, charge.BilledDate)));
                cellModel.Colspan = 2; cellModel.BorderWidthLeft = 0f;
                table.AddCell(MakeCell(charge.EffDate ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));
                cellModel.Colspan = 3; cellModel.HorizontalAlignment = Element.ALIGN_RIGHT; cellModel.PaddingRight = 3f;
                table.AddCell(MakeCell(amount, cellModel, fontSize: 9f, baseFont: Helvetica));
                cellModel.PaddingRight = 2f;
                pageSize--;
                emptyRows--;

                if (pageSize == 0 && page + 1 < totalPage)
                {
                    cellModel.Colspan = 15; cellModel.BorderWidthLeft = 1f; cellModel.BorderWidthBottom = 0f;
                    cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(MakeCell("", cellModel, ChargeDetails("TO BE CONT'D......", " ", " ")));
                    cellModel.Colspan = 2; cellModel.BorderWidthLeft = 0f;
                    table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
                    cellModel.Colspan = 3; cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
                    emptyRows--;
                }
            }
        }

        // Empty rows
        for (int j = 0; j < emptyRows; j++)
        {
            cellModel.Colspan = 15; cellModel.BorderWidthLeft = 1f; cellModel.BorderWidthBottom = 0f;
            cellModel.BorderWidthTop = 0f; cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(MakeCell(" ", cellModel, ChargeDetails(" ", " ", " ")));
            cellModel.Colspan = 2; cellModel.BorderWidthLeft = 0f;
            table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
            cellModel.Colspan = 3; cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
            table.AddCell(MakeCell(" ", cellModel, fontSize: 9f, baseFont: Helvetica));
        }

        // Total row
        cellModel = new PdfPCell
        {
            VerticalAlignment = Element.ALIGN_BOTTOM,
            Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER,
            BorderWidth = 1f, BorderWidthBottom = 0f, BorderColor = ForegroundColor
        };
        cellModel.Colspan = 15; cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
        cellModel.BorderWidthTop = 1f; cellModel.BorderWidthBottom = 1f; cellModel.BorderWidthLeft = 1f;
        table.AddCell(MakeCell(entity.PaidByAutopay == "Y" ? "" : " ", cellModel,
            entity.PaidByAutopay == "Y" ? PaidByAutopay() : null));
        cellModel.Colspan = 2; cellModel.BorderWidthLeft = 0f;
        table.AddCell(MakeCell("總額\n", "Total", cellModel));
        cellModel.Colspan = 3; cellModel.HorizontalAlignment = Element.ALIGN_RIGHT; cellModel.PaddingRight = 3f;
        table.AddCell(MakeCell(page + 1 == totalPage && entity.CloseBal.HasValue ? entity.CloseBal.Value.ToString("N") : " ",
            cellModel, fontSize: 9f, baseFont: Helvetica));

        return table;
    }

    // ===================================================================
    // Helper tables
    // ===================================================================
    private static PdfPTable PaidByAutopay()
    {
        var table = new PdfPTable(2);
        var cellModel = new PdfPCell { VerticalAlignment = Element.ALIGN_BOTTOM, HorizontalAlignment = Element.ALIGN_LEFT, Border = PdfRectangle.NO_BORDER };
        table.AddCell(MakeCell("PAY BY AUTOPAY", cellModel, fontSize: 10f));
        table.AddCell(MakeCell("用自動轉賬繳付", cellModel, fontSize: 10f));
        return table;
    }

    private static PdfPTable ChargeDetails(string chargeDesc, string chargeChiDesc, string? billedDate)
    {
        var table = new PdfPTable(24);
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;
        var cellModel = new PdfPCell { VerticalAlignment = Element.ALIGN_BOTTOM, HorizontalAlignment = Element.ALIGN_LEFT, Border = PdfRectangle.NO_BORDER, Padding = 0f };
        cellModel.FixedHeight = 12f;
        cellModel.Colspan = 14;
        table.AddCell(MakeCell(chargeDesc, cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 8;
        table.AddCell(MakeCell(chargeChiDesc, cellModel));
        cellModel.Colspan = 2;
        table.AddCell(MakeCell(billedDate ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));
        return table;
    }

    // ===================================================================
    // FirstTimePrintedItems
    // ===================================================================
    private PdfPTable FirstTimePrintedItems()
    {
        var table = new PdfPTable(31) { TotalWidth = 560f };
        table.LockedWidth = true;
        table.SpacingBefore = 0f;
        table.SpacingAfter = 0f;

        var cellModel = new PdfPCell { VerticalAlignment = Element.ALIGN_BOTTOM, Border = PdfRectangle.NO_BORDER };
        cellModel.FixedHeight = 22f;
        cellModel.Colspan = 26;
        cellModel.Padding = 0f;
        cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
        cellModel.PaddingRight = 5f;
        table.AddCell(MakeCell("*\n", "*", cellModel, 8f, 8f, null));
        cellModel.Colspan = 5;
        cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
        table.AddCell(MakeCell("首次列印項目", "First Time Printed Items", cellModel, 8f, 8f, 1.2f));
        return table;
    }

    // ===================================================================
    // DateDesc
    // ===================================================================
    private PdfPTable DateDesc(int noteType)
    {
        var table = new PdfPTable(1) { TotalWidth = 560f };
        table.LockedWidth = true;
        table.SpacingBefore = 2f;
        var cellModel = new PdfPCell { VerticalAlignment = Element.ALIGN_TOP, Border = PdfRectangle.NO_BORDER };

        if (noteType == 1)
        {
            table.AddCell(MakeCell("本月十四日後繳付的款項均未入賬，請於付款時扣除。\n",
                "Payments made after the 14th of this month have not been included into this reminder note and should be deducted from payment.",
                cellModel, 9f, 8f, 1.2f));
        }
        else
        {
            table.AddCell(MakeCell("上月廿二日後繳付的款項均未入賬，請於付款時扣除。\n",
                "Payments made after the 22nd of previous month have not been included into this demand note and should be deducted from payment.",
                cellModel, 9f, 8f, 1.2f));
        }
        return table;
    }

    // ===================================================================
    // Presentation
    // ===================================================================
    private static PdfPTable Presentation()
    {
        var table = new PdfPTable(1) { TotalWidth = 560f };
        table.LockedWidth = true;
        var cellModel = new PdfPCell { VerticalAlignment = Element.ALIGN_TOP, Border = PdfRectangle.NO_BORDER };
        table.AddCell(MakeCell("貴戶賬款已到期，請即繳付。過期付款，可被加收利息。繳款細則及方法，詳見背頁。\n",
            "The account is due on presentation. Interest is chargeable on late payment. Please see overleaf for payment instructions and methods.",
            cellModel, 9f, 8f, 1.2f));
        return table;
    }

    // ===================================================================
    // Cheques
    // ===================================================================
    private PdfPTable Cheques(string? zhEnquiry)
    {
        var table = new PdfPTable(19) { TotalWidth = 560f };
        table.LockedWidth = true;
        table.SpacingBefore = 5f;
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;
        table.DefaultCell.PaddingBottom = 0f;
        table.DefaultCell.PaddingTop = 0f;
        table.DefaultCell.PaddingRight = 0f;
        table.DefaultCell.PaddingLeft = 1.2f;

        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER };
        cellModel.Colspan = 13;
        table.AddCell(MakeCell("", cellModel, PayableTo(zhEnquiry)));
        cellModel.Colspan = 4;
        cellModel.Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER;
        cellModel.BorderColor = ForegroundColor;
        table.AddCell(MakeCell("如閣下選擇「轉數快」支付賬款，請掃描印在右邊之二維碼。",
            "Please scan the QR Code as shown on the right to enjoy fast payment experience with FPS.",
            cellModel, 8f, 7f));

        var spacer = new PdfPCell(new Phrase(" ")) { Border = PdfRectangle.NO_BORDER, Colspan = 2 };
        table.AddCell(spacer);
        return table;
    }

    private PdfPTable PayableTo(string? zhEnquiry)
    {
        var table = new PdfPTable(1);
        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER };
        cellModel.FixedHeight = 26f;
        table.AddCell(MakeCell("", cellModel, PayableToDetails()));
        cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER, PaddingRight = 0f, VerticalAlignment = Element.ALIGN_MIDDLE, FixedHeight = 11f };
        table.AddCell(MakeCell("如有查詢   " + (zhEnquiry ?? ""), cellModel, fontSize: 7.8f));
        return table;
    }

    private PdfPTable PayableToDetails()
    {
        var table = new PdfPTable(1);
        var cellModel = new PdfPCell
        {
            Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER,
            BorderColor = ForegroundColor
        };
        table.AddCell(MakeCell("繳款支票必須劃線及註明支付「中国鐵路有限公司」\n",
            "Cheques must be crossed and made payable to \"MTR Corporation Limited\"",
            cellModel, 12f, 11f));
        return table;
    }

    // ===================================================================
    // GenQrCode
    // ===================================================================
    private iTextSharp.text.Image? GenQrCode(string? fpsQrContent, float absoluteY = 282f)
    {
        try
        {
            byte[]? fileByte = GetPaymentQRCodeByte(fpsQrContent);
            if (fileByte == null || fileByte.Length == 0) return null;

            using var stream = new MemoryStream(fileByte);
            var img = iTextSharp.text.Image.GetInstance(stream);
            img.ScalePercent(22f);
            img.SetAbsolutePosition(520f, absoluteY);
            return img;
        }
        catch
        {
            return null;
        }
    }

    private static byte[]? GetPaymentQRCodeByte(string? fpsQrContent)
    {
        if (string.IsNullOrWhiteSpace(fpsQrContent))
            return null;

        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(fpsQrContent, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(20);
            using var bitmap = new Bitmap(qrCodeImage, 250, 250);
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Gif);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    // ===================================================================
    // ForEnquiry
    // ===================================================================
    private PdfPTable ForEnquiry(string? enEnquiry)
    {
        var table = new PdfPTable(26) { TotalWidth = 558f };
        table.LockedWidth = true;
        table.SpacingBefore = 3f;
        table.SpacingAfter = 2f;

        var cell = new PdfPCell(new Phrase("For enquiry:" + (enEnquiry ?? ""),
            new iTextSharp.text.Font(Helvetica, 7.8f)))
        { Border = PdfRectangle.NO_BORDER, FixedHeight = 11f, PaddingLeft = 3f, Colspan = 20 };
        table.AddCell(cell);

        var zhChunk = new Chunk("持牌物業管理公司 ", new iTextSharp.text.Font(SimHei, 6f));
        var enChunk = new Chunk("Licensed PMC(C-114608)", new iTextSharp.text.Font(Helvetica, 6f));
        var phrase = new Phrase();
        phrase.Add(zhChunk);
        phrase.Add(enChunk);
        var cellModel = new PdfPCell(phrase)
        { Border = PdfRectangle.NO_BORDER, Colspan = 6, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE };
        table.AddCell(cellModel);
        return table;
    }

    // ===================================================================
    // Bank
    // ===================================================================
    private PdfPTable Bank(string? bankAc, string? dnNo, decimal? closeBal, string? propertyAc)
    {
        var table = new PdfPTable(2) { TotalWidth = 560f };
        table.LockedWidth = true;
        table.SpacingBefore = 3f;
        table.SpacingAfter = 0f;

        var cellModel = new PdfPCell { BorderWidth = 1f, Padding = 0f, BorderWidthBottom = 0f };
        cellModel.BorderColor = ForegroundColor;
        table.AddCell(MakeCell("", cellModel, Dsb(bankAc)));
        cellModel.BorderWidthLeft = 0f;
        table.AddCell(MakeCell("", cellModel, TotalAmount(dnNo, closeBal, propertyAc)));
        cellModel.BorderWidthLeft = 1f;
        cellModel.Colspan = 2;
        cellModel.FixedHeight = 52f;
        table.AddCell(MakeCell(" ", cellModel));
        return table;
    }

    private static PdfPTable Dsb(string? bankAc)
    {
        var table = new PdfPTable(5);
        table.DefaultCell.Padding = 0f;
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;
        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_BOTTOM };
        cellModel.Colspan = 2;
        cellModel.PaddingLeft = 3f;
        table.AddCell(MakeCell("大新銀行有限公司\n", "DAH SING BANK LTD.", cellModel, 9f, 9f, 1.2f));
        cellModel.PaddingLeft = 2f;
        cellModel.Colspan = 1;
        table.AddCell(MakeCell("賬戶號碼\n", "A/C NO.", cellModel, 9f, 9f, 1.2f));
        cellModel.Colspan = 2;
        table.AddCell(MakeCell(bankAc ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));
        return table;
    }

    private static PdfPTable TotalAmount(string? dnNo, decimal? closeBal, string? propertyAc)
    {
        var table = new PdfPTable(10);
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;
        table.DefaultCell.Padding = 0f;
        table.SpacingAfter = 0f;
        table.SpacingBefore = 2f;
        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_BOTTOM };
        cellModel.Colspan = 3;
        table.AddCell(MakeCell("繳款通知書編號", cellModel));
        cellModel.Colspan = 3;
        table.AddCell(MakeCell("Demand Note No.", cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 1;
        table.AddCell(MakeCell(":", cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 3;
        cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
        cellModel.PaddingRight = 3f;
        table.AddCell(MakeCell(dnNo ?? "", cellModel, fontSize: 9f, baseFont: Helvetica));

        cellModel.Colspan = 3;
        cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
        table.AddCell(MakeCell("總額", cellModel));
        cellModel.Colspan = 3;
        table.AddCell(MakeCell("Total Amount", cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 1;
        table.AddCell(MakeCell(":", cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 3;
        cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
        table.AddCell(MakeCell(closeBal?.ToString("N") + "  ", cellModel, fontSize: 9f, baseFont: Helvetica));

        cellModel.Colspan = 3;
        cellModel.HorizontalAlignment = Element.ALIGN_LEFT;
        table.AddCell(MakeCell("物業單位號碼", cellModel));
        cellModel.Colspan = 3;
        table.AddCell(MakeCell("Property A/C Code", cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 1;
        table.AddCell(MakeCell(":", cellModel, fontSize: 9f, baseFont: Helvetica));
        cellModel.Colspan = 3;
        cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
        table.AddCell(MakeCell(propertyAc + "  ", cellModel, fontSize: 9f, baseFont: Helvetica));
        return table;
    }

    // ===================================================================
    // Cheque stub
    // ===================================================================
    private PdfPTable Cheque()
    {
        var table = new PdfPTable(20) { TotalWidth = 560f };
        table.LockedWidth = true;
        table.SpacingBefore = 0f;
        table.DefaultCell.Border = PdfRectangle.NO_BORDER;

        // Row 1
        var cellModel = new PdfPCell { BorderWidth = 1f, BorderColor = ForegroundColor };
        cellModel.Colspan = 5; cellModel.BorderWidthRight = 0f; cellModel.FixedHeight = 57f; cellModel.PaddingTop = 3f;
        table.AddCell(MakeCell("支票號碼 ", "Cheque No.", cellModel));
        cellModel.Colspan = 5; cellModel.BorderWidthLeft = 0f;
        table.AddCell(MakeCell("銀行 ", "Bank", cellModel));
        cellModel.Colspan = 4;
        table.AddCell(MakeCell("分行 ", "Branch", cellModel));
        cellModel.Colspan = 6; cellModel.Padding = 0f;
        table.AddCell(MakeCell("", cellModel, Cash()));

        // Row 2
        cellModel = new PdfPCell { BorderWidth = 1f, BorderWidthTop = 0f, BorderWidthBottom = 0f, BorderColor = ForegroundColor };
        cellModel.PaddingTop = 3f;
        cellModel.Colspan = 4;
        table.AddCell(MakeCell("櫃員 ", "Teller", cellModel));
        cellModel.Colspan = 4; cellModel.BorderWidthLeft = 0f;
        table.AddCell(MakeCell("主任 ", "Officer", cellModel));

        cellModel = new PdfPCell { BorderWidthTop = 0f, BorderWidthBottom = 0f, BorderColor = ForegroundColor };
        cellModel.Colspan = 8; cellModel.BorderWidthLeft = 0f; cellModel.VerticalAlignment = Element.ALIGN_BOTTOM;
        cellModel.HorizontalAlignment = Element.ALIGN_RIGHT;
        table.AddCell(MakeCell("共計 ", "Total HK$", cellModel));

        cellModel = new PdfPCell { BorderColor = ForegroundColor, BorderWidthTop = 0f, BorderWidthBottom = 0f, BorderWidthRight = 1f };
        cellModel.Colspan = 4; cellModel.Padding = 0f;
        table.AddCell(MakeCell("", cellModel, CashTotal()));

        // Bottom line
        cellModel = new PdfPCell { BorderWidth = 1f, BorderColor = ForegroundColor };
        cellModel.Colspan = 20; cellModel.FixedHeight = 7f;
        table.AddCell(MakeCell(" ", cellModel));

        return table;
    }

    private static PdfPTable Cash()
    {
        var table = new PdfPTable(9);
        table.DefaultCell.Padding = 0f;
        var cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, BorderWidthLeft = 1f, PaddingTop = 3f };
        table.AddCell(MakeCell(" 現金 Cash", cellModel));

        cellModel = new PdfPCell { Border = PdfRectangle.TOP_BORDER | PdfRectangle.BOTTOM_BORDER | PdfRectangle.LEFT_BORDER | PdfRectangle.RIGHT_BORDER, BorderColor = ForegroundColor };
        cellModel.Colspan = 3; cellModel.Padding = 0f;
        table.AddCell(MakeCell(" ", cellModel));

        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, BorderWidthRight = 1f, Padding = 0f };
        table.AddCell(MakeCell(" ", cellModel));

        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, BorderWidthLeft = 0f, BorderWidthBottom = 0f };
        table.AddCell(MakeCell(" ", cellModel));

        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, Padding = 0f };
        table.AddCell(MakeCell(" ", cellModel));
        table.AddCell(MakeCell(" ", cellModel));

        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, BorderWidthRight = 1f, Padding = 0f };
        table.AddCell(MakeCell(" ", cellModel));

        // Row 3
        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, BorderWidthTop = 0f, BorderWidthLeft = 0f, BorderWidthBottom = 0f };
        table.AddCell(MakeCell(" ", cellModel));
        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, Padding = 0f };
        table.AddCell(MakeCell(" ", cellModel));
        cellModel = new PdfPCell { BorderColor = ForegroundColor, Colspan = 3, BorderWidthRight = 1f, Padding = 0f };
        table.AddCell(MakeCell(" ", cellModel));

        return table;
    }

    private static PdfPTable CashTotal()
    {
        var table = new PdfPTable(6);
        table.DefaultCell.Padding = 0f;

        var cellModel = new PdfPCell { BorderWidthTop = 0f, BorderWidthBottom = 0f, BorderWidthLeft = 0f, BorderColor = ForegroundColor };
        cellModel.Colspan = 3; cellModel.Padding = 0f;
        table.AddCell(MakeCell(" ", cellModel));

        cellModel = new PdfPCell { BorderWidthTop = 0f, BorderWidthBottom = 0f, BorderWidthRight = 0f, BorderColor = ForegroundColor };
        cellModel.Colspan = 3; cellModel.Padding = 0f;
        table.AddCell(MakeCell(" ", cellModel));

        return table;
    }

    // ===================================================================
    // GenBatCode
    // ===================================================================
    private iTextSharp.text.Image GenBatCode(string code, float absoluteY = 58f)
    {
        var code128 = new Barcode128
        {
            CodeType = 9,
            ChecksumText = true,
            GenerateChecksum = true,
            StartStopText = true,
            Code = code,
            Font = Helvetica
        };

        using var bm = new Bitmap(code128.CreateDrawingImage(Color.Black, Color.White));
        using var ms = new MemoryStream();
        bm.Save(ms, ImageFormat.Gif);
        var bytes = ms.ToArray();

        var img = iTextSharp.text.Image.GetInstance(bytes);
        img.ScaleAbsoluteHeight(35f);
        img.ScaleAbsoluteWidth(270f);
        img.SetAbsolutePosition(230f, absoluteY);
        return img;
    }

    // ===================================================================
    // BatCodeTitle
    // ===================================================================
    private PdfPTable BatCodeTitle(string? title)
    {
        var table = new PdfPTable(1) { TotalWidth = 560f };
        table.LockedWidth = true;
        table.SpacingBefore = 44f;

        var cellModel = new PdfPCell { Border = PdfRectangle.NO_BORDER };
        cellModel.PaddingLeft = 256f;
        table.AddCell(MakeCell(title ?? "", cellModel, fontSize: 8f, baseFont: Helvetica, baseColor: BaseColor.BLACK));
        return table;
    }

    // ===================================================================
    // Watermark
    // ===================================================================
    private void AddTextWatermark(PdfWriter writer, string zhText, string enText)
    {
        var gs = new PdfGState { FillOpacity = 0.2f };
        var content = writer.DirectContentUnder;
        content.BeginText();
        content.SetFontAndSize(SimHei, 26f);
        content.SetGState(gs);
        content.ShowTextAligned(Element.ALIGN_LEFT, zhText, 160f, 500f, 0f);
        content.SetColorFill(new BaseColor(193, 193, 193));
        content.EndText();

        var enGs = new PdfGState { FillOpacity = 0.6f };
        content.BeginText();
        content.SetFontAndSize(Helvetica, 26f);
        content.SetGState(enGs);
        content.ShowTextAligned(Element.ALIGN_LEFT, enText, 120f, 470f, 0f);
        content.SetColorFill(new BaseColor(193, 193, 193));
        content.EndText();
    }

    // ===================================================================
    // Cell helpers
    // ===================================================================
    private static PdfPCell MakeCell(string zhValue, string enValue, PdfPCell cellModel, float zhFontSize = 9f, float enFontSize = 9f, float? multipliedLeading = null, BaseFont? baseFont = null)
    {
        var zhFont = new iTextSharp.text.Font(baseFont ?? SimHei, zhFontSize);
        var enFont = new iTextSharp.text.Font(baseFont ?? Helvetica, enFontSize);
        var cell = new PdfPCell();

        if (multipliedLeading.HasValue)
        {
            var zhPhrase = new Phrase();
            zhPhrase.Add(new Chunk(zhValue, zhFont));
            var zhPara = new Paragraph(zhPhrase);
            zhPara.SetLeading(0f, 1f);
            cell.AddElement(zhPara);

            var enPhrase = new Phrase();
            enPhrase.Add(new Chunk(enValue, enFont));
            var enPara = new Paragraph(enPhrase);
            enPara.SetLeading(0f, multipliedLeading.Value);
            cell.AddElement(enPara);
        }
        else
        {
            var phrase = new Phrase();
            phrase.Add(new Chunk(zhValue, zhFont));
            phrase.Add(new Chunk(enValue, enFont));
            cell = new PdfPCell(new Paragraph(phrase));
        }

        CopyCellStyle(cell, cellModel);
        return cell;
    }

    private static PdfPCell MakeCell(string value, PdfPCell cellModel, PdfPTable? table = null, float fontSize = 9f, BaseFont? baseFont = null, BaseColor? baseColor = null)
    {
        var fontColor = baseColor ?? ForegroundColor;
        var font = new iTextSharp.text.Font(baseFont ?? SimHei, fontSize, 0, fontColor);

        PdfPCell cell;
        if (table != null)
            cell = new PdfPCell(table);
        else
            cell = new PdfPCell(new Paragraph(value, font));

        CopyCellStyle(cell, cellModel);
        return cell;
    }

    private static void CopyCellStyle(PdfPCell target, PdfPCell source)
    {
        if (source == null) return;
        target.Border = source.Border;
        target.VerticalAlignment = source.VerticalAlignment;
        target.HorizontalAlignment = source.HorizontalAlignment;
        target.PaddingLeft = source.PaddingLeft;
        target.PaddingRight = source.PaddingRight;
        target.MinimumHeight = source.MinimumHeight;
        target.FixedHeight = source.FixedHeight;
        target.Colspan = source.Colspan;
        target.BorderWidth = source.BorderWidth;
        target.BorderWidthTop = source.BorderWidthTop;
        target.BorderWidthRight = source.BorderWidthRight;
        target.BorderWidthBottom = source.BorderWidthBottom;
        target.BorderWidthLeft = source.BorderWidthLeft;
        target.BorderColor = source.BorderColor;
        target.BorderColorTop = source.BorderColorTop;
        target.BorderColorBottom = source.BorderColorBottom;
        target.BackgroundColor = source.BackgroundColor;
    }

    // ===================================================================
    // String utilities (instance methods accessed via static context)
    // ===================================================================
    private static bool IsIncludeChar(string? value)
    {
        return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, "[\\u4e00-\\u9fa5]");
    }

    private static string TruncateString(string? value, int maxLen)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLen - 1)
            return value ?? "";

        int len = 0;
        string newVal = "";
        foreach (char item in value)
        {
            byte[] b = System.Text.Encoding.Default.GetBytes(item.ToString());
            len += b.Length <= 1 ? 1 : 2;
            if (len > maxLen && newVal == "")
            {
                newVal = value[..value.IndexOf(item)];
                break;
            }
        }
        return newVal != "" ? newVal : value;
    }
}
