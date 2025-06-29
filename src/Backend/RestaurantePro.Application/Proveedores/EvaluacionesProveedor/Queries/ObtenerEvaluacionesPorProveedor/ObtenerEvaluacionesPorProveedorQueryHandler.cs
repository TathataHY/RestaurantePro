using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerEvaluacionesPorProveedor;

/// <summary>
/// Handler para obtener evaluaciones de un proveedor específico
/// </summary>
public class ObtenerEvaluacionesPorProveedorQueryHandler : IRequestHandler<ObtenerEvaluacionesPorProveedorQuery, Result<List<EvaluacionProveedorDto>>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<ObtenerEvaluacionesPorProveedorQueryHandler> _logger;

    public ObtenerEvaluacionesPorProveedorQueryHandler(
        IProveedoresDbContext context,
        ILogger<ObtenerEvaluacionesPorProveedorQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<EvaluacionProveedorDto>>> Handle(
        ObtenerEvaluacionesPorProveedorQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo evaluaciones del proveedor {ProveedorId}. SoloActivas: {SoloActivas}",
                request.ProveedorId, request.SoloActivas);

            // Verificar que el proveedor existe
            var proveedorExiste = await _context.Proveedores
                .AnyAsync(p => p.Id == request.ProveedorId, cancellationToken);

            if (!proveedorExiste)
            {
                _logger.LogWarning("No se encontró el proveedor con ID: {ProveedorId}", request.ProveedorId);
                return Result.Failure<List<EvaluacionProveedorDto>>("Proveedor no encontrado");
            }

            // Obtener nombre del proveedor
            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == request.ProveedorId, cancellationToken);
            var nombreProveedor = proveedor?.Nombre ?? string.Empty;

            var query = _context.EvaluacionesProveedores
                .Where(e => e.ProveedorId == request.ProveedorId);

            // Filtrar por estado activo si se solicita
            if (request.SoloActivas)
            {
                query = query.Where(e => e.Activa);
            }

            // Ordenar por fecha de evaluación (más reciente primero)
            query = query.OrderByDescending(e => e.FechaEvaluacion);

            // Aplicar paginación
            var evaluaciones = await query
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var dtos = evaluaciones.Select(e => new EvaluacionProveedorDto
            {
                Id = e.Id,
                ProveedorId = e.ProveedorId,
                NombreProveedor = nombreProveedor,
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

            _logger.LogInformation("Se obtuvieron {Count} evaluaciones del proveedor {ProveedorId}", 
                evaluaciones.Count, request.ProveedorId);

            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las evaluaciones del proveedor {ProveedorId}", request.ProveedorId);
            return Result.Failure<List<EvaluacionProveedorDto>>("Error interno al obtener las evaluaciones");
        }
    }
} 