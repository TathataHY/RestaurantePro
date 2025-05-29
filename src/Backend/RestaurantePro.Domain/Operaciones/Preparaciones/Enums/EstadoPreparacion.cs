namespace RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

/// <summary>
/// Estados posibles de una preparación diaria
/// </summary>
public enum EstadoPreparacion
{
    /// <summary>
    /// La preparación está en proceso
    /// </summary>
    Preparando = 1,
    
    /// <summary>
    /// La preparación está lista y disponible para ser consumida
    /// </summary>
    Disponible = 2,
    
    /// <summary>
    /// La preparación está por vencer (últimas horas)
    /// </summary>
    PorVencer = 3,
    
    /// <summary>
    /// La preparación ha vencido y no debe ser consumida
    /// </summary>
    Vencida = 4,
    
    /// <summary>
    /// La preparación se ha agotado completamente
    /// </summary>
    Agotada = 5
} 