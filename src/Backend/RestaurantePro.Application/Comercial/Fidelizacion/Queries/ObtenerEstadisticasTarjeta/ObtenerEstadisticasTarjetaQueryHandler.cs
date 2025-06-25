using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerEstadisticasTarjeta;

/// <summary>
/// Handler para obtener las estadísticas de una tarjeta de fidelización
/// </summary>
public class ObtenerEstadisticasTarjetaQueryHandler : IRequestHandler<ObtenerEstadisticasTarjetaQuery, Result<EstadisticasTarjetaDto>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<ObtenerEstadisticasTarjetaQueryHandler> _logger;

    public ObtenerEstadisticasTarjetaQueryHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<ObtenerEstadisticasTarjetaQueryHandler> logger)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
    }

    public async Task<Result<EstadisticasTarjetaDto>> Handle(
        ObtenerEstadisticasTarjetaQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Verificar que la tarjeta existe
            var tarjeta = await _tarjetaRepository.ObtenerPorIdSinExcepcionAsync(request.TarjetaFidelizacionId, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<EstadisticasTarjetaDto>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            // Obtener estadísticas
            var estadisticas = await _tarjetaRepository.ObtenerEstadisticasAsync(request.TarjetaFidelizacionId, cancellationToken);

            var estadisticasDto = new EstadisticasTarjetaDto
            {
                TarjetaFidelizacionId = tarjeta.Id,
                PuntosActuales = tarjeta.PuntosDisponibles,
                PuntosAcumulados = estadisticas.PuntosAcumulados,
                PuntosCanjeados = estadisticas.PuntosCanjeados,
                TotalMovimientos = estadisticas.TotalMovimientos,
                MontoTotalGastado = estadisticas.MontoTotalGastado,
                FechaCreacion = tarjeta.FechaCreacion,
                UltimaActividad = estadisticas.UltimaActividad,
                NivelActual = tarjeta.NivelFidelizacion.ToString(),
                MultiplicadorActual = tarjeta.MultiplicadorPuntos
            };

            _logger.LogInformation("Estadísticas obtenidas para tarjeta {TarjetaId}", request.TarjetaFidelizacionId);

            return Result.Success(estadisticasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de la tarjeta {TarjetaId}", request.TarjetaFidelizacionId);
            return Result.Failure<EstadisticasTarjetaDto>(new List<string> { "Error interno al procesar la solicitud" });
        }
    }
} 