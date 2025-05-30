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

            // 🔍 Obtener el cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(factura.ClienteId.Value, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Cliente no encontrado para factura: {ClienteId}", factura.ClienteId);
                return;
            }

            // 🔍 Obtener items de la factura
            var itemsFactura = await _facturaRepository.ObtenerItemsFacturaAsync(factura.Id, cancellationToken) ?? new List<dynamic>();

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
                Items = itemsFactura,
                FechaVencimiento = factura.FechaVencimiento,
                MetodoPagoPreferido = "Efectivo", // Valor por defecto
                EsClienteFidelizado = true // Asumiendo que todos son fidelizados
            };

            // 6. Enviar email de factura
            await EnviarFacturaPorEmailAsync(factura, cliente, itemsFactura);

            // 7. Enviar SMS si está habilitado
            await EnviarFacturaPorSMSAsync(factura, cliente);

            // 8. Registrar estadísticas de notificación
            await RegistrarEstadisticasNotificacionAsync(evento.FacturaId, canalPreferido, true, cliente);

            _logger.LogInformation("✅ Notificaciones enviadas exitosamente para Factura {NumeroFactura} al cliente {ClienteNombre}", 
                evento.NumeroFactura, nombreCliente);
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
    private async Task EnviarFacturaPorEmailAsync(dynamic factura, dynamic cliente, List<dynamic> itemsFactura)
    {
        var destinatario = cliente.Email.Value;
        var nombreCliente = ObtenerNombreCompleto(cliente);
        var asunto = $"Factura #{factura.Numero} - RestaurantePro";
        var cuerpo = GenerarCuerpoEmailFactura(factura, nombreCliente, itemsFactura);

        // Obtener información del canal de pago preferido para personalización
        var metodoPagoPreferido = "Efectivo"; // Valor por defecto
        
        await EnviarEmailAsync(destinatario, asunto, cuerpo);
        
        _logger.LogInformation("📧 Email de factura enviado a: {Email}", destinatario);
    }

    /// <summary>
    /// 📱 Envía notificación por SMS al cliente
    /// </summary>
    private async Task EnviarFacturaPorSMSAsync(dynamic factura, dynamic cliente)
    {
        if (cliente.Telefono == null) return;

        var numeroTelefono = cliente.Telefono.Value;
        var nombreCliente = ObtenerNombreCompleto(cliente);
        var mensaje = $"Hola {nombreCliente}, tu factura #{factura.Numero} por ${factura.Total:N0} está lista. Gracias por elegirnos! - RestaurantePro";

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
    private string ObtenerCanalPreferido(dynamic cliente)
    {
        // Lógica para determinar canal preferido basado en información básica
        var email = cliente.Email?.Value;
        var telefono = cliente.Telefono?.Value;

        return "Email"; // Por defecto email, pero podría ser más inteligente
    }

    /// <summary>
    /// 📊 Registra estadísticas del envío de notificación
    /// </summary>
    private async Task RegistrarEstadisticasNotificacionAsync(Guid facturaId, string canal, bool exitoso, dynamic cliente)
    {
        var estadisticas = new
        {
            FechaHora = DateTime.UtcNow,
            FacturaId = facturaId,
            ClienteId = cliente.Id,
            TipoNotificacion = "Email+SMS",
            TotalFactura = 0, // Valor por defecto
            TipoFactura = "Estándar",
            NivelClienteFidelizacion = "Básico",
            TieneEmail = !string.IsNullOrEmpty(cliente.Email?.Value),
            TieneTelefono = !string.IsNullOrEmpty(cliente.Telefono?.Value),
            EmailEnviado = exitoso,
            SMSEnviado = !string.IsNullOrEmpty(cliente.Telefono?.Value)
        };

        _logger.LogInformation("📊 Estadísticas de notificación registradas: {@Estadisticas}", estadisticas);
        
        // TODO: Enviar a sistema de analytics
        // await _analyticsService.RecordNotificationStatsAsync(estadisticas, cancellationToken);
    }

    /// <summary>
    /// 👤 Obtiene el nombre completo del cliente
    /// </summary>
    private string ObtenerNombreCompleto(dynamic cliente)
    {
        return cliente.Nombre.NombreCompleto;
    }

    /// <summary>
    /// 📝 Genera el cuerpo del email para la factura
    /// </summary>
    private string GenerarCuerpoEmailFactura(dynamic factura, string nombreCliente, List<dynamic> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Estimado/a {nombreCliente},");
        sb.AppendLine();
        sb.AppendLine($"Su factura #{factura.Numero} ha sido generada exitosamente.");
        sb.AppendLine($"Total: ${factura.Total:N0}");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy}");
        sb.AppendLine();
        sb.AppendLine("¡Gracias por elegirnos!");
        sb.AppendLine();
        sb.AppendLine("Equipo RestaurantePro");
        
        return sb.ToString();
    }
} 