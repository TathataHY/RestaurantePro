using RestaurantePro.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Implementación temporal del servicio de SMS
/// TODO: Implementar con proveedor real (Twilio, Azure Communication Services, etc.)
/// </summary>
public class SMSService : ISMSService
{
    private readonly ILogger<SMSService> _logger;

    public SMSService(ILogger<SMSService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendSMSAsync(string phoneNumber, string message)
    {
        if (!IsValidPhoneNumber(phoneNumber))
        {
            _logger.LogWarning("📱 [TEMP] Número de teléfono inválido: {PhoneNumber}", phoneNumber);
            return false;
        }

        _logger.LogInformation("📱 [TEMP] Enviando SMS a {PhoneNumber}: {Message}", phoneNumber, message);
        
        // TODO: Implementar envío real
        await Task.Delay(100); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] SMS enviado exitosamente");
        return true;
    }

    public async Task<bool> SendSMSWithTrackingAsync(string phoneNumber, string message, Guid? clienteId = null, string? tipoNotificacion = null)
    {
        if (!IsValidPhoneNumber(phoneNumber))
        {
            _logger.LogWarning("📱 [TEMP] Número de teléfono inválido: {PhoneNumber}", phoneNumber);
            return false;
        }

        _logger.LogInformation("📱 [TEMP] Enviando SMS con tracking a {PhoneNumber} para cliente {ClienteId}, tipo: {TipoNotificacion}", 
            phoneNumber, clienteId, tipoNotificacion);
        
        // TODO: Implementar envío real con tracking
        await Task.Delay(120); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] SMS con tracking enviado exitosamente");
        return true;
    }

    public async Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message)
    {
        var validNumbers = phoneNumbers.Where(IsValidPhoneNumber).ToList();
        
        _logger.LogInformation("📱 [TEMP] Enviando SMS masivo a {ValidCount}/{TotalCount} números válidos", 
            validNumbers.Count, phoneNumbers.Count);
        
        // TODO: Implementar envío masivo real
        await Task.Delay(200); // Simular latencia
        
        _logger.LogInformation("✅ [TEMP] SMS masivos enviados exitosamente");
        return true;
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Expresión regular básica para validar números de teléfono
        // Acepta formatos como: +1234567890, (123) 456-7890, 123-456-7890, 1234567890
        var phoneRegex = new Regex(@"^(\+?1\s?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$|^\+?\d{8,15}$");
        return phoneRegex.IsMatch(phoneNumber.Trim());
    }

    public async Task<string> GetDeliveryStatusAsync(string messageId)
    {
        _logger.LogInformation("📱 [TEMP] Consultando estado de entrega para mensaje: {MessageId}", messageId);
        
        // TODO: Implementar consulta real de estado
        await Task.Delay(50); // Simular latencia
        
        // Retornar estado simulado
        return "delivered"; // Posibles valores: pending, sent, delivered, failed
    }
} 