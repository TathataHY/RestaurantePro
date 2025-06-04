namespace RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;

/// <summary>
/// Query para obtener proveedores con paginación y filtros avanzados
/// Permite búsqueda, filtrado y ordenamiento de proveedores
/// </summary>
public class ObtenerProveedoresPaginadosQuery : IRequest<Result<PaginatedList<ProveedorDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Elementos por página
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Término de búsqueda (nombre, RFC, email, ciudad, etc.)
    /// </summary>
    public string? TerminoBusqueda { get; set; }

    /// <summary>
    /// Filtrar solo proveedores activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Filtrar por ciudad específica
    /// </summary>
    public string? Ciudad { get; set; }

    /// <summary>
    /// Filtrar por país específico
    /// </summary>
    public string? Pais { get; set; }

    /// <summary>
    /// Filtrar por días de crédito mínimos
    /// </summary>
    public int? DiasCredito_Min { get; set; }

    /// <summary>
    /// Filtrar por días de crédito máximos
    /// </summary>
    public int? DiasCredito_Max { get; set; }

    /// <summary>
    /// Incluir contactos en el resultado
    /// </summary>
    public bool IncluirContactos { get; set; } = false;

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string CampoOrden { get; set; } = "Nombre";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string DireccionOrden { get; set; } = "asc";

    /// <summary>
    /// Usuario que solicita la consulta
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerProveedoresPaginadosQuery() { }

    /// <summary>
    /// Constructor básico con paginación
    /// </summary>
    public ObtenerProveedoresPaginadosQuery(int pageNumber, int pageSize, Guid usuarioId)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Factory method para consulta básica paginada
    /// </summary>
    public static ObtenerProveedoresPaginadosQuery ConsultaBasica(
        int pageNumber = 1, 
        int pageSize = 10, 
        Guid usuarioId = default)
    {
        return new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SoloActivos = true,
            CampoOrden = "Nombre",
            DireccionOrden = "asc",
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para búsqueda por término
    /// </summary>
    public static ObtenerProveedoresPaginadosQuery BuscarPorTermino(
        string termino,
        int pageNumber = 1,
        int pageSize = 20,
        Guid usuarioId = default)
    {
        return new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TerminoBusqueda = termino,
            SoloActivos = true,
            CampoOrden = "Nombre",
            DireccionOrden = "asc",
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para filtrar por ubicación
    /// </summary>
    public static ObtenerProveedoresPaginadosQuery FiltrarPorUbicacion(
        string? ciudad = null,
        string? pais = null,
        int pageNumber = 1,
        int pageSize = 10,
        Guid usuarioId = default)
    {
        return new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Ciudad = ciudad,
            Pais = pais,
            SoloActivos = true,
            CampoOrden = "Ciudad",
            DireccionOrden = "asc",
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para filtrar por condiciones de crédito
    /// </summary>
    public static ObtenerProveedoresPaginadosQuery FiltrarPorCredito(
        int? diasMin = null,
        int? diasMax = null,
        int pageNumber = 1,
        int pageSize = 10,
        Guid usuarioId = default)
    {
        return new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            DiasCredito_Min = diasMin,
            DiasCredito_Max = diasMax,
            SoloActivos = true,
            CampoOrden = "DiasCredito",
            DireccionOrden = "desc",
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para consulta completa con contactos
    /// </summary>
    public static ObtenerProveedoresPaginadosQuery ConsultaConContactos(
        int pageNumber = 1,
        int pageSize = 5,
        Guid usuarioId = default)
    {
        return new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncluirContactos = true,
            SoloActivos = true,
            CampoOrden = "Nombre",
            DireccionOrden = "asc",
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para obtener todos los proveedores (incluyendo inactivos)
    /// </summary>
    public static ObtenerProveedoresPaginadosQuery ConsultaTodos(
        int pageNumber = 1,
        int pageSize = 15,
        Guid usuarioId = default)
    {
        return new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SoloActivos = false,
            CampoOrden = "FechaRegistro",
            DireccionOrden = "desc",
            UsuarioId = usuarioId
        };
    }
} 