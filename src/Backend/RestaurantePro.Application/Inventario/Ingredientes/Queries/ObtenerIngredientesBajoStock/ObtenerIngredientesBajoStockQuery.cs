namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;

/// <summary>
/// Query para obtener ingredientes con stock bajo
/// Crítico para dashboards operativos y alertas de reposición
/// </summary>
public class ObtenerIngredientesBajoStockQuery : IRequest<Result<List<IngredienteSummaryDto>>>
{
    /// <summary>
    /// Porcentaje del stock mínimo considerado crítico (por defecto 0 = no filtrar)
    /// </summary>
    public decimal PorcentajeCritico { get; set; } = 0;

    /// <summary>
    /// Incluir solo ingredientes activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Filtro por rotación (opcional)
    /// </summary>
    public string? FiltroRotacion { get; set; }

    /// <summary>
    /// Filtro por temporada (opcional)
    /// </summary>
    public string? FiltroTemporada { get; set; }

    /// <summary>
    /// Límite máximo de resultados
    /// </summary>
    public int LimiteResultados { get; set; } = 50;

    /// <summary>
    /// Ordenar por prioridad (más críticos primero)
    /// </summary>
    public bool OrdenarPorPrioridad { get; set; } = true;

    /// <summary>
    /// Incluir ingredientes bloqueados por control de calidad
    /// </summary>
    public bool IncluirBloqueados { get; set; } = false;

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerIngredientesBajoStockQuery() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ObtenerIngredientesBajoStockQuery(decimal porcentajeCritico, bool soloActivos = true)
    {
        PorcentajeCritico = porcentajeCritico;
        SoloActivos = soloActivos;
    }

    /// <summary>
    /// Factory method para alertas críticas (stock < 25% del mínimo)
    /// </summary>
    public static ObtenerIngredientesBajoStockQuery CrearParaAlertasCriticas()
        => new(25, true) { LimiteResultados = 20 };

    /// <summary>
    /// Factory method para reporte de reposición (stock < 50% del mínimo)
    /// </summary>
    public static ObtenerIngredientesBajoStockQuery CrearParaReporteReposicion()
        => new(50, true) { LimiteResultados = 100 };

    /// <summary>
    /// Factory method para dashboard ejecutivo (stock < 40% del mínimo)
    /// </summary>
    public static ObtenerIngredientesBajoStockQuery CrearParaDashboard()
        => new(40, true) { LimiteResultados = 30, OrdenarPorPrioridad = true };

    /// <summary>
    /// Factory method para ingredientes de alta rotación bajo stock
    /// </summary>
    public static ObtenerIngredientesBajoStockQuery CrearParaAltaRotacion()
        => new(30, true) { FiltroRotacion = "Alta", LimiteResultados = 25 };
} 