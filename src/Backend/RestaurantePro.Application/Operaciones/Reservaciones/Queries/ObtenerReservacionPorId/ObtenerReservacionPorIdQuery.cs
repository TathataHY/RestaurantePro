namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;

/// <summary>
/// Query para obtener una reservación específica por su ID
/// </summary>
public class ObtenerReservacionPorIdQuery : IRequest<Result<ReservacionDto>>
{
    /// <summary>
    /// ID de la reservación a buscar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Incluir información detallada de cliente y mesa
    /// </summary>
    public bool IncluirDetalles { get; set; } = true;

    /// <summary>
    /// Incluir historial de estados
    /// </summary>
    public bool IncluirHistorial { get; set; } = true;

    /// <summary>
    /// Incluir información de servicios adicionales
    /// </summary>
    public bool IncluirServicios { get; set; } = true;

    /// <summary>
    /// Factory method para consulta básica por ID
    /// </summary>
    public static ObtenerReservacionPorIdQuery Basica(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID de la reservación es requerido", nameof(id));
            
        return new ObtenerReservacionPorIdQuery
        {
            Id = id,
            IncluirDetalles = true,
            IncluirHistorial = false,
            IncluirServicios = false
        };
    }

    /// <summary>
    /// Factory method para consulta completa con toda la información
    /// </summary>
    public static ObtenerReservacionPorIdQuery Completa(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID de la reservación es requerido", nameof(id));
            
        return new ObtenerReservacionPorIdQuery
        {
            Id = id,
            IncluirDetalles = true,
            IncluirHistorial = true,
            IncluirServicios = true
        };
    }

    /// <summary>
    /// Factory method para consulta simplificada sin detalles
    /// </summary>
    public static ObtenerReservacionPorIdQuery Simple(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID de la reservación es requerido", nameof(id));
            
        return new ObtenerReservacionPorIdQuery
        {
            Id = id,
            IncluirDetalles = false,
            IncluirHistorial = false,
            IncluirServicios = false
        };
    }
} 