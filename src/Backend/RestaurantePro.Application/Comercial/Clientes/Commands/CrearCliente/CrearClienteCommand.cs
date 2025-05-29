using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;

/// <summary>
/// Comando para crear un nuevo cliente en el sistema
/// Implementa el patrón CQRS con MediatR
/// </summary>
public class CrearClienteCommand : IRequest<Result<ClienteDto>>
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
    /// Indica si el cliente debe estar activo al crearse
    /// </summary>
    public bool EstaActivo { get; set; } = true;
} 