namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando un usuario cambia su rol
/// </summary>
public class UsuarioRolCambiado : DomainEvent
{
    /// <summary>
    /// ID del usuario que cambió de rol
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Nuevo rol del usuario
    /// </summary>
    public string Rol { get; }
    
    /// <summary>
    /// Nuevo nivel de acceso del usuario
    /// </summary>
    public int NivelAcceso { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioRolCambiado(Guid usuarioId, string rol, int nivelAcceso)
    {
        UsuarioId = usuarioId;
        Rol = rol;
        NivelAcceso = nivelAcceso;
    }
} 