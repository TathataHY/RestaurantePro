namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO con información de una mesa
/// </summary>
public class MesaDto
{
    /// <summary>
    /// ID único de la mesa
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public string Numero { get; set; } = string.Empty;
    
    /// <summary>
    /// Capacidad de la mesa (número de personas)
    /// </summary>
    public int Capacidad { get; set; }
    
    /// <summary>
    /// Estado actual de la mesa
    /// </summary>
    public string Estado { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del cliente asignado (si está ocupada)
    /// </summary>
    public Guid? ClienteId { get; set; }
    
    /// <summary>
    /// Nombre del cliente (si está disponible)
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Zona o área donde se encuentra la mesa
    /// </summary>
    public string Zona { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de mesa
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Hora de la última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }

    /// <summary>
    /// Color para mostrar en la UI según el estado
    /// </summary>
    public Microsoft.Maui.Graphics.Color EstadoColor => Estado.ToLowerInvariant() switch
    {
        "disponible" => Microsoft.Maui.Graphics.Color.FromArgb("#4CAF50"), // Verde
        "ocupada" => Microsoft.Maui.Graphics.Color.FromArgb("#F44336"),    // Rojo
        "reservada" => Microsoft.Maui.Graphics.Color.FromArgb("#FF9800"),  // Naranja
        "fuera_de_servicio" => Microsoft.Maui.Graphics.Color.FromArgb("#9E9E9E"), // Gris
        _ => Microsoft.Maui.Graphics.Color.FromArgb("#607D8B") // Gris azulado por defecto
    };

    /// <summary>
    /// Descripción amigable del estado
    /// </summary>
    public string EstadoDescripcion => Estado.ToLowerInvariant() switch
    {
        "disponible" => "Disponible",
        "ocupada" => "Ocupada",
        "reservada" => "Reservada",
        "fuera_de_servicio" => "Fuera de Servicio",
        _ => Estado
    };

    /// <summary>
    /// Indica si la mesa está disponible
    /// </summary>
    public bool Disponible => Estado.ToLowerInvariant() == "disponible";

    /// <summary>
    /// Indica si la mesa está ocupada
    /// </summary>
    public bool Ocupada => Estado.ToLowerInvariant() == "ocupada";

    /// <summary>
    /// Indica si la mesa está reservada
    /// </summary>
    public bool Reservada => Estado.ToLowerInvariant() == "reservada";

    /// <summary>
    /// Indica si la mesa está fuera de servicio
    /// </summary>
    public bool FueraDeServicio => Estado.ToLowerInvariant() == "fuera_de_servicio";
}

