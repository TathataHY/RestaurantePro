namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;

/// <summary>
/// Query para obtener ingredientes paginados con filtros avanzados
/// Ideal para pantallas de gestión y reportes de inventario
/// </summary>
public class ObtenerIngredientesPaginadosQuery : IRequest<Result<PaginatedList<IngredienteSummaryDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (máximo 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filtro de texto libre (nombre, código, descripción)
    /// </summary>
    public string? FiltroTexto { get; set; }

    /// <summary>
    /// Filtro por rotación específica
    /// </summary>
    public string? FiltroRotacion { get; set; }

    /// <summary>
    /// Filtro por temporada específica
    /// </summary>
    public string? FiltroTemporada { get; set; }

    /// <summary>
    /// Filtro por unidad de medida
    /// </summary>
    public string? FiltroUnidadMedida { get; set; }

    /// <summary>
    /// Solo ingredientes activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Solo ingredientes con stock disponible
    /// </summary>
    public bool SoloConStock { get; set; } = false;

    /// <summary>
    /// Solo ingredientes bajo stock mínimo
    /// </summary>
    public bool SoloBajoStock { get; set; } = false;

    /// <summary>
    /// Excluir ingredientes bloqueados por control de calidad
    /// </summary>
    public bool ExcluirBloqueados { get; set; } = true;

    /// <summary>
    /// Filtro por rango de stock actual mínimo
    /// </summary>
    public decimal? StockMinimo { get; set; }

    /// <summary>
    /// Filtro por rango de stock actual máximo
    /// </summary>
    public decimal? StockMaximo { get; set; }

    /// <summary>
    /// Filtro por rango de costo mínimo
    /// </summary>
    public decimal? CostoMinimo { get; set; }

    /// <summary>
    /// Filtro por rango de costo máximo
    /// </summary>
    public decimal? CostoMaximo { get; set; }

    /// <summary>
    /// Filtro por proveedor principal
    /// </summary>
    public Guid? ProveedorPrincipalId { get; set; }

    /// <summary>
    /// Campo de ordenamiento
    /// </summary>
    public string OrdenarPor { get; set; } = "Nombre";

    /// <summary>
    /// Dirección del ordenamiento (Asc/Desc)
    /// </summary>
    public string DireccionOrden { get; set; } = "Asc";

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerIngredientesPaginadosQuery() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ObtenerIngredientesPaginadosQuery(int pageNumber, int pageSize, string? filtroTexto = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        FiltroTexto = filtroTexto;
    }

    /// <summary>
    /// Factory method para búsqueda básica
    /// </summary>
    public static ObtenerIngredientesPaginadosQuery Crear(int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize);

    /// <summary>
    /// Factory method para búsqueda con texto
    /// </summary>
    public static ObtenerIngredientesPaginadosQuery CrearConFiltro(string filtroTexto, int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize, filtroTexto);

    /// <summary>
    /// Factory method para ingredientes bajo stock
    /// </summary>
    public static ObtenerIngredientesPaginadosQuery CrearParaBajoStock(int pageNumber = 1, int pageSize = 50)
        => new(pageNumber, pageSize) 
        { 
            SoloBajoStock = true, 
            OrdenarPor = "PorcentajeStock", 
            DireccionOrden = "Asc" 
        };

    /// <summary>
    /// Factory method para ingredientes sin stock
    /// </summary>
    public static ObtenerIngredientesPaginadosQuery CrearParaSinStock(int pageNumber = 1, int pageSize = 100)
        => new(pageNumber, pageSize) 
        { 
            StockMinimo = 0, 
            StockMaximo = 0, 
            OrdenarPor = "FechaCreacion", 
            DireccionOrden = "Desc" 
        };

    /// <summary>
    /// Factory method para reporte de alta rotación
    /// </summary>
    public static ObtenerIngredientesPaginadosQuery CrearParaAltaRotacion(int pageNumber = 1, int pageSize = 30)
        => new(pageNumber, pageSize) 
        { 
            FiltroRotacion = "Alta", 
            OrdenarPor = "ValorTotalStock", 
            DireccionOrden = "Desc" 
        };
} 