using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;

/// <summary>
/// Comando para actualizar los datos de un cliente existente
/// Implementa actualizaciones parciales con validación de reglas de negocio
/// </summary>
public class ActualizarClienteCommand : IRequest<Result<ClienteDto>>
{
    /// <summary>
    /// ID del cliente a actualizar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo nombre completo del cliente (opcional)
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Nuevo email del cliente (opcional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Nuevo teléfono del cliente (opcional)
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Nueva fecha de nacimiento del cliente (opcional)
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Estado de activación del cliente (opcional)
    /// </summary>
    public bool? EstaActivo { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public ActualizarClienteCommand(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ActualizarClienteCommand()
    {
    }
} 