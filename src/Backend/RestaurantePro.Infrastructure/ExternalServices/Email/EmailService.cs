using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.ExternalServices.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<Result> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                var message = new MailMessage(_emailSettings.FromEmail, to, subject, body)
                {
                    IsBodyHtml = isHtml
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email enviado a {To} con asunto {Subject}", to, subject);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {To} con asunto {Subject}", to, subject);
                return Result.Failure(new[] { $"Error al enviar email: {ex.Message}" });
            }
        }

        public async Task<Result> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                var message = new MailMessage(_emailSettings.FromEmail, to, subject, body)
                {
                    IsBodyHtml = isHtml
                };

                // Agregar adjunto
                message.Attachments.Add(new Attachment(attachmentPath));

                await client.SendMailAsync(message);
                _logger.LogInformation("Email con adjunto enviado a {To} con asunto {Subject}", to, subject);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email con adjunto a {To} con asunto {Subject}", to, subject);
                return Result.Failure(new[] { $"Error al enviar email con adjunto: {ex.Message}" });
            }
        }

        public async Task<Result> SendBulkEmailAsync(string[] to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                // Agregar destinatarios
                foreach (var recipient in to)
                {
                    message.Bcc.Add(recipient);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Email masivo enviado a {Count} destinatarios con asunto {Subject}", to.Length, subject);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email masivo a {Count} destinatarios con asunto {Subject}", to.Length, subject);
                return Result.Failure(new[] { $"Error al enviar email masivo: {ex.Message}" });
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public bool EnableSsl { get; set; }
    }
} 