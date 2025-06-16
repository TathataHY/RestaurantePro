using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;

/// <summary>
/// Trabajo para limpiar notificaciones antiguas del sistema
/// </summary>
public class NotificationCleanupJob : BackgroundJobBase
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationCleanupOptions _options;

    /// <inheritdoc/>
    public override string JobName => "NotificationCleanup";

    /// <inheritdoc/>
    public override string Description => "Elimina notificaciones antiguas del sistema para mantener la base de datos optimizada";

    public NotificationCleanupJob(
        ILogger<NotificationCleanupJob> logger,
        INotificacionRepository notificacionRepository,
        IUnitOfWork unitOfWork,
        IOptions<NotificationCleanupOptions> options) : base(logger)
    {
        _notificacionRepository = notificacionRepository ?? throw new ArgumentNullException(nameof(notificacionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-_options.DaysToKeep);
        _logger.LogInformation("Eliminando notificaciones anteriores a {FechaLimite}", fechaLimite);

        var eliminadas = await _notificacionRepository.EliminarAnterioresAFechaAsync(fechaLimite, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        _logger.LogInformation("Se eliminaron {Cantidad} notificaciones antiguas", eliminadas);
    }
}

/// <summary>
/// Opciones de configuración para la limpieza de notificaciones
/// </summary>
public class NotificationCleanupOptions
{
    /// <summary>
    /// Número de días que se conservarán las notificaciones
    /// </summary>
    public int DaysToKeep { get; set; } = 30;
} 