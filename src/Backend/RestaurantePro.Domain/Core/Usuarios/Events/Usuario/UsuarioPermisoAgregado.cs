namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando se agrega un permiso a un usuario
/// </summary>
public class UsuarioPermisoAgregado : DomainEvent
{
    /// <summary>
    /// ID del usuario al que se agregó el permiso
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Permiso que se agregó
    /// </summary>
    public string Permiso { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioPermisoAgregado(Guid usuarioId, string permiso)
    {
        UsuarioId = usuarioId;
        Permiso = permiso;
    }
} 