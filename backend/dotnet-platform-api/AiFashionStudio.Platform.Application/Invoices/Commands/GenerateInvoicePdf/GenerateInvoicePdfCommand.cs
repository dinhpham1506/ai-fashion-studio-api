using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Invoices.Commands.GenerateInvoicePdf
{
    public record GenerateInvoicePdfCommand(Guid InvoiceId) : IRequest;
}
