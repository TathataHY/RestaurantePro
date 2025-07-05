namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO con información de una mesa para la aplicación móvil
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
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>
    /// Indica si la mesa está disponible para ser reservada
    /// </summary>
    public bool Disponible => Estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si la mesa está ocupada
    /// </summary>
    public bool Ocupada => Estado.Equals("Ocupada", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si la mesa está reservada
    /// </summary>
    public bool Reservada => Estado.Equals("Reservada", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Obtiene el color asociado al estado para la UI
    /// </summary>
    public string ColorEstado => Estado.ToLowerInvariant() switch
    {
        "disponible" => "#4CAF50", // Verde
        "ocupada" => "#F44336",    // Rojo
        "reservada" => "#FF9800",  // Naranja
        "fuera de servicio" => "#9E9E9E", // Gris
        _ => "#2196F3" // Azul por defecto
    };

    /// <summary>
    /// Descripción amigable del estado para la UI móvil
    /// </summary>
    public string EstadoDescripcion => Estado.ToLowerInvariant() switch
    {
        "disponible" => "Libre",
        "ocupada" => "Ocupada",
        "reservada" => "Reservada",
        "fuera de servicio" => "Fuera de servicio",
        _ => Estado
    };
} 