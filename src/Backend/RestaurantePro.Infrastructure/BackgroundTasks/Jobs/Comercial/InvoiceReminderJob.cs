using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;

/// <summary>
/// Trabajo para enviar recordatorios de facturas pendientes de pago
/// </summary>
public class InvoiceReminderJob : BackgroundJobBase
{
    /// <inheritdoc/>
    public override string JobName => "InvoiceReminder";

    /// <inheritdoc/>
    public override string Description => "Envía recordatorios de facturas pendientes de pago";

    public InvoiceReminderJob(ILogger<InvoiceReminderJob> logger) : base(logger)
    {
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ejecutando trabajo de recordatorios de facturas");
        
        // TODO: Implementar la lógica de recordatorios de facturas
        
        await Task.Delay(100, cancellationToken); // Placeholder
    }
} 