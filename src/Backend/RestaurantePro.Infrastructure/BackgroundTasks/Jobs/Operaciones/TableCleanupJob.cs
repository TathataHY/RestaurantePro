using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Operaciones;

/// <summary>
/// Trabajo para gestionar la limpieza de mesas inactivas
/// </summary>
public class TableCleanupJob : BackgroundJobBase
{
    /// <inheritdoc/>
    public override string JobName => "TableCleanup";

    /// <inheritdoc/>
    public override string Description => "Gestiona el estado de mesas inactivas o abandonadas";

    public TableCleanupJob(ILogger<TableCleanupJob> logger) : base(logger)
    {
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ejecutando trabajo de limpieza de mesas");
        
        // TODO: Implementar la lógica de limpieza de mesas
        
        await Task.Delay(100, cancellationToken); // Placeholder
    }
} 