using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Dtos
{
    // xác thực mã OTP và trả về token để reset password
    public record VerifyResetOtpRespone (
        string ResetToken,
        int ExpiresIn
    );
}
