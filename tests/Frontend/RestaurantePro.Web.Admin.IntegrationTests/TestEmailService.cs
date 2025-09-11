using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Web.Admin.IntegrationTests;

/// <summary>
/// Implementación de prueba para IEmailService
/// </summary>
public class TestEmailService : IEmailService
{
    public Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }
}
