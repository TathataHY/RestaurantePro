using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientoPorId;

/// <summary>
/// Query para obtener un movimiento de inventario específico por su ID
/// </summary>
public class ObtenerMovimientoPorIdQuery : IRequest<Result<MovimientoInventarioDto>>
{
    /// <summary>
    /// ID del movimiento a buscar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerMovimientoPorIdQuery() { }

    /// <summary>
    /// Constructor con ID del movimiento
    /// </summary>
    public ObtenerMovimientoPorIdQuery(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Factory method para crear la query
    /// </summary>
    public static ObtenerMovimientoPorIdQuery Create(Guid id)
    {
        return new ObtenerMovimientoPorIdQuery(id);
    }
} 