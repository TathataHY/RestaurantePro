namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerTarjetasFidelizacion;

/// <summary>
/// Query para obtener tarjetas de fidelización con filtros opcionales
/// </summary>
public class ObtenerTarjetasFidelizacionQuery : IRequest<Result<List<TarjetaFidelizacionDto>>>
{
    /// <summary>
    /// Filtrar por estado de tarjeta
    /// </summary>
    public EstadoTarjeta? Estado { get; set; }

    /// <summary>
    /// Filtrar por nivel de fidelización
    /// </summary>
    public NivelFidelizacion? Nivel { get; set; }

    /// <summary>
    /// Filtrar por cliente específico
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Número de página para paginación
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    public int PageSize { get; set; } = 10;
} 