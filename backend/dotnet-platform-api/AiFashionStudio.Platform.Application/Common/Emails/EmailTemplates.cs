using System;

namespace AiFashionStudio.Platform.Application.Common.Emails
{
    /// <summary>
    /// Template HTML cho email hệ thống, theo design system của FE Fitwear Studio
    /// (nền beige #fdfcfa, card trắng bo góc, accent tím #7c3aed, heading serif).
    /// Email client không load webfont/stylesheet nên toàn bộ style phải inline
    /// và layout dùng table; Georgia thay cho Playfair Display, Arial thay cho Inter.
    /// </summary>
    public static class EmailTemplates
    {
        public static string PasswordResetOtp(string otp, int expiresInMinutes)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Password reset code</title>
</head>
<body style=""margin:0;padding:0;background-color:#fdfcfa;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#fdfcfa;padding:40px 16px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:520px;"">

          <!-- Brand -->
          <tr>
            <td align=""center"" style=""padding-bottom:24px;font-family:Georgia,'Times New Roman',serif;font-size:22px;font-weight:600;color:#050505;"">
              Fitwear <span style=""color:#8b5cf6;"">Studio</span>
            </td>
          </tr>

          <!-- Card -->
          <tr>
            <td style=""background-color:#ffffff;border:1px solid #efefef;border-radius:24px;padding:40px 36px;"">
              <p style=""margin:0;font-family:'Segoe UI',Arial,sans-serif;font-size:13px;font-weight:600;letter-spacing:0.5px;color:#7c3aed;"">
                PASSWORD RESET
              </p>
              <h1 style=""margin:12px 0 0;font-family:Georgia,'Times New Roman',serif;font-size:30px;line-height:1.2;font-weight:600;color:#050505;"">
                Your one-time code
              </h1>
              <p style=""margin:16px 0 0;font-family:'Segoe UI',Arial,sans-serif;font-size:14px;line-height:1.75;color:#595959;"">
                Use the code below to reset the password for your Fitwear Studio account.
                This code expires in <strong style=""color:#262626;"">{expiresInMinutes} minutes</strong>.
              </p>

              <!-- OTP box -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:28px;"">
                <tr>
                  <td align=""center"" style=""background-color:#f5f3ff;border:1px solid #ddd6fe;border-radius:16px;padding:24px;"">
                    <span style=""font-family:'Segoe UI',Arial,sans-serif;font-size:34px;font-weight:700;letter-spacing:10px;color:#050505;"">{otp}</span>
                  </td>
                </tr>
              </table>

              <p style=""margin:28px 0 0;font-family:'Segoe UI',Arial,sans-serif;font-size:13px;line-height:1.7;color:#8c8c8c;"">
                If you didn't request a password reset, you can safely ignore this email —
                your password will stay unchanged.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td align=""center"" style=""padding-top:24px;font-family:'Segoe UI',Arial,sans-serif;font-size:12px;color:#bfbfbf;"">
              &copy; {DateTime.UtcNow.Year} Fitwear Studio. All rights reserved.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
