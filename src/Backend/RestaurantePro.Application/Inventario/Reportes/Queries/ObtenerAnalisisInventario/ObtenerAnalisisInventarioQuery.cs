using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;

/// <summary>
/// 🎯 Query avanzada para análisis completo de inventario con alertas y predicciones
/// Integra datos de múltiples contextos para análisis inteligente
/// </summary>
public class ObtenerAnalisisInventarioQuery : IRequest<Result<RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisInventarioDto>>
{
    /// <summary>
    /// Fecha desde para el análisis
    /// </summary>
    public DateTime? FechaDesde { get; set; }
    
    /// <summary>
    /// Fecha hasta para el análisis
    /// </summary>
    public DateTime? FechaHasta { get; set; }
    
    /// <summary>
    /// Categorías específicas a incluir en el análisis
    /// </summary>
    public List<string>? Categorias { get; set; }
    
    /// <summary>
    /// Indica si incluir predicciones
    /// </summary>
    public bool IncluirPredicciones { get; set; } = true;
    
    /// <summary>
    /// Indica si incluir análisis financiero
    /// </summary>
    public bool IncluirAnalisisFinanciero { get; set; } = true;

    /// <summary>
    /// ID de categoría específica (opcional)
    /// </summary>
    public Guid? CategoriaId { get; set; }

    /// <summary>
    /// Solo incluir ingredientes con alerta de stock
    /// </summary>
    public bool SoloAlertaStock { get; set; } = false;

    /// <summary>
    /// Solo incluir ingredientes críticos
    /// </summary>
    public bool SoloCriticos { get; set; } = false;

    /// <summary>
    /// Incluir análisis de tendencias
    /// </summary>
    public bool IncluirTendencias { get; set; }

    /// <summary>
    /// Incluir recomendaciones automáticas
    /// </summary>
    public bool IncluirRecomendaciones { get; set; } = true;

    /// <summary>
    /// Nivel de detalle del análisis
    /// </summary>
    public string NivelDetalle { get; set; } = "Completo"; // Completo, Básico, Resumen

    /// <summary>
    /// ID del usuario que solicita el análisis
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Factory method para análisis diario estándar
    /// </summary>
    public static ObtenerAnalisisInventarioQuery CrearAnalisisDiario(
        DateTime? fecha = null,
        bool incluirPredicciones = true,
        bool incluirRecomendaciones = true)
    {
        var fechaAnalisis = fecha ?? DateTime.Today;
        
        return new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = fechaAnalisis,
            FechaHasta = fechaAnalisis,
            IncluirTendencias = true,
            IncluirRecomendaciones = incluirRecomendaciones,
            NivelDetalle = "Completo",
            SoloAlertaStock = false,
            SoloCriticos = false
        };
    }

    /// <summary>
    /// Factory method para análisis semanal completo
    /// </summary>
    public static ObtenerAnalisisInventarioQuery CrearAnalisisSemanal(
        DateTime? fechaInicio = null,
        string nivelDetalle = "Completo")
    {
        var inicio = fechaInicio ?? DateTime.Today.AddDays(-7);
        var fin = inicio.AddDays(7);

        return new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = inicio,
            FechaHasta = fin,
            IncluirTendencias = true,
            IncluirRecomendaciones = true,
            NivelDetalle = nivelDetalle,
            SoloAlertaStock = false,
            SoloCriticos = false
        };
    }

    /// <summary>
    /// Factory method para análisis específico de ingredientes críticos
    /// </summary>
    public static ObtenerAnalisisInventarioQuery CrearAnalisisCriticos(
        Guid? categoriaId = null,
        bool incluirMovimientos = true)
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Today.AddDays(-30),
            FechaHasta = DateTime.Today,
            CategoriaId = categoriaId,
            SoloCriticos = true,
            SoloAlertaStock = true,
            IncluirTendencias = incluirMovimientos,
            IncluirRecomendaciones = true,
            NivelDetalle = "Completo"
        };
    }
}

/// <summary>
/// 🎯 Niveles de análisis de inventario
/// </summary>
public enum NivelAnalisisInventario
{
    Basico = 1,       // Solo stock actual y alertas básicas
    Diario = 2,       // Análisis diario con movimientos principales
    Semanal = 3,      // Análisis semanal con tendencias
    Completo = 4,     // Análisis completo con predicciones
    Criticos = 5,     // Enfocado en ingredientes críticos
    Financiero = 6    // Enfocado en análisis de costos
}

/// <summary>
/// 🎯 Resultado completo del análisis de inventario
/// </summary>
public class AnalisisInventarioResult
{
    // Información del Análisis
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public TimeSpan TiempoGeneracion { get; set; }
    public NivelAnalisisInventario NivelAnalisis { get; set; }

    // Resumen Ejecutivo
    public ResumenInventario ResumenExecutivo { get; set; } = new();

    // Análisis Detallados
    public List<AnalisisIngrediente> AnalisisIngredientes { get; set; } = new();
    public List<AnalisisCategoria> AnalisisCategorias { get; set; } = new();

    // Alertas y Predicciones
    public List<AlertaInventario> AlertasInventario { get; set; } = new();
    public PrediccionesInventario? Predicciones { get; set; }

    // Recomendaciones
    public List<RecomendacionCompra> RecomendacionesCompra { get; set; } = new();

    // Análisis Financiero (si se solicitó)
    public AnalisisFinancieroInventario? AnalisisFinanciero { get; set; }

    // Movimientos Detallados (si se solicitó)
    public List<MovimientoInventarioDetalle>? MovimientosDetallados { get; set; }

    // Métricas de Eficiencia
    public MetricasEficienciaInventario MetricasEficiencia { get; set; } = new();
}

/// <summary>
/// 📊 Resumen ejecutivo del inventario
/// </summary>
public class ResumenInventario
{
    // Stock General
    public int TotalIngredientes { get; set; }
    public int IngredientesEnStock { get; set; }
    public int IngredientesBajoStock { get; set; }
    public int IngredientesStockCritico { get; set; }
    public int IngredientesSinStock { get; set; }

    // Valorización
    public decimal ValorTotalInventario { get; set; }
    public decimal ValorPromedioIngrediente { get; set; }
    public decimal CostoTotalPeriodo { get; set; }

    // Movimientos
    public int TotalMovimientos { get; set; }
    public int MovimientosEntrada { get; set; }
    public int MovimientosSalida { get; set; }
    public int MovimientosAjuste { get; set; }

    // Eficiencia
    public decimal TasaRotacionInventario { get; set; }
    public decimal TiempoPromedioReposicion { get; set; }
    public decimal PorcentajeDesperdicios { get; set; }
    
    // Estado General
    public string EstadoGeneralInventario { get; set; } = string.Empty; // "Óptimo", "Atención", "Crítico"
    public int AlertasActivas { get; set; }
}

/// <summary>
/// 🥬 Análisis detallado por ingrediente
/// </summary>
public class AnalisisIngrediente
{
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public string CategoriaIngrediente { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;

    // Stock Actual
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public decimal StockOptimo { get; set; }
    public EstadoStock EstadoStock { get; set; }

    // Movimientos del Período
    public decimal TotalEntradas { get; set; }
    public decimal TotalSalidas { get; set; }
    public decimal TotalAjustes { get; set; }
    public decimal ConsumoPromedioDiario { get; set; }

    // Análisis Financiero
    public decimal CostoUnitarioPromedio { get; set; }
    public decimal ValorStockActual { get; set; }
    public decimal CostoTotalPeriodo { get; set; }

    // Predicciones
    public int DiasStockRestante { get; set; }
    public DateTime? FechaAgotamientoEstimada { get; set; }
    public decimal CantidadRecomendadaCompra { get; set; }

    // Eficiencia
    public decimal TasaRotacion { get; set; }
    public decimal PorcentajeDesperdicios { get; set; }
    public string TendenciaConsumo { get; set; } = string.Empty; // "Ascendente", "Descendente", "Estable"

    // Alertas específicas
    public List<string> AlertasEspecificas { get; set; } = new();
}

/// <summary>
/// 📂 Análisis por categoría de ingredientes
/// </summary>
public class AnalisisCategoria
{
    public string NombreCategoria { get; set; } = string.Empty;
    public int TotalIngredientes { get; set; }
    public int IngredientesEnStock { get; set; }
    public int IngredientesBajoStock { get; set; }
    public decimal ValorTotalCategoria { get; set; }
    public decimal PorcentajeValorTotal { get; set; }
    public decimal ConsumoPromedioCategoria { get; set; }
    public decimal TasaRotacionCategoria { get; set; }
    public string EstadoCategoria { get; set; } = string.Empty;
    public List<string> IngredientesCriticos { get; set; } = new();
}

/// <summary>
/// ⚠️ Alerta de inventario
/// </summary>
public class AlertaInventario
{
    public Guid? IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public TipoAlertaInventario Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public NivelPrioridad Prioridad { get; set; }
    public DateTime FechaDeteccion { get; set; } = DateTime.UtcNow;
    public decimal? ValorActual { get; set; }
    public decimal? ValorEsperado { get; set; }
    public string AccionRecomendada { get; set; } = string.Empty;
    public Dictionary<string, object> DatosAdicionales { get; set; } = new();
}

/// <summary>
/// 🔮 Predicciones de inventario
/// </summary>
public class PrediccionesInventario
{
    public List<PrediccionIngrediente> PrediccionesPorIngrediente { get; set; } = new();
    public List<PrediccionCategoria> PrediccionesPorCategoria { get; set; } = new();
    public PrediccionGeneral PrediccionGeneral { get; set; } = new();
    public string ConfiabilidadPredicciones { get; set; } = string.Empty; // "Alta", "Media", "Baja"
    public DateTime FechaProximaRevision { get; set; }
}

/// <summary>
/// 🔮 Predicción por ingrediente
/// </summary>
public class PrediccionIngrediente
{
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public int DiasRestantesStock { get; set; }
    public DateTime FechaAgotamientoEstimada { get; set; }
    public decimal ConsumoProyectado7Dias { get; set; }
    public decimal ConsumoProyectado30Dias { get; set; }
    public decimal CantidadOptimalPedido { get; set; }
    public DateTime FechaOptimalPedido { get; set; }
    public decimal ConfiabilidadPrediccion { get; set; }
}

/// <summary>
/// 🔮 Predicción por categoría
/// </summary>
public class PrediccionCategoria
{
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal InversionRecomendada7Dias { get; set; }
    public decimal InversionRecomendada30Dias { get; set; }
    public int IngredientesCriticosProyectados { get; set; }
    public List<string> IngredientesPrioritarios { get; set; } = new();
}

/// <summary>
/// 🔮 Predicción general
/// </summary>
public class PrediccionGeneral
{
    public decimal InversionTotalRecomendada { get; set; }
    public int DiasAutonomiaPropedio { get; set; }
    public decimal RiesgoDesabastecimiento { get; set; }
    public string RecomendacionGeneral { get; set; } = string.Empty;
}

/// <summary>
/// 🛒 Recomendación de compra
/// </summary>
public class RecomendacionCompra
{
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public decimal CantidadRecomendada { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal CostoEstimado { get; set; }
    public NivelPrioridad PrioridadCompra { get; set; }
    public DateTime FechaRecomendadaPedido { get; set; }
    public string Justificacion { get; set; } = string.Empty;
    public List<Guid> ProveedoresRecomendados { get; set; } = new();
    public decimal AhorroEstimado { get; set; }
}

/// <summary>
/// 💰 Análisis financiero del inventario
/// </summary>
public class AnalisisFinancieroInventario
{
    public decimal InversionTotalActual { get; set; }
    public decimal CostoTotalPeriodo { get; set; }
    public decimal CostoPromedioDiario { get; set; }
    public decimal AhorrosPotenciales { get; set; }
    public decimal PerdidaDesperdicios { get; set; }
    public List<AnalisisCostoCategoria> CostosPorCategoria { get; set; } = new();
    public List<OpportunidadAhorro> OportunidadesAhorro { get; set; } = new();
    public string ResumenFinanciero { get; set; } = string.Empty;
}

/// <summary>
/// 💰 Análisis de costo por categoría
/// </summary>
public class AnalisisCostoCategoria
{
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal CostoTotal { get; set; }
    public decimal PorcentajeCostoTotal { get; set; }
    public decimal CostoPromedioPorIngrediente { get; set; }
    public decimal VariacionCostoPeriodo { get; set; }
    public string TendenciaCosto { get; set; } = string.Empty;
}

/// <summary>
/// 💡 Oportunidad de ahorro
/// </summary>
public class OpportunidadAhorro
{
    public string TipoOportunidad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal AhorroEstimado { get; set; }
    public string AccionRequerida { get; set; } = string.Empty;
    public NivelPrioridad Prioridad { get; set; }
    public int TiempoImplementacionDias { get; set; }
}

/// <summary>
/// 📊 Movimiento detallado de inventario
/// </summary>
public class MovimientoInventarioDetalle
{
    public Guid MovimientoId { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario TipoMovimiento { get; set; }
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
}

/// <summary>
/// ⚡ Métricas de eficiencia del inventario
/// </summary>
public class MetricasEficienciaInventario
{
    public decimal EficienciaGeneralInventario { get; set; }
    public decimal PrecisionPredicciones { get; set; }
    public decimal TiempoPromedioReposicion { get; set; }
    public decimal TasaRotacionGlobal { get; set; }
    public decimal PorcentajeStockOptimo { get; set; }
    public decimal ReduccionDesperdicios { get; set; }
    public string ClasificacionEficiencia { get; set; } = string.Empty; // "Excelente", "Buena", "Regular", "Deficiente"
    public List<MetricaIndividual> MetricasDetalladas { get; set; } = new();
}

/// <summary>
/// 📊 Métrica individual
/// </summary>
public class MetricaIndividual
{
    public string NombreMetrica { get; set; } = string.Empty;
    public decimal ValorActual { get; set; }
    public decimal ValorObjetivo { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
    public string Estado { get; set; } = string.Empty; // "Excelente", "Bueno", "Necesita Mejora"
}

/// <summary>
/// 📦 Estado del stock
/// </summary>
public enum EstadoStock
{
    Optimo = 1,
    Suficiente = 2,
    BajoStock = 3,
    StockCritico = 4,
    SinStock = 5,
    Sobrestock = 6
}

/// <summary>
/// 🚨 Tipos de alerta de inventario
/// </summary>
public enum TipoAlertaInventario
{
    StockBajo = 1,
    StockCritico = 2,
    SinStock = 3,
    Sobrestock = 4,
    VencimientoProximo = 5,
    CostoElevado = 6,
    ConsumoAnormal = 7,
    DesviacionPrediccion = 8
}

public class AnalisisInventarioDto
{
    public DateTime FechaGeneracion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int TotalIngredientes { get; set; }
    public decimal ValorTotalInventario { get; set; }
    public decimal ValorPromedioPorIngrediente { get; set; }
    public int IngredientesConMovimiento { get; set; }
    public List<TendenciaInventarioDto>? Tendencias { get; set; }
    public List<CategoriaAnalisisDto> AnalisisPorCategoria { get; set; } = new();
}

public class TendenciaInventarioDto
{
    public DateTime Fecha { get; set; }
    public decimal ValorTotal { get; set; }
    public int CantidadIngredientes { get; set; }
}

public class CategoriaAnalisisDto
{
    public string Categoria { get; set; } = string.Empty;
    public int CantidadIngredientes { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
    public int StockBajo { get; set; }
    public int StockCritico { get; set; }
} 