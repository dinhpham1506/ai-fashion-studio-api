using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Identity
{
    /// <summary>
    /// lớp cấu hình smtp
    /// </summary>
    public class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
