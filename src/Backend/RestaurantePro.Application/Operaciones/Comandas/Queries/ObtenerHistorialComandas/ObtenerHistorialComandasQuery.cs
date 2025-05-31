namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerHistorialComandas;

/// <summary>
/// 📋 Query para obtener historial de comandas con filtros avanzados
/// </summary>
public class ObtenerHistorialComandasQuery : IRequest<Result<PaginatedList<ComandaSummaryDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public DateTime? FechaDesde { get; init; }
    public DateTime? FechaHasta { get; init; }
    public Guid? MesaId { get; init; }
    public Guid? MeseroId { get; init; }
    public Guid? ClienteId { get; init; }
    public EstadoComanda? Estado { get; init; }
    public decimal? MontoMinimo { get; init; }
    public decimal? MontoMaximo { get; init; }
    public string? TerminoBusqueda { get; init; }
    public bool IncluirCanceladas { get; init; } = false;
    public bool SoloFinalizadas { get; init; } = true;
    public OrdenHistorial OrdenarPor { get; init; } = OrdenHistorial.FechaMasReciente;

    /// <summary>
    /// Factory method para historial básico
    /// </summary>
    public static ObtenerHistorialComandasQuery Basico(int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerHistorialComandasQuery
        {
            PageNumber = pagina,
            PageSize = tamanoPagina,
            FechaDesde = DateTime.Today.AddDays(-30), // Últimos 30 días
            SoloFinalizadas = true,
            IncluirCanceladas = false,
            OrdenarPor = OrdenHistorial.FechaMasReciente
        };
    }

    /// <summary>
    /// Factory method para historial de una mesa específica
    /// </summary>
    public static ObtenerHistorialComandasQuery PorMesa(
        Guid mesaId, 
        DateTime? fechaDesde = null, 
        int pagina = 1)
    {
        return new ObtenerHistorialComandasQuery
        {
            MesaId = mesaId,
            PageNumber = pagina,
            PageSize = 50,
            FechaDesde = fechaDesde ?? DateTime.Today.AddDays(-7),
            SoloFinalizadas = true,
            OrdenarPor = OrdenHistorial.FechaMasReciente
        };
    }

    /// <summary>
    /// Factory method para historial de un mesero
    /// </summary>
    public static ObtenerHistorialComandasQuery PorMesero(
        Guid meseroId, 
        DateTime? fechaDesde = null, 
        int pagina = 1)
    {
        return new ObtenerHistorialComandasQuery
        {
            MeseroId = meseroId,
            PageNumber = pagina,
            PageSize = 30,
            FechaDesde = fechaDesde ?? DateTime.Today.AddDays(-7),
            SoloFinalizadas = true,
            OrdenarPor = OrdenHistorial.MontoMayor
        };
    }

    /// <summary>
    /// Factory method para historial de un cliente
    /// </summary>
    public static ObtenerHistorialComandasQuery PorCliente(
        Guid clienteId, 
        int pagina = 1, 
        int tamanoPagina = 20)
    {
        return new ObtenerHistorialComandasQuery
        {
            ClienteId = clienteId,
            PageNumber = pagina,
            PageSize = tamanoPagina,
            FechaDesde = null, // Historial completo
            SoloFinalizadas = true,
            IncluirCanceladas = true, // Incluir canceladas para el cliente
            OrdenarPor = OrdenHistorial.FechaMasReciente
        };
    }

    /// <summary>
    /// Factory method para búsqueda por rango de fechas
    /// </summary>
    public static ObtenerHistorialComandasQuery PorRangoFechas(
        DateTime fechaDesde, 
        DateTime fechaHasta, 
        int pagina = 1)
    {
        return new ObtenerHistorialComandasQuery
        {
            FechaDesde = fechaDesde.Date,
            FechaHasta = fechaHasta.Date.AddDays(1).AddSeconds(-1),
            PageNumber = pagina,
            PageSize = 50,
            SoloFinalizadas = true,
            OrdenarPor = OrdenHistorial.FechaMasReciente
        };
    }

    /// <summary>
    /// Factory method para búsqueda por monto
    /// </summary>
    public static ObtenerHistorialComandasQuery PorRangoMonto(
        decimal montoMinimo, 
        decimal montoMaximo, 
        int pagina = 1)
    {
        return new ObtenerHistorialComandasQuery
        {
            MontoMinimo = montoMinimo,
            MontoMaximo = montoMaximo,
            PageNumber = pagina,
            PageSize = 30,
            FechaDesde = DateTime.Today.AddDays(-30),
            SoloFinalizadas = true,
            OrdenarPor = OrdenHistorial.MontoMayor
        };
    }

    /// <summary>
    /// Factory method para análisis completo (incluye todo)
    /// </summary>
    public static ObtenerHistorialComandasQuery ParaAnalisis(
        DateTime? fechaDesde = null, 
        int tamanoPagina = 100)
    {
        return new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = tamanoPagina,
            FechaDesde = fechaDesde ?? DateTime.Today.AddDays(-90),
            SoloFinalizadas = false, // Incluir todos los estados
            IncluirCanceladas = true,
            OrdenarPor = OrdenHistorial.FechaMasReciente
        };
    }
}

/// <summary>
/// 📊 Opciones de ordenamiento para el historial
/// </summary>
public enum OrdenHistorial
{
    FechaMasReciente = 1,
    FechaMasAntigua = 2,
    MontoMayor = 3,
    MontoMenor = 4,
    MesaNombre = 5,
    MeseroNombre = 6,
    ClienteNombre = 7,
    EstadoComanda = 8
} 