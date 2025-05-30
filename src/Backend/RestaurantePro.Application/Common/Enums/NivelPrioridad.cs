namespace RestaurantePro.Application.Common.Enums;

/// <summary>
/// ⚡ Nivel de prioridad para alertas y notificaciones
/// </summary>
public enum NivelPrioridad
{
    /// <summary>
    /// Prioridad baja - no requiere acción inmediata
    /// </summary>
    Baja = 1,
    
    /// <summary>
    /// Prioridad media - requiere atención en tiempo razonable
    /// </summary>
    Media = 2,
    
    /// <summary>
    /// Prioridad alta - requiere atención urgente
    /// </summary>
    Alta = 3,
    
    /// <summary>
    /// Prioridad crítica - requiere acción inmediata
    /// </summary>
    Critica = 4
} 