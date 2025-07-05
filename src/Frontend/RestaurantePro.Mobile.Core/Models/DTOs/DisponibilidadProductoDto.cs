namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para disponibilidad de producto - Versión móvil simplificada
/// </summary>
public class DisponibilidadProductoDto
{
    /// <summary>
    /// ID del producto verificado
    /// </summary>
    public Guid ProductoId { get; set; }
    
    /// <summary>
    /// Nombre del producto verificado
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el producto está disponible en la cantidad solicitada
    /// </summary>
    public bool EstaDisponible { get; set; }
    
    /// <summary>
    /// Cantidad verificada del producto
    /// </summary>
    public int CantidadVerificada { get; set; }
    
    /// <summary>
    /// Cantidad máxima disponible del producto
    /// </summary>
    public int CantidadDisponible { get; set; }
    
    /// <summary>
    /// Motivo por el cual el producto no está disponible (si aplica)
    /// </summary>
    public string? MotivoNoDisponibilidad { get; set; }
    
    /// <summary>
    /// Fecha de verificación
    /// </summary>
    public DateTime FechaVerificacion { get; set; }
    
    /// <summary>
    /// Tiempo estimado de preparación en minutos
    /// </summary>
    public int TiempoPreparacionMinutos { get; set; }

    // ========================================
    // PROPIEDADES CALCULADAS PARA UI MÓVIL
    // ========================================

    /// <summary>
    /// Estado de disponibilidad para mostrar en UI
    /// </summary>
    public string EstadoDisponibilidad => EstaDisponible switch
    {
        true when CantidadDisponible > 10 => "Disponible",
        true when CantidadDisponible > 0 => "Pocas Unidades",
        _ => "No Disponible"
    };

    /// <summary>
    /// Color para mostrar según disponibilidad
    /// </summary>
    public string ColorDisponibilidad => EstaDisponible switch
    {
        true when CantidadDisponible > 10 => "#4CAF50", // Verde
        true when CantidadDisponible > 0 => "#FF9800",  // Naranja
        _ => "#F44336"                                   // Rojo
    };

    /// <summary>
    /// Icono para mostrar según disponibilidad
    /// </summary>
    public string IconoDisponibilidad => EstaDisponible switch
    {
        true when CantidadDisponible > 10 => "✅",
        true when CantidadDisponible > 0 => "⚠️",
        _ => "❌"
    };

    /// <summary>
    /// Mensaje completo para mostrar al usuario
    /// </summary>
    public string MensajeUsuario => EstaDisponible switch
    {
        true => $"Disponible: {CantidadDisponible} unidades",
        false => MotivoNoDisponibilidad ?? "Producto no disponible"
    };

    /// <summary>
    /// Tiempo de preparación formateado para UI
    /// </summary>
    public string TiempoPreparacionFormateado => TiempoPreparacionMinutos switch
    {
        <= 5 => "⚡ Muy Rápido (5 min)",
        <= 15 => "🕒 Rápido (15 min)",
        <= 30 => "⏳ Normal (30 min)",
        _ => $"⏰ {TiempoPreparacionMinutos} minutos"
    };
} 