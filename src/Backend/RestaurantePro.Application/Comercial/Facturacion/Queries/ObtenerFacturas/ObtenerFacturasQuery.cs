namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturas;

/// <summary>
/// Query para obtener facturas con filtros opcionales
/// Permite filtrar por estado, cliente, fechas y otros criterios
/// </summary>
public class ObtenerFacturasQuery : IRequest<Result<List<FacturaDto>>>
{
    /// <summary>
    /// Filtrar por estado de factura
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Filtrar por cliente específico
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Fecha desde para filtrar
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta para filtrar
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Filtrar por tipo de factura
    /// </summary>
    public string? TipoFactura { get; set; }

    /// <summary>
    /// Filtrar por días de vencimiento
    /// </summary>
    public int? DiasVencimiento { get; set; }

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
    /// Límite de resultados
    /// </summary>
    public int? Limite { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ObtenerFacturasQuery()
    {
    }

    /// <summary>
    /// Factory method para obtener facturas pendientes
    /// </summary>
    public static ObtenerFacturasQuery FacturasPendientes(int? diasVencimiento = null)
    {
        return new ObtenerFacturasQuery
        {
            Estado = "Emitida",
            SoloActivas = true,
            OrdenarPor = "FechaVencimiento",
            DireccionOrdenamiento = "asc",
            DiasVencimiento = diasVencimiento
        };
    }

    /// <summary>
    /// Factory method para obtener facturas por cliente
    /// </summary>
    public static ObtenerFacturasQuery PorCliente(Guid clienteId, bool soloActivas = true)
    {
        return new ObtenerFacturasQuery
        {
            ClienteId = clienteId,
            SoloActivas = soloActivas,
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }

    /// <summary>
    /// Factory method para obtener facturas por rango de fechas
    /// </summary>
    public static ObtenerFacturasQuery PorRangoFechas(DateTime fechaDesde, DateTime fechaHasta)
    {
        return new ObtenerFacturasQuery
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            SoloActivas = true,
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }

    /// <summary>
    /// Factory method para obtener facturas por estado
    /// </summary>
    public static ObtenerFacturasQuery PorEstado(string estado)
    {
        return new ObtenerFacturasQuery
        {
            Estado = estado,
            SoloActivas = true,
            OrdenarPor = "FechaEmision",
            DireccionOrdenamiento = "desc"
        };
    }
} 