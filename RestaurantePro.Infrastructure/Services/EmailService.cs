using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Models;

namespace RestaurantePro.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendEmailWithTemplateAsync(string to, string templateName, object model)
        {
            // Aquí implementarías la lógica para cargar y renderizar la plantilla
            var body = $"Template: {templateName}, Model: {model}"; // Simplificado para el ejemplo
            await SendEmailAsync(to, $"Notificación - {templateName}", body);
        }
    }
} 