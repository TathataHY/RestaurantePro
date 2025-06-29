using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerPromedioEvaluaciones;

/// <summary>
/// Handler para obtener el promedio de evaluaciones de un proveedor
/// </summary>
public class ObtenerPromedioEvaluacionesQueryHandler : IRequestHandler<ObtenerPromedioEvaluacionesQuery, Result<PromedioEvaluacionesDto>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<ObtenerPromedioEvaluacionesQueryHandler> _logger;

    public ObtenerPromedioEvaluacionesQueryHandler(
        IProveedoresDbContext context,
        ILogger<ObtenerPromedioEvaluacionesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PromedioEvaluacionesDto>> Handle(
        ObtenerPromedioEvaluacionesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Calculando promedio de evaluaciones del proveedor {ProveedorId}. SoloActivas: {SoloActivas}",
                request.ProveedorId, request.SoloActivas);

            // Verificar que el proveedor existe
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == request.ProveedorId, cancellationToken);

            if (proveedor == null)
            {
                _logger.LogWarning("No se encontró el proveedor con ID: {ProveedorId}", request.ProveedorId);
                return Result.Failure<PromedioEvaluacionesDto>("Proveedor no encontrado");
            }

            var query = _context.EvaluacionesProveedores
                .Where(e => e.ProveedorId == request.ProveedorId);

            // Filtrar por estado activo si se solicita
            if (request.SoloActivas)
            {
                query = query.Where(e => e.Activa);
            }

            var evaluaciones = await query.ToListAsync(cancellationToken);

            if (!evaluaciones.Any())
            {
                // Retornar promedio vacío si no hay evaluaciones
                var promedioVacio = new PromedioEvaluacionesDto
                {
                    ProveedorId = request.ProveedorId,
                    NombreProveedor = proveedor.Nombre,
                    PromedioGeneral = 0,
                    PromedioCalidad = 0,
                    PromedioPuntualidad = 0,
                    PromedioComunicacion = 0,
                    PromedioPrecios = 0,
                    PromedioPonderado = 0,
                    TotalEvaluaciones = 0,
                    FechaUltimaEvaluacion = null,
                    CalificacionMaxima = 0,
                    CalificacionMinima = 0
                };

                return Result.Success(promedioVacio);
            }

            // Calcular promedios
            var promedioGeneral = (decimal)evaluaciones.Average(e => e.CalificacionGeneral);
            var promedioCalidad = (decimal)evaluaciones.Average(e => e.CalificacionCalidad);
            var promedioPuntualidad = (decimal)evaluaciones.Average(e => e.CalificacionPuntualidad);
            var promedioComunicacion = (decimal)evaluaciones.Average(e => e.CalificacionComunicacion);
            var promedioPrecios = (decimal)evaluaciones.Average(e => e.CalificacionPrecios);
            var promedioPonderado = (decimal)evaluaciones.Average(e => (double)e.PromedioPonderado);

            var fechaUltimaEvaluacion = evaluaciones.Max(e => e.FechaEvaluacion);
            var calificacionMaxima = evaluaciones.Max(e => e.PromedioPonderado);
            var calificacionMinima = evaluaciones.Min(e => e.PromedioPonderado);

            var promedioDto = new PromedioEvaluacionesDto
            {
                ProveedorId = request.ProveedorId,
                NombreProveedor = proveedor.Nombre,
                PromedioGeneral = Math.Round(promedioGeneral, 2),
                PromedioCalidad = Math.Round(promedioCalidad, 2),
                PromedioPuntualidad = Math.Round(promedioPuntualidad, 2),
                PromedioComunicacion = Math.Round(promedioComunicacion, 2),
                PromedioPrecios = Math.Round(promedioPrecios, 2),
                PromedioPonderado = Math.Round(promedioPonderado, 2),
                TotalEvaluaciones = evaluaciones.Count,
                FechaUltimaEvaluacion = fechaUltimaEvaluacion,
                CalificacionMaxima = Math.Round(calificacionMaxima, 2),
                CalificacionMinima = Math.Round(calificacionMinima, 2)
            };

            _logger.LogInformation("Promedio calculado para proveedor {ProveedorId}: {PromedioPonderado} (de {TotalEvaluaciones} evaluaciones)",
                request.ProveedorId, promedioDto.PromedioPonderado, promedioDto.TotalEvaluaciones);

            return Result.Success(promedioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular el promedio de evaluaciones del proveedor {ProveedorId}", request.ProveedorId);
            return Result.Failure<PromedioEvaluacionesDto>("Error interno al calcular el promedio");
        }
    }
} 