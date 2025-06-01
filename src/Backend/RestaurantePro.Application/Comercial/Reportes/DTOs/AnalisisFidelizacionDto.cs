namespace RestaurantePro.Application.Comercial.Reportes.DTOs;

/// <summary>
/// DTO de respuesta para el análisis completo de fidelización
/// </summary>
public class AnalisisFidelizacionDto
{
    /// <summary>
    /// Información general del análisis
    /// </summary>
    public InfoAnalisisFidelizacionDto InfoAnalisis { get; set; } = new();

    /// <summary>
    /// Resumen ejecutivo de fidelización
    /// </summary>
    public ResumenFidelizacionDto ResumenExecutivo { get; set; } = new();

    /// <summary>
    /// Segmentación RFM de clientes
    /// </summary>
    public SegmentacionRFMDto? SegmentacionRFM { get; set; }

    /// <summary>
    /// Análisis de Customer Lifetime Value (CLV)
    /// </summary>
    public AnalisisCLVDto? AnalisisCLV { get; set; }

    /// <summary>
    /// Patrones de comportamiento de clientes
    /// </summary>
    public PatronesComportamientoDto? PatronesComportamiento { get; set; }

    /// <summary>
    /// Análisis de tendencias de fidelización
    /// </summary>
    public TendenciasFidelizacionDto? Tendencias { get; set; }

    /// <summary>
    /// Métricas de programa de fidelización
    /// </summary>
    public MetricasProgramaFidelizacionDto MetricasPrograma { get; set; } = new();

    /// <summary>
    /// Recomendaciones estratégicas
    /// </summary>
    public List<RecomendacionEstrategicaDto> RecomendacionesEstrategicas { get; set; } = new();

    /// <summary>
    /// Análisis de riesgo de abandono
    /// </summary>
    public AnalisisRiesgoAbandonoDto? AnalisisRiesgo { get; set; }
}

/// <summary>
/// DTO con información general del análisis de fidelización
/// </summary>
public class InfoAnalisisFidelizacionDto
{
    /// <summary>
    /// Fecha de inicio del análisis
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin del análisis
    /// </summary>
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Tipo de análisis realizado
    /// </summary>
    public string TipoAnalisis { get; set; } = string.Empty;

    /// <summary>
    /// Período de análisis
    /// </summary>
    public string PeriodoAnalisis { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de generación del reporte
    /// </summary>
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tiempo de procesamiento
    /// </summary>
    public TimeSpan TiempoProcesamiento { get; set; }

    /// <summary>
    /// Usuario que solicitó el análisis
    /// </summary>
    public string UsuarioSolicitante { get; set; } = string.Empty;

    /// <summary>
    /// Filtros aplicados en el análisis
    /// </summary>
    public List<string> FiltrosAplicados { get; set; } = new();

    /// <summary>
    /// Nivel de confiabilidad del análisis
    /// </summary>
    public string NivelConfiabilidad { get; set; } = string.Empty;
}

/// <summary>
/// DTO con resumen ejecutivo de fidelización
/// </summary>
public class ResumenFidelizacionDto
{
    /// <summary>
    /// Total de clientes analizados
    /// </summary>
    public int TotalClientesAnalizados { get; set; }

    /// <summary>
    /// Clientes activos en el período
    /// </summary>
    public int ClientesActivos { get; set; }

    /// <summary>
    /// Clientes nuevos en el período
    /// </summary>
    public int ClientesNuevos { get; set; }

    /// <summary>
    /// Clientes que abandonaron
    /// </summary>
    public int ClientesAbandonaron { get; set; }

    /// <summary>
    /// Tasa de retención de clientes
    /// </summary>
    public decimal TasaRetencion { get; set; }

    /// <summary>
    /// Tasa de abandono de clientes
    /// </summary>
    public decimal TasaAbandono { get; set; }

    /// <summary>
    /// Valor promedio por cliente
    /// </summary>
    public decimal ValorPromedioCliente { get; set; }

    /// <summary>
    /// Frecuencia promedio de visitas
    /// </summary>
    public decimal FrecuenciaPromedioVisitas { get; set; }

    /// <summary>
    /// Ticket promedio
    /// </summary>
    public decimal TicketPromedio { get; set; }

    /// <summary>
    /// Total de puntos acumulados en el período
    /// </summary>
    public long TotalPuntosAcumulados { get; set; }

    /// <summary>
    /// Total de puntos canjeados en el período
    /// </summary>
    public long TotalPuntosCanjeados { get; set; }

    /// <summary>
    /// Tasa de canje de puntos
    /// </summary>
    public decimal TasaCanjeoPuntos { get; set; }

    /// <summary>
    /// Ingresos totales del período
    /// </summary>
    public decimal IngresosTotales { get; set; }

    /// <summary>
    /// Crecimiento vs período anterior
    /// </summary>
    public decimal CrecimientoVsPeriodoAnterior { get; set; }

    /// <summary>
    /// Estado general del programa de fidelización
    /// </summary>
    public string EstadoGeneralPrograma { get; set; } = string.Empty;
}

/// <summary>
/// DTO con segmentación RFM de clientes
/// </summary>
public class SegmentacionRFMDto
{
    /// <summary>
    /// Distribución de clientes por segmento
    /// </summary>
    public List<SegmentoRFMDto> SegmentosRFM { get; set; } = new();

    /// <summary>
    /// Métricas RFM promedio
    /// </summary>
    public MetricasRFMPromedioDto MetricasPromedio { get; set; } = new();

    /// <summary>
    /// Análisis de migración entre segmentos
    /// </summary>
    public AnalisisMigracionSegmentosDto? AnalisisMigracion { get; set; }

    /// <summary>
    /// Recomendaciones por segmento
    /// </summary>
    public List<RecomendacionSegmentoDto> RecomendacionesPorSegmento { get; set; } = new();
}

/// <summary>
/// DTO con información de un segmento RFM
/// </summary>
public class SegmentoRFMDto
{
    /// <summary>
    /// Nombre del segmento
    /// </summary>
    public string NombreSegmento { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del segmento
    /// </summary>
    public string DescripcionSegmento { get; set; } = string.Empty;

    /// <summary>
    /// Número de clientes en el segmento
    /// </summary>
    public int NumeroClientes { get; set; }

    /// <summary>
    /// Porcentaje del total de clientes
    /// </summary>
    public decimal PorcentajeClientes { get; set; }

    /// <summary>
    /// Valor total del segmento
    /// </summary>
    public decimal ValorTotalSegmento { get; set; }

    /// <summary>
    /// Porcentaje del valor total
    /// </summary>
    public decimal PorcentajeValorTotal { get; set; }

    /// <summary>
    /// Recencia promedio (días desde última compra)
    /// </summary>
    public decimal RecenciaPromedio { get; set; }

    /// <summary>
    /// Frecuencia promedio (número de compras)
    /// </summary>
    public decimal FrecuenciaPromedio { get; set; }

    /// <summary>
    /// Valor monetario promedio
    /// </summary>
    public decimal ValorMonetarioPromedio { get; set; }

    /// <summary>
    /// Características del segmento
    /// </summary>
    public List<string> CaracteristicasSegmento { get; set; } = new();

    /// <summary>
    /// Estrategias recomendadas
    /// </summary>
    public List<string> EstrategiasRecomendadas { get; set; } = new();

    /// <summary>
    /// Nivel de prioridad del segmento
    /// </summary>
    public string NivelPrioridad { get; set; } = string.Empty;
}

/// <summary>
/// DTO con métricas RFM promedio
/// </summary>
public class MetricasRFMPromedioDto
{
    /// <summary>
    /// Recencia promedio general
    /// </summary>
    public decimal RecenciaPromedioGeneral { get; set; }

    /// <summary>
    /// Frecuencia promedio general
    /// </summary>
    public decimal FrecuenciaPromedioGeneral { get; set; }

    /// <summary>
    /// Valor monetario promedio general
    /// </summary>
    public decimal ValorMonetarioPromedioGeneral { get; set; }

    /// <summary>
    /// Score RFM promedio
    /// </summary>
    public decimal ScoreRFMPromedio { get; set; }

    /// <summary>
    /// Distribución de scores RFM
    /// </summary>
    public Dictionary<string, int> DistribucionScores { get; set; } = new();
}

/// <summary>
/// DTO con análisis de migración entre segmentos
/// </summary>
public class AnalisisMigracionSegmentosDto
{
    /// <summary>
    /// Matriz de migración entre segmentos
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> MatrizMigracion { get; set; } = new();

    /// <summary>
    /// Segmentos con mayor crecimiento
    /// </summary>
    public List<string> SegmentosCrecimiento { get; set; } = new();

    /// <summary>
    /// Segmentos con mayor pérdida
    /// </summary>
    public List<string> SegmentosPerdida { get; set; } = new();

    /// <summary>
    /// Tendencias de migración
    /// </summary>
    public List<TendenciaMigracionDto> TendenciasMigracion { get; set; } = new();
}

/// <summary>
/// DTO con tendencia de migración
/// </summary>
public class TendenciaMigracionDto
{
    /// <summary>
    /// Segmento origen
    /// </summary>
    public string SegmentoOrigen { get; set; } = string.Empty;

    /// <summary>
    /// Segmento destino
    /// </summary>
    public string SegmentoDestino { get; set; } = string.Empty;

    /// <summary>
    /// Número de clientes migrados
    /// </summary>
    public int ClientesMigrados { get; set; }

    /// <summary>
    /// Porcentaje de migración
    /// </summary>
    public decimal PorcentajeMigracion { get; set; }

    /// <summary>
    /// Tendencia (Positiva/Negativa)
    /// </summary>
    public string Tendencia { get; set; } = string.Empty;
}

/// <summary>
/// DTO con recomendación por segmento
/// </summary>
public class RecomendacionSegmentoDto
{
    /// <summary>
    /// Nombre del segmento
    /// </summary>
    public string NombreSegmento { get; set; } = string.Empty;

    /// <summary>
    /// Estrategia principal recomendada
    /// </summary>
    public string EstrategiaPrincipal { get; set; } = string.Empty;

    /// <summary>
    /// Acciones específicas
    /// </summary>
    public List<string> AccionesEspecificas { get; set; } = new();

    /// <summary>
    /// Canales de comunicación recomendados
    /// </summary>
    public List<string> CanalesRecomendados { get; set; } = new();

    /// <summary>
    /// Frecuencia de contacto recomendada
    /// </summary>
    public string FrecuenciaContacto { get; set; } = string.Empty;

    /// <summary>
    /// Presupuesto estimado requerido
    /// </summary>
    public decimal? PresupuestoEstimado { get; set; }

    /// <summary>
    /// ROI esperado
    /// </summary>
    public decimal? ROIEsperado { get; set; }
}

/// <summary>
/// DTO con análisis de Customer Lifetime Value
/// </summary>
public class AnalisisCLVDto
{
    /// <summary>
    /// CLV promedio de todos los clientes
    /// </summary>
    public decimal CLVPromedio { get; set; }

    /// <summary>
    /// CLV mediano
    /// </summary>
    public decimal CLVMediano { get; set; }

    /// <summary>
    /// CLV total de la base de clientes
    /// </summary>
    public decimal CLVTotal { get; set; }

    /// <summary>
    /// Distribución de CLV por rangos
    /// </summary>
    public List<RangoCLVDto> DistribucionCLV { get; set; } = new();

    /// <summary>
    /// Top clientes por CLV
    /// </summary>
    public List<ClienteCLVDto> TopClientesCLV { get; set; } = new();

    /// <summary>
    /// CLV por segmento RFM
    /// </summary>
    public List<CLVPorSegmentoDto> CLVPorSegmento { get; set; } = new();

    /// <summary>
    /// Predicciones de CLV
    /// </summary>
    public PrediccionesCLVDto? PrediccionesCLV { get; set; }

    /// <summary>
    /// Factores que influyen en el CLV
    /// </summary>
    public List<FactorInfluenciaCLVDto> FactoresInfluencia { get; set; } = new();
}

/// <summary>
/// DTO con rango de CLV
/// </summary>
public class RangoCLVDto
{
    /// <summary>
    /// Nombre del rango
    /// </summary>
    public string NombreRango { get; set; } = string.Empty;

    /// <summary>
    /// Valor mínimo del rango
    /// </summary>
    public decimal ValorMinimo { get; set; }

    /// <summary>
    /// Valor máximo del rango
    /// </summary>
    public decimal ValorMaximo { get; set; }

    /// <summary>
    /// Número de clientes en el rango
    /// </summary>
    public int NumeroClientes { get; set; }

    /// <summary>
    /// Porcentaje de clientes
    /// </summary>
    public decimal PorcentajeClientes { get; set; }

    /// <summary>
    /// Valor total del rango
    /// </summary>
    public decimal ValorTotalRango { get; set; }
}

/// <summary>
/// DTO con información de cliente por CLV
/// </summary>
public class ClienteCLVDto
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Valor CLV del cliente
    /// </summary>
    public decimal ValorCLV { get; set; }

    /// <summary>
    /// Segmento RFM del cliente
    /// </summary>
    public string SegmentoRFM { get; set; } = string.Empty;

    /// <summary>
    /// Número total de transacciones
    /// </summary>
    public int TotalTransacciones { get; set; }

    /// <summary>
    /// Valor total gastado
    /// </summary>
    public decimal ValorTotalGastado { get; set; }

    /// <summary>
    /// Fecha de primera compra
    /// </summary>
    public DateTime FechaPrimeraCompra { get; set; }

    /// <summary>
    /// Fecha de última compra
    /// </summary>
    public DateTime FechaUltimaCompra { get; set; }

    /// <summary>
    /// Días como cliente
    /// </summary>
    public int DiasComoCliente { get; set; }
}

/// <summary>
/// DTO con CLV por segmento
/// </summary>
public class CLVPorSegmentoDto
{
    /// <summary>
    /// Nombre del segmento
    /// </summary>
    public string NombreSegmento { get; set; } = string.Empty;

    /// <summary>
    /// CLV promedio del segmento
    /// </summary>
    public decimal CLVPromedioSegmento { get; set; }

    /// <summary>
    /// CLV total del segmento
    /// </summary>
    public decimal CLVTotalSegmento { get; set; }

    /// <summary>
    /// Número de clientes en el segmento
    /// </summary>
    public int NumeroClientesSegmento { get; set; }

    /// <summary>
    /// Contribución al CLV total
    /// </summary>
    public decimal ContribucionCLVTotal { get; set; }
}

/// <summary>
/// DTO con predicciones de CLV
/// </summary>
public class PrediccionesCLVDto
{
    /// <summary>
    /// CLV proyectado a 12 meses
    /// </summary>
    public decimal CLVProyectado12Meses { get; set; }

    /// <summary>
    /// CLV proyectado a 24 meses
    /// </summary>
    public decimal CLVProyectado24Meses { get; set; }

    /// <summary>
    /// Crecimiento esperado del CLV
    /// </summary>
    public decimal CrecimientoEsperadoCLV { get; set; }

    /// <summary>
    /// Confiabilidad de las predicciones
    /// </summary>
    public decimal ConfiabilidadPredicciones { get; set; }
}

/// <summary>
/// DTO con factor de influencia en CLV
/// </summary>
public class FactorInfluenciaCLVDto
{
    /// <summary>
    /// Nombre del factor
    /// </summary>
    public string NombreFactor { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de influencia (0-100)
    /// </summary>
    public decimal NivelInfluencia { get; set; }

    /// <summary>
    /// Descripción del impacto
    /// </summary>
    public string DescripcionImpacto { get; set; } = string.Empty;

    /// <summary>
    /// Recomendación para optimizar
    /// </summary>
    public string RecomendacionOptimizacion { get; set; } = string.Empty;
}

/// <summary>
/// DTO con patrones de comportamiento
/// </summary>
public class PatronesComportamientoDto
{
    /// <summary>
    /// Patrones de días preferidos
    /// </summary>
    public List<PatronDiaDto> PatronesDias { get; set; } = new();

    /// <summary>
    /// Patrones de horarios preferidos
    /// </summary>
    public List<PatronHorarioDto> PatronesHorarios { get; set; } = new();

    /// <summary>
    /// Patrones de gasto
    /// </summary>
    public List<PatronGastoDto> PatronesGasto { get; set; } = new();

    /// <summary>
    /// Patrones estacionales
    /// </summary>
    public List<PatronEstacionalDto> PatronesEstacionales { get; set; } = new();

    /// <summary>
    /// Productos más consumidos por segmento
    /// </summary>
    public List<ProductosPorSegmentoDto> ProductosPorSegmento { get; set; } = new();
}

/// <summary>
/// DTO con patrón de día
/// </summary>
public class PatronDiaDto
{
    /// <summary>
    /// Día de la semana
    /// </summary>
    public string DiaSemana { get; set; } = string.Empty;

    /// <summary>
    /// Número de visitas
    /// </summary>
    public int NumeroVisitas { get; set; }

    /// <summary>
    /// Porcentaje de visitas totales
    /// </summary>
    public decimal PorcentajeVisitas { get; set; }

    /// <summary>
    /// Valor promedio por visita
    /// </summary>
    public decimal ValorPromedioVisita { get; set; }

    /// <summary>
    /// Segmentos más activos en este día
    /// </summary>
    public List<string> SegmentosMasActivos { get; set; } = new();
}

/// <summary>
/// DTO con patrón de horario
/// </summary>
public class PatronHorarioDto
{
    /// <summary>
    /// Rango de horario
    /// </summary>
    public string RangoHorario { get; set; } = string.Empty;

    /// <summary>
    /// Número de visitas en el horario
    /// </summary>
    public int NumeroVisitas { get; set; }

    /// <summary>
    /// Porcentaje de visitas totales
    /// </summary>
    public decimal PorcentajeVisitas { get; set; }

    /// <summary>
    /// Ticket promedio en el horario
    /// </summary>
    public decimal TicketPromedio { get; set; }

    /// <summary>
    /// Tipo de cliente predominante
    /// </summary>
    public string TipoClientePredominante { get; set; } = string.Empty;
}

/// <summary>
/// DTO con patrón de gasto
/// </summary>
public class PatronGastoDto
{
    /// <summary>
    /// Rango de gasto
    /// </summary>
    public string RangoGasto { get; set; } = string.Empty;

    /// <summary>
    /// Número de transacciones
    /// </summary>
    public int NumeroTransacciones { get; set; }

    /// <summary>
    /// Porcentaje de transacciones
    /// </summary>
    public decimal PorcentajeTransacciones { get; set; }

    /// <summary>
    /// Valor total del rango
    /// </summary>
    public decimal ValorTotalRango { get; set; }

    /// <summary>
    /// Frecuencia promedio de compra
    /// </summary>
    public decimal FrecuenciaPromedioCompra { get; set; }
}

/// <summary>
/// DTO con patrón estacional
/// </summary>
public class PatronEstacionalDto
{
    /// <summary>
    /// Período estacional
    /// </summary>
    public string PeriodoEstacional { get; set; } = string.Empty;

    /// <summary>
    /// Variación en ventas (%)
    /// </summary>
    public decimal VariacionVentas { get; set; }

    /// <summary>
    /// Variación en frecuencia (%)
    /// </summary>
    public decimal VariacionFrecuencia { get; set; }

    /// <summary>
    /// Productos más demandados
    /// </summary>
    public List<string> ProductosMasDemandados { get; set; } = new();

    /// <summary>
    /// Recomendaciones para el período
    /// </summary>
    public List<string> RecomendacionesPeriodo { get; set; } = new();
}

/// <summary>
/// DTO con productos por segmento
/// </summary>
public class ProductosPorSegmentoDto
{
    /// <summary>
    /// Nombre del segmento
    /// </summary>
    public string NombreSegmento { get; set; } = string.Empty;

    /// <summary>
    /// Productos más consumidos
    /// </summary>
    public List<ProductoConsumoDto> ProductosMasConsumidos { get; set; } = new();

    /// <summary>
    /// Categorías preferidas
    /// </summary>
    public List<string> CategoriasPreferidas { get; set; } = new();

    /// <summary>
    /// Ticket promedio del segmento
    /// </summary>
    public decimal TicketPromedioSegmento { get; set; }
}

/// <summary>
/// DTO con información de consumo de producto
/// </summary>
public class ProductoConsumoDto
{
    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad consumida
    /// </summary>
    public int CantidadConsumida { get; set; }

    /// <summary>
    /// Valor total del producto
    /// </summary>
    public decimal ValorTotalProducto { get; set; }

    /// <summary>
    /// Frecuencia de consumo
    /// </summary>
    public decimal FrecuenciaConsumo { get; set; }

    /// <summary>
    /// Porcentaje de clientes que lo consumen
    /// </summary>
    public decimal PorcentajeClientesConsumen { get; set; }
}

/// <summary>
/// DTO con tendencias de fidelización
/// </summary>
public class TendenciasFidelizacionDto
{
    /// <summary>
    /// Tendencia de retención mensual
    /// </summary>
    public List<TendenciaMensualDto> TendenciaRetencion { get; set; } = new();

    /// <summary>
    /// Tendencia de adquisición de clientes
    /// </summary>
    public List<TendenciaMensualDto> TendenciaAdquisicion { get; set; } = new();

    /// <summary>
    /// Tendencia de valor por cliente
    /// </summary>
    public List<TendenciaMensualDto> TendenciaValorCliente { get; set; } = new();

    /// <summary>
    /// Tendencia de frecuencia de visitas
    /// </summary>
    public List<TendenciaMensualDto> TendenciaFrecuencia { get; set; } = new();

    /// <summary>
    /// Proyecciones futuras
    /// </summary>
    public ProyeccionesFidelizacionDto? Proyecciones { get; set; }
}

/// <summary>
/// DTO con tendencia mensual
/// </summary>
public class TendenciaMensualDto
{
    /// <summary>
    /// Año y mes
    /// </summary>
    public string Periodo { get; set; } = string.Empty;

    /// <summary>
    /// Valor de la métrica
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Variación respecto al período anterior
    /// </summary>
    public decimal VariacionPeriodoAnterior { get; set; }

    /// <summary>
    /// Tendencia (Creciente/Decreciente/Estable)
    /// </summary>
    public string Tendencia { get; set; } = string.Empty;
}

/// <summary>
/// DTO con proyecciones de fidelización
/// </summary>
public class ProyeccionesFidelizacionDto
{
    /// <summary>
    /// Proyección de clientes activos a 6 meses
    /// </summary>
    public int ClientesActivosProyectados6M { get; set; }

    /// <summary>
    /// Proyección de clientes activos a 12 meses
    /// </summary>
    public int ClientesActivosProyectados12M { get; set; }

    /// <summary>
    /// Proyección de ingresos a 6 meses
    /// </summary>
    public decimal IngresosProyectados6M { get; set; }

    /// <summary>
    /// Proyección de ingresos a 12 meses
    /// </summary>
    public decimal IngresosProyectados12M { get; set; }

    /// <summary>
    /// Tasa de crecimiento proyectada
    /// </summary>
    public decimal TasaCrecimientoProyectada { get; set; }
}

/// <summary>
/// DTO con métricas del programa de fidelización
/// </summary>
public class MetricasProgramaFidelizacionDto
{
    /// <summary>
    /// Tasa de participación en el programa
    /// </summary>
    public decimal TasaParticipacion { get; set; }

    /// <summary>
    /// Tasa de activación de tarjetas
    /// </summary>
    public decimal TasaActivacion { get; set; }

    /// <summary>
    /// Tiempo promedio hasta primera compra
    /// </summary>
    public decimal TiempoPromedioPrimeraCompra { get; set; }

    /// <summary>
    /// ROI del programa de fidelización
    /// </summary>
    public decimal ROIPrograma { get; set; }

    /// <summary>
    /// Costo por cliente adquirido
    /// </summary>
    public decimal CostoPorClienteAdquirido { get; set; }

    /// <summary>
    /// Valor de vida del cliente promedio
    /// </summary>
    public decimal ValorVidaClientePromedio { get; set; }

    /// <summary>
    /// Efectividad de campañas
    /// </summary>
    public decimal EfectividadCampanas { get; set; }

    /// <summary>
    /// Satisfacción del cliente (si disponible)
    /// </summary>
    public decimal? SatisfaccionCliente { get; set; }
}

/// <summary>
/// DTO con recomendación estratégica
/// </summary>
public class RecomendacionEstrategicaDto
{
    /// <summary>
    /// Título de la recomendación
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Categoría de la recomendación
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Prioridad de implementación
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;

    /// <summary>
    /// Impacto esperado
    /// </summary>
    public string ImpactoEsperado { get; set; } = string.Empty;

    /// <summary>
    /// Esfuerzo requerido
    /// </summary>
    public string EsfuerzoRequerido { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de implementación estimado
    /// </summary>
    public string TiempoImplementacion { get; set; } = string.Empty;

    /// <summary>
    /// ROI esperado
    /// </summary>
    public decimal? ROIEsperado { get; set; }

    /// <summary>
    /// Métricas a monitorear
    /// </summary>
    public List<string> MetricasMonitorear { get; set; } = new();

    /// <summary>
    /// Pasos de implementación
    /// </summary>
    public List<string> PasosImplementacion { get; set; } = new();
}

/// <summary>
/// DTO con análisis de riesgo de abandono
/// </summary>
public class AnalisisRiesgoAbandonoDto
{
    /// <summary>
    /// Clientes en riesgo alto de abandono
    /// </summary>
    public List<ClienteRiesgoDto> ClientesRiesgoAlto { get; set; } = new();

    /// <summary>
    /// Clientes en riesgo medio de abandono
    /// </summary>
    public List<ClienteRiesgoDto> ClientesRiesgoMedio { get; set; } = new();

    /// <summary>
    /// Factores de riesgo identificados
    /// </summary>
    public List<FactorRiesgoDto> FactoresRiesgo { get; set; } = new();

    /// <summary>
    /// Estrategias de retención recomendadas
    /// </summary>
    public List<EstrategiaRetencionDto> EstrategiasRetencion { get; set; } = new();

    /// <summary>
    /// Valor en riesgo total
    /// </summary>
    public decimal ValorEnRiesgoTotal { get; set; }

    /// <summary>
    /// Porcentaje de la base en riesgo
    /// </summary>
    public decimal PorcentajeBaseEnRiesgo { get; set; }
}

/// <summary>
/// DTO con información de cliente en riesgo
/// </summary>
public class ClienteRiesgoDto
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de riesgo (0-100)
    /// </summary>
    public decimal NivelRiesgo { get; set; }

    /// <summary>
    /// Días desde última compra
    /// </summary>
    public int DiasSinCompra { get; set; }

    /// <summary>
    /// Valor CLV del cliente
    /// </summary>
    public decimal ValorCLV { get; set; }

    /// <summary>
    /// Segmento RFM actual
    /// </summary>
    public string SegmentoRFMActual { get; set; } = string.Empty;

    /// <summary>
    /// Factores de riesgo específicos
    /// </summary>
    public List<string> FactoresRiesgoEspecificos { get; set; } = new();

    /// <summary>
    /// Acciones recomendadas
    /// </summary>
    public List<string> AccionesRecomendadas { get; set; } = new();
}

/// <summary>
/// DTO con factor de riesgo
/// </summary>
public class FactorRiesgoDto
{
    /// <summary>
    /// Nombre del factor
    /// </summary>
    public string NombreFactor { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del factor
    /// </summary>
    public string DescripcionFactor { get; set; } = string.Empty;

    /// <summary>
    /// Peso del factor en el riesgo total
    /// </summary>
    public decimal PesoFactor { get; set; }

    /// <summary>
    /// Número de clientes afectados
    /// </summary>
    public int ClientesAfectados { get; set; }

    /// <summary>
    /// Recomendación para mitigar
    /// </summary>
    public string RecomendacionMitigacion { get; set; } = string.Empty;
}

/// <summary>
/// DTO con estrategia de retención
/// </summary>
public class EstrategiaRetencionDto
{
    /// <summary>
    /// Nombre de la estrategia
    /// </summary>
    public string NombreEstrategia { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la estrategia
    /// </summary>
    public string DescripcionEstrategia { get; set; } = string.Empty;

    /// <summary>
    /// Segmentos objetivo
    /// </summary>
    public List<string> SegmentosObjetivo { get; set; } = new();

    /// <summary>
    /// Canales de implementación
    /// </summary>
    public List<string> CanalesImplementacion { get; set; } = new();

    /// <summary>
    /// Costo estimado de implementación
    /// </summary>
    public decimal CostoEstimado { get; set; }

    /// <summary>
    /// Efectividad esperada (%)
    /// </summary>
    public decimal EfectividadEsperada { get; set; }

    /// <summary>
    /// Tiempo de implementación
    /// </summary>
    public string TiempoImplementacion { get; set; } = string.Empty;

    /// <summary>
    /// ROI esperado
    /// </summary>
    public decimal ROIEsperado { get; set; }
} 