using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Payments.Commands.ProcessPaymentWebhook
{
    public record ProcessPaymentWebhookCommand(string RawBody) : IRequest;
}
