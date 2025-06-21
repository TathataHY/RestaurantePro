using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenesCompraPendientes;

/// <summary>
/// Handler para obtener órdenes de compra pendientes
/// </summary>
public class ObtenerOrdenesCompraPendientesHandler : IRequestHandler<ObtenerOrdenesCompraPendientesQuery, Result<List<OrdenCompraDto>>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerOrdenesCompraPendientesHandler> _logger;

    public ObtenerOrdenesCompraPendientesHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<ObtenerOrdenesCompraPendientesHandler> logger)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<OrdenCompraDto>>> Handle(
        ObtenerOrdenesCompraPendientesQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo órdenes de compra pendientes");

        try
        {
            // Obtener todas las órdenes de compra como IQueryable
            var query = await _ordenCompraRepository.ObtenerTodasAsync(cancellationToken);
            
            // Filtrar por estado pendiente
            query = query.Where(o => o.Estado == EstadoOrdenCompra.Pendiente);

            // Aplicar filtros adicionales
            if (request.ProveedorId.HasValue)
            {
                query = query.Where(o => o.ProveedorId == request.ProveedorId.Value);
            }

            if (request.FechaDesde.HasValue)
            {
                query = query.Where(o => o.FechaEmision >= request.FechaDesde.Value);
            }

            if (request.FechaHasta.HasValue)
            {
                query = query.Where(o => o.FechaEmision <= request.FechaHasta.Value);
            }

            // Ordenar por fecha de emisión (más recientes primero)
            query = query.OrderByDescending(o => o.FechaEmision);

            // Aplicar límite si se especifica
            if (request.Limite.HasValue && request.Limite.Value > 0)
            {
                query = query.Take(request.Limite.Value);
            }

            // Ejecutar la consulta
            var ordenesPendientes = await query.ToListAsync(cancellationToken);

            // Mapear a DTOs
            var ordenesCompraDto = _mapper.Map<List<OrdenCompraDto>>(ordenesPendientes);

            _logger.LogInformation("✅ Órdenes de compra pendientes obtenidas exitosamente - {Count} órdenes", 
                ordenesCompraDto.Count);

            return Result.Success(ordenesCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener órdenes de compra pendientes");
            return Result.Failure<List<OrdenCompraDto>>($"Error al obtener órdenes de compra pendientes: {ex.Message}");
        }
    }
} 