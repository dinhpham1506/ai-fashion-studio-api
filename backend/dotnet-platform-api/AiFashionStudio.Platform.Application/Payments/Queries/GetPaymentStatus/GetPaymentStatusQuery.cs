using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Payments.Queries.GetPaymentStatus
{
    public record GetPaymentStatusQuery(Guid UserId, Guid? PaymentId = null, Guid? OrderId = null) : IRequest<PaymentStatusResponse>;
}
