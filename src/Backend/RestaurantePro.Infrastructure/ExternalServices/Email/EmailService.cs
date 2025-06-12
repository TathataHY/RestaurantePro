using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Envía un correo electrónico simple
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
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
                    IsBodyHtml = false
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email enviado a {To} con asunto {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {To} con asunto {Subject}", to, subject);
                return false;
            }
        }

        /// <summary>
        /// Envía un correo electrónico HTML
        /// </summary>
        public async Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                var message = new MailMessage(_emailSettings.FromEmail, to, subject, htmlBody)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email HTML enviado a {To} con asunto {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email HTML a {To} con asunto {Subject}", to, subject);
                return false;
            }
        }

        /// <summary>
        /// Envía un correo electrónico con archivo adjunto
        /// </summary>
        public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
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
                    IsBodyHtml = false
                };

                // Agregar adjunto
                message.Attachments.Add(new Attachment(attachmentPath));

                await client.SendMailAsync(message);
                _logger.LogInformation("Email con adjunto enviado a {To} con asunto {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email con adjunto a {To} con asunto {Subject}", to, subject);
                return false;
            }
        }

        /// <summary>
        /// Envía un correo electrónico a múltiples destinatarios
        /// </summary>
        public async Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body)
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
                    IsBodyHtml = false
                };

                // Agregar destinatarios
                foreach (var recipient in toList)
                {
                    message.Bcc.Add(recipient);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Email masivo enviado a {Count} destinatarios con asunto {Subject}", toList.Count, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email masivo a {Count} destinatarios con asunto {Subject}", toList.Count, subject);
                return false;
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