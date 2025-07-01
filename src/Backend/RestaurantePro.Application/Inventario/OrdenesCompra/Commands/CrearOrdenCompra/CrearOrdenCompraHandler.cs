using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.CrearOrdenCompra;

/// <summary>
/// Handler para crear una nueva orden de compra
/// </summary>
public class CrearOrdenCompraHandler : IRequestHandler<CrearOrdenCompraCommand, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearOrdenCompraHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public CrearOrdenCompraHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IIngredienteRepository ingredienteRepository,
        IMapper mapper,
        ILogger<CrearOrdenCompraHandler> logger,
        IDateTimeService dateTimeService)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _ingredienteRepository = ingredienteRepository;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        CrearOrdenCompraCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📦 Iniciando creación de orden de compra para proveedor {ProveedorId}", request.ProveedorId);

        try
        {
            // Validar que el proveedor existe (asumiendo que hay un repositorio de proveedores)
            // TODO: Agregar validación de proveedor cuando se implemente el repositorio

            // Crear la orden de compra usando el factory method del dominio
            var ordenCompra = OrdenCompra.Crear(
                request.ProveedorId,
                request.Observaciones ?? "Orden de compra creada desde API",
                _dateTimeService.Now
            );

            // Establecer fecha de entrega esperada
            ordenCompra.EstablecerFechaEntrega(request.FechaEntregaEsperada);

            // Agregar items a la orden
            foreach (var itemCommand in request.Items)
            {
                // Validar que el ingrediente existe
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(itemCommand.IngredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    _logger.LogWarning("⚠️ Ingrediente no encontrado: {IngredienteId}", itemCommand.IngredienteId);
                    return Result.Failure<OrdenCompraDto>($"Ingrediente no encontrado: {itemCommand.IngredienteId}");
                }

                // Agregar item a la orden
                var itemOrden = ordenCompra.AgregarItem(
                    itemCommand.IngredienteId,
                    ingrediente.Nombre,
                    itemCommand.Cantidad,
                    ingrediente.UnidadMedida
                );

                // Establecer precio unitario si se proporciona
                if (itemCommand.PrecioUnitario > 0)
                {
                    itemOrden.Actualizar(itemCommand.Cantidad, itemCommand.PrecioUnitario);
                    // Recalcular el total de la orden después de actualizar el precio
                    ordenCompra.RecalcularTotal();
                }

                // Agregar observaciones del item si existen
                if (!string.IsNullOrEmpty(itemCommand.Observaciones))
                {
                    // TODO: Implementar método para agregar observaciones al item si es necesario
                }
            }

            // Validar que la orden tenga al menos un item
            if (!ordenCompra.Items.Any())
            {
                _logger.LogWarning("⚠️ La orden de compra debe tener al menos un item");
                return Result.Failure<OrdenCompraDto>("La orden de compra debe tener al menos un item");
            }

            // Guardar la orden de compra
            await _ordenCompraRepository.AgregarAsync(ordenCompra, cancellationToken);

            _logger.LogInformation("✅ Orden de compra creada exitosamente: {OrdenCompraId} con {ItemCount} items", 
                ordenCompra.Id, ordenCompra.Items.Count);

            // Mapear a DTO y retornar
            var ordenCompraDto = _mapper.Map<OrdenCompraDto>(ordenCompra);
            return Result.Success(ordenCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al crear orden de compra para proveedor {ProveedorId}", request.ProveedorId);
            return Result.Failure<OrdenCompraDto>($"Error al crear la orden de compra: {ex.Message}");
        }
    }
} 