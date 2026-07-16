using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceItems
{
    public record GetInvoiceItemsQuery(Guid UserId, bool IsStaffOrAdmin, Guid InvoiceId) : IRequest<InvoiceItemsResponse>;
}
