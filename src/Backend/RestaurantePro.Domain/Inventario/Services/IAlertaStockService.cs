namespace RestaurantePro.Domain.Inventario.Services;

/// <summary>
/// Servicio de dominio para gestión de alertas de stock
/// </summary>
public interface IAlertaStockService
{
    /// <summary>
    /// Verifica si un ingrediente está en nivel de alerta por stock bajo
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual del ingrediente</param>
    /// <param name="stockMinimo">Stock mínimo configurado</param>
    /// <returns>True si está en alerta, false en caso contrario</returns>
    Task<bool> VerificarAlertaStockAsync(
        Guid ingredienteId,
        decimal stockActual,
        decimal stockMinimo);

    /// <summary>
    /// Evalúa si debe generarse una alerta para un ingrediente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    /// <param name="stockCritico">Stock crítico (opcional)</param>
    /// <returns>Información de la alerta a generar o null si no aplica</returns>
    Task<InfoAlertaStock?> EvaluarNecesidadAlertaAsync(
        Guid ingredienteId,
        decimal stockActual,
        decimal stockMinimo,
        decimal? stockCritico = null);

    /// <summary>
    /// Calcula el nivel de prioridad de una alerta según el stock
    /// </summary>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    /// <param name="stockCritico">Stock crítico</param>
    /// <returns>Nivel de prioridad calculado</returns>
    NivelPrioridadAlerta CalcularNivelPrioridadAlerta(
        decimal stockActual,
        decimal stockMinimo,
        decimal stockCritico);

    /// <summary>
    /// Determina el tipo de alerta según los niveles de stock
    /// </summary>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    /// <param name="stockCritico">Stock crítico</param>
    /// <returns>Tipo de alerta</returns>
    TipoAlertaStock DeterminarTipoAlerta(
        decimal stockActual,
        decimal stockMinimo,
        decimal stockCritico);
}

/// <summary>
/// Información de una alerta de stock a generar
/// </summary>
public class InfoAlertaStock
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Tipo de alerta
    /// </summary>
    public TipoAlertaStock TipoAlerta { get; set; }

    /// <summary>
    /// Nivel de prioridad
    /// </summary>
    public NivelPrioridadAlerta NivelPrioridad { get; set; }

    /// <summary>
    /// Mensaje de la alerta
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Stock actual
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Stock crítico
    /// </summary>
    public decimal? StockCritico { get; set; }

    /// <summary>
    /// Indica si requiere acción inmediata
    /// </summary>
    public bool RequiereAccionInmediata { get; set; }
}

/// <summary>
/// Niveles de prioridad para alertas de stock
/// </summary>
public enum NivelPrioridadAlerta
{
    /// <summary>
    /// Prioridad baja - informativa
    /// </summary>
    Baja = 1,

    /// <summary>
    /// Prioridad media - requiere atención
    /// </summary>
    Media = 2,

    /// <summary>
    /// Prioridad alta - requiere acción pronta
    /// </summary>
    Alta = 3,

    /// <summary>
    /// Prioridad crítica - requiere acción inmediata
    /// </summary>
    Critica = 4
}

/// <summary>
/// Tipos de alerta de stock
/// </summary>
public enum TipoAlertaStock
{
    /// <summary>
    /// Stock bajo - cerca del mínimo
    /// </summary>
    StockBajo = 1,

    /// <summary>
    /// Stock crítico - por debajo del mínimo
    /// </summary>
    StockCritico = 2,

    /// <summary>
    /// Stock agotado - sin existencias
    /// </summary>
    StockAgotado = 3,

    /// <summary>
    /// Stock excesivo - por encima del máximo
    /// </summary>
    StockExcesivo = 4
} 