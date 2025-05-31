namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando se establece la identificación de un usuario
/// </summary>
public class UsuarioIdentificacionEstablecida : DomainEvent
{
    /// <summary>
    /// ID del usuario cuya identificación se estableció
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// Identificación establecida
    /// </summary>
    public string Identificacion { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioIdentificacionEstablecida(Guid usuarioId, string identificacion)
    {
        UsuarioId = usuarioId;
        Identificacion = identificacion;
    }
} 