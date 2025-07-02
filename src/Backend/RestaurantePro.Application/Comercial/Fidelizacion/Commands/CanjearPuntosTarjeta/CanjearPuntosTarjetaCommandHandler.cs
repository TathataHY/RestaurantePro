using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntosTarjeta;

/// <summary>
/// Handler para canjear puntos de una tarjeta de fidelización
/// </summary>
public class CanjearPuntosTarjetaCommandHandler : IRequestHandler<CanjearPuntosTarjetaCommand, Result<CanjearPuntosTarjetaResponse>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<CanjearPuntosTarjetaCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CanjearPuntosTarjetaCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<CanjearPuntosTarjetaCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CanjearPuntosTarjetaResponse>> Handle(
        CanjearPuntosTarjetaCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener la entidad fresca desde la base de datos
            var tarjetaFresca = await _tarjetaRepository.ObtenerPorIdAsync(request.TarjetaFidelizacionId, cancellationToken);
            
            if (tarjetaFresca == null)
            {
                _logger.LogWarning("Tarjeta de fidelización no encontrada: {TarjetaId}", request.TarjetaFidelizacionId);
                return Result.Failure<CanjearPuntosTarjetaResponse>("Tarjeta de fidelización no encontrada");
            }

            // Validar que la tarjeta esté activa
            if (tarjetaFresca.Estado != EstadoTarjeta.Activa)
            {
                _logger.LogWarning("Tarjeta de fidelización no está activa: {TarjetaId}, Estado: {Estado}", 
                    request.TarjetaFidelizacionId, tarjetaFresca.Estado);
                return Result.Failure<CanjearPuntosTarjetaResponse>("La tarjeta debe estar activa para canjear puntos");
            }

            // Validar puntos positivos
            if (request.PuntosACanjear <= 0)
            {
                _logger.LogWarning("Puntos a canjear deben ser positivos: {Puntos}", request.PuntosACanjear);
                return Result.Failure<CanjearPuntosTarjetaResponse>("Los puntos a canjear deben ser mayores a cero");
            }

            // Validar que tenga suficientes puntos
            if (tarjetaFresca.PuntosDisponibles < request.PuntosACanjear)
            {
                _logger.LogWarning("Puntos insuficientes en tarjeta {TarjetaId}: Disponibles {Disponibles}, Solicitados {Solicitados}", 
                    request.TarjetaFidelizacionId, tarjetaFresca.PuntosDisponibles, request.PuntosACanjear);
                return Result.Failure<CanjearPuntosTarjetaResponse>("No tiene suficientes puntos para realizar el canje");
            }

            // Canjear puntos de la tarjeta
            var puntosAnteriores = tarjetaFresca.PuntosDisponibles;
            tarjetaFresca.ExpirarPuntos(request.PuntosACanjear, request.Descripcion);

            // Guardar cambios usando el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjetaFresca, cancellationToken);

            _logger.LogInformation("Puntos canjeados exitosamente de la tarjeta {TarjetaId}: {PuntosCanjeados} puntos", 
                request.TarjetaFidelizacionId, request.PuntosACanjear);

            var response = new CanjearPuntosTarjetaResponse
            {
                TarjetaFidelizacionId = tarjetaFresca.Id,
                PuntosCanjeados = request.PuntosACanjear,
                PuntosActuales = tarjetaFresca.PuntosDisponibles,
                Mensaje = $"Se canjearon {request.PuntosACanjear} puntos exitosamente"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al canjear puntos de la tarjeta {TarjetaId}", request.TarjetaFidelizacionId);
            return Result.Failure<CanjearPuntosTarjetaResponse>("Error interno del servidor");
        }
    }
} 