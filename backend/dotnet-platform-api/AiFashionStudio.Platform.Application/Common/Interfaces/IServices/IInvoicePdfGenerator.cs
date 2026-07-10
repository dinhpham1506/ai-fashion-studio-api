using AiFashionStudio.Platform.Domain.Invoice.Entities;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

public interface IInvoicePdfGenerator
{
    /// <summary>
/// Renders an invoice as PDF bytes.
/// </summary>
/// <param name="invoice">The invoice to render.</param>
/// <returns>The rendered PDF content as a byte array.</returns>
    byte[] Generate(Invoice invoice);
}
