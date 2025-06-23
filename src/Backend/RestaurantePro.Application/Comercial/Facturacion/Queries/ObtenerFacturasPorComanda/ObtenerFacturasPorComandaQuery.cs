namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturasPorComanda;

/// <summary>
/// Query para obtener facturas asociadas a una comanda específica
/// Útil para verificar si una comanda ya fue facturada
/// </summary>
public class ObtenerFacturasPorComandaQuery : IRequest<Result<List<FacturaDto>>>
{
    /// <summary>
    /// ID de la comanda
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Incluir solo facturas activas (no anuladas)
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Ordenar por campo específico
    /// </summary>
    public string OrdenarPor { get; set; } = "FechaEmision";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string DireccionOrdenamiento { get; set; } = "desc";

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ObtenerFacturasPorComandaQuery()
    {
    }

    /// <summary>
    /// Constructor con ID de la comanda
    /// </summary>
    public ObtenerFacturasPorComandaQuery(Guid comandaId)
    {
        ComandaId = comandaId;
    }

    /// <summary>
    /// Factory method para obtener facturas activas de la comanda
    /// </summary>
    public static ObtenerFacturasPorComandaQuery FacturasActivas(Guid comandaId)
    {
        return new ObtenerFacturasPorComandaQuery
        {
            ComandaId = comandaId,
            SoloActivas = true,
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }

    /// <summary>
    /// Factory method para obtener todas las facturas de la comanda (incluyendo anuladas)
    /// </summary>
    public static ObtenerFacturasPorComandaQuery TodasLasFacturas(Guid comandaId)
    {
        return new ObtenerFacturasPorComandaQuery
        {
            ComandaId = comandaId,
            SoloActivas = false,
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }
} 