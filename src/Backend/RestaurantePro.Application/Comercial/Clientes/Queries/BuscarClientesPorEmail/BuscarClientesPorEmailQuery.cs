namespace RestaurantePro.Application.Comercial.Clientes.Queries.BuscarClientesPorEmail;

/// <summary>
/// Query para buscar clientes por email
/// Permite búsqueda exacta o parcial con opciones de filtrado
/// </summary>
public class BuscarClientesPorEmailQuery : IRequest<Result<PaginatedList<ClienteSummaryDto>>>
{
    /// <summary>
    /// Email o parte del email a buscar
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la búsqueda debe ser exacta o parcial
    /// </summary>
    public bool BusquedaExacta { get; set; } = false;

    /// <summary>
    /// Incluir solo clientes activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Número de página para paginación
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    public int TamanoPagina { get; set; } = 20;

    /// <summary>
    /// Campo por el cual ordenar los resultados
    /// </summary>
    public string? OrdenarPor { get; set; }

    /// <summary>
    /// Dirección del ordenamiento (asc, desc)
    /// </summary>
    public string DireccionOrden { get; set; } = "asc";

    /// <summary>
    /// Filtro por dominio de email específico
    /// </summary>
    public string? Dominio { get; set; }

    /// <summary>
    /// Incluir información de fidelización en los resultados
    /// </summary>
    public bool IncluirFidelizacion { get; set; } = false;

    /// <summary>
    /// Filtro por segmento de cliente
    /// </summary>
    public string? Segmento { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public BuscarClientesPorEmailQuery()
    {
    }

    /// <summary>
    /// Constructor con email
    /// </summary>
    public BuscarClientesPorEmailQuery(string email)
    {
        Email = email;
    }

    /// <summary>
    /// Factory method para búsqueda exacta
    /// </summary>
    public static BuscarClientesPorEmailQuery CreateExacta(string email)
    {
        return new BuscarClientesPorEmailQuery
        {
            Email = email,
            BusquedaExacta = true,
            TamanoPagina = 10
        };
    }

    /// <summary>
    /// Factory method para búsqueda parcial
    /// </summary>
    public static BuscarClientesPorEmailQuery CreateParcial(string emailParcial, int tamanoPagina = 20)
    {
        return new BuscarClientesPorEmailQuery
        {
            Email = emailParcial,
            BusquedaExacta = false,
            TamanoPagina = tamanoPagina,
            OrdenarPor = "Email"
        };
    }

    /// <summary>
    /// Factory method para búsqueda por dominio
    /// </summary>
    public static BuscarClientesPorEmailQuery CreatePorDominio(string dominio, int tamanoPagina = 50)
    {
        return new BuscarClientesPorEmailQuery
        {
            Email = "",
            Dominio = dominio,
            BusquedaExacta = false,
            TamanoPagina = tamanoPagina,
            OrdenarPor = "FechaCreacion",
            DireccionOrden = "desc"
        };
    }
} 