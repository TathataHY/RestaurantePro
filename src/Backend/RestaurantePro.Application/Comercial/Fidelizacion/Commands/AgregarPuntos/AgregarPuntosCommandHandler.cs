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
    private readonly IUnitOfWork _unitOfWork;

    public AgregarPuntosCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<AgregarPuntosCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _tarjetaRepository = tarjetaRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AgregarPuntosResponse>> Handle(AgregarPuntosCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de agregar {Puntos} puntos a tarjeta {TarjetaId}", 
                request.Puntos, request.TarjetaId);

            // 1. Validar que la tarjeta existe y está activa
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(request.TarjetaId, cancellationToken, asNoTracking: true);
            if (tarjeta == null)
            {
                _logger.LogWarning("Tarjeta de fidelización no encontrada: {TarjetaId}", request.TarjetaId);
                return Result.Failure<AgregarPuntosResponse>("Tarjeta de fidelización no encontrada");
            }

            if (tarjeta.Estado != EstadoTarjeta.Activa)
            {
                _logger.LogWarning("Tarjeta de fidelización no está activa: {TarjetaId}, Estado: {Estado}", 
                    request.TarjetaId, tarjeta.Estado);
                return Result.Failure<AgregarPuntosResponse>("Tarjeta de fidelización no está activa");
            }

            // 2. Agregar puntos usando actualización directa
            HistorialPuntos historial;
            if (request.MontoCompra.HasValue)
            {
                // Usar factor de conversión fijo (ejemplo: 100 = 1 punto por cada $100)
                historial = tarjeta.AgregarPuntosPorCompra(request.MontoCompra.Value, 100, request.Descripcion);
            }
            else
            {
                historial = tarjeta.AgregarPuntos(request.Puntos, request.Descripcion);
            }

            // 3. Guardar cambios usando actualización directa
            await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);

            // 4. Crear y retornar respuesta AgregarPuntosResponse
            var response = new AgregarPuntosResponse
            {
                TarjetaFidelizacionId = tarjeta.Id,
                PuntosAgregados = historial.Puntos,
                PuntosActuales = tarjeta.PuntosAcumulados,
                PuntosDisponibles = tarjeta.PuntosDisponibles,
                NivelFidelizacion = tarjeta.NivelFidelizacion.ToString(),
                Mensaje = $"Se agregaron {historial.Puntos} puntos exitosamente"
            };

            _logger.LogInformation("Puntos agregados exitosamente a tarjeta {TarjetaId}. Puntos actuales: {PuntosActuales}", 
                request.TarjetaId, tarjeta.PuntosAcumulados);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar puntos a la tarjeta {TarjetaId}", request.TarjetaId);
            return Result.Failure<AgregarPuntosResponse>("Error interno del servidor");
        }
    }
} 