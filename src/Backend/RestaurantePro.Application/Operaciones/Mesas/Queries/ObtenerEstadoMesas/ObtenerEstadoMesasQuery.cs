namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerEstadoMesas;

/// <summary>
/// Query para obtener el estado completo de todas las mesas (para dashboard)
/// </summary>
public class ObtenerEstadoMesasQuery : IRequest<Result<EstadoMesasDto>>
{
    /// <summary>
    /// Incluir solo mesas activas
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Zona específica (Interior, Terraza, VIP, etc.)
    /// </summary>
    public string? Zona { get; set; }

    /// <summary>
    /// Incluir estadísticas detalladas
    /// </summary>
    public bool IncluirEstadisticas { get; set; } = true;

    /// <summary>
    /// Incluir información de comandas actuales
    /// </summary>
    public bool IncluirComandas { get; set; } = false;

    /// <summary>
    /// Factory method para consulta básica de dashboard
    /// </summary>
    public static ObtenerEstadoMesasQuery Dashboard()
    {
        return new ObtenerEstadoMesasQuery
        {
            SoloActivas = true,
            IncluirEstadisticas = true,
            IncluirComandas = false
        };
    }

    /// <summary>
    /// Factory method para consulta detallada con comandas
    /// </summary>
    public static ObtenerEstadoMesasQuery Detallado()
    {
        return new ObtenerEstadoMesasQuery
        {
            SoloActivas = true,
            IncluirEstadisticas = true,
            IncluirComandas = true
        };
    }

    /// <summary>
    /// Factory method para consulta por zona específica
    /// </summary>
    public static ObtenerEstadoMesasQuery PorZona(string zona)
    {
        return new ObtenerEstadoMesasQuery
        {
            Zona = zona,
            SoloActivas = true,
            IncluirEstadisticas = true,
            IncluirComandas = false
        };
    }
} 