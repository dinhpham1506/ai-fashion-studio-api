using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AiFashionStudio.Platform.Infrastructure.Identity
{
    /// <summary>
    /// Service gửi email sử dụng SMTP
    /// </summary>
    public class EmailSenderService : IEmailSenderService
    {
        private readonly SmtpSettings _settings;

        public EmailSenderService(IOptions<SmtpSettings> options) => _settings = options.Value;
        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }


    }
}
