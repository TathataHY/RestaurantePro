using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RechazarOrdenCompra;

/// <summary>
/// Handler para rechazar una orden de compra
/// </summary>
public class RechazarOrdenCompraHandler : IRequestHandler<RechazarOrdenCompraCommand, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RechazarOrdenCompraHandler> _logger;

    public RechazarOrdenCompraHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<RechazarOrdenCompraHandler> logger)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        RechazarOrdenCompraCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("❌ Iniciando rechazo de orden de compra {OrdenCompraId}", request.Id);

        try
        {
            // Buscar la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (ordenCompra == null)
            {
                _logger.LogWarning("⚠️ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
                return Result.Failure<OrdenCompraDto>("Orden de compra no encontrada");
            }

            // Validar que la orden esté en estado pendiente
            if (ordenCompra.Estado != EstadoOrdenCompra.Pendiente)
            {
                _logger.LogWarning("⚠️ Solo se pueden rechazar órdenes en estado Pendiente. Estado actual: {Estado}", 
                    ordenCompra.Estado);
                return Result.Failure<OrdenCompraDto>($"Solo se pueden rechazar órdenes en estado Pendiente. Estado actual: {ordenCompra.Estado}");
            }

            // Rechazar la orden usando el método de la entidad
            ordenCompra.Cancelar(request.MotivoRechazo);

            // Guardar cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra, cancellationToken);

            _logger.LogInformation("✅ Orden de compra rechazada exitosamente: {OrdenCompraId}", request.Id);

            var ordenCompraDto = _mapper.Map<OrdenCompraDto>(ordenCompra);
            return Result.Success(ordenCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al rechazar orden de compra {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>($"Error al rechazar la orden de compra: {ex.Message}");
        }
    }
} 