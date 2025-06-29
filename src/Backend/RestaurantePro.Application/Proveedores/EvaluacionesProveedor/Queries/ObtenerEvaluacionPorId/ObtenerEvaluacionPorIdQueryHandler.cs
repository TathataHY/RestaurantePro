using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerEvaluacionPorId;

/// <summary>
/// Handler para obtener una evaluación de proveedor por ID
/// </summary>
public class ObtenerEvaluacionPorIdQueryHandler : IRequestHandler<ObtenerEvaluacionPorIdQuery, Result<EvaluacionProveedorDto>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<ObtenerEvaluacionPorIdQueryHandler> _logger;

    public ObtenerEvaluacionPorIdQueryHandler(
        IProveedoresDbContext context,
        ILogger<ObtenerEvaluacionPorIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<EvaluacionProveedorDto>> Handle(
        ObtenerEvaluacionPorIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo evaluación de proveedor con ID: {Id}", request.Id);

            var evaluacion = await _context.EvaluacionesProveedores
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (evaluacion == null)
            {
                _logger.LogWarning("No se encontró la evaluación con ID: {Id}", request.Id);
                return Result.Failure<EvaluacionProveedorDto>("Evaluación no encontrada");
            }

            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == evaluacion.ProveedorId, cancellationToken);
            var nombreProveedor = proveedor?.Nombre ?? string.Empty;

            var dto = new EvaluacionProveedorDto
            {
                Id = evaluacion.Id,
                ProveedorId = evaluacion.ProveedorId,
                NombreProveedor = nombreProveedor,
                EvaluadorId = evaluacion.EvaluadorId,
                CalificacionGeneral = evaluacion.CalificacionGeneral,
                CalificacionCalidad = evaluacion.CalificacionCalidad,
                CalificacionPuntualidad = evaluacion.CalificacionPuntualidad,
                CalificacionComunicacion = evaluacion.CalificacionComunicacion,
                CalificacionPrecios = evaluacion.CalificacionPrecios,
                Comentarios = evaluacion.Comentarios,
                FechaEvaluacion = evaluacion.FechaEvaluacion,
                Activa = evaluacion.Activa,
                FechaActualizacion = evaluacion.FechaActualizacion,
                PromedioPonderado = evaluacion.PromedioPonderado,
                CreadoPor = evaluacion.CreatedBy,
                ActualizadoPor = evaluacion.LastModifiedBy
            };

            _logger.LogInformation("Evaluación obtenida exitosamente: {Id}", request.Id);

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la evaluación con ID: {Id}", request.Id);
            return Result.Failure<EvaluacionProveedorDto>("Error interno al obtener la evaluación");
        }
    }
} 