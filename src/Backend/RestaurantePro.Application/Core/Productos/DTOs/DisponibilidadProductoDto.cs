namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO para representar la disponibilidad de un producto
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
    /// ID de la comanda asociada (opcional)
    /// </summary>
    public Guid? ComandaAsociadaId { get; set; }
    
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
    
    /// <summary>
    /// Prioridad de preparación (1-10)
    /// </summary>
    public int PrioridadPreparacion { get; set; }
    
    /// <summary>
    /// Análisis de ingredientes necesarios
    /// </summary>
    public List<AnalisisIngredienteDto> AnalisisIngredientes { get; set; } = new();
    
    /// <summary>
    /// Productos alternativos disponibles
    /// </summary>
    public List<ProductoSummaryDto> AlternativasDisponibles { get; set; } = new();
}

/// <summary>
/// DTO para análisis de disponibilidad de ingredientes
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
    /// Indica si el ingrediente está disponible
    /// </summary>
    public bool EstaDisponible { get; set; }
    
    /// <summary>
    /// Cantidad necesaria
    /// </summary>
    public decimal CantidadNecesaria { get; set; }
    
    /// <summary>
    /// Cantidad disponible
    /// </summary>
    public decimal CantidadDisponible { get; set; }
    
    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;
    
    /// <summary>
    /// Porcentaje de disponibilidad (0-100)
    /// </summary>
    public decimal PorcentajeDisponibilidad { get; set; }
} 