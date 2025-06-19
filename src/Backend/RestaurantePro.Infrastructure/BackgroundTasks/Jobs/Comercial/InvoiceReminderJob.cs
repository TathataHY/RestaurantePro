using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;

/// <summary>
/// Trabajo para enviar recordatorios de facturas pendientes de pago
/// </summary>
public class InvoiceReminderJob : BackgroundJobBase
{
    private readonly IFacturaRepository _facturaRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InvoiceReminderOptions _options;
    
    /// <inheritdoc/>
    public override string JobName => "InvoiceReminder";

    /// <inheritdoc/>
    public override string Description => "Envía recordatorios de facturas pendientes de pago";

    public InvoiceReminderJob(
        ILogger<InvoiceReminderJob> logger,
        IFacturaRepository facturaRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IOptions<InvoiceReminderOptions> options) : base(logger)
    {
        _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ejecutando trabajo de recordatorios de facturas pendientes que vencen en {Days} días.", _options.DaysBeforeExpiration);

        var fechaLimite = DateTime.UtcNow.AddDays(_options.DaysBeforeExpiration);

        var facturasPendientes = await _facturaRepository.ObtenerFacturasPendientesConVencimientoAsync(fechaLimite);

        if (!facturasPendientes.Any())
        {
            _logger.LogInformation("No se encontraron facturas pendientes para enviar recordatorios.");
            return;
        }
        
        foreach (var factura in facturasPendientes)
        {
            var cliente = factura.Cliente; 
            if (cliente != null && !cancellationToken.IsCancellationRequested)
            {
                var subject = $"Recordatorio de Pago - Factura {factura.NumeroFactura}";
                var body = $"Hola {cliente.Nombre.NombreCompleto},\n\nTe recordamos que tu factura {factura.NumeroFactura} por un monto de {factura.Total:C} vence el {factura.FechaVencimiento:d}.\n\nGracias,\nEl equipo de RestaurantePro";
                
                await _emailService.SendEmailAsync(cliente.Email.Value, subject, body);
                
                _logger.LogInformation("Recordatorio de factura {NumeroFactura} enviado a {EmailCliente}", factura.NumeroFactura, cliente.Email.Value);
            }
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        
        _logger.LogInformation("Trabajo de recordatorios de facturas completado. Se enviaron {Count} recordatorios.", facturasPendientes.Count());
    }
}

/// <summary>
/// Opciones de configuración para el recordatorio de facturas
/// </summary>
public class InvoiceReminderOptions
{
    /// <summary>
    /// Número de días antes del vencimiento para enviar el recordatorio.
    /// </summary>
    public int DaysBeforeExpiration { get; set; } = 3;
} 