using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices
{
    /// <summary>
    /// Interface của dịch vụ tạo OTP và reset token
    /// </summary>
    public interface IOtpGeneratorService
    {
        // tạo một OTP ngẫu nhiên với độ dài mặc định là 6 ký tự
        string GenerateOtp();
        // tạo một reset token ngẫu nhiên 
        string GenerateResetToken();
        // băm để lưu vào cơ sở dữ liệu
        string Hash(string value);
    }
}
