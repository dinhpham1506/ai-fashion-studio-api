using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Invoice.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AiFashionStudio.Platform.Infrastructure.Pdf
{
    public sealed class QuestPdfInvoiceGenerator : IInvoicePdfGenerator
    {
        static QuestPdfInvoiceGenerator() => QuestPDF.Settings.License = LicenseType.Community;

        public byte[] Generate(Invoice invoice)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Text($"HÓA ĐƠN {invoice.InvoiceNumber}").Bold().FontSize(20);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Ngày phát hành: {invoice.IssuedAt:dd/MM/yyyy HH:mm} UTC");
                        col.Item().Text($"Mã đơn hàng: {invoice.OrderId}");
                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Text("Sản phẩm").Bold();
                                h.Cell().Text("SL").Bold();
                                h.Cell().Text("Đơn giá").Bold();
                                h.Cell().Text("Thành tiền").Bold();
                            });
                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Text(item.ProductNameSnapshot);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.UnitPrice:N0}");
                                table.Cell().Text($"{item.TotalPrice:N0}");
                            }
                        });
                        col.Item().PaddingTop(15).AlignRight()
                            .Text($"TỔNG CỘNG: {invoice.TotalAmount:N0} {invoice.Currency}")
                            .Bold().FontSize(14);
                    });
                });
            }).GeneratePdf();
        }
    }
}
