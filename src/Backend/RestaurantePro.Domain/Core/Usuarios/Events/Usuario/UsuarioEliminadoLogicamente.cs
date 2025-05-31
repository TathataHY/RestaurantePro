namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando un usuario es eliminado lógicamente
/// </summary>
public class UsuarioEliminadoLogicamente : DomainEvent
{
    /// <summary>
    /// ID del usuario que fue eliminado
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Fecha y hora de la eliminación
    /// </summary>
    public DateTime FechaEliminacion { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioEliminadoLogicamente(Guid usuarioId, DateTime fechaEliminacion)
    {
        UsuarioId = usuarioId;
        FechaEliminacion = fechaEliminacion;
    }
} 