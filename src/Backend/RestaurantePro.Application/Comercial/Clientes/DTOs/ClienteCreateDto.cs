namespace RestaurantePro.Application.Comercial.Clientes.DTOs;

/// <summary>
/// DTO para crear un nuevo cliente
/// Contiene solo las propiedades requeridas para la creación
/// </summary>
public class ClienteCreateDto
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
    public SegmentoCliente Tipo { get; set; } = SegmentoCliente.Regular;

    /// <summary>
    /// Notas del cliente
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Indica si se debe crear una tarjeta de fidelización al crearse (por defecto true)
    /// </summary>
    public bool CrearTarjetaFidelizacion { get; set; } = true;
} 