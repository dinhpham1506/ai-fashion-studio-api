using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices
{
    /// <summary>
    /// Interface của dịch vụ gửi email
    /// </summary>
    public interface IEmailSenderService
    {
        
        Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
