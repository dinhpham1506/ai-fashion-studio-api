using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceByOrder
{
    public record GetInvoiceByOrderQuery(Guid UserId, bool IsStaffOrAdmin, Guid OrderId) : IRequest<InvoiceResponse>;
}
