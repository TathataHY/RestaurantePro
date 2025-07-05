namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para categoría de productos - Versión móvil
/// </summary>
public class CategoriaProductoDto
{
    /// <summary>
    /// Identificador único de la categoría
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Color asociado a la categoría para UI
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icono asociado a la categoría
    /// </summary>
    public string? Icono { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int Orden { get; set; }

    /// <summary>
    /// Indica si la categoría está activa
    /// </summary>
    public bool Activa { get; set; }

    /// <summary>
    /// Cantidad de productos en esta categoría
    /// </summary>
    public int CantidadProductos { get; set; }

    /// <summary>
    /// Cantidad de productos disponibles en esta categoría
    /// </summary>
    public int ProductosDisponibles { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    // ========================================
    // PROPIEDADES CALCULADAS PARA UI MÓVIL
    // ========================================

    /// <summary>
    /// Color para mostrar en UI (con fallback)
    /// </summary>
    public string ColorUI => Color ?? "#2196F3"; // Azul por defecto

    /// <summary>
    /// Icono para mostrar en UI (con fallback)
    /// </summary>
    public string IconoUI => Icono ?? "🍽️"; // Plato por defecto

    /// <summary>
    /// Información de disponibilidad para mostrar
    /// </summary>
    public string DisponibilidadInfo => $"{ProductosDisponibles}/{CantidadProductos} disponibles";

    /// <summary>
    /// Indica si la categoría tiene productos disponibles
    /// </summary>
    public bool TieneProductosDisponibles => ProductosDisponibles > 0;

    /// <summary>
    /// Porcentaje de productos disponibles
    /// </summary>
    public double PorcentajeDisponibilidad => CantidadProductos > 0 
        ? (double)ProductosDisponibles / CantidadProductos * 100 
        : 0;

    /// <summary>
    /// Estado de disponibilidad para UI
    /// </summary>
    public string EstadoDisponibilidad => (Activa, ProductosDisponibles, CantidadProductos) switch
    {
        (false, _, _) => "Inactiva",
        (true, 0, _) => "Sin Stock", 
        (true, var disp, var total) when disp == total => "Completa",
        (true, _, _) => "Parcial"
    };

    /// <summary>
    /// Color del estado de disponibilidad
    /// </summary>
    public string ColorEstadoDisponibilidad => EstadoDisponibilidad switch
    {
        "Completa" => "#4CAF50",    // Verde
        "Parcial" => "#FF9800",     // Naranja
        "Sin Stock" => "#F44336",   // Rojo
        "Inactiva" => "#9E9E9E",    // Gris
        _ => "#607D8B"              // Gris azulado
    };

    /// <summary>
    /// Resumen para mostrar en listas
    /// </summary>
    public string ResumenCategoria => $"{Nombre} • {DisponibilidadInfo}";
} 