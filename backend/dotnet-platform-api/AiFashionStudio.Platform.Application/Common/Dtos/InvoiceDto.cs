using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Dtos
{
    public record InvoiceResponse(
    Guid InvoiceId, Guid OrderId, string InvoiceNumber,
    decimal TotalAmount, string Currency, string Status,
    string? PdfUrl, DateTime IssuedAt);

    public record InvoiceItemResponse(
        string ProductNameSnapshot, string? VariantSnapshot,
        int Quantity, decimal UnitPrice, decimal TotalPrice);

    public record InvoiceItemsResponse(IReadOnlyCollection<InvoiceItemResponse> Items);

    public record InvoicePdfResponse(string InvoiceNumber, string PdfUrl);
}
