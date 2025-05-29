using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarEstadoComanda;

/// <summary>
/// Comando para actualizar el estado de una comanda en el flujo operativo
/// Gestiona las transiciones: Creada → EnProceso → Lista → Entregada → Finalizada
/// </summary>
public class ActualizarEstadoComandaCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda a actualizar
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Nuevo estado de la comanda
    /// </summary>
    public string NuevoEstado { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que realiza el cambio (para auditoría)
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Observaciones sobre el cambio de estado (opcional)
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public ActualizarEstadoComandaCommand(Guid comandaId, string nuevoEstado, Guid usuarioId)
    {
        ComandaId = comandaId;
        NuevoEstado = nuevoEstado;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ActualizarEstadoComandaCommand()
    {
    }
} 