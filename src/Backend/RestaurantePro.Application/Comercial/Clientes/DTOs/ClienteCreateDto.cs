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
    /// Indica si el cliente debe estar activo al crearse (por defecto true)
    /// </summary>
    public bool EstaActivo { get; set; } = true;
} 