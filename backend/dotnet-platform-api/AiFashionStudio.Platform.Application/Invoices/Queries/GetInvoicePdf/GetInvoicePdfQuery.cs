using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoicePdf
{
    public record GetInvoicePdfQuery(Guid UserId, bool IsStaffOrAdmin, Guid InvoiceId) : IRequest<InvoicePdfResponse>;
}
