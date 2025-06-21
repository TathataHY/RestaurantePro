using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenesCompraPaginadas;

/// <summary>
/// Handler para obtener órdenes de compra con paginación
/// </summary>
public class ObtenerOrdenesCompraPaginadasHandler : IRequestHandler<ObtenerOrdenesCompraPaginadasQuery, Result<PaginatedList<OrdenCompraDto>>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerOrdenesCompraPaginadasHandler> _logger;

    public ObtenerOrdenesCompraPaginadasHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<ObtenerOrdenesCompraPaginadasHandler> logger)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<OrdenCompraDto>>> Handle(
        ObtenerOrdenesCompraPaginadasQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo órdenes de compra paginadas - Página {PageNumber}, Tamaño {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // Obtener todas las órdenes de compra como IQueryable
            var query = await _ordenCompraRepository.ObtenerTodasAsync(cancellationToken);

            // Aplicar filtros
            query = AplicarFiltros(query, request);

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // Contar total de registros
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var ordenesCompraDto = _mapper.Map<List<OrdenCompraDto>>(items);

            // Crear resultado paginado
            var result = new PaginatedList<OrdenCompraDto>
            {
                Items = ordenesCompraDto,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            _logger.LogInformation("✅ Órdenes de compra obtenidas exitosamente - {Count} de {TotalCount} total", 
                ordenesCompraDto.Count, totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener órdenes de compra paginadas");
            return Result.Failure<PaginatedList<OrdenCompraDto>>($"Error al obtener órdenes de compra: {ex.Message}");
        }
    }

    private static IQueryable<OrdenCompra> AplicarFiltros(IQueryable<OrdenCompra> query, ObtenerOrdenesCompraPaginadasQuery request)
    {
        // Filtro por término de búsqueda
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(o => 
                o.Id.ToString().ToLower().Contains(searchTerm) ||
                o.Observaciones.ToLower().Contains(searchTerm));
        }

        // Filtro por estado
        if (request.Estado.HasValue)
        {
            query = query.Where(o => o.Estado == request.Estado.Value);
        }

        // Filtro por proveedor
        if (request.ProveedorId.HasValue)
        {
            query = query.Where(o => o.ProveedorId == request.ProveedorId.Value);
        }

        // Filtro por rango de fechas
        if (request.FechaDesde.HasValue)
        {
            query = query.Where(o => o.FechaEmision >= request.FechaDesde.Value);
        }

        if (request.FechaHasta.HasValue)
        {
            query = query.Where(o => o.FechaEmision <= request.FechaHasta.Value);
        }

        return query;
    }

    private static IQueryable<OrdenCompra> AplicarOrdenamiento(IQueryable<OrdenCompra> query, ObtenerOrdenesCompraPaginadasQuery request)
    {
        var sortBy = request.SortBy?.ToLower() ?? "fechaemision";
        var sortDirection = request.SortDirection?.ToLower() ?? "desc";

        return sortBy switch
        {
            "fechaemision" => sortDirection == "asc" 
                ? query.OrderBy(o => o.FechaEmision)
                : query.OrderByDescending(o => o.FechaEmision),
            
            "id" => sortDirection == "asc"
                ? query.OrderBy(o => o.Id)
                : query.OrderByDescending(o => o.Id),
            
            "estado" => sortDirection == "asc"
                ? query.OrderBy(o => o.Estado)
                : query.OrderByDescending(o => o.Estado),
            
            "total" => sortDirection == "asc"
                ? query.OrderBy(o => o.Total)
                : query.OrderByDescending(o => o.Total),
            
            "proveedorid" => sortDirection == "asc"
                ? query.OrderBy(o => o.ProveedorId)
                : query.OrderByDescending(o => o.ProveedorId),
            
            _ => query.OrderByDescending(o => o.FechaEmision) // Ordenamiento por defecto
        };
    }
} 