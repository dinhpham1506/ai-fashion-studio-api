using AiFashionStudio.Platform.Application.Common.Emails;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Identities.Commands.ForgotPassword
{

    /// <summary>
    ///  Xử lý yêu cầu quên mật khẩu bằng cách tạo mã OTP và gửi email cho người dùng.
    /// </summary>
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetByOtpRepository _passwordResetByOtpRepository;
        private readonly IOtpGeneratorService _otpGenerator;
        private readonly IEmailSenderService _emailService;

        public ForgotPasswordCommandHandler(IUserRepository userRepository, IPasswordResetByOtpRepository passwordResetByOtpRepository, IOtpGeneratorService otpGenerator, IEmailSenderService emailService)
        {
            _userRepository = userRepository;
            _passwordResetByOtpRepository = passwordResetByOtpRepository;
            _otpGenerator = otpGenerator;
            _emailService = emailService;
        }
        public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            if (user == null)
            {
                return; // Không tiết lộ thông tin về sự tồn tại của người dùng để bảo mật
            }

            // Kiểm tra xem người dùng có yêu cầu OTP quá thường xuyên không
            var userLatest = await _passwordResetByOtpRepository.GetLatestByUserIdAsync(user.Id, cancellationToken);

            // Nếu người dùng đã yêu cầu OTP trong vòng 60 giây qua, ném ra ngoại lệ
            if (userLatest != null && DateTime.UtcNow - userLatest.CreatedAt < TimeSpan.FromSeconds(60))
            {
                throw new ConflictException("OTP_REQUEST_TOO_FREQUENT", "Please wait before requesting a new OTP");
            }

            // Thu hồi tất cả các OTP đang hoạt động của người dùng trước khi tạo OTP mới
            await _passwordResetByOtpRepository.RevokeAllActiveByUserIdAsync(user.Id, cancellationToken);
            
            var otp = _otpGenerator.GenerateOtp();

            var otpLifetime = TimeSpan.FromMinutes(5);

            var record = PasswordResetByOtp.Create(user.Id, _otpGenerator.Hash(otp), otpLifetime);

            await _passwordResetByOtpRepository.AddAsync(record, cancellationToken);

            await _emailService.SendEmailAsync(
                user.Email,
                "Your password reset code — Fitwear Studio",
                EmailTemplates.PasswordResetOtp(otp, (int)otpLifetime.TotalMinutes),
                cancellationToken);
        }
    }
}
