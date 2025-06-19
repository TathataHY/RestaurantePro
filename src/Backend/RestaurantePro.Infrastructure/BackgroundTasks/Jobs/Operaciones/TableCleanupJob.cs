using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.BackgroundTasks.Settings;
using RestaurantePro.Application.Common.Interfaces;
using System.Linq;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Operaciones;

/// <summary>
/// Trabajo para gestionar la limpieza de mesas inactivas
/// </summary>
public class TableCleanupJob : BackgroundJobBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMesaRepository _mesaRepository;
    private readonly TableCleanupJobSettings _settings;

    /// <inheritdoc/>
    public override string JobName => "TableCleanup";

    /// <inheritdoc/>
    public override string Description => "Gestiona el estado de mesas inactivas o abandonadas";

    public TableCleanupJob(
        ILogger<TableCleanupJob> logger, 
        IUnitOfWork unitOfWork,
        IMesaRepository mesaRepository,
        IOptions<TableCleanupJobSettings> settings) : base(logger)
    {
        _unitOfWork = unitOfWork;
        _mesaRepository = mesaRepository;
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando trabajo de limpieza de mesas.");

        var mesasSujas = await _mesaRepository.ObtenerMesasSuciaPorAntiguedad(_settings.StaleTimeMinutes);

        if (!mesasSujas.Any())
        {
            _logger.LogInformation("No se encontraron mesas para limpiar.");
            return;
        }

        _logger.LogInformation("Se encontraron {Count} mesas para limpiar.", mesasSujas.Count());

        foreach (var mesa in mesasSujas)
        {
            mesa.MarcarComoDisponible();
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        _logger.LogInformation("Trabajo de limpieza de mesas completado. Se actualizaron {Count} mesas.", mesasSujas.Count());
    }
} 