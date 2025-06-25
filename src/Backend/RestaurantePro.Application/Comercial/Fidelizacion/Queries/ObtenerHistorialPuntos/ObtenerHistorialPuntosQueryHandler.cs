using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerHistorialPuntos;

/// <summary>
/// Handler para obtener el historial de puntos de una tarjeta de fidelización
/// </summary>
public class ObtenerHistorialPuntosQueryHandler : IRequestHandler<ObtenerHistorialPuntosQuery, Result<List<HistorialPuntosDto>>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<ObtenerHistorialPuntosQueryHandler> _logger;

    public ObtenerHistorialPuntosQueryHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<ObtenerHistorialPuntosQueryHandler> logger)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
    }

    public async Task<Result<List<HistorialPuntosDto>>> Handle(
        ObtenerHistorialPuntosQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Verificar que la tarjeta existe
            var tarjeta = await _tarjetaRepository.ObtenerPorIdSinExcepcionAsync(request.TarjetaFidelizacionId, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<List<HistorialPuntosDto>>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            // Obtener el historial de puntos
            var historial = await _tarjetaRepository.ObtenerHistorialPuntosAsync(
                request.TarjetaFidelizacionId, 
                request.PageNumber, 
                request.PageSize, 
                cancellationToken);

            // Mapear a DTOs
            var historialDto = historial.Select(h => new HistorialPuntosDto
            {
                Id = h.Id,
                TarjetaFidelizacionId = h.TarjetaFidelizacionId,
                Puntos = h.Puntos,
                TipoMovimiento = h.TipoOperacion.ToString(),
                Descripcion = h.Concepto,
                FechaMovimiento = h.FechaOperacion,
                MontoTransaccion = h.MontoCompra
            }).ToList();

            _logger.LogInformation("Historial de puntos obtenido para tarjeta {TarjetaId}: {CantidadMovimientos} movimientos", 
                request.TarjetaFidelizacionId, historialDto.Count);

            return Result.Success(historialDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de puntos de la tarjeta {TarjetaId}", request.TarjetaFidelizacionId);
            return Result.Failure<List<HistorialPuntosDto>>(new List<string> { "Error interno al procesar la solicitud" });
        }
    }
} 