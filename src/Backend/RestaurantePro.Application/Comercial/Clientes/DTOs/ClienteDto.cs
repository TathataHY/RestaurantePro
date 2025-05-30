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
    /// Apellido del cliente
    /// </summary>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Fecha de nacimiento del cliente
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Dirección del cliente
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Segmento de cliente
    /// </summary>
    public SegmentoCliente Tipo { get; set; }

    /// <summary>
    /// Tipo de cliente como texto
    /// </summary>
    public string TipoTexto => Tipo.ToString();

    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public new bool Activo { get; set; }

    /// <summary>
    /// Notas del cliente
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización (si tiene)
    /// </summary>
    public Guid? TarjetaFidelizacionId { get; set; }

    /// <summary>
    /// Puntos acumulados en el programa de fidelización
    /// </summary>
    public int PuntosFidelizacion { get; set; }

    /// <summary>
    /// Nivel de fidelización del cliente
    /// </summary>
    public NivelFidelizacion NivelFidelizacion { get; set; }

    /// <summary>
    /// Nivel de fidelización del cliente como texto
    /// </summary>
    public string NivelFidelizacionTexto => NivelFidelizacion.ToString();

    /// <summary>
    /// Cantidad total de visitas registradas
    /// </summary>
    public int TotalVisitas { get; set; }

    /// <summary>
    /// Total gastado por el cliente
    /// </summary>
    public decimal TotalGastado { get; set; }

    /// <summary>
    /// Última visita registrada del cliente
    /// </summary>
    public DateTime? UltimaVisita { get; set; }

    /// <summary>
    /// Promedio de gasto por visita del cliente
    /// </summary>
    public decimal PromedioGasto { get; set; }
} 