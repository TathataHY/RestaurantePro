namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el tipo de un usuario
    /// </summary>
    public class UsuarioTipoActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Nuevo tipo de usuario
        /// </summary>
        public TipoUsuario TipoUsuario { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public UsuarioTipoActualizado(Guid usuarioId, TipoUsuario tipoUsuario)
        {
            UsuarioId = usuarioId;
            TipoUsuario = tipoUsuario;
        }
    }
} 