using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;

namespace RBS.Infrastructure.PdfGeneration;

/// <summary>
/// 账单 PDF 生成器 — 使用 QuestPDF 渲染欠款通知单
/// </summary>
/// <remarks>
/// 功能说明：
/// <list type="bullet">
///   <item><description>静态构造函数设置 QuestPDF 为 Community 许可证（免费使用）</description></item>
///   <item><description>Generate 方法生成 A4 版式的 PDF 字节流</description></item>
///   <item><description>PDF 内容包含：公司名称、单号、合同/租客/账期信息、费用明细表格、合计金额（数字+大写）、备注说明</description></item>
///   <item><description>草稿状态的账单标题显示"欠款通知单（草稿）"</description></item>
///   <item><description>使用 SimSun（宋体）作为默认字体，适合中文显示</description></item>
///   <item><description>页脚显示"第 X 页 / 共 Y 页"</description></item>
/// </list>
/// 设计模式：Strategy Pattern — PDF 生成策略的具体实现。
/// 依赖 QuestPDF 开源库。
/// </remarks>
public class BillPdfGenerator : IBillPdfGenerator
{
    /// <summary>
    /// 静态构造函数 — 设置 QuestPDF Community 许可证
    /// </summary>
    static BillPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// 生成欠款通知单 PDF 字节流
    /// </summary>
    /// <param name="note">欠款通知单实体</param>
    /// <param name="items">费用明细列表（费用名称 + 金额）</param>
    /// <param name="contractNo">合同编号</param>
    /// <param name="tenantName">租客姓名</param>
    /// <param name="companyName">公司名称</param>
    /// <returns>PDF 文件的字节数组</returns>
    public byte[] Generate(DebitNote note, IReadOnlyList<(string FeeName, decimal Amount)> items,
        string contractNo, string tenantName, string? companyName)
    {
        var title = note.Status == "Draft" ? "欠款通知单（草稿）" : "欠款通知单";
        var amountCn = ChineseAmountHelper.Convert(note.TotalAmount);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("SimSun"));

                page.Header().Element(c => ComposeHeader(c, title, note.NoteNo, companyName));
                page.Content().Element(c => ComposeContent(c, note, items, contractNo, tenantName, amountCn));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string title, string noteNo, string? companyName)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(5).Text(companyName ?? "").FontSize(11).Bold();
            col.Item().PaddingBottom(20).AlignCenter()
                .Text(title).FontSize(18).Bold();

            col.Item().AlignRight().Text($"单号：{noteNo}").FontSize(9).FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container, DebitNote note,
        IReadOnlyList<(string FeeName, decimal Amount)> items,
        string contractNo, string tenantName, string amountCn)
    {
        container.Column(col =>
        {
            // 基本信息
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"合同编号：{contractNo}");
                row.RelativeItem().Text($"租客姓名：{tenantName}");
                row.RelativeItem().Text($"账期：{note.Period}");
                row.RelativeItem().Text($"日期：{note.CreatedAt:yyyy-MM-dd}");
            });

            col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // 费用明细表头
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(60);  // 序号
                    c.RelativeColumn();     // 费用项目
                    c.ConstantColumn(120);  // 金额
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("序号").FontSize(10).Bold();
                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("费用项目").FontSize(10).Bold();
                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("金额（元）").FontSize(10).Bold();
                });

                int idx = 1;
                foreach (var item in items)
                {
                    table.Cell().Border(1).AlignCenter().Text(idx++.ToString());
                    table.Cell().Border(1).PaddingLeft(8).Text(item.FeeName);
                    table.Cell().Border(1).AlignRight().PaddingRight(8).Text($"{item.Amount:N2}");
                }
            });

            // 合计
            col.Item().PaddingTop(10).AlignRight().Text($"合计金额：{note.TotalAmount:N2} 元").FontSize(12).Bold();
            col.Item().PaddingTop(4).AlignRight().Text($"大写：{amountCn}").FontSize(10);

            // 备注
            col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(10).Text("备注：").FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().Text("1. 请在收到本通知后 7 日内完成付款。").FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().Text("2. 逾期未付将按合同约定收取滞纳金。").FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().Text("3. 如有疑问请联系运营人员。").FontSize(9).FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("第 ").FontSize(8).FontColor(Colors.Grey.Darken1);
            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
            x.Span(" 页 / 共 ").FontSize(8).FontColor(Colors.Grey.Darken1);
            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
            x.Span(" 页").FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }
}
