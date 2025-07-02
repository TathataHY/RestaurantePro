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
            // Obtener la entidad SIN tracking
            var tarjetaFresca = await _tarjetaRepository.ObtenerPorIdAsync(request.TarjetaId, cancellationToken, asNoTracking: true);
            
            if (tarjetaFresca == null)
            {
                _logger.LogWarning("Tarjeta de fidelización no encontrada: {TarjetaId}", request.TarjetaId);
                return Result.Failure<CanjearPuntosTarjetaResponse>("Tarjeta de fidelización no encontrada");
            }

            // Adjuntar manualmente la entidad al contexto
            _unitOfWork.GetDbContext().Attach(tarjetaFresca);

            // Validar que la tarjeta esté activa
            if (tarjetaFresca.Estado != EstadoTarjeta.Activa)
            {
                _logger.LogWarning("Tarjeta de fidelización no está activa: {TarjetaId}, Estado: {Estado}", 
                    request.TarjetaId, tarjetaFresca.Estado);
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
                    request.TarjetaId, tarjetaFresca.PuntosDisponibles, request.PuntosACanjear);
                return Result.Failure<CanjearPuntosTarjetaResponse>("No tiene suficientes puntos para realizar el canje");
            }

            // Canjear puntos de la tarjeta
            var puntosAnteriores = tarjetaFresca.PuntosDisponibles;
            var historialCanje = tarjetaFresca.CanjearPuntos(request.PuntosACanjear, request.Descripcion);

            // Persistir cambios
            _tarjetaRepository.Update(tarjetaFresca);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Puntos canjeados exitosamente de la tarjeta {TarjetaId}: {PuntosCanjeados} puntos. Puntos anteriores: {PuntosAnteriores}, Puntos actuales: {PuntosActuales}", 
                request.TarjetaId, request.PuntosACanjear, puntosAnteriores, tarjetaFresca.PuntosDisponibles);

            var response = new CanjearPuntosTarjetaResponse
            {
                TarjetaId = tarjetaFresca.Id,
                PuntosCanjeados = request.PuntosACanjear,
                PuntosActuales = tarjetaFresca.PuntosDisponibles,
                Mensaje = $"Se canjearon {request.PuntosACanjear} puntos exitosamente. Puntos restantes: {tarjetaFresca.PuntosDisponibles}"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al canjear puntos de la tarjeta {TarjetaId}", request.TarjetaId);
            return Result.Failure<CanjearPuntosTarjetaResponse>("Error interno del servidor");
        }
    }
} 