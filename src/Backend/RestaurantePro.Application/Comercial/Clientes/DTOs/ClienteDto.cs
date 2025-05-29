namespace RestaurantePro.Application.Comercial.Clientes.DTOs;

/// <summary>
/// DTO principal para Cliente con información completa
/// Hereda de BaseDto para tener propiedades de auditoría
/// </summary>
public class ClienteDto : BaseDto
{
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
    /// Fecha de nacimiento del cliente
    /// </summary>
    public DateTime FechaNacimiento { get; set; }

    /// <summary>
    /// Edad calculada del cliente
    /// </summary>
    public int Edad { get; set; }

    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public bool EstaActivo { get; set; }

    /// <summary>
    /// Puntos acumulados en el programa de fidelización
    /// </summary>
    public int PuntosAcumulados { get; set; }

    /// <summary>
    /// Cantidad de visitas registradas
    /// </summary>
    public int CantidadVisitas { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización principal (si tiene)
    /// </summary>
    public Guid? TarjetaFidelizacionPrincipalId { get; set; }

    /// <summary>
    /// Segmento del cliente como texto
    /// </summary>
    public string Segmento { get; set; } = string.Empty;

    /// <summary>
    /// Indica si tiene tarjeta de fidelización
    /// </summary>
    public bool TieneTarjetaFidelizacion { get; set; }

    /// <summary>
    /// Indicador si es cliente frecuente (más de 10 visitas)
    /// </summary>
    public bool EsClienteFrecuente { get; set; }

    /// <summary>
    /// Último registro de actividad (fecha de última modificación o creación)
    /// </summary>
    public DateTime UltimaActividad { get; set; }
} 