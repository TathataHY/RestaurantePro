using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;

/// <summary>
/// Query para obtener clientes con paginación y filtros avanzados
/// Optimizada para dashboards comerciales y búsquedas administrativas
/// </summary>
public class ObtenerClientesPaginadosQuery : IRequest<Result<PaginatedList<ClienteSummaryDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (registros por página)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filtro de texto libre (busca en nombre, email, teléfono)
    /// </summary>
    public string? FiltroTexto { get; set; }

    /// <summary>
    /// Filtro por estado (null = todos, true = activos, false = inactivos)
    /// </summary>
    public bool? SoloActivos { get; set; }

    /// <summary>
    /// Filtro por segmento de cliente
    /// </summary>
    public string? Segmento { get; set; }

    /// <summary>
    /// Filtro por presencia de tarjeta de fidelización
    /// </summary>
    public bool? SoloConTarjetaFidelizacion { get; set; }

    /// <summary>
    /// Filtro por clientes frecuentes (más de 10 visitas)
    /// </summary>
    public bool SoloClientesFrecuentes { get; set; } = false;

    /// <summary>
    /// Fecha de registro desde (filtro por fecha de creación)
    /// </summary>
    public DateTime? FechaRegistroDesde { get; set; }

    /// <summary>
    /// Fecha de registro hasta (filtro por fecha de creación)
    /// </summary>
    public DateTime? FechaRegistroHasta { get; set; }

    /// <summary>
    /// Campo por el cual ordenar los resultados
    /// </summary>
    public string? OrdenarPor { get; set; } = "FechaCreacion";

    /// <summary>
    /// Dirección del ordenamiento (asc, desc)
    /// </summary>
    public string? DireccionOrden { get; set; } = "desc";

    /// <summary>
    /// Constructor vacío para model binding
    /// </summary>
    public ObtenerClientesPaginadosQuery() { }

    /// <summary>
    /// Constructor para búsquedas básicas
    /// </summary>
    public ObtenerClientesPaginadosQuery(int pageNumber, int pageSize, string? filtroTexto = null, bool? soloActivos = true)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        FiltroTexto = filtroTexto;
        SoloActivos = soloActivos;
    }

    /// <summary>
    /// Factory method para búsqueda de clientes activos
    /// </summary>
    public static ObtenerClientesPaginadosQuery CrearParaActivos(int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize, soloActivos: true);

    /// <summary>
    /// Factory method para búsqueda de clientes frecuentes
    /// </summary>
    public static ObtenerClientesPaginadosQuery CrearParaFrecuentes(int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize, soloActivos: true) { SoloClientesFrecuentes = true };

    /// <summary>
    /// Factory method para búsqueda por texto específico
    /// </summary>
    public static ObtenerClientesPaginadosQuery CrearParaBusqueda(string filtroTexto, int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize, filtroTexto, soloActivos: true);

    /// <summary>
    /// Factory method para clientes con tarjeta de fidelización
    /// </summary>
    public static ObtenerClientesPaginadosQuery CrearParaConTarjeta(int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize, soloActivos: true) { SoloConTarjetaFidelizacion = true };
} 