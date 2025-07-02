using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using System.Data;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AgregarPuntos;

/// <summary>
/// Handler para agregar puntos a una tarjeta de fidelización
/// </summary>
public class AgregarPuntosCommandHandler : IRequestHandler<AgregarPuntosCommand, Result<AgregarPuntosResponse>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<AgregarPuntosCommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AgregarPuntosCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<AgregarPuntosCommandHandler> logger,
        IServiceProvider serviceProvider,
        IUnitOfWork unitOfWork)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AgregarPuntosResponse>> Handle(
        AgregarPuntosCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de agregar {Puntos} puntos a tarjeta {TarjetaId}", 
                request.Puntos, request.TarjetaFidelizacionId);

            // Obtener la entidad fresca desde la base de datos
            var tarjetaFresca = await _tarjetaRepository.ObtenerPorIdAsync(request.TarjetaFidelizacionId, cancellationToken);
            
            if (tarjetaFresca == null)
            {
                _logger.LogWarning("Tarjeta de fidelización no encontrada: {TarjetaId}", request.TarjetaFidelizacionId);
                return Result.Failure<AgregarPuntosResponse>("Tarjeta de fidelización no encontrada");
            }

            // Validar que la tarjeta esté activa
            if (tarjetaFresca.Estado != EstadoTarjeta.Activa)
            {
                _logger.LogWarning("Tarjeta de fidelización no está activa: {TarjetaId}, Estado: {Estado}", 
                    request.TarjetaFidelizacionId, tarjetaFresca.Estado);
                return Result.Failure<AgregarPuntosResponse>("La tarjeta debe estar activa para agregar puntos");
            }

            // Validar puntos positivos
            if (request.Puntos <= 0)
            {
                _logger.LogWarning("Puntos a agregar deben ser positivos: {Puntos}", request.Puntos);
                return Result.Failure<AgregarPuntosResponse>("Los puntos a agregar deben ser mayores a cero");
            }

            // Validar límite mensual si está configurado
            if (tarjetaFresca.LimiteMensual.HasValue)
            {
                var puntosMesActual = await _tarjetaRepository.ObtenerPuntosAcumuladosMesActualAsync(
                    request.TarjetaFidelizacionId, cancellationToken);
                
                if (puntosMesActual + request.Puntos > tarjetaFresca.LimiteMensual.Value)
                {
                    _logger.LogWarning("Límite mensual excedido para tarjeta {TarjetaId}: Actual {Actual}, Límite {Limite}, Solicitado {Solicitado}", 
                        request.TarjetaFidelizacionId, puntosMesActual, tarjetaFresca.LimiteMensual.Value, request.Puntos);
                    return Result.Failure<AgregarPuntosResponse>($"Límite mensual excedido. Máximo permitido: {tarjetaFresca.LimiteMensual.Value} puntos");
                }
            }

            // Agregar puntos a la tarjeta
            var historial = tarjetaFresca.AgregarPuntos(request.Puntos, request.Descripcion);

            // Guardar cambios usando el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjetaFresca, cancellationToken);

            _logger.LogInformation("Puntos agregados exitosamente a la tarjeta {TarjetaId}: {Puntos} puntos. Total acumulado: {Total}", 
                request.TarjetaFidelizacionId, request.Puntos, tarjetaFresca.PuntosAcumulados);

            return Result.Success(new AgregarPuntosResponse
            {
                TarjetaFidelizacionId = tarjetaFresca.Id,
                PuntosAgregados = request.Puntos,
                PuntosActuales = tarjetaFresca.PuntosAcumulados,
                PuntosDisponibles = tarjetaFresca.PuntosDisponibles,
                NivelFidelizacion = tarjetaFresca.NivelFidelizacion.ToString(),
                Mensaje = "Puntos agregados exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar puntos a la tarjeta {TarjetaId}", request.TarjetaFidelizacionId);
            return Result.Failure<AgregarPuntosResponse>("Error interno del servidor");
        }
    }
} 