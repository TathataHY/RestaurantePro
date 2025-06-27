using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosPorTipo;

/// <summary>
/// Query para obtener movimientos de inventario por tipo específico
/// </summary>
public class ObtenerMovimientosPorTipoQuery : IRequest<Result<PaginatedList<MovimientoInventarioDto>>>
{
    /// <summary>
    /// Tipo de movimiento a filtrar
    /// </summary>
    public TipoMovimientoInventario TipoMovimiento { get; set; }

    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (máximo 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Fecha de inicio para filtrar movimientos
    /// </summary>
    public DateTime? FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin para filtrar movimientos
    /// </summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Filtro por ID de ingrediente
    /// </summary>
    public Guid? IngredienteId { get; set; }

    /// <summary>
    /// Ordenar por campo específico
    /// </summary>
    public string OrdenarPor { get; set; } = "Fecha";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string DireccionOrdenamiento { get; set; } = "desc";

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerMovimientosPorTipoQuery() { }

    /// <summary>
    /// Constructor con tipo de movimiento
    /// </summary>
    public ObtenerMovimientosPorTipoQuery(TipoMovimientoInventario tipoMovimiento)
    {
        TipoMovimiento = tipoMovimiento;
    }

    /// <summary>
    /// Factory method para crear la query
    /// </summary>
    public static ObtenerMovimientosPorTipoQuery Create(TipoMovimientoInventario tipoMovimiento)
    {
        return new ObtenerMovimientosPorTipoQuery(tipoMovimiento);
    }
} 