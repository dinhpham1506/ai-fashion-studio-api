using AiFashionStudio.Platform.Domain.Invoice.Entities;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

public interface IInvoicePdfGenerator
{
    // Render invoice thành PDF bytes. Không I/O mạng, chỉ CPU.
    byte[] Generate(Invoice invoice);
}
