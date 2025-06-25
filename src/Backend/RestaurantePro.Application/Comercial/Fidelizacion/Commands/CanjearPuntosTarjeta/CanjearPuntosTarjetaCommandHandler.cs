using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntosTarjeta;

/// <summary>
/// Handler para canjear puntos de una tarjeta de fidelización
/// </summary>
public class CanjearPuntosTarjetaCommandHandler : IRequestHandler<CanjearPuntosTarjetaCommand, Result<CanjearPuntosTarjetaResponse>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<CanjearPuntosTarjetaCommandHandler> _logger;

    public CanjearPuntosTarjetaCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<CanjearPuntosTarjetaCommandHandler> logger)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
    }

    public async Task<Result<CanjearPuntosTarjetaResponse>> Handle(
        CanjearPuntosTarjetaCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener la tarjeta
            var tarjeta = await _tarjetaRepository.ObtenerPorIdSinExcepcionAsync(request.TarjetaFidelizacionId, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<CanjearPuntosTarjetaResponse>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            // Validar que la tarjeta esté activa
            if (tarjeta.Estado != EstadoTarjeta.Activa)
            {
                return Result.Failure<CanjearPuntosTarjetaResponse>(new List<string> { "La tarjeta debe estar activa para canjear puntos" });
            }

            // Validar puntos positivos
            if (request.PuntosACanjear <= 0)
            {
                return Result.Failure<CanjearPuntosTarjetaResponse>(new List<string> { "Los puntos a canjear deben ser mayores a cero" });
            }

            // Validar que tenga suficientes puntos
            if (tarjeta.PuntosDisponibles < request.PuntosACanjear)
            {
                return Result.Failure<CanjearPuntosTarjetaResponse>(new List<string> { "No tiene suficientes puntos para realizar el canje" });
            }

            // Canjear puntos de la tarjeta
            var puntosAnteriores = tarjeta.PuntosDisponibles;
            tarjeta.ExpirarPuntos(request.PuntosACanjear, request.Descripcion);

            // Guardar cambios
            await _tarjetaRepository.ActualizarAsync(tarjeta);

            _logger.LogInformation("Puntos canjeados de tarjeta {TarjetaId}: {PuntosCanjeados} puntos", 
                request.TarjetaFidelizacionId, request.PuntosACanjear);

            var response = new CanjearPuntosTarjetaResponse
            {
                TarjetaFidelizacionId = tarjeta.Id,
                PuntosCanjeados = request.PuntosACanjear,
                PuntosActuales = tarjeta.PuntosDisponibles,
                Mensaje = $"Se canjearon {request.PuntosACanjear} puntos exitosamente"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al canjear puntos de la tarjeta {TarjetaId}", request.TarjetaFidelizacionId);
            return Result.Failure<CanjearPuntosTarjetaResponse>(new List<string> { "Error interno al procesar la solicitud" });
        }
    }
} 