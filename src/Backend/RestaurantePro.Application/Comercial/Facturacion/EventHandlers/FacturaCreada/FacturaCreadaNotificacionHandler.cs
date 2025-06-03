namespace RestaurantePro.Application.Comercial.Facturacion.EventHandlers.FacturaCreada;

/// <summary>
/// 📧 Handler que procesa el evento FacturaCreada para envío automático por email
/// </summary>
public class FacturaCreadaNotificacionHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Comercial.Facturacion.Events.FacturaCreada>
{
    private readonly IFacturaRepository _facturaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<FacturaCreadaNotificacionHandler> _logger;
    private readonly IMediator _mediator;

    public FacturaCreadaNotificacionHandler(
        IFacturaRepository facturaRepository,
        IClienteRepository clienteRepository,
        ILogger<FacturaCreadaNotificacionHandler> logger,
        IMediator mediator)
    {
        _facturaRepository = facturaRepository;
        _clienteRepository = clienteRepository;
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

            // 2. Obtener información básica de la factura (sin items detallados)
            var datosFactura = new
            {
                FacturaId = factura.Id,
                ClienteId = factura.ClienteId,
                Total = factura.Total,
                FechaCreacion = factura.FechaCreacion,
                Estado = factura.Estado.ToString()
            };

            // 📧 Enviar notificación según el canal preferido del cliente
            var canalPreferido = ObtenerCanalPreferido(cliente);
            var nombreCliente = ObtenerNombreCompleto(cliente);

            // 4. Validar email del cliente
            if (string.IsNullOrEmpty(cliente.Email.Value))
            {
                _logger.LogWarning("⚠️ Cliente {ClienteNombre} no tiene email válido para envío de factura", 
                    nombreCliente);
                return;
            }

            // 5. Preparar datos para el email
            var datosEmail = new
            {
                FacturaId = evento.FacturaId,
                NumeroFactura = evento.NumeroFactura,
                TipoFactura = evento.TipoFactura,
                FechaEmision = evento.FechaEmision,
                ClienteEmail = cliente.Email.Value,
                ClienteNombre = nombreCliente,
                TotalFactura = factura.Total,
                FechaVencimiento = factura.FechaVencimiento,
                MetodoPagoPreferido = "Efectivo", // Valor por defecto
                EsClienteFidelizado = true // Asumiendo que todos son fidelizados
            };

            // 6. Enviar email de factura
            await EnviarFacturaPorEmailAsync((object)factura, (object)cliente, new List<dynamic>());

            // 7. Enviar SMS si está habilitado
            await EnviarFacturaPorSMSAsync((object)factura, (object)cliente);

            // 8. Registrar estadísticas de notificación
            await RegistrarEstadisticasNotificacionAsync(evento.FacturaId, canalPreferido, true, (object)cliente);

            _logger.LogInformation("✅ Notificaciones enviadas exitosamente para Factura {NumeroFactura} al cliente {ClienteNombre}", 
                evento.NumeroFactura, nombreCliente);

            _logger.LogInformation("📧 Enviando notificación de factura: Cliente={ClienteNombre}, Total={Total:C}", 
                nombreCliente, factura.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar notificación para Factura {FacturaId}", evento.FacturaId);
            throw;
        }
    }

    /// <summary>
    /// 📧 Envía la factura por email al cliente
    /// </summary>
    private async Task EnviarFacturaPorEmailAsync(object factura, object cliente, List<dynamic> itemsFactura)
    {
        var clienteDynamic = (dynamic)cliente;
        var facturaDynamic = (dynamic)factura;
        
        var destinatario = (string)clienteDynamic.Email.Value;
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
        
        if (clienteDynamic.Telefono == null) return;

        var numeroTelefono = (string)clienteDynamic.Telefono.Value;
        var nombreCliente = ObtenerNombreCompleto(clienteDynamic);
        var mensaje = $"Hola {nombreCliente}, tu factura #{facturaDynamic.NumeroFactura} por ${facturaDynamic.Total:N0} está lista. Gracias por elegirnos! - RestaurantePro";

        if (!string.IsNullOrEmpty(numeroTelefono))
        {
            await EnviarSMSAsync(numeroTelefono, mensaje);
            _logger.LogInformation("📱 SMS de factura enviado a: {Telefono}", numeroTelefono);
        }
    }

    /// <summary>
    /// 📧 Envía email genérico de forma asíncrona
    /// </summary>
    private async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpo)
    {
        // Simulación de envío de email - en producción se conectaría con servicio real
        await Task.Delay(100); // Simular latencia de API
        _logger.LogDebug("📧 Email enviado - Destinatario: {Email}, Asunto: {Asunto}", destinatario, asunto);
    }

    /// <summary>
    /// 📱 Envía SMS de forma asíncrona
    /// </summary>
    private async Task EnviarSMSAsync(string numero, string mensaje)
    {
        // Simulación de envío de SMS - en producción se conectaría con servicio real  
        await Task.Delay(50); // Simular latencia de API
        _logger.LogDebug("📱 SMS enviado - Número: {Numero}, Mensaje: {Mensaje}", numero, mensaje);
    }

    /// <summary>
    /// 🎯 Determina el canal de comunicación preferido del cliente
    /// </summary>
    private string ObtenerCanalPreferido(object cliente)
    {
        var clienteDynamic = (dynamic)cliente;
        // Lógica para determinar canal preferido basado en información básica
        var email = clienteDynamic.Email?.Value;
        var telefono = clienteDynamic.Telefono?.Value;

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
            FechaHora = DateTime.UtcNow,
            FacturaId = facturaId,
            ClienteId = clienteDynamic.Id,
            TipoNotificacion = "Email+SMS",
            TotalFactura = 0, // Valor por defecto
            TipoFactura = "Estándar",
            NivelClienteFidelizacion = "Básico",
            TieneEmail = !string.IsNullOrEmpty(clienteDynamic.Email?.Value),
            TieneTelefono = !string.IsNullOrEmpty(clienteDynamic.Telefono?.Value),
            EmailEnviado = exitoso,
            SMSEnviado = !string.IsNullOrEmpty(clienteDynamic.Telefono?.Value)
        };

        _logger.LogInformation("📊 Estadísticas de notificación registradas: {@Estadisticas}", estadisticas);
        
        // TODO: Enviar a sistema de analytics
        // await _analyticsService.RecordNotificationStatsAsync(estadisticas, cancellationToken);
    }

    /// <summary>
    /// 👤 Obtiene el nombre completo del cliente
    /// </summary>
    private string ObtenerNombreCompleto(object cliente)
    {
        var clienteDynamic = (dynamic)cliente;
        return clienteDynamic.Nombre.NombreCompleto;
    }

    /// <summary>
    /// 📝 Genera el cuerpo del email para la factura
    /// </summary>
    private string GenerarCuerpoEmailFactura(object factura, string nombreCliente, List<dynamic> items)
    {
        var facturaDynamic = (dynamic)factura;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Estimado/a {nombreCliente},");
        sb.AppendLine();
        sb.AppendLine($"Su factura #{facturaDynamic.NumeroFactura} ha sido generada exitosamente.");
        sb.AppendLine($"Total: ${facturaDynamic.Total:N0}");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy}");
        sb.AppendLine();
        sb.AppendLine("¡Gracias por elegirnos!");
        sb.AppendLine();
        sb.AppendLine("Equipo RestaurantePro");
        
        return sb.ToString();
    }
} 