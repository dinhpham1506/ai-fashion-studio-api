using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Identity
{
    /// <summary>
    /// Service xử lý OTP 
    /// </summary>
    public class OtpGeneratorService : IOtpGeneratorService
    {
        public string GenerateOtp()
         => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        public string GenerateResetToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public string Hash(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
