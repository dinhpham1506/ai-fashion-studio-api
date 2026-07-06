using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Identity.Commands.VerifyResetOtp
{
    /// <summary>
    /// xử lý xác thực OTP để đặt lại mật khẩu. Nó kiểm tra tính hợp lệ của OTP, thời gian hết hạn và tạo một mã thông báo đặt lại nếu OTP hợp lệ.
    /// </summary>
    public class VerifyResetOtpCommandHandler : IRequestHandler<VerifyResetOtpCommand, VerifyResetOtpRespone>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetByOtpRepository _passwordResetByOtpRepository;
        private readonly IOtpGeneratorService _otpGenerator;
        
        public VerifyResetOtpCommandHandler(IUserRepository userRepository, IPasswordResetByOtpRepository passwordResetByOtpRepository, IOtpGeneratorService otpGenerator)
        {
            _userRepository = userRepository;
            _passwordResetByOtpRepository = passwordResetByOtpRepository;
            _otpGenerator = otpGenerator;
        }
        public async Task<VerifyResetOtpRespone> Handle(VerifyResetOtpCommand command, CancellationToken cancellationToken)
        {
            // Lấy người dùng theo email
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            // Nếu người dùng không tồn tại, ném ra ngoại lệ UnauthorizedAccessException
            var record = await _passwordResetByOtpRepository.GetLatestByUserIdAsync(user.Id, cancellationToken);

            // Nếu không tìm thấy bản ghi OTP, ném ra ngoại lệ UnauthorizedAccessException
            if (record.isResetTokenExpired())
            {
                throw new UnauthorizedAccessException("OTP has expired.");
            }

            if (!record.isOtpValid())
            {
                throw new UnauthorizedAccessException("Invalid OTP.");
            }

            // Kiểm tra xem OTP có hợp lệ không
            if (record.OtpHash != _otpGenerator.Hash(command.Otp))
            {
                // Nếu OTP không hợp lệ, tăng số lần thử và kiểm tra xem người dùng có bị khóa không
                var lockedOut = record.FailOtpAttempt();
                await _passwordResetByOtpRepository.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException("Invalid OTP.");
            }

            // Nếu OTP hợp lệ, tạo một mã thông báo đặt lại và lưu vào cơ sở dữ liệu
            var resetToken = _otpGenerator.GenerateResetToken();
            // Lưu mã thông báo đặt lại đã hash vào cơ sở dữ liệu với thời gian sống là 10 phút
            record.AttachResetToken(_otpGenerator.Hash(resetToken), TimeSpan.FromMinutes(10));
            await _passwordResetByOtpRepository.SaveChangesAsync(cancellationToken);

            // Trả về mã thông báo đặt lại và thời gian sống 
            return new VerifyResetOtpRespone(resetToken, 600);
        }
    }
}
