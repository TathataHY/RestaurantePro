namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando se remueve un permiso de un usuario
/// </summary>
public class UsuarioPermisoRemovido : DomainEvent
{
    /// <summary>
    /// ID del usuario al que se removió el permiso
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Permiso que se removió
    /// </summary>
    public string Permiso { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioPermisoRemovido(Guid usuarioId, string permiso)
    {
        UsuarioId = usuarioId;
        Permiso = permiso;
    }
} 