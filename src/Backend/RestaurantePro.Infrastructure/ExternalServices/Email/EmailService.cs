using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.ExternalServices.Email.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.ExternalServices.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var mensaje = new MailMessage();
                using var cliente = CrearClienteSMTP();
                
                mensaje.From = new MailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                mensaje.To.Add(to);
                mensaje.Subject = subject;
                mensaje.Body = body;
                mensaje.IsBodyHtml = false;
                
                await cliente.SendMailAsync(mensaje);
                _logger.LogInformation("Correo enviado a {Destinatario} con asunto {Asunto}", to, subject);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Destinatario} con asunto {Asunto}", to, subject);
                return false;
            }
        }
        
        public async Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                using var mensaje = new MailMessage();
                using var cliente = CrearClienteSMTP();
                
                mensaje.From = new MailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                mensaje.To.Add(to);
                mensaje.Subject = subject;
                mensaje.Body = htmlBody;
                mensaje.IsBodyHtml = true;
                
                await cliente.SendMailAsync(mensaje);
                _logger.LogInformation("Correo HTML enviado a {Destinatario} con asunto {Asunto}", to, subject);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo HTML a {Destinatario} con asunto {Asunto}", to, subject);
                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
        {
            try
            {
                using var mensaje = new MailMessage();
                using var cliente = CrearClienteSMTP();
                
                mensaje.From = new MailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                mensaje.To.Add(to);
                mensaje.Subject = subject;
                mensaje.Body = body;
                mensaje.IsBodyHtml = false;
                
                // Agregar archivo adjunto
                if (File.Exists(attachmentPath))
                {
                    mensaje.Attachments.Add(new Attachment(attachmentPath));
                }
                else
                {
                    _logger.LogWarning("No se encontró el archivo adjunto: {Archivo}", attachmentPath);
                    return false;
                }
                
                await cliente.SendMailAsync(mensaje);
                _logger.LogInformation("Correo con adjunto enviado a {Destinatario}", to);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo con adjunto a {Destinatario}", to);
                return false;
            }
        }

        public async Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body)
        {
            try
            {
                if (toList.Count == 0)
                {
                    _logger.LogWarning("No hay destinatarios para el envío de correo masivo");
                    return false;
                }
                
                using var mensaje = new MailMessage();
                using var cliente = CrearClienteSMTP();
                
                mensaje.From = new MailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                
                // Usar BCC para envíos masivos
                foreach (var destinatario in toList)
                {
                    mensaje.Bcc.Add(destinatario);
                }
                
                mensaje.Subject = subject;
                mensaje.Body = body;
                mensaje.IsBodyHtml = false;
                
                await cliente.SendMailAsync(mensaje);
                _logger.LogInformation("Correo masivo enviado a {NumDestinatarios} destinatarios", toList.Count);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo masivo a {NumDestinatarios} destinatarios", toList.Count);
                return false;
            }
        }
        
        protected virtual SmtpClient CrearClienteSMTP()
        {
            var cliente = new SmtpClient(_emailSettings.ServidorSMTP, _emailSettings.PuertoSMTP)
            {
                EnableSsl = _emailSettings.UsarSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Usuario, _emailSettings.Password)
            };
            
            return cliente;
        }
    }
} 