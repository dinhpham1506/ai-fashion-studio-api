using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Identity.Commands.VerifyResetOtp
{
    public class VerifyResetOtpCommandValidator : AbstractValidator<VerifyResetOtpCommand>
    {
        public VerifyResetOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .Length(6).WithMessage("OTP must be 6 digits.");
        }
    }
}
