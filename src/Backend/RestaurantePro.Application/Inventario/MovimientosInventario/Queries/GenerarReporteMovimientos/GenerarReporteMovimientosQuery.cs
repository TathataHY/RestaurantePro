using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.GenerarReporteMovimientos;

/// <summary>
/// Query para generar reportes de movimientos de inventario
/// </summary>
public class GenerarReporteMovimientosQuery : IRequest<Result<ReporteMovimientosDto>>
{
    /// <summary>
    /// Fecha de inicio del reporte
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin del reporte
    /// </summary>
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Tipo de movimiento específico (opcional)
    /// </summary>
    public TipoMovimientoInventario? TipoMovimiento { get; set; }

    /// <summary>
    /// ID del ingrediente específico (opcional)
    /// </summary>
    public Guid? IngredienteId { get; set; }

    /// <summary>
    /// ID del usuario que solicita el reporte
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Incluir resumen por tipo de movimiento
    /// </summary>
    public bool IncluirResumenPorTipo { get; set; } = true;

    /// <summary>
    /// Incluir resumen por ingrediente
    /// </summary>
    public bool IncluirResumenPorIngrediente { get; set; } = true;

    /// <summary>
    /// Incluir análisis de tendencias
    /// </summary>
    public bool IncluirTendencias { get; set; } = false;

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public GenerarReporteMovimientosQuery() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public GenerarReporteMovimientosQuery(DateTime fechaInicio, DateTime fechaFin, Guid usuarioId)
    {
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Factory method para crear la query
    /// </summary>
    public static GenerarReporteMovimientosQuery Create(DateTime fechaInicio, DateTime fechaFin, Guid usuarioId)
    {
        return new GenerarReporteMovimientosQuery(fechaInicio, fechaFin, usuarioId);
    }
} 