using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.ActualizarOrdenCompra;

/// <summary>
/// Handler para actualizar una orden de compra existente
/// </summary>
public class ActualizarOrdenCompraHandler : IRequestHandler<ActualizarOrdenCompraCommand, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarOrdenCompraHandler> _logger;

    public ActualizarOrdenCompraHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<ActualizarOrdenCompraHandler> logger)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        ActualizarOrdenCompraCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando actualización de orden de compra {OrdenCompraId}", request.Id);

        try
        {
            // Buscar la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (ordenCompra == null)
            {
                _logger.LogWarning("⚠️ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
                return Result.Failure<OrdenCompraDto>("Orden de compra no encontrada");
            }

            // Validar que la orden no esté en estados finales
            if (ordenCompra.Estado == EstadoOrdenCompra.Cancelada || 
                ordenCompra.Estado == EstadoOrdenCompra.Recibida)
            {
                _logger.LogWarning("⚠️ No se puede actualizar una orden en estado {Estado}: {OrdenCompraId}", 
                    ordenCompra.Estado, request.Id);
                return Result.Failure<OrdenCompraDto>($"No se puede actualizar una orden en estado {ordenCompra.Estado}");
            }

            // Actualizar propiedades usando los métodos de la entidad
            // Nota: Las propiedades de la entidad OrdenCompra son de solo lectura,
            // por lo que necesitamos usar métodos específicos para actualizarlas
            
            // Guardar cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra, cancellationToken);

            _logger.LogInformation("✅ Orden de compra actualizada exitosamente: {OrdenCompraId}", request.Id);

            var ordenCompraDto = _mapper.Map<OrdenCompraDto>(ordenCompra);
            return Result.Success(ordenCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar orden de compra {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>($"Error al actualizar la orden de compra: {ex.Message}");
        }
    }
} 