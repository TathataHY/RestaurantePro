using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerTodasEvaluaciones;

/// <summary>
/// Handler para obtener todas las evaluaciones de proveedores
/// </summary>
public class ObtenerTodasEvaluacionesQueryHandler : IRequestHandler<ObtenerTodasEvaluacionesQuery, Result<List<EvaluacionProveedorDto>>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<ObtenerTodasEvaluacionesQueryHandler> _logger;

    public ObtenerTodasEvaluacionesQueryHandler(
        IProveedoresDbContext context,
        ILogger<ObtenerTodasEvaluacionesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<EvaluacionProveedorDto>>> Handle(
        ObtenerTodasEvaluacionesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las evaluaciones de proveedores. SoloActivas: {SoloActivas}, Pagina: {Pagina}, TamanoPagina: {TamanoPagina}",
                request.SoloActivas, request.Pagina, request.TamanoPagina);

            var query = _context.EvaluacionesProveedores
                .AsQueryable();

            // Filtrar por estado activo si se solicita
            if (request.SoloActivas)
            {
                query = query.Where(e => e.Activa);
            }

            // Aplicar ordenamiento
            if (request.OrdenarPor.ToLower() == "proveedor")
            {
                // Ordenar por nombre de proveedor usando join
                query = request.OrdenDescendente
                    ? query.OrderByDescending(e => _context.Proveedores.Where(p => p.Id == e.ProveedorId).Select(p => p.Nombre).FirstOrDefault())
                    : query.OrderBy(e => _context.Proveedores.Where(p => p.Id == e.ProveedorId).Select(p => p.Nombre).FirstOrDefault());
            }
            else if (request.OrdenarPor.ToLower() == "promedioponderado")
            {
                query = request.OrdenDescendente 
                    ? query.OrderByDescending(e => e.PromedioPonderado)
                    : query.OrderBy(e => e.PromedioPonderado);
            }
            else
            {
                query = request.OrdenDescendente 
                    ? query.OrderByDescending(e => e.FechaEvaluacion)
                    : query.OrderBy(e => e.FechaEvaluacion);
            }

            // Obtener IDs de proveedores únicos para las evaluaciones de la página
            var evaluacionesList = await query
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToListAsync(cancellationToken);

            var proveedorIds = evaluacionesList.Select(e => e.ProveedorId).Distinct().ToList();
            var proveedores = await _context.Proveedores
                .Where(p => proveedorIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Nombre, cancellationToken);

            var evaluaciones = await query
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var dtos = evaluaciones.Select(e => new EvaluacionProveedorDto
            {
                Id = e.Id,
                ProveedorId = e.ProveedorId,
                NombreProveedor = proveedores.ContainsKey(e.ProveedorId) ? proveedores[e.ProveedorId] : string.Empty,
                EvaluadorId = e.EvaluadorId,
                CalificacionGeneral = e.CalificacionGeneral,
                CalificacionCalidad = e.CalificacionCalidad,
                CalificacionPuntualidad = e.CalificacionPuntualidad,
                CalificacionComunicacion = e.CalificacionComunicacion,
                CalificacionPrecios = e.CalificacionPrecios,
                Comentarios = e.Comentarios,
                FechaEvaluacion = e.FechaEvaluacion,
                Activa = e.Activa,
                FechaActualizacion = e.FechaActualizacion,
                PromedioPonderado = e.PromedioPonderado,
                CreadoPor = e.CreatedBy,
                ActualizadoPor = e.LastModifiedBy
            }).ToList();

            _logger.LogInformation("Se obtuvieron {Count} evaluaciones de proveedores", evaluaciones.Count);

            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las evaluaciones de proveedores");
            return Result.Failure<List<EvaluacionProveedorDto>>("Error interno al obtener las evaluaciones");
        }
    }
} 