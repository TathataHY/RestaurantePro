using RestaurantePro.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Implementación temporal del servicio de email
/// TODO: Implementar con proveedor real (SendGrid, Azure Communication Services, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("📧 [TEMP] Enviando email a {To} con asunto: {Subject}", to, subject);
        
        // TODO: Implementar envío real
        await Task.Delay(100); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] Email enviado exitosamente");
        return true;
    }

    public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
    {
        _logger.LogInformation("📧 [TEMP] Enviando email con adjunto a {To}: {AttachmentPath}", to, attachmentPath);
        
        // TODO: Implementar envío real con adjunto
        await Task.Delay(150); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] Email con adjunto enviado exitosamente");
        return true;
    }

    public async Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body)
    {
        _logger.LogInformation("📧 [TEMP] Enviando email masivo a {Count} destinatarios", toList.Count);
        
        // TODO: Implementar envío masivo real
        await Task.Delay(200); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] Emails masivos enviados exitosamente");
        return true;
    }

    public async Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("📧 [TEMP] Enviando email HTML a {To} con asunto: {Subject}", to, subject);
        _logger.LogDebug("📧 [TEMP] Contenido HTML: {HtmlLength} caracteres", htmlBody.Length);
        
        // TODO: Implementar envío real de HTML
        await Task.Delay(120); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] Email HTML enviado exitosamente");
        return true;
    }
} 