using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromociones;

/// <summary>
/// Query para obtener promociones con filtros opcionales
/// </summary>
public class ObtenerPromocionesQuery : IRequest<Result<List<PromocionDto>>>
{
    /// <summary>
    /// Filtro por estado de la promoción
    /// </summary>
    public EstadoPromocion? Estado { get; set; }

    /// <summary>
    /// Filtro por tipo de promoción
    /// </summary>
    public TipoPromocion? Tipo { get; set; }

    /// <summary>
    /// Filtro por código de promoción
    /// </summary>
    public string? Codigo { get; set; }

    /// <summary>
    /// Filtro por nombre de promoción (búsqueda parcial)
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Filtro por fecha de inicio (promociones que empiecen después de esta fecha)
    /// </summary>
    public DateTime? FechaInicioDesde { get; set; }

    /// <summary>
    /// Filtro por fecha de fin (promociones que terminen antes de esta fecha)
    /// </summary>
    public DateTime? FechaFinHasta { get; set; }

    /// <summary>
    /// Filtro por vigencia (solo promociones vigentes)
    /// </summary>
    public bool? SoloVigentes { get; set; }

    /// <summary>
    /// Filtro por acumulabilidad
    /// </summary>
    public bool? EsAcumulable { get; set; }

    /// <summary>
    /// Ordenar por campo
    /// </summary>
    public string? OrdenarPor { get; set; } = "FechaCreacion";

    /// <summary>
    /// Dirección del ordenamiento
    /// </summary>
    public string? DireccionOrdenamiento { get; set; } = "desc";

    /// <summary>
    /// Número de página para paginación
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    public int TamanoPagina { get; set; } = 20;
} 