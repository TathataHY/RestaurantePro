using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerColaPreparaciones;

/// <summary>
/// Handler para obtener la cola de preparaciones pendientes
/// </summary>
public class ObtenerColaPreparacionesHandler : IRequestHandler<ObtenerColaPreparacionesQuery, Result<List<PreparacionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerColaPreparacionesHandler> _logger;

    public ObtenerColaPreparacionesHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerColaPreparacionesHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<PreparacionDto>>> Handle(
        ObtenerColaPreparacionesQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo cola de preparaciones - Estado: {Estado}, ChefId: {ChefId}, OrdenarPorPrioridad: {OrdenarPorPrioridad}", 
            request.Estado, request.ChefId, request.OrdenarPorPrioridad);

        try
        {
            // Construir la consulta base
            var query = _context.PreparacionesDiarias.AsQueryable();

            // Aplicar filtros
            if (request.Estado.HasValue)
            {
                query = query.Where(p => p.Estado == request.Estado.Value);
            }

            if (request.ChefId.HasValue)
            {
                query = query.Where(p => p.ChefId == request.ChefId.Value);
            }

            // Filtrar solo preparaciones en proceso o por vencer
            query = query.Where(p => p.Estado == EstadoPreparacion.Preparando || 
                                   p.Estado == EstadoPreparacion.PorVencer);

            // Aplicar ordenamiento
            if (request.OrdenarPorPrioridad)
            {
                query = query.OrderBy(p => p.FechaVencimiento)
                            .ThenBy(p => p.FechaPreparacion);
            }
            else
            {
                query = query.OrderBy(p => p.FechaPreparacion);
            }

            // Aplicar límite si se especifica
            if (request.Limite.HasValue)
            {
                query = query.Take(request.Limite.Value);
            }

            // Ejecutar la consulta
            var preparaciones = await query.ToListAsync(cancellationToken);

            // Mapear a DTOs
            var preparacionesDto = _mapper.Map<List<PreparacionDto>>(preparaciones);

            _logger.LogInformation("✅ Cola de preparaciones obtenida exitosamente - {Count} preparaciones", preparacionesDto.Count);

            return Result<List<PreparacionDto>>.Success(preparacionesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener cola de preparaciones");
            return Result.Failure<List<PreparacionDto>>($"Error al obtener cola de preparaciones: {ex.Message}");
        }
    }
} 