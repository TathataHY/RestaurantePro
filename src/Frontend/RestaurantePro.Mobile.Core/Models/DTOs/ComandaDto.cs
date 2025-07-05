namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para información completa de una comanda
/// </summary>
public class ComandaDto
{
    /// <summary>
    /// ID único de la comanda
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de la comanda
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// ID de la mesa asociada
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public string MesaNumero { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la comanda
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// ID del cliente (si está disponible)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string ClienteNombre { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha y hora de última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }

    /// <summary>
    /// Observaciones de la comanda
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Productos incluidos en la comanda
    /// </summary>
    public List<ComandaProductoDto> Productos { get; set; } = new List<ComandaProductoDto>();

    /// <summary>
    /// Total de la comanda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Subtotal de la comanda
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// IVA de la comanda
    /// </summary>
    public decimal Iva { get; set; }

    /// <summary>
    /// Descuentos aplicados
    /// </summary>
    public decimal Descuentos { get; set; }

    /// <summary>
    /// Cantidad total de productos
    /// </summary>
    public int CantidadProductos => Productos.Sum(p => p.Cantidad);

    /// <summary>
    /// Indica si la comanda está activa
    /// </summary>
    public bool EstaActiva => Estado.ToLowerInvariant() switch
    {
        "pendiente" => true,
        "en_preparacion" => true,
        "lista" => true,
        _ => false
    };

    /// <summary>
    /// Indica si la comanda puede ser editada
    /// </summary>
    public bool PuedeSerEditada => Estado.ToLowerInvariant() switch
    {
        "pendiente" => true,
        "en_preparacion" => false,
        _ => false
    };

    /// <summary>
    /// Color para mostrar en la UI según el estado
    /// </summary>
    public string ColorEstado => Estado.ToLowerInvariant() switch
    {
        "pendiente" => "#FFC107",      // Amarillo
        "en_preparacion" => "#FF9800", // Naranja
        "lista" => "#4CAF50",          // Verde
        "entregada" => "#2196F3",      // Azul
        "cancelada" => "#F44336",      // Rojo
        "finalizada" => "#9E9E9E",     // Gris
        _ => "#607D8B"                 // Gris azulado por defecto
    };

    /// <summary>
    /// Descripción amigable del estado
    /// </summary>
    public string EstadoDescripcion => Estado.ToLowerInvariant() switch
    {
        "pendiente" => "Pendiente",
        "en_preparacion" => "En Preparación", 
        "lista" => "Lista",
        "entregada" => "Entregada",
        "cancelada" => "Cancelada",
        "finalizada" => "Finalizada",
        _ => Estado
    };
} 