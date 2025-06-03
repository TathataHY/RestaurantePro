namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

/// <summary>
/// DTO de respuesta para el análisis completo de inventario
/// </summary>
public class AnalisisInventarioDto
{
    /// <summary>
    /// Información general del análisis
    /// </summary>
    public InfoAnalisisDto InfoAnalisis { get; set; } = new();

    /// <summary>
    /// Resumen ejecutivo del inventario
    /// </summary>
    public ResumenInventarioDto ResumenExecutivo { get; set; } = new();

    /// <summary>
    /// Análisis detallado por ingrediente
    /// </summary>
    public List<AnalisisIngredienteDto> AnalisisIngredientes { get; set; } = new();

    /// <summary>
    /// Análisis por categorías
    /// </summary>
    public List<AnalisisCategoriaDto> AnalisisCategorias { get; set; } = new();

    /// <summary>
    /// Alertas de inventario
    /// </summary>
    public List<AlertaInventarioDto> Alertas { get; set; } = new();

    /// <summary>
    /// Predicciones de inventario
    /// </summary>
    public PrediccionesInventarioDto? Predicciones { get; set; }

    /// <summary>
    /// Recomendaciones de compra
    /// </summary>
    public List<RecomendacionCompraDto> Recomendaciones { get; set; } = new();

    /// <summary>
    /// Análisis financiero (si se solicitó)
    /// </summary>
    public AnalisisFinancieroDto? AnalisisFinanciero { get; set; }

    /// <summary>
    /// Métricas de eficiencia
    /// </summary>
    public MetricasEficienciaDto MetricasEficiencia { get; set; } = new();
}

/// <summary>
/// DTO con información general del análisis
/// </summary>
public class InfoAnalisisDto
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
    /// Fecha de generación del reporte
    /// </summary>
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tiempo que tomó generar el análisis
    /// </summary>
    public TimeSpan TiempoGeneracion { get; set; }

    /// <summary>
    /// Nivel de detalle del análisis
    /// </summary>
    public string NivelDetalle { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que solicitó el análisis
    /// </summary>
    public string UsuarioSolicitante { get; set; } = string.Empty;

    /// <summary>
    /// Filtros aplicados
    /// </summary>
    public List<string> FiltrosAplicados { get; set; } = new();
}

/// <summary>
/// DTO con resumen ejecutivo del inventario
/// </summary>
public class ResumenInventarioDto
{
    /// <summary>
    /// Total de ingredientes analizados
    /// </summary>
    public int TotalIngredientes { get; set; }

    /// <summary>
    /// Ingredientes en stock óptimo
    /// </summary>
    public int IngredientesEnStock { get; set; }

    /// <summary>
    /// Ingredientes con bajo stock
    /// </summary>
    public int IngredientesBajoStock { get; set; }

    /// <summary>
    /// Ingredientes en estado crítico
    /// </summary>
    public int IngredientesStockCritico { get; set; }

    /// <summary>
    /// Ingredientes sin stock
    /// </summary>
    public int IngredientesSinStock { get; set; }

    /// <summary>
    /// Valor total del inventario
    /// </summary>
    public decimal ValorTotalInventario { get; set; }

    /// <summary>
    /// Valor promedio por ingrediente
    /// </summary>
    public decimal ValorPromedioIngrediente { get; set; }

    /// <summary>
    /// Costo total del período analizado
    /// </summary>
    public decimal CostoTotalPeriodo { get; set; }

    /// <summary>
    /// Total de movimientos en el período
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Tasa de rotación promedio del inventario
    /// </summary>
    public decimal TasaRotacionInventario { get; set; }

    /// <summary>
    /// Tiempo promedio de reposición en días
    /// </summary>
    public decimal TiempoPromedioReposicion { get; set; }

    /// <summary>
    /// Porcentaje de desperdicios
    /// </summary>
    public decimal PorcentajeDesperdicios { get; set; }

    /// <summary>
    /// Estado general del inventario
    /// </summary>
    public string EstadoGeneralInventario { get; set; } = string.Empty;

    /// <summary>
    /// Número de alertas activas
    /// </summary>
    public int AlertasActivas { get; set; }

    /// <summary>
    /// Tendencia general del inventario
    /// </summary>
    public string TendenciaGeneral { get; set; } = string.Empty;
}

/// <summary>
/// DTO con análisis detallado por ingrediente
/// </summary>
public class AnalisisIngredienteDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del ingrediente
    /// </summary>
    public string CategoriaIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Stock actual
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo recomendado
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Stock máximo recomendado
    /// </summary>
    public decimal StockMaximo { get; set; }

    /// <summary>
    /// Estado del stock
    /// </summary>
    public string EstadoStock { get; set; } = string.Empty;

    /// <summary>
    /// Total de entradas en el período
    /// </summary>
    public decimal TotalEntradas { get; set; }

    /// <summary>
    /// Total de salidas en el período
    /// </summary>
    public decimal TotalSalidas { get; set; }

    /// <summary>
    /// Consumo promedio diario
    /// </summary>
    public decimal ConsumoPromedioDiario { get; set; }

    /// <summary>
    /// Costo unitario promedio
    /// </summary>
    public decimal CostoUnitarioPromedio { get; set; }

    /// <summary>
    /// Valor del stock actual
    /// </summary>
    public decimal ValorStockActual { get; set; }

    /// <summary>
    /// Días de stock restante estimados
    /// </summary>
    public int DiasStockRestante { get; set; }

    /// <summary>
    /// Fecha estimada de agotamiento
    /// </summary>
    public DateTime? FechaAgotamientoEstimada { get; set; }

    /// <summary>
    /// Cantidad recomendada para próxima compra
    /// </summary>
    public decimal CantidadRecomendadaCompra { get; set; }

    /// <summary>
    /// Tasa de rotación del ingrediente
    /// </summary>
    public decimal TasaRotacion { get; set; }

    /// <summary>
    /// Porcentaje de desperdicios
    /// </summary>
    public decimal PorcentajeDesperdicios { get; set; }

    /// <summary>
    /// Tendencia de consumo
    /// </summary>
    public string TendenciaConsumo { get; set; } = string.Empty;

    /// <summary>
    /// Alertas específicas del ingrediente
    /// </summary>
    public List<string> AlertasEspecificas { get; set; } = new();

    /// <summary>
    /// Nivel de prioridad para reposición
    /// </summary>
    public string PrioridadReposicion { get; set; } = string.Empty;

    /// <summary>
    /// Código del ingrediente
    /// </summary>
    public string CodigoIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Valor total del inventario para este ingrediente
    /// </summary>
    public decimal ValorInventario { get; set; }

    /// <summary>
    /// Porcentaje del stock óptimo actual
    /// </summary>
    public decimal PorcentajeStockOptimo { get; set; }

    /// <summary>
    /// Total de movimientos en el período analizado
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Consumo total en el período
    /// </summary>
    public decimal ConsumoTotal { get; set; }

    /// <summary>
    /// Ingreso total en el período
    /// </summary>
    public decimal IngresoTotal { get; set; }

    /// <summary>
    /// Tasa de rotación del ingrediente
    /// </summary>
    public decimal RotacionIngrediente { get; set; }

    /// <summary>
    /// Indica si el ingrediente requiere atención inmediata
    /// </summary>
    public bool RequiereAtencion { get; set; }

    /// <summary>
    /// Sugerencia de acción para el ingrediente
    /// </summary>
    public string SugerenciaAccion { get; set; } = string.Empty;
}

/// <summary>
/// DTO con análisis por categoría
/// </summary>
public class AnalisisCategoriaDto
{
    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string NombreCategoria { get; set; } = string.Empty;

    /// <summary>
    /// Total de ingredientes en la categoría
    /// </summary>
    public int TotalIngredientes { get; set; }

    /// <summary>
    /// Ingredientes en stock adecuado
    /// </summary>
    public int IngredientesEnStock { get; set; }

    /// <summary>
    /// Ingredientes con bajo stock
    /// </summary>
    public int IngredientesBajoStock { get; set; }

    /// <summary>
    /// Valor total de la categoría
    /// </summary>
    public decimal ValorTotalCategoria { get; set; }

    /// <summary>
    /// Porcentaje del valor total del inventario
    /// </summary>
    public decimal PorcentajeValorTotal { get; set; }

    /// <summary>
    /// Consumo promedio de la categoría
    /// </summary>
    public decimal ConsumoPromedioCategoria { get; set; }

    /// <summary>
    /// Tasa de rotación de la categoría
    /// </summary>
    public decimal TasaRotacionCategoria { get; set; }

    /// <summary>
    /// Estado general de la categoría
    /// </summary>
    public string EstadoCategoria { get; set; } = string.Empty;

    /// <summary>
    /// Ingredientes críticos en la categoría
    /// </summary>
    public List<string> IngredientesCriticos { get; set; } = new();

    /// <summary>
    /// Recomendaciones específicas para la categoría
    /// </summary>
    public List<string> RecomendacionesCategoria { get; set; } = new();

    /// <summary>
    /// ID de la categoría
    /// </summary>
    public Guid CategoriaId { get; set; }

    /// <summary>
    /// Rotación promedio de la categoría
    /// </summary>
    public decimal RotacionPromedio { get; set; }

    /// <summary>
    /// Alertas activas en la categoría
    /// </summary>
    public int AlertasActivas { get; set; }

    /// <summary>
    /// Tendencia general de la categoría
    /// </summary>
    public string TendenciaCategoria { get; set; } = string.Empty;
}

/// <summary>
/// DTO con información de alertas de inventario
/// </summary>
public class AlertaInventarioDto
{
    /// <summary>
    /// ID del ingrediente (si aplica)
    /// </summary>
    public Guid? IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de alerta
    /// </summary>
    public string TipoAlerta { get; set; } = string.Empty;

    /// <summary>
    /// Título de la alerta
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de prioridad
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de detección
    /// </summary>
    public DateTime FechaDeteccion { get; set; }

    /// <summary>
    /// Valor actual
    /// </summary>
    public decimal? ValorActual { get; set; }

    /// <summary>
    /// Valor esperado
    /// </summary>
    public decimal? ValorEsperado { get; set; }

    /// <summary>
    /// Acción recomendada
    /// </summary>
    public string AccionRecomendada { get; set; } = string.Empty;

    /// <summary>
    /// Impacto estimado
    /// </summary>
    public string ImpactoEstimado { get; set; } = string.Empty;
}

/// <summary>
/// DTO con predicciones de inventario
/// </summary>
public class PrediccionesInventarioDto
{
    /// <summary>
    /// Predicciones por ingrediente
    /// </summary>
    public List<PrediccionIngredienteDto> PrediccionesPorIngrediente { get; set; } = new();

    /// <summary>
    /// Predicciones por categoría
    /// </summary>
    public List<PrediccionCategoriaDto> PrediccionesPorCategoria { get; set; } = new();

    /// <summary>
    /// Predicción general del inventario
    /// </summary>
    public PrediccionGeneralDto PrediccionGeneral { get; set; } = new();

    /// <summary>
    /// Nivel de confiabilidad de las predicciones
    /// </summary>
    public string ConfiabilidadPredicciones { get; set; } = string.Empty;

    /// <summary>
    /// Fecha recomendada para próxima revisión
    /// </summary>
    public DateTime FechaProximaRevision { get; set; }

    /// <summary>
    /// Fecha de la predicción
    /// </summary>
    public DateTime FechaPrediccion { get; set; }

    /// <summary>
    /// Tipo de predicción utilizada
    /// </summary>
    public string TipoPrediccion { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de confianza de la predicción (0-100)
    /// </summary>
    public int NivelConfianza { get; set; }

    /// <summary>
    /// Consumo proyectado para el próximo mes
    /// </summary>
    public decimal ConsumoProximoMes { get; set; }

    /// <summary>
    /// Lista de ingredientes en riesgo de agotamiento
    /// </summary>
    public List<PrediccionIngredienteDto> IngredientesEnRiesgo { get; set; } = new();

    /// <summary>
    /// Tendencia general de consumo
    /// </summary>
    public string TendenciaGeneralConsumo { get; set; } = string.Empty;

    /// <summary>
    /// Recomendaciones automáticas generadas
    /// </summary>
    public List<string> RecomendacionesAutomaticas { get; set; } = new();
}

/// <summary>
/// DTO con predicción por ingrediente
/// </summary>
public class PrediccionIngredienteDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Días restantes de stock
    /// </summary>
    public int DiasRestantesStock { get; set; }

    /// <summary>
    /// Fecha estimada de agotamiento
    /// </summary>
    public DateTime FechaAgotamientoEstimada { get; set; }

    /// <summary>
    /// Consumo proyectado a 7 días
    /// </summary>
    public decimal ConsumoProyectado7Dias { get; set; }

    /// <summary>
    /// Consumo proyectado a 30 días
    /// </summary>
    public decimal ConsumoProyectado30Dias { get; set; }

    /// <summary>
    /// Cantidad óptima para próximo pedido
    /// </summary>
    public decimal CantidadOptimalPedido { get; set; }

    /// <summary>
    /// Fecha óptima para realizar pedido
    /// </summary>
    public DateTime FechaOptimalPedido { get; set; }

    /// <summary>
    /// Nivel de confiabilidad de la predicción
    /// </summary>
    public decimal ConfiabilidadPrediccion { get; set; }

    /// <summary>
    /// Cantidad recomendada para compra
    /// </summary>
    public decimal CantidadRecomendadaCompra { get; set; }

    /// <summary>
    /// Nivel de riesgo del ingrediente
    /// </summary>
    public string NivelRiesgo { get; set; } = string.Empty;
}

/// <summary>
/// DTO con predicción por categoría
/// </summary>
public class PrediccionCategoriaDto
{
    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string NombreCategoria { get; set; } = string.Empty;

    /// <summary>
    /// Inversión recomendada a 7 días
    /// </summary>
    public decimal InversionRecomendada7Dias { get; set; }

    /// <summary>
    /// Inversión recomendada a 30 días
    /// </summary>
    public decimal InversionRecomendada30Dias { get; set; }

    /// <summary>
    /// Ingredientes críticos proyectados
    /// </summary>
    public int IngredientesCriticosProyectados { get; set; }

    /// <summary>
    /// Ingredientes prioritarios para compra
    /// </summary>
    public List<string> IngredientesPrioritarios { get; set; } = new();
}

/// <summary>
/// DTO con predicción general
/// </summary>
public class PrediccionGeneralDto
{
    /// <summary>
    /// Inversión total recomendada
    /// </summary>
    public decimal InversionTotalRecomendada { get; set; }

    /// <summary>
    /// Días de autonomía promedio
    /// </summary>
    public int DiasAutonomiaPropedio { get; set; }

    /// <summary>
    /// Nivel de riesgo de desabastecimiento
    /// </summary>
    public decimal RiesgoDesabastecimiento { get; set; }

    /// <summary>
    /// Recomendación general
    /// </summary>
    public string RecomendacionGeneral { get; set; } = string.Empty;

    /// <summary>
    /// Acciones prioritarias
    /// </summary>
    public List<string> AccionesPrioritarias { get; set; } = new();
}

/// <summary>
/// DTO con recomendación de compra
/// </summary>
public class RecomendacionCompraDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad recomendada
    /// </summary>
    public decimal CantidadRecomendada { get; set; }

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Costo estimado
    /// </summary>
    public decimal CostoEstimado { get; set; }

    /// <summary>
    /// Prioridad de la compra
    /// </summary>
    public string PrioridadCompra { get; set; } = string.Empty;

    /// <summary>
    /// Fecha recomendada para el pedido
    /// </summary>
    public DateTime FechaRecomendadaPedido { get; set; }

    /// <summary>
    /// Justificación de la recomendación
    /// </summary>
    public string Justificacion { get; set; } = string.Empty;

    /// <summary>
    /// Proveedores recomendados
    /// </summary>
    public List<string> ProveedoresRecomendados { get; set; } = new();

    /// <summary>
    /// Ahorro estimado
    /// </summary>
    public decimal AhorroEstimado { get; set; }

    /// <summary>
    /// Impacto en el servicio si no se compra
    /// </summary>
    public string ImpactoSinCompra { get; set; } = string.Empty;
}

/// <summary>
/// DTO con análisis financiero
/// </summary>
public class AnalisisFinancieroDto
{
    /// <summary>
    /// Inversión total actual en inventario
    /// </summary>
    public decimal InversionTotalActual { get; set; }

    /// <summary>
    /// Costo total del período
    /// </summary>
    public decimal CostoTotalPeriodo { get; set; }

    /// <summary>
    /// Costo promedio diario
    /// </summary>
    public decimal CostoPromedioDiario { get; set; }

    /// <summary>
    /// Ahorros potenciales identificados
    /// </summary>
    public decimal AhorrosPotenciales { get; set; }

    /// <summary>
    /// Pérdidas por desperdicios
    /// </summary>
    public decimal PerdidaDesperdicios { get; set; }

    /// <summary>
    /// Costos por categoría
    /// </summary>
    public List<CostoCategoriaDto> CostosPorCategoria { get; set; } = new();

    /// <summary>
    /// Oportunidades de ahorro
    /// </summary>
    public List<OportunidadAhorroDto> OportunidadesAhorro { get; set; } = new();

    /// <summary>
    /// Resumen financiero
    /// </summary>
    public string ResumenFinanciero { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del análisis
    /// </summary>
    public DateTime FechaAnalisis { get; set; }

    /// <summary>
    /// Valor total del inventario
    /// </summary>
    public decimal ValorTotalInventario { get; set; }

    /// <summary>
    /// Lista de costos detallados por ingrediente
    /// </summary>
    public List<CostoDetalladoDto> CostosDetallados { get; set; } = new();

    /// <summary>
    /// Top 10 ingredientes más caros
    /// </summary>
    public List<CostoDetalladoDto> Top10IngredientesMasCaros { get; set; } = new();

    /// <summary>
    /// Distribución de costos por rangos
    /// </summary>
    public DistribucionCostosDto DistribucionCostos { get; set; } = new();

    /// <summary>
    /// Análisis de ROI del inventario
    /// </summary>
    public AnalisisROIDto AnalisisROI { get; set; } = new();

    /// <summary>
    /// Recomendaciones financieras
    /// </summary>
    public List<string> RecomendacionesFinancieras { get; set; } = new();
}

/// <summary>
/// DTO con costo por categoría
/// </summary>
public class CostoCategoriaDto
{
    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string NombreCategoria { get; set; } = string.Empty;

    /// <summary>
    /// Costo total de la categoría
    /// </summary>
    public decimal CostoTotal { get; set; }

    /// <summary>
    /// Porcentaje del costo total
    /// </summary>
    public decimal PorcentajeCostoTotal { get; set; }

    /// <summary>
    /// Costo promedio por ingrediente
    /// </summary>
    public decimal CostoPromedioPorIngrediente { get; set; }

    /// <summary>
    /// Variación de costo en el período
    /// </summary>
    public decimal VariacionCostoPeriodo { get; set; }

    /// <summary>
    /// Tendencia de costo
    /// </summary>
    public string TendenciaCosto { get; set; } = string.Empty;
}

/// <summary>
/// DTO con oportunidad de ahorro
/// </summary>
public class OportunidadAhorroDto
{
    /// <summary>
    /// Tipo de oportunidad
    /// </summary>
    public string TipoOportunidad { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la oportunidad
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ahorro estimado
    /// </summary>
    public decimal AhorroEstimado { get; set; }

    /// <summary>
    /// Acción requerida
    /// </summary>
    public string AccionRequerida { get; set; } = string.Empty;

    /// <summary>
    /// Prioridad de implementación
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo estimado de implementación en días
    /// </summary>
    public int TiempoImplementacionDias { get; set; }
}

/// <summary>
/// DTO con métricas de eficiencia del inventario
/// </summary>
public class MetricasEficienciaDto
{
    /// <summary>
    /// Eficiencia general del inventario (0-100)
    /// </summary>
    public decimal EficienciaGeneralInventario { get; set; }

    /// <summary>
    /// Precisión de las predicciones (0-100)
    /// </summary>
    public decimal PrecisionPredicciones { get; set; }

    /// <summary>
    /// Tiempo promedio de reposición en días
    /// </summary>
    public decimal TiempoPromedioReposicion { get; set; }

    /// <summary>
    /// Tasa de rotación global del inventario
    /// </summary>
    public decimal TasaRotacionGlobal { get; set; }

    /// <summary>
    /// Porcentaje de stock en nivel óptimo
    /// </summary>
    public decimal PorcentajeStockOptimo { get; set; }

    /// <summary>
    /// Reducción de desperdicios comparado con período anterior
    /// </summary>
    public decimal ReduccionDesperdicios { get; set; }

    /// <summary>
    /// Clasificación de eficiencia
    /// </summary>
    public string ClasificacionEficiencia { get; set; } = string.Empty;

    /// <summary>
    /// Métricas detalladas individuales
    /// </summary>
    public List<MetricaIndividualDto> MetricasDetalladas { get; set; } = new();

    /// <summary>
    /// Recomendaciones de mejora
    /// </summary>
    public List<string> RecomendacionesMejora { get; set; } = new();
}

/// <summary>
/// DTO con métrica individual
/// </summary>
public class MetricaIndividualDto
{
    /// <summary>
    /// Nombre de la métrica
    /// </summary>
    public string NombreMetrica { get; set; } = string.Empty;

    /// <summary>
    /// Valor actual de la métrica
    /// </summary>
    public decimal ValorActual { get; set; }

    /// <summary>
    /// Valor objetivo
    /// </summary>
    public decimal ValorObjetivo { get; set; }

    /// <summary>
    /// Porcentaje de cumplimiento
    /// </summary>
    public decimal PorcentajeCumplimiento { get; set; }

    /// <summary>
    /// Estado de la métrica
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la métrica
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// DTO para recomendaciones de inventario
/// </summary>
public class RecomendacionInventario
{
    /// <summary>
    /// ID único de la recomendación
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tipo de recomendación
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Prioridad de la recomendación
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la recomendación
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Acción recomendada
    /// </summary>
    public string AccionRecomendada { get; set; } = string.Empty;

    /// <summary>
    /// Impacto estimado si no se toma la acción
    /// </summary>
    public string ImpactoSinAccion { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo estimado para implementar la recomendación
    /// </summary>
    public int TiempoImplementacionDias { get; set; }

    /// <summary>
    /// Costo estimado de implementación
    /// </summary>
    public decimal? CostoImplementacion { get; set; }

    /// <summary>
    /// Beneficio estimado
    /// </summary>
    public decimal? BeneficioEstimado { get; set; }

    /// <summary>
    /// Fecha en que se generó la recomendación
    /// </summary>
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estado de la recomendación
    /// </summary>
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Ingredientes afectados por la recomendación
    /// </summary>
    public List<Guid> IngredientesAfectados { get; set; } = new();

    /// <summary>
    /// Categorías afectadas por la recomendación
    /// </summary>
    public List<string> CategoriasAfectadas { get; set; } = new();
}

/// <summary>
/// DTO para movimientos de stock
/// </summary>
public class MovimientoStock
{
    /// <summary>
    /// ID único del movimiento
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    public TipoMovimientoInventario TipoMovimiento { get; set; }

    /// <summary>
    /// Cantidad del movimiento
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Costo unitario del movimiento
    /// </summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Costo total del movimiento
    /// </summary>
    public decimal CostoTotal { get; set; }

    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    public DateTime FechaMovimiento { get; set; }

    /// <summary>
    /// Motivo del movimiento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que registró el movimiento
    /// </summary>
    public string UsuarioRegistro { get; set; } = string.Empty;

    /// <summary>
    /// Stock anterior al movimiento
    /// </summary>
    public decimal StockAnterior { get; set; }

    /// <summary>
    /// Stock después del movimiento
    /// </summary>
    public decimal StockPosterior { get; set; }

    /// <summary>
    /// Comentarios adicionales
    /// </summary>
    public string? Comentarios { get; set; }

    /// <summary>
    /// ID del documento relacionado (factura, orden de compra, etc.)
    /// </summary>
    public Guid? DocumentoRelacionadoId { get; set; }

    /// <summary>
    /// Tipo de documento relacionado
    /// </summary>
    public string? TipoDocumentoRelacionado { get; set; }
}

/// <summary>
/// DTO con información de costo detallado por ingrediente
/// </summary>
public class CostoDetalladoDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Costo unitario del ingrediente
    /// </summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Cantidad en stock
    /// </summary>
    public decimal CantidadStock { get; set; }

    /// <summary>
    /// Valor total del ingrediente en inventario
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Porcentaje del total del inventario
    /// </summary>
    public decimal PorcentajeDelTotal { get; set; }

    /// <summary>
    /// Código del ingrediente
    /// </summary>
    public string CodigoIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;
}

/// <summary>
/// DTO para distribución de costos por rangos
/// </summary>
public class DistribucionCostosDto
{
    /// <summary>
    /// Valor total de ingredientes de alto costo (>= 10% del total)
    /// </summary>
    public decimal IngredientesAltoCosto { get; set; }

    /// <summary>
    /// Valor total de ingredientes de costo medio (5-10% del total)
    /// </summary>
    public decimal IngredientesCostoMedio { get; set; }

    /// <summary>
    /// Valor total de ingredientes de bajo costo (< 5% del total)
    /// </summary>
    public decimal IngredientesBajoCosto { get; set; }

    /// <summary>
    /// Porcentaje de ingredientes de alto costo
    /// </summary>
    public decimal PorcentajeAltoCosto { get; set; }

    /// <summary>
    /// Porcentaje de ingredientes de costo medio
    /// </summary>
    public decimal PorcentajeCostoMedio { get; set; }

    /// <summary>
    /// Porcentaje de ingredientes de bajo costo
    /// </summary>
    public decimal PorcentajeBajoCosto { get; set; }
}

/// <summary>
/// DTO para análisis de ROI del inventario
/// </summary>
public class AnalisisROIDto
{
    /// <summary>
    /// Rotación de capital en el inventario
    /// </summary>
    public decimal RotacionCapital { get; set; }

    /// <summary>
    /// Días promedio de inventario
    /// </summary>
    public decimal DiasInventarioPromedio { get; set; }

    /// <summary>
    /// Eficiencia del capital (0-100)
    /// </summary>
    public decimal EficienciaCapital { get; set; }

    /// <summary>
    /// ROI estimado del inventario
    /// </summary>
    public decimal ROIEstimado { get; set; }

    /// <summary>
    /// Costo de oportunidad del capital inmovilizado
    /// </summary>
    public decimal CostoOportunidad { get; set; }

    /// <summary>
    /// Recomendación de optimización
    /// </summary>
    public string RecomendacionOptimizacion { get; set; } = string.Empty;
}

/// <summary>
/// DTO con información del stock óptimo para un ingrediente
/// </summary>
public class StockOptimoIngrediente
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de stock óptimo recomendada
    /// </summary>
    public decimal StockOptimo { get; set; }

    /// <summary>
    /// Cantidad actual en stock
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Punto de reorden recomendado
    /// </summary>
    public decimal PuntoReorden { get; set; }

    /// <summary>
    /// Stock de seguridad recomendado
    /// </summary>
    public decimal StockSeguridad { get; set; }

    /// <summary>
    /// Diferencia entre stock actual y óptimo
    /// </summary>
    public decimal DiferenciaStock { get; set; }

    /// <summary>
    /// Indica si el stock actual está dentro del rango óptimo
    /// </summary>
    public bool EnRangoOptimo { get; set; }

    /// <summary>
    /// Recomendación de acción (comprar, reducir, mantener)
    /// </summary>
    public string RecomendacionAccion { get; set; } = string.Empty;

    /// <summary>
    /// Costo estimado de mantener el stock óptimo
    /// </summary>
    public decimal CostoMantenimiento { get; set; }

    /// <summary>
    /// Prioridad de ajuste (Alta, Media, Baja)
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;
} 