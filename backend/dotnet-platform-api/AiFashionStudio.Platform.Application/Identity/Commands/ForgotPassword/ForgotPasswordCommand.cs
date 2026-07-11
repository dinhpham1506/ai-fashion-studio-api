using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Identity.Commands.ForgotPassword
{
    public record  ForgotPasswordCommand(string Email) : IRequest;

}
