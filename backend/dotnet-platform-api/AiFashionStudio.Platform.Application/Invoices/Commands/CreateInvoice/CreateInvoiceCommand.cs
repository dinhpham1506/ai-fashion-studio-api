using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Commands.CreateInvoice
{
    /// <summary>
    ///  Trả về InvoiceId nếu tạo thành công
    /// </summary>
    public record CreateInvoiceCommand(Guid OrderId, Guid PaymentId, Guid CustomerId, string Description, decimal Amount) : IRequest<Guid?>
    {
    }
}
