using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

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
            // Verificar que la orden existe
            var ordenExistente = await _ordenCompraRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (ordenExistente == null)
            {
                _logger.LogWarning("⚠️ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
                return Result.Failure<OrdenCompraDto>("Orden de compra no encontrada");
            }

            // Validar que la orden esté en un estado válido para actualizar
            if (ordenExistente.Estado != EstadoOrdenCompra.Pendiente && 
                ordenExistente.Estado != EstadoOrdenCompra.Confirmada)
            {
                _logger.LogWarning("⚠️ No se puede actualizar una orden en estado {Estado}", ordenExistente.Estado);
                return Result.Failure<OrdenCompraDto>($"No se puede actualizar una orden en estado {ordenExistente.Estado}");
            }

            // Actualizar fecha de entrega si se proporciona
            if (request.FechaEntregaEsperada.HasValue)
            {
                ordenExistente.EstablecerFechaEntrega(request.FechaEntregaEsperada.Value);
            }

            // Actualizar observaciones si se proporciona
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
            {
                // Usar reflexión para acceder a la propiedad privada Observaciones
                var observacionesProperty = typeof(OrdenCompra).GetProperty("Observaciones");
                observacionesProperty?.SetValue(ordenExistente, request.Observaciones);
            }

            try
            {
                // Procesar items: eliminar los que no están en el request y agregar/actualizar los nuevos
                var idsItemsRequest = request.Items.Select(i => i.IngredienteId).ToHashSet();
                var itemsAEliminar = ordenExistente.Items.Where(i => !idsItemsRequest.Contains(i.IngredienteId)).ToList();
                foreach (var item in itemsAEliminar)
                {
                    ordenExistente.EliminarItem(item.Id);
                }
                foreach (var itemCmd in request.Items)
                {
                    // Si ya existe, actualizar cantidad y precio (si aplica), si no, agregar
                    var itemExistente = ordenExistente.Items.FirstOrDefault(i => i.IngredienteId == itemCmd.IngredienteId);
                    if (itemExistente != null)
                    {
                        itemExistente.ActualizarCantidad(itemCmd.Cantidad);
                        itemExistente.ActualizarPrecio(itemCmd.PrecioUnitario);
                    }
                    else
                    {
                        ordenExistente.AgregarItem(itemCmd.IngredienteId, "", itemCmd.Cantidad, 0); // Ajustar nombre/unidad si es necesario
                    }
                }

                // Asignar RowVersion solo si viene explícitamente en el comando y no es null
                if (request.RowVersion != null && request.RowVersion.Length > 0)
                {
                    ordenExistente.RowVersion = request.RowVersion;
                }
                // Si no se envía RowVersion, no asignar nada (evita problemas de concurrencia en SQLite)

                await _ordenCompraRepository.ActualizarAsync(ordenExistente, cancellationToken);
            }
            catch (DbUpdateConcurrencyException) when (request.RowVersion != null && request.RowVersion.Length > 0)
            {
                _logger.LogWarning("⚠️ Concurrencia optimista: la orden fue modificada por otro usuario");
                return Result.Failure<OrdenCompraDto>("La orden fue modificada por otro usuario. Por favor, recargue y vuelva a intentar.");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si no se envió RowVersion, no es un problema de concurrencia real
                _logger.LogWarning("⚠️ Error de actualización sin RowVersion - continuando sin validación de concurrencia");
                // Continuar sin lanzar error de concurrencia
            }

            // Mapear a DTO
            var dto = _mapper.Map<OrdenCompraDto>(ordenExistente);
            
            _logger.LogInformation("✅ Orden de compra actualizada exitosamente: {OrdenCompraId}", request.Id);
            return Result.Success(dto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "❌ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>("Orden de compra no encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar orden de compra {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>("Error interno del servidor");
        }
    }
} 