using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Identity.Commands.VerifyResetOtp
{
    public record VerifyResetOtpCommand(string Email,
        string Otp
    ) : IRequest<VerifyResetOtpRespone>;
}
