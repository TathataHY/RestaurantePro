namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturasPorCliente;

/// <summary>
/// Query para obtener facturas de un cliente específico
/// Incluye filtros por estado y ordenamiento
/// </summary>
public class ObtenerFacturasPorClienteQuery : IRequest<Result<List<FacturaDto>>>
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Filtrar solo facturas activas (no anuladas)
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Filtrar por estado específico
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Fecha desde para filtrar
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta para filtrar
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Ordenar por campo específico
    /// </summary>
    public string OrdenarPor { get; set; } = "FechaEmision";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string DireccionOrdenamiento { get; set; } = "desc";

    /// <summary>
    /// Límite de resultados
    /// </summary>
    public int? Limite { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ObtenerFacturasPorClienteQuery()
    {
    }

    /// <summary>
    /// Constructor con ID del cliente
    /// </summary>
    public ObtenerFacturasPorClienteQuery(Guid clienteId)
    {
        ClienteId = clienteId;
    }

    /// <summary>
    /// Factory method para obtener facturas activas del cliente
    /// </summary>
    public static ObtenerFacturasPorClienteQuery FacturasActivas(Guid clienteId)
    {
        return new ObtenerFacturasPorClienteQuery
        {
            ClienteId = clienteId,
            SoloActivas = true,
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }

    /// <summary>
    /// Factory method para obtener facturas pendientes del cliente
    /// </summary>
    public static ObtenerFacturasPorClienteQuery FacturasPendientes(Guid clienteId)
    {
        return new ObtenerFacturasPorClienteQuery
        {
            ClienteId = clienteId,
            Estado = "Emitida",
            SoloActivas = true,
            OrdenarPor = "FechaVencimiento",
            DireccionOrdenamiento = "asc"
        };
    }

    /// <summary>
    /// Factory method para obtener historial completo del cliente
    /// </summary>
    public static ObtenerFacturasPorClienteQuery HistorialCompleto(Guid clienteId)
    {
        return new ObtenerFacturasPorClienteQuery
        {
            ClienteId = clienteId,
            SoloActivas = false, // Incluir anuladas
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }
} 