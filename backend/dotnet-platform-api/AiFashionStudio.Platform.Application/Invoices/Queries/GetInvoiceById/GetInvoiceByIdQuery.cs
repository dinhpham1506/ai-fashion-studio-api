using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceById
{
    public record GetInvoiceByIdQuery(Guid UserId, bool IsStaffOrAdmin, Guid InvoiceId) : IRequest<InvoiceResponse>;
}
