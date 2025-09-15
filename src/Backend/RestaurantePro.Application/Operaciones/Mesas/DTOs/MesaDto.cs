using System;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.DTOs;

/// <summary>
/// DTO con información de una mesa
/// </summary>
public class MesaDto : BaseDto
{
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
    /// Hora de la última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }
    
    /// <summary>
    /// Indica si la mesa está disponible para ser reservada
    /// </summary>
    public bool Disponible => Estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase);
} 