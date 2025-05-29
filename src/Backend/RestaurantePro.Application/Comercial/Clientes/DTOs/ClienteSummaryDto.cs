namespace RestaurantePro.Application.Comercial.Clientes.DTOs;

/// <summary>
/// DTO resumido para Cliente en listas y búsquedas
/// Contiene solo la información esencial para mostrar en grids
/// </summary>
public class ClienteSummaryDto
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Edad calculada del cliente
    /// </summary>
    public int Edad { get; set; }

    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public bool EstaActivo { get; set; }

    /// <summary>
    /// Puntos acumulados
    /// </summary>
    public int PuntosAcumulados { get; set; }

    /// <summary>
    /// Cantidad de visitas
    /// </summary>
    public int CantidadVisitas { get; set; }

    /// <summary>
    /// Segmento del cliente
    /// </summary>
    public string Segmento { get; set; } = string.Empty;

    /// <summary>
    /// Indica si es cliente frecuente
    /// </summary>
    public bool EsClienteFrecuente { get; set; }

    /// <summary>
    /// Fecha de última actividad
    /// </summary>
    public DateTime UltimaActividad { get; set; }

    /// <summary>
    /// Fecha de creación del cliente
    /// </summary>
    public DateTime FechaCreacion { get; set; }
} 