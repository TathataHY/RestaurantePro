namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando un usuario cambia su contraseña
/// </summary>
public class UsuarioPasswordCambiado : DomainEvent
{
    /// <summary>
    /// ID del usuario que cambió la contraseña
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioPasswordCambiado(Guid usuarioId)
    {
        UsuarioId = usuarioId;
    }
} 