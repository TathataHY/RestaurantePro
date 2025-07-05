namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO de producto optimizado para aplicación móvil
/// </summary>
public class ProductoDto
{
    /// <summary>
    /// Identificador único del producto
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Precio del producto
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// ID de la categoría a la que pertenece
    /// </summary>
    public Guid CategoriaId { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string CategoriaNombre { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el producto está activo y disponible
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Nivel de popularidad (0-10) - para ordenamiento en UI
    /// </summary>
    public int Popularidad { get; set; }

    /// <summary>
    /// Cantidad disponible para preparación (solo lectura para móvil)
    /// </summary>
    public int CantidadDisponible { get; set; }

    /// <summary>
    /// Tiempo estimado de preparación en minutos
    /// </summary>
    public int TiempoPreparacion { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    // ========================================
    // PROPIEDADES CALCULADAS PARA UI MÓVIL
    // ========================================

    /// <summary>
    /// Precio formateado para mostrar en UI
    /// </summary>
    public string PrecioFormateado => Precio.ToString("C");

    /// <summary>
    /// Estado de disponibilidad para UI
    /// </summary>
    public string EstadoDisponibilidad => (Activo, CantidadDisponible) switch
    {
        (false, _) => "No Disponible",
        (true, <= 0) => "Agotado", 
        (true, <= 5) => "Pocas Unidades",
        (true, _) => "Disponible"
    };

    /// <summary>
    /// Color para mostrar en UI según disponibilidad
    /// </summary>
    public string ColorDisponibilidad => (Activo, CantidadDisponible) switch
    {
        (false, _) => "#9E9E9E",       // Gris - No disponible
        (true, <= 0) => "#F44336",     // Rojo - Agotado
        (true, <= 5) => "#FF9800",     // Naranja - Pocas unidades
        (true, _) => "#4CAF50"         // Verde - Disponible
    };

    /// <summary>
    /// Icono para mostrar según disponibilidad
    /// </summary>
    public string IconoDisponibilidad => (Activo, CantidadDisponible) switch
    {
        (false, _) => "❌",           // No disponible
        (true, <= 0) => "🚫",        // Agotado
        (true, <= 5) => "⚠️",        // Pocas unidades
        (true, _) => "✅"            // Disponible
    };

    /// <summary>
    /// Indica si el producto puede ser agregado a una comanda
    /// </summary>
    public bool PuedeAgregarAComanda => Activo && CantidadDisponible > 0;

    /// <summary>
    /// Nivel de popularidad para mostrar en UI
    /// </summary>
    public string PopularidadDescripcion => Popularidad switch
    {
        >= 8 => "⭐⭐⭐ Muy Popular",
        >= 6 => "⭐⭐ Popular", 
        >= 4 => "⭐ Moderado",
        _ => "Nuevo"
    };

    /// <summary>
    /// Tiempo de preparación formateado
    /// </summary>
    public string TiempoPreparacionFormateado => TiempoPreparacion switch
    {
        <= 5 => "⚡ Rápido (≤5 min)",
        <= 15 => "🕒 Normal (≤15 min)",
        <= 30 => "⏳ Demorado (≤30 min)",
        _ => $"⏰ {TiempoPreparacion} min"
    };

    /// <summary>
    /// Información resumida para listas
    /// </summary>
    public string InformacionResumen => $"{Nombre} - {PrecioFormateado} • {EstadoDisponibilidad}";

    /// <summary>
    /// Texto de búsqueda (incluye nombre, descripción y categoría)
    /// </summary>
    public string TextoBusqueda => $"{Nombre} {Descripcion} {CategoriaNombre}".ToLowerInvariant();
} 