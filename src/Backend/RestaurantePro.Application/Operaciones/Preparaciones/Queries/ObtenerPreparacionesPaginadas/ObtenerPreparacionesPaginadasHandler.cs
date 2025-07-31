using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesPaginadas;

/// <summary>
/// Handler para obtener preparaciones con paginación
/// </summary>
public class ObtenerPreparacionesPaginadasHandler : IRequestHandler<ObtenerPreparacionesPaginadasQuery, Result<PaginatedList<PreparacionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerPreparacionesPaginadasHandler> _logger;

    public ObtenerPreparacionesPaginadasHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerPreparacionesPaginadasHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<PreparacionDto>>> Handle(
        ObtenerPreparacionesPaginadasQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo preparaciones paginadas - Página {PageNumber}, Tamaño {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // Construir la consulta base
            var query = _context.PreparacionesDiarias.AsQueryable();

            // Aplicar filtros
            query = AplicarFiltros(query, request);

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // Obtener el total de registros
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var preparaciones = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var preparacionesDto = _mapper.Map<List<PreparacionDto>>(preparaciones);

            // Crear resultado paginado
            var result = new PaginatedList<PreparacionDto>(
                preparacionesDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Preparaciones paginadas obtenidas exitosamente - {Count} de {TotalCount}", 
                preparacionesDto.Count, totalCount);

            return Result<PaginatedList<PreparacionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener preparaciones paginadas");
            return Result.Failure<PaginatedList<PreparacionDto>>($"Error al obtener preparaciones paginadas: {ex.Message}");
        }
    }

    private IQueryable<Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria> AplicarFiltros(
        IQueryable<Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria> query, 
        ObtenerPreparacionesPaginadasQuery request)
    {
        // Filtro por término de búsqueda (buscar en observaciones)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Observaciones.Contains(request.SearchTerm));
        }

        // Filtro por estado
        if (request.Estado.HasValue)
        {
            query = query.Where(p => p.Estado == request.Estado.Value);
        }

        // Filtro por producto
        if (request.ProductoId.HasValue)
        {
            query = query.Where(p => p.ProductoId == request.ProductoId.Value);
        }

        // Filtro por chef
        if (request.ChefId.HasValue)
        {
            query = query.Where(p => p.ChefId == request.ChefId.Value);
        }

        // Filtro por fecha desde
        if (request.FechaDesde.HasValue)
        {
            query = query.Where(p => p.FechaPreparacion >= request.FechaDesde.Value);
        }

        // Filtro por fecha hasta
        if (request.FechaHasta.HasValue)
        {
            query = query.Where(p => p.FechaPreparacion <= request.FechaHasta.Value);
        }

        return query;
    }

    private IQueryable<Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria> AplicarOrdenamiento(
        IQueryable<Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria> query, 
        ObtenerPreparacionesPaginadasQuery request)
    {
        return request.SortBy?.ToLower() switch
        {
            "fecha" => request.SortDirection == "desc" 
                ? query.OrderByDescending(p => p.FechaPreparacion)
                : query.OrderBy(p => p.FechaPreparacion),
            "estado" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.Estado)
                : query.OrderBy(p => p.Estado),
            "vencimiento" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.FechaVencimiento)
                : query.OrderBy(p => p.FechaVencimiento),
            "cantidad" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.CantidadDisponible)
                : query.OrderBy(p => p.CantidadDisponible),
            _ => query.OrderByDescending(p => p.FechaPreparacion) // Ordenamiento por defecto
        };
    }
} 