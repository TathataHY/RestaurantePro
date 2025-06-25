using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AgregarPuntos;

/// <summary>
/// Handler para agregar puntos a una tarjeta de fidelización
/// </summary>
public class AgregarPuntosCommandHandler : IRequestHandler<AgregarPuntosCommand, Result<AgregarPuntosResponse>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<AgregarPuntosCommandHandler> _logger;

    public AgregarPuntosCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<AgregarPuntosCommandHandler> logger)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
    }

    public async Task<Result<AgregarPuntosResponse>> Handle(
        AgregarPuntosCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener la tarjeta
            var tarjeta = await _tarjetaRepository.ObtenerPorIdSinExcepcionAsync(request.TarjetaFidelizacionId, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<AgregarPuntosResponse>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            // Validar que la tarjeta esté activa
            if (tarjeta.Estado != EstadoTarjeta.Activa)
            {
                return Result.Failure<AgregarPuntosResponse>(new List<string> { "La tarjeta debe estar activa para agregar puntos" });
            }

            // Validar puntos positivos
            if (request.Puntos <= 0)
            {
                return Result.Failure<AgregarPuntosResponse>(new List<string> { "Los puntos a agregar deben ser mayores a cero" });
            }

            // Agregar puntos a la tarjeta
            var puntosAnteriores = tarjeta.PuntosDisponibles;
            tarjeta.AgregarPuntos(request.Puntos, request.Descripcion);

            // Guardar cambios
            await _tarjetaRepository.ActualizarAsync(tarjeta);

            _logger.LogInformation("Puntos agregados a tarjeta {TarjetaId}: {PuntosAgregados} puntos", 
                request.TarjetaFidelizacionId, request.Puntos);

            var response = new AgregarPuntosResponse
            {
                TarjetaFidelizacionId = tarjeta.Id,
                PuntosAgregados = request.Puntos,
                PuntosActuales = tarjeta.PuntosDisponibles,
                Mensaje = $"Se agregaron {request.Puntos} puntos exitosamente"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar puntos a la tarjeta {TarjetaId}", request.TarjetaFidelizacionId);
            return Result.Failure<AgregarPuntosResponse>(new List<string> { "Error interno al procesar la solicitud" });
        }
    }
} 