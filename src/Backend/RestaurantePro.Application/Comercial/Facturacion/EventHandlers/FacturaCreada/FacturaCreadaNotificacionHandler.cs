using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Enums;

namespace RestaurantePro.Application.Comercial.Facturacion.EventHandlers.FacturaCreada;

/// <summary>
/// 📧 Handler que procesa el evento FacturaCreada para envío automático por email
/// </summary>
public class FacturaCreadaNotificacionHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Comercial.Facturacion.Events.FacturaCreada>
{
    private readonly IFacturaRepository _facturaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FacturaCreadaNotificacionHandler> _logger;
    private readonly IMediator _mediator;

    public FacturaCreadaNotificacionHandler(
        IFacturaRepository facturaRepository,
        IClienteRepository clienteRepository,
        IEmailService emailService,
        ISMSService smsService,
        INotificationService notificationService,
        ILogger<FacturaCreadaNotificacionHandler> logger,
        IMediator mediator)
    {
        _facturaRepository = facturaRepository;
        _clienteRepository = clienteRepository;
        _emailService = emailService;
        _smsService = smsService;
        _notificationService = notificationService;
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// 🚀 Procesa la creación de factura para envío automático de notificaciones
    /// </summary>
    public async Task Handle(Domain.Comercial.Facturacion.Events.FacturaCreada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 Iniciando envío de notificación para Factura {FacturaId} - {NumeroFactura}", 
            evento.FacturaId, evento.NumeroFactura);

        try
        {
            // 🔄 Obtener la factura
            var facturaResult = await _facturaRepository.ObtenerPorIdAsync(evento.FacturaId, cancellationToken);
            if (facturaResult == null)
            {
                _logger.LogWarning("⚠️ Factura no encontrada para notificación: {FacturaId}", evento.FacturaId);
                return;
            }

            var factura = facturaResult;

            // 🔍 Verificar si la factura tiene cliente asociado
            if (!factura.ClienteId.HasValue)
            {
                _logger.LogWarning("⚠️ Factura {FacturaId} no tiene cliente asociado, omitiendo notificación", evento.FacturaId);
                return;
            }

            // 🔍 Obtener el cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(factura.ClienteId.Value, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Cliente no encontrado para factura: {ClienteId}", factura.ClienteId);
                return;
            }

            // ✅ Obtener el nombre del cliente DESPUÉS de validar que no es null
            var nombreCliente = ObtenerNombreCompleto(cliente);

            // 2. Obtener información básica de la factura (sin items detallados)
            var datosFactura = new
            {
                FacturaId = factura.Id,
                ClienteId = factura.ClienteId,
                Total = GetTotalSafely(factura),
                FechaCreacion = GetFechaCreacionSafely(factura),
                Estado = GetEstadoSafely(factura)
            };

            // 📧 Enviar notificación según el canal preferido del cliente
            var canalPreferido = ObtenerCanalPreferido(cliente);

            // 4. Validar email del cliente - añado validación null
            var emailCliente = GetEmailSafely(cliente);
            if (string.IsNullOrEmpty(emailCliente))
            {
                _logger.LogInformation("📧 Cliente sin email registrado: {ClienteNombre}, saltando envío de email", 
                    nombreCliente);
                // No hacer return aquí, continuar con SMS si disponible
            }
            else
            {
                // 5. Preparar datos para el email
                var datosEmail = new
                {
                    FacturaId = evento.FacturaId,
                    NumeroFactura = evento.NumeroFactura,
                    TipoFactura = evento.TipoFactura,
                    FechaEmision = evento.FechaEmision,
                    ClienteEmail = emailCliente,
                    ClienteNombre = nombreCliente,
                    TotalFactura = GetTotalSafely(factura),
                    FechaVencimiento = GetFechaVencimientoSafely(factura),
                    MetodoPagoPreferido = "Efectivo", // Valor por defecto
                    EsClienteFidelizado = true // Asumiendo que todos son fidelizados
                };

                // 6. Enviar email de factura
                await EnviarFacturaPorEmailAsync((object)factura, (object)cliente, new List<dynamic>());
            }

            // 7. Validar teléfono del cliente para SMS
            var telefonoCliente = GetTelefonoSafely(cliente);
            if (string.IsNullOrEmpty(telefonoCliente))
            {
                _logger.LogInformation("📱 Cliente sin teléfono registrado: {ClienteNombre}, saltando envío de SMS", 
                    nombreCliente);
            }
            else
            {
                // Enviar SMS si está habilitado
                await EnviarFacturaPorSMSAsync((object)factura, (object)cliente);
            }

            // 8. Enviar notificación con prioridad basada en monto
            var totalFactura = GetTotalSafely(factura);
            var prioridad = DeterminarPrioridad(totalFactura);
            await EnviarNotificacionPorPrioridad(evento.FacturaId, evento.NumeroFactura, totalFactura, prioridad);

            // 9. Registrar estadísticas de notificación
            await RegistrarEstadisticasNotificacionAsync(evento.FacturaId, canalPreferido, true, (object)cliente);

            _logger.LogInformation("✅ Notificaciones enviadas exitosamente para Factura {NumeroFactura} al cliente {ClienteNombre}", 
                evento.NumeroFactura, nombreCliente);

            _logger.LogInformation("📧 Enviando notificación de factura: Cliente={ClienteNombre}, Total={Total:C}", 
                nombreCliente, GetTotalSafely(factura));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar notificación para Factura {FacturaId}", evento.FacturaId);
            throw;
        }
    }

    /// <summary>
    /// Método auxiliar para obtener total de forma segura
    /// </summary>
    private decimal GetTotalSafely(object factura)
    {
        try
        {
            var facturaDynamic = (dynamic)factura;
            return facturaDynamic.Total ?? 0m;
        }
        catch
        {
            return 0m;
        }
    }

    /// <summary>
    /// Método auxiliar para obtener fecha de creación de forma segura
    /// </summary>
    private DateTime GetFechaCreacionSafely(object factura)
    {
        try
        {
            var facturaDynamic = (dynamic)factura;
            return facturaDynamic.FechaCreacion ?? DateTime.UtcNow;
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Método auxiliar para obtener estado de forma segura
    /// </summary>
    private string GetEstadoSafely(object factura)
    {
        try
        {
            var facturaDynamic = (dynamic)factura;
            return facturaDynamic.Estado?.ToString() ?? "Desconocido";
        }
        catch
        {
            return "Desconocido";
        }
    }

    /// <summary>
    /// Método auxiliar para obtener fecha de vencimiento de forma segura
    /// </summary>
    private DateTime? GetFechaVencimientoSafely(object factura)
    {
        try
        {
            var facturaDynamic = (dynamic)factura;
            return facturaDynamic.FechaVencimiento;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Método auxiliar para obtener email de forma segura
    /// </summary>
    private string GetEmailSafely(object cliente)
    {
        try
        {
            var clienteDynamic = (dynamic)cliente;
            return clienteDynamic.Email?.Value?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 📧 Envía la factura por email al cliente
    /// </summary>
    private async Task EnviarFacturaPorEmailAsync(object factura, object cliente, List<dynamic> itemsFactura)
    {
        var clienteDynamic = (dynamic)cliente;
        var facturaDynamic = (dynamic)factura;
        
        var destinatario = GetEmailSafely(cliente);
        if (string.IsNullOrEmpty(destinatario)) return;
        
        var nombreCliente = ObtenerNombreCompleto(clienteDynamic);
        var asunto = $"Factura #{facturaDynamic.NumeroFactura} - RestaurantePro";
        var cuerpo = GenerarCuerpoEmailFactura(facturaDynamic, nombreCliente, itemsFactura);

        // Obtener información del canal de pago preferido para personalización
        var metodoPagoPreferido = "Efectivo"; // Valor por defecto
        
        await EnviarEmailAsync(destinatario, asunto, cuerpo);
        
        _logger.LogInformation("📧 Email de factura enviado a: {Email}", destinatario);
    }

    /// <summary>
    /// 📱 Envía notificación por SMS al cliente
    /// </summary>
    private async Task EnviarFacturaPorSMSAsync(object factura, object cliente)
    {
        var clienteDynamic = (dynamic)cliente;
        var facturaDynamic = (dynamic)factura;
        
        var numeroTelefono = GetTelefonoSafely(cliente);
        if (string.IsNullOrEmpty(numeroTelefono)) return;

        var nombreCliente = ObtenerNombreCompleto(clienteDynamic);
        var mensaje = $"Hola {nombreCliente}, tu factura #{facturaDynamic.NumeroFactura} por ${facturaDynamic.Total:N0} está lista. Gracias por elegirnos! - RestaurantePro";

        await EnviarSMSAsync(numeroTelefono, mensaje);
        _logger.LogInformation("📱 SMS de factura enviado a: {Telefono}", numeroTelefono);
    }

    /// <summary>
    /// Método auxiliar para obtener teléfono de forma segura
    /// </summary>
    private string GetTelefonoSafely(object cliente)
    {
        try
        {
            var clienteDynamic = (dynamic)cliente;
            return clienteDynamic.Telefono?.Value?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 📧 Envía email genérico de forma asíncrona
    /// </summary>
    private async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpo)
    {
        await _emailService.SendEmailAsync(destinatario, asunto, cuerpo);
        _logger.LogDebug("📧 Email enviado - Destinatario: {Email}, Asunto: {Asunto}", destinatario, asunto);
    }

    /// <summary>
    /// 📱 Envía SMS de forma asíncrona
    /// </summary>
    private async Task EnviarSMSAsync(string numero, string mensaje)
    {
        await _smsService.SendSMSAsync(numero, mensaje);
        _logger.LogDebug("📱 SMS enviado - Número: {Numero}, Mensaje: {Mensaje}", numero, mensaje);
    }

    /// <summary>
    /// 🎯 Determina el canal de comunicación preferido del cliente
    /// </summary>
    private string ObtenerCanalPreferido(object cliente)
    {
        var clienteDynamic = (dynamic)cliente;
        // Lógica para determinar canal preferido basado en información básica
        var email = GetEmailSafely(cliente);
        var telefono = GetTelefonoSafely(cliente);

        return "Email"; // Por defecto email, pero podría ser más inteligente
    }

    /// <summary>
    /// 📊 Registra estadísticas del envío de notificación
    /// </summary>
    private async Task RegistrarEstadisticasNotificacionAsync(Guid facturaId, string canal, bool exitoso, object cliente)
    {
        var clienteDynamic = (dynamic)cliente;
        var estadisticas = new
        {
            FacturaId = facturaId,
            Canal = canal,
            Exitoso = exitoso,
            FechaEnvio = DateTime.UtcNow,
            ClienteId = ((dynamic)cliente).Id
        };

        // Simulación de registro de estadísticas
        await Task.Delay(10);
        
        _logger.LogDebug("📊 Estadísticas registradas - Canal: {Canal}, Exitoso: {Exitoso}", canal, exitoso);
    }

    /// <summary>
    /// 👤 Obtiene el nombre completo del cliente de forma segura
    /// </summary>
    private string ObtenerNombreCompleto(object cliente)
    {
        try
        {
            var clienteDynamic = (dynamic)cliente;
            return clienteDynamic.Nombre?.Value?.ToString() ?? "Cliente";
        }
        catch
        {
            return "Cliente";
        }
    }

    /// <summary>
    /// 📄 Genera el cuerpo del email para la factura
    /// </summary>
    private string GenerarCuerpoEmailFactura(object factura, string nombreCliente, List<dynamic> items)
    {
        try
        {
            var facturaDynamic = (dynamic)factura;
            return $@"
                <h2>Factura Electrónica</h2>
                <p>Estimado/a {nombreCliente},</p>
                <p>Se ha generado su factura correctamente.</p>
                <p><strong>Número:</strong> {facturaDynamic.NumeroFactura}<br>
                <strong>Total:</strong> ${facturaDynamic.Total:N2}</p>
                <p>Gracias por su preferencia.<br>Equipo RestaurantePro</p>";
        }
        catch
        {
            return $@"
                <h2>Factura Electrónica</h2>
                <p>Estimado/a {nombreCliente},</p>
                <p>Se ha generado su factura correctamente.</p>
                <p>Gracias por su preferencia.<br>Equipo RestaurantePro</p>";
        }
    }

    /// <summary>
    /// Determina la prioridad de notificación basada en el monto
    /// </summary>
    private NivelPrioridad DeterminarPrioridad(decimal monto)
    {
        if (monto >= 1000) return NivelPrioridad.Alta;
        if (monto >= 100) return NivelPrioridad.Media;
        return NivelPrioridad.Baja;
    }

    /// <summary>
    /// Envía notificación usando el servicio con prioridad
    /// </summary>
    private async Task EnviarNotificacionPorPrioridad(Guid facturaId, string numeroFactura, decimal total, NivelPrioridad prioridad)
    {
        var mensaje = $"Nueva factura #{numeroFactura} generada por {total:C}";
        var titulo = "Factura Creada";
        var prioridadTexto = prioridad.ToString();

        await _notificationService.EnviarNotificacionAsync(facturaId, mensaje, titulo, prioridadTexto);
    }
} 