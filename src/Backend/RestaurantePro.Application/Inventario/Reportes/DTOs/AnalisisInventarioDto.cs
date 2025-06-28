namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

/// <summary>
/// DTO principal para análisis completo de inventario
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
    /// Alertas activas del inventario
    /// </summary>
    public List<AlertaInventarioDto> Alertas { get; set; } = new();

    /// <summary>
    /// Predicciones de inventario (opcional)
    /// </summary>
    public PrediccionesInventarioDto? Predicciones { get; set; }

    /// <summary>
    /// Recomendaciones de compra
    /// </summary>
    public List<RecomendacionCompraDto> Recomendaciones { get; set; } = new();

    /// <summary>
    /// Análisis financiero (opcional)
    /// </summary>
    public AnalisisFinancieroDto? AnalisisFinanciero { get; set; }

    /// <summary>
    /// Métricas de eficiencia
    /// </summary>
    public MetricasEficienciaDto MetricasEficiencia { get; set; } = new();
}

/// <summary>
/// DTO con información del análisis
/// </summary>
public class InfoAnalisisDto
{
    /// <summary>
    /// Fecha de inicio del período analizado
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin del período analizado
    /// </summary>
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Fecha de generación del análisis
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
    /// Filtros aplicados al análisis
    /// </summary>
    public List<string> FiltrosAplicados { get; set; } = new();
}

/// <summary>
/// DTO con resumen ejecutivo del inventario
/// </summary>
public class ResumenInventarioDto
{
    /// <summary>
    /// Total de ingredientes en el sistema
    /// </summary>
    public int TotalIngredientes { get; set; }

    /// <summary>
    /// Ingredientes con stock normal
    /// </summary>
    public int IngredientesEnStock { get; set; }

    /// <summary>
    /// Ingredientes con stock bajo
    /// </summary>
    public int IngredientesBajoStock { get; set; }

    /// <summary>
    /// Ingredientes con stock crítico
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
    /// Costo total del período
    /// </summary>
    public decimal CostoTotalPeriodo { get; set; }

    /// <summary>
    /// Total de movimientos en el período
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Tasa de rotación del inventario
    /// </summary>
    public decimal TasaRotacionInventario { get; set; }

    /// <summary>
    /// Tiempo promedio de reposición
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
/// DTO con análisis detallado de un ingrediente
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
    /// Stock mínimo
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Stock máximo
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
    /// Días de stock restante
    /// </summary>
    public int DiasStockRestante { get; set; }

    /// <summary>
    /// Fecha estimada de agotamiento
    /// </summary>
    public DateTime? FechaAgotamientoEstimada { get; set; }

    /// <summary>
    /// Cantidad recomendada para compra
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
    /// Prioridad de reposición
    /// </summary>
    public string PrioridadReposicion { get; set; } = string.Empty;

    /// <summary>
    /// Código del ingrediente
    /// </summary>
    public string CodigoIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Valor del inventario
    /// </summary>
    public decimal ValorInventario { get; set; }

    /// <summary>
    /// Porcentaje del stock óptimo
    /// </summary>
    public decimal PorcentajeStockOptimo { get; set; }

    /// <summary>
    /// Total de movimientos
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Consumo total
    /// </summary>
    public decimal ConsumoTotal { get; set; }

    /// <summary>
    /// Ingreso total
    /// </summary>
    public decimal IngresoTotal { get; set; }

    /// <summary>
    /// Rotación del ingrediente
    /// </summary>
    public decimal RotacionIngrediente { get; set; }

    /// <summary>
    /// Indica si requiere atención
    /// </summary>
    public bool RequiereAtencion { get; set; }

    /// <summary>
    /// Sugerencia de acción
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
    /// Ingredientes con stock normal
    /// </summary>
    public int IngredientesEnStock { get; set; }

    /// <summary>
    /// Ingredientes con stock bajo
    /// </summary>
    public int IngredientesBajoStock { get; set; }

    /// <summary>
    /// Valor total de la categoría
    /// </summary>
    public decimal ValorTotalCategoria { get; set; }

    /// <summary>
    /// Porcentaje del valor total
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
    /// Estado de la categoría
    /// </summary>
    public string EstadoCategoria { get; set; } = string.Empty;

    /// <summary>
    /// Lista de ingredientes críticos
    /// </summary>
    public List<string> IngredientesCriticos { get; set; } = new();

    /// <summary>
    /// Recomendaciones para la categoría
    /// </summary>
    public List<string> RecomendacionesCategoria { get; set; } = new();

    /// <summary>
    /// ID de la categoría
    /// </summary>
    public Guid CategoriaId { get; set; }

    /// <summary>
    /// Rotación promedio
    /// </summary>
    public decimal RotacionPromedio { get; set; }

    /// <summary>
    /// Alertas activas
    /// </summary>
    public int AlertasActivas { get; set; }

    /// <summary>
    /// Tendencia de la categoría
    /// </summary>
    public string TendenciaCategoria { get; set; } = string.Empty;
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
    /// Consumo proyectado a 7 días
    /// </summary>
    public decimal ConsumoProyectado7Dias { get; set; }

    /// <summary>
    /// Consumo proyectado a 30 días
    /// </summary>
    public decimal ConsumoProyectado30Dias { get; set; }

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
    /// Lista de ingredientes prioritarios
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
    /// Riesgo de desabastecimiento
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
    /// Prioridad de compra
    /// </summary>
    public string PrioridadCompra { get; set; } = string.Empty;

    /// <summary>
    /// Fecha recomendada para pedido
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
    /// Impacto si no se realiza la compra
    /// </summary>
    public string ImpactoSinCompra { get; set; } = string.Empty;
}

/// <summary>
/// DTO con análisis financiero
/// </summary>
public class AnalisisFinancieroDto
{
    /// <summary>
    /// Inversión total actual
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
    /// Ahorros potenciales
    /// </summary>
    public decimal AhorrosPotenciales { get; set; }

    /// <summary>
    /// Pérdida por desperdicios
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
    /// Costos detallados
    /// </summary>
    public List<CostoDetalladoDto> CostosDetallados { get; set; } = new();

    /// <summary>
    /// Top 10 ingredientes más caros
    /// </summary>
    public List<CostoDetalladoDto> Top10IngredientesMasCaros { get; set; } = new();

    /// <summary>
    /// Distribución de costos
    /// </summary>
    public DistribucionCostosDto DistribucionCostos { get; set; } = new();

    /// <summary>
    /// Análisis ROI
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
    /// Costo total
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
    /// Variación del costo en el período
    /// </summary>
    public decimal VariacionCostoPeriodo { get; set; }

    /// <summary>
    /// Tendencia del costo
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
    /// Prioridad
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de implementación en días
    /// </summary>
    public int TiempoImplementacionDias { get; set; }
}

/// <summary>
/// DTO con métricas de eficiencia
/// </summary>
public class MetricasEficienciaDto
{
    /// <summary>
    /// Eficiencia general del inventario
    /// </summary>
    public decimal EficienciaGeneralInventario { get; set; }

    /// <summary>
    /// Precisión de las predicciones
    /// </summary>
    public decimal PrecisionPredicciones { get; set; }

    /// <summary>
    /// Tiempo promedio de reposición
    /// </summary>
    public decimal TiempoPromedioReposicion { get; set; }

    /// <summary>
    /// Tasa de rotación global
    /// </summary>
    public decimal TasaRotacionGlobal { get; set; }

    /// <summary>
    /// Porcentaje de stock óptimo
    /// </summary>
    public decimal PorcentajeStockOptimo { get; set; }

    /// <summary>
    /// Reducción de desperdicios
    /// </summary>
    public decimal ReduccionDesperdicios { get; set; }

    /// <summary>
    /// Clasificación de eficiencia
    /// </summary>
    public string ClasificacionEficiencia { get; set; } = string.Empty;

    /// <summary>
    /// Métricas detalladas
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
    /// Valor actual
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
/// DTO con recomendación de inventario
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
    /// Impacto si no se toma acción
    /// </summary>
    public string ImpactoSinAccion { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de implementación en días
    /// </summary>
    public int TiempoImplementacionDias { get; set; }

    /// <summary>
    /// Costo de implementación
    /// </summary>
    public decimal? CostoImplementacion { get; set; }

    /// <summary>
    /// Beneficio estimado
    /// </summary>
    public decimal? BeneficioEstimado { get; set; }

    /// <summary>
    /// Fecha de generación
    /// </summary>
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estado de la recomendación
    /// </summary>
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Ingredientes afectados
    /// </summary>
    public List<Guid> IngredientesAfectados { get; set; } = new();

    /// <summary>
    /// Categorías afectadas
    /// </summary>
    public List<string> CategoriasAfectadas { get; set; } = new();
}

/// <summary>
/// DTO con movimiento de stock
/// </summary>
public class MovimientoStock
{
    /// <summary>
    /// ID del movimiento
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
    /// Costo unitario
    /// </summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Costo total
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
    /// Stock anterior
    /// </summary>
    public decimal StockAnterior { get; set; }

    /// <summary>
    /// Stock posterior
    /// </summary>
    public decimal StockPosterior { get; set; }

    /// <summary>
    /// Comentarios adicionales
    /// </summary>
    public string? Comentarios { get; set; }

    /// <summary>
    /// ID del documento relacionado
    /// </summary>
    public Guid? DocumentoRelacionadoId { get; set; }

    /// <summary>
    /// Tipo de documento relacionado
    /// </summary>
    public string? TipoDocumentoRelacionado { get; set; }
}

/// <summary>
/// DTO con costo detallado
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
    /// Costo unitario
    /// </summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Cantidad en stock
    /// </summary>
    public decimal CantidadStock { get; set; }

    /// <summary>
    /// Valor total
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Porcentaje del total
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
/// DTO con distribución de costos
/// </summary>
public class DistribucionCostosDto
{
    /// <summary>
    /// Ingredientes de alto costo
    /// </summary>
    public decimal IngredientesAltoCosto { get; set; }

    /// <summary>
    /// Ingredientes de costo medio
    /// </summary>
    public decimal IngredientesCostoMedio { get; set; }

    /// <summary>
    /// Ingredientes de bajo costo
    /// </summary>
    public decimal IngredientesBajoCosto { get; set; }

    /// <summary>
    /// Porcentaje de alto costo
    /// </summary>
    public decimal PorcentajeAltoCosto { get; set; }

    /// <summary>
    /// Porcentaje de costo medio
    /// </summary>
    public decimal PorcentajeCostoMedio { get; set; }

    /// <summary>
    /// Porcentaje de bajo costo
    /// </summary>
    public decimal PorcentajeBajoCosto { get; set; }
}

/// <summary>
/// DTO con análisis ROI
/// </summary>
public class AnalisisROIDto
{
    /// <summary>
    /// Rotación del capital
    /// </summary>
    public decimal RotacionCapital { get; set; }

    /// <summary>
    /// Días de inventario promedio
    /// </summary>
    public decimal DiasInventarioPromedio { get; set; }

    /// <summary>
    /// Eficiencia del capital
    /// </summary>
    public decimal EficienciaCapital { get; set; }

    /// <summary>
    /// ROI estimado
    /// </summary>
    public decimal ROIEstimado { get; set; }

    /// <summary>
    /// Costo de oportunidad
    /// </summary>
    public decimal CostoOportunidad { get; set; }

    /// <summary>
    /// Recomendación de optimización
    /// </summary>
    public string RecomendacionOptimizacion { get; set; } = string.Empty;
}

/// <summary>
/// DTO con stock óptimo de ingrediente
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
    /// Stock óptimo
    /// </summary>
    public decimal StockOptimo { get; set; }

    /// <summary>
    /// Stock actual
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Punto de reorden
    /// </summary>
    public decimal PuntoReorden { get; set; }

    /// <summary>
    /// Stock de seguridad
    /// </summary>
    public decimal StockSeguridad { get; set; }

    /// <summary>
    /// Diferencia de stock
    /// </summary>
    public decimal DiferenciaStock { get; set; }

    /// <summary>
    /// Indica si está en rango óptimo
    /// </summary>
    public bool EnRangoOptimo { get; set; }

    /// <summary>
    /// Recomendación de acción
    /// </summary>
    public string RecomendacionAccion { get; set; } = string.Empty;

    /// <summary>
    /// Costo de mantenimiento
    /// </summary>
    public decimal CostoMantenimiento { get; set; }

    /// <summary>
    /// Prioridad
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;
}

/// <summary>
/// DTO con resultado de exportación
/// </summary>
public class ResultadoExportacionDto
{
    /// <summary>
    /// Fecha de exportación
    /// </summary>
    public DateTime FechaExportacion { get; set; }

    /// <summary>
    /// Formato de exportación
    /// </summary>
    public string FormatoExportacion { get; set; } = string.Empty;

    /// <summary>
    /// Total de registros
    /// </summary>
    public int TotalRegistros { get; set; }

    /// <summary>
    /// Nombre del archivo
    /// </summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>
    /// Datos exportados
    /// </summary>
    public List<DatoExportacionDto> DatosExportados { get; set; } = new();

    /// <summary>
    /// Usuario solicitante
    /// </summary>
    public string UsuarioSolicitante { get; set; } = string.Empty;

    /// <summary>
    /// Tamaño del archivo estimado
    /// </summary>
    public string TamañoArchivoEstimado { get; set; } = string.Empty;
}

/// <summary>
/// DTO con dato de exportación
/// </summary>
public class DatoExportacionDto
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
    /// Código del ingrediente
    /// </summary>
    public string CodigoIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Descripción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Stock actual
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Costo promedio
    /// </summary>
    public decimal CostoPromedio { get; set; }

    /// <summary>
    /// Valor del stock
    /// </summary>
    public decimal ValorStock { get; set; }

    /// <summary>
    /// Estado del stock
    /// </summary>
    public string EstadoStock { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// Rotación
    /// </summary>
    public string Rotacion { get; set; } = string.Empty;

    /// <summary>
    /// Temporada
    /// </summary>
    public string Temporada { get; set; } = string.Empty;

    /// <summary>
    /// Indica si está bloqueado por control de calidad
    /// </summary>
    public bool BloqueadoControlCalidad { get; set; }
} 