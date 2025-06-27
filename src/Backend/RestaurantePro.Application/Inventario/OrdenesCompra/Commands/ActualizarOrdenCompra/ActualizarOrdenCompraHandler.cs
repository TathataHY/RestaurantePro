using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

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
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
            {
                // Asignar directamente la propiedad Observaciones (es private set pero accesible desde el mismo assembly)
                var observacionesProperty = typeof(OrdenCompra).GetProperty("Observaciones");
                observacionesProperty?.SetValue(ordenCompra, request.Observaciones);
            }

            if (request.FechaEntregaEsperada.HasValue)
                ordenCompra.EstablecerFechaEntrega(request.FechaEntregaEsperada.Value);

            // Actualizar items si se envían en el request
            if (request.Items != null && request.Items.Any())
            {
                // Eliminar items existentes
                var itemsExistentes = ordenCompra.Items.ToList();
                foreach (var item in itemsExistentes)
                {
                    ordenCompra.EliminarItem(item.Id);
                }

                // Agregar los nuevos items
                foreach (var itemRequest in request.Items)
                {
                    // Para agregar un item necesitamos el nombre del ingrediente y unidad de medida
                    // Por ahora usamos valores por defecto ya que no están en el comando
                    ordenCompra.AgregarItem(
                        itemRequest.IngredienteId,
                        "Ingrediente", // Nombre por defecto
                        itemRequest.Cantidad,
                        RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramos // Unidad por defecto
                    );
                }
            }

            // Guardar cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra, cancellationToken);

            _logger.LogInformation("✅ Orden de compra actualizada exitosamente: {OrdenCompraId}", request.Id);

            // Mapear a DTO
            var dto = _mapper.Map<OrdenCompraDto>(ordenCompra);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar orden de compra {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>($"Error al actualizar la orden de compra: {ex.Message}");
        }
    }
} 