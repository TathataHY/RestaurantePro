namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando un usuario es restaurado de la eliminación lógica
/// </summary>
public class UsuarioRestauradoDeEliminacion : DomainEvent
{
    /// <summary>
    /// ID del usuario que fue restaurado
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioRestauradoDeEliminacion(Guid usuarioId)
    {
        UsuarioId = usuarioId;
    }
} 