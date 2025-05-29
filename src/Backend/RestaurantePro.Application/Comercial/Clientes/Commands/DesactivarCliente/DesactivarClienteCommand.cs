namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Comando para desactivar un cliente (eliminación lógica)
/// Implementa soft delete manteniendo la integridad referencial
/// </summary>
public class DesactivarClienteCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID del cliente a desactivar
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Motivo de la desactivación (opcional)
    /// </summary>
    public string? MotivoDesactivacion { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public DesactivarClienteCommand(Guid clienteId, string? motivo = null)
    {
        ClienteId = clienteId;
        MotivoDesactivacion = motivo;
    }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public DesactivarClienteCommand()
    {
    }
} 