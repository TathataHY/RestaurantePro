namespace RestaurantePro.Application.Comercial.Clientes.DTOs;

/// <summary>
/// DTO para actualizar información de un cliente existente
/// Contiene solo las propiedades que pueden ser modificadas
/// </summary>
public class ClienteUpdateDto
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
} 