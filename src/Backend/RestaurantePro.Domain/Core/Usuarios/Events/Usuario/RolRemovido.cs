namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se remueve un rol de un usuario
    /// </summary>
    public class RolRemovido : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Rol removido del usuario
        /// </summary>
        public RolUsuario Rol { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public RolRemovido(Guid usuarioId, RolUsuario rol)
        {
            UsuarioId = usuarioId;
            Rol = rol;
        }
    }
} 