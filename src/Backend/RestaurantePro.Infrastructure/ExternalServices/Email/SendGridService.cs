/*
 * IMPLEMENTACIÓN FUTURA - NO IMPLEMENTADO ACTUALMENTE
 * 
 * Este servicio requiere el paquete NuGet SendGrid y se implementará en el futuro 
 * cuando sea necesario. Se deja el código comentado como referencia.
 */

/*
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.ExternalServices.Email.Interfaces;
using RestaurantePro.Infrastructure.ExternalServices.Email.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RestaurantePro.Infrastructure.ExternalServices.Email
{
    /// <summary>
    /// Implementación del servicio de correo electrónico utilizando SendGrid
    /// </summary>
    public class SendGridService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SendGridService> _logger;

        public SendGridService(
            IOptions<EmailSettings> emailSettings,
            ILogger<SendGridService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string contenido, bool esHtml = false)
        {
            try
            {
                var cliente = new SendGridClient(_emailSettings.SendGridApiKey);
                var from = new EmailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                var to = new EmailAddress(destinatario);
                
                var mensaje = MailHelper.CreateSingleEmail(
                    from, 
                    to, 
                    asunto, 
                    esHtml ? null : contenido, // Contenido de texto plano
                    esHtml ? contenido : null  // Contenido HTML
                );
                
                var respuesta = await cliente.SendEmailAsync(mensaje);
                
                if (respuesta.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Correo enviado a {Destinatario} con asunto {Asunto} usando SendGrid", 
                        destinatario, asunto);
                    return true;
                }
                
                _logger.LogWarning("Error al enviar correo con SendGrid: Código {StatusCode}", respuesta.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Destinatario} con asunto {Asunto} usando SendGrid", 
                    destinatario, asunto);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> EnviarEmailConAdjuntosAsync(string destinatario, string asunto, string contenido, 
            string[] archivosAdjuntos, bool esHtml = false)
        {
            try
            {
                var cliente = new SendGridClient(_emailSettings.SendGridApiKey);
                var from = new EmailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                var to = new EmailAddress(destinatario);
                
                var mensaje = MailHelper.CreateSingleEmail(
                    from, 
                    to, 
                    asunto, 
                    esHtml ? null : contenido,
                    esHtml ? contenido : null
                );
                
                // Agregar archivos adjuntos
                foreach (var archivo in archivosAdjuntos)
                {
                    if (File.Exists(archivo))
                    {
                        var bytes = await File.ReadAllBytesAsync(archivo);
                        var base64Content = Convert.ToBase64String(bytes);
                        var adjunto = new Attachment
                        {
                            Content = base64Content,
                            Filename = Path.GetFileName(archivo),
                            Type = ObtenerContentType(archivo),
                            Disposition = "attachment"
                        };
                        
                        mensaje.AddAttachment(adjunto);
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró el archivo adjunto: {Archivo}", archivo);
                    }
                }
                
                var respuesta = await cliente.SendEmailAsync(mensaje);
                
                if (respuesta.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Correo con {NumArchivos} adjuntos enviado a {Destinatario} usando SendGrid", 
                        archivosAdjuntos.Length, destinatario);
                    return true;
                }
                
                _logger.LogWarning("Error al enviar correo con adjuntos con SendGrid: Código {StatusCode}", respuesta.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo con adjuntos a {Destinatario} usando SendGrid", 
                    destinatario);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> EnviarEmailMasivosAsync(string[] destinatarios, string asunto, string contenido, bool esHtml = false)
        {
            try
            {
                if (destinatarios.Length == 0)
                {
                    _logger.LogWarning("No hay destinatarios para el envío de correo masivo");
                    return false;
                }
                
                var cliente = new SendGridClient(_emailSettings.SendGridApiKey);
                var from = new EmailAddress(_emailSettings.DireccionRemitente, _emailSettings.NombreRemitente);
                
                // SendGrid tiene límites para la cantidad de destinatarios por envío,
                // así que procesamos en lotes de 1000 destinatarios
                const int tamañoLote = 1000;
                var lotes = (int)Math.Ceiling(destinatarios.Length / (double)tamañoLote);
                
                for (int i = 0; i < lotes; i++)
                {
                    var loteDestinatarios = destinatarios
                        .Skip(i * tamañoLote)
                        .Take(tamañoLote)
                        .Select(email => new EmailAddress(email))
                        .ToList();
                    
                    var personalizations = loteDestinatarios.Select(email => 
                        new Personalization 
                        { 
                            Tos = new List<EmailAddress> { email } 
                        }).ToList();
                    
                    var mensaje = new SendGridMessage
                    {
                        From = from,
                        Subject = asunto,
                        PlainTextContent = esHtml ? null : contenido,
                        HtmlContent = esHtml ? contenido : null,
                        Personalizations = personalizations
                    };
                    
                    var respuesta = await cliente.SendEmailAsync(mensaje);
                    
                    if (!respuesta.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Error al enviar lote {NumeroLote}/{TotalLotes} de correo masivo: Código {StatusCode}", 
                            i + 1, lotes, respuesta.StatusCode);
                        return false;
                    }
                }
                
                _logger.LogInformation("Correo masivo enviado a {NumDestinatarios} destinatarios usando SendGrid", 
                    destinatarios.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo masivo a {NumDestinatarios} destinatarios usando SendGrid", 
                    destinatarios.Length);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> EnviarEmailConPlantillaAsync(string destinatario, string asunto, string nombrePlantilla, 
            Dictionary<string, string> datos)
        {
            try
            {
                // Cargar plantilla
                var rutaPlantilla = Path.Combine(_emailSettings.DirectorioPlantillas, $"{nombrePlantilla}.html");
                if (!File.Exists(rutaPlantilla))
                {
                    _logger.LogError("No se encontró la plantilla: {RutaPlantilla}", rutaPlantilla);
                    return false;
                }
                
                var contenidoPlantilla = await File.ReadAllTextAsync(rutaPlantilla);
                
                // Reemplazar tokens en la plantilla
                foreach (var kvp in datos)
                {
                    contenidoPlantilla = contenidoPlantilla.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                
                // Enviar email con la plantilla
                return await EnviarEmailAsync(destinatario, asunto, contenidoPlantilla, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo con plantilla {NombrePlantilla} a {Destinatario} usando SendGrid", 
                    nombrePlantilla, destinatario);
                return false;
            }
        }
        
        private string ObtenerContentType(string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo).ToLower();
            
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }
}
*/ 