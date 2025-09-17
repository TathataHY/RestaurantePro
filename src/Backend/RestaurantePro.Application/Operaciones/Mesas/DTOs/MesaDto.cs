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
    /// Ubicación de la mesa
    /// </summary>
    public string Ubicacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Zona o área donde se encuentra la mesa
    /// </summary>
    public string Zona { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de mesa
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Observaciones sobre la mesa
    /// </summary>
    public string Observaciones { get; set; } = string.Empty;
    
    /// <summary>
    /// Hora de la última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }
    
    /// <summary>
    /// Descripción adicional de la mesa
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Notas especiales sobre la mesa
    /// </summary>
    public string? Notas { get; set; }
    
    /// <summary>
    /// Indica si la mesa tiene ventana
    /// </summary>
    public bool TieneVentana { get; set; }
    
    /// <summary>
    /// Indica si la mesa tiene sofá
    /// </summary>
    public bool TieneSofa { get; set; }
    
    /// <summary>
    /// Indica si la mesa es accesible para personas con discapacidad
    /// </summary>
    public bool EsAccesible { get; set; }
    
    /// <summary>
    /// Indica si la mesa tiene enchufes disponibles
    /// </summary>
    public bool TieneEnchufe { get; set; }
    
    /// <summary>
    /// Indica si la mesa está disponible para ser reservada
    /// </summary>
    public bool Disponible => Estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase);
} 