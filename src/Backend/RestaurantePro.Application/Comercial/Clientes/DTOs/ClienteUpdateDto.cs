using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Application.Comercial.Clientes.DTOs;

/// <summary>
/// DTO para actualizar información de un cliente existente
/// Contiene solo las propiedades que pueden ser modificadas
/// </summary>
public class ClienteUpdateDto
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid Id { get; set; }

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
    public SegmentoCliente Tipo { get; set; }

    /// <summary>
    /// Notas del cliente
    /// </summary>
    public string? Notas { get; set; }
} 