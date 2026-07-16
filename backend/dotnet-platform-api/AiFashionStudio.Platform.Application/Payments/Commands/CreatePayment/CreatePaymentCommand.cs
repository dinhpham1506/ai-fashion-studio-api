using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Payments.Commands
{
    /// <param name="OrderId">ID đơn hàng bên Java Order Service (optional) — dùng để correlate trong event PaymentSucceeded.</param>
    public record CreatePaymentCommand(Guid UserId, int Amount, string Description, Guid? OrderId = null) : IRequest<CreatePaymentLinkResponse>;
}
