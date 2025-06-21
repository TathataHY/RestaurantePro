namespace RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuariosPaginados;

/// <summary>
/// Query para obtener usuarios con paginación y filtros
/// </summary>
public class ObtenerUsuariosPaginadosQuery : IRequest<Result<List<UsuarioDto>>>
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
    /// Término de búsqueda (nombre, email, etc.)
    /// </summary>
    public string? Filtro { get; set; }

    /// <summary>
    /// Filtrar solo usuarios activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string OrderBy { get; set; } = "NombreCompleto";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string OrderDirection { get; set; } = "asc";

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerUsuariosPaginadosQuery() { }

    /// <summary>
    /// Constructor básico con paginación
    /// </summary>
    public ObtenerUsuariosPaginadosQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Factory method para consulta básica paginada
    /// </summary>
    public static ObtenerUsuariosPaginadosQuery ConsultaBasica(
        int pageNumber = 1, 
        int pageSize = 10)
    {
        return new ObtenerUsuariosPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SoloActivos = true,
            OrderBy = "NombreCompleto",
            OrderDirection = "asc"
        };
    }

    /// <summary>
    /// Factory method para búsqueda por término
    /// </summary>
    public static ObtenerUsuariosPaginadosQuery BuscarPorTermino(
        string filtro,
        int pageNumber = 1,
        int pageSize = 20)
    {
        return new ObtenerUsuariosPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Filtro = filtro,
            SoloActivos = true,
            OrderBy = "NombreCompleto",
            OrderDirection = "asc"
        };
    }
} 