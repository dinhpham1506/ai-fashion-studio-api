using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Payments.Commands
{
    public record CreatePaymentCommand(Guid UserId, int Amount, string Description) : IRequest<CreatePaymentLinkResponse>;
}
