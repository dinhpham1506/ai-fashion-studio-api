using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Identity.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IPasswordResetByOtpRepository _passwordResetByOtpRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IOtpGeneratorService _otpGeneratorService;

        public ResetPasswordCommandHandler(IPasswordResetByOtpRepository passwordResetByOtpRepository,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IOtpGeneratorService otpGeneratorService)
        {
            _passwordResetByOtpRepository = passwordResetByOtpRepository;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _otpGeneratorService = otpGeneratorService;
        }

        public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            // Kiểm tra tính hợp lệ của token bằng cách hash token và tìm kiếm trong cơ sở dữ liệu
            var tokenHash = _otpGeneratorService.Hash(command.ResetToken);

            // Lấy bản ghi PasswordResetByOtp từ cơ sở dữ liệu dựa trên token hash
            var resetRecord = await _passwordResetByOtpRepository.GetByResetTokenHashAsync(tokenHash, cancellationToken);

            // Kiểm tra xem bản ghi có tồn tại và token có hợp lệ hay không
            if (resetRecord == null || !resetRecord.isResetTokenValid())
            {
                throw new UnauthorizedException("INVALID_RESET_TOKEN", "Invalid or expired reset token");
            }
            // Lấy người dùng liên quan đến bản ghi PasswordResetByOtp
            var user = await _userRepository.GetByIdWithRolesAsync(resetRecord.UserId, cancellationToken);

            // Thay đổi mật khẩu của người dùng bằng mật khẩu mới đã hash
            user.ChangePassword(_passwordHasher.Hash(command.NewPassword));
            await _userRepository.UpdateAsync(user, cancellationToken);
            // Thu hồi tất cả các OTP đang hoạt động của người dùng sau khi thay đổi mật khẩu thành công
            await _passwordResetByOtpRepository.RevokeAllActiveByUserIdAsync(user.Id, cancellationToken);
            // Đánh dấu bản ghi PasswordResetByOtp là đã sử dụng để tránh sử dụng lại
            resetRecord.MarkAsUsed();
            await _passwordResetByOtpRepository.SaveChangesAsync(cancellationToken);
        }

    }
}
