namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se asigna un rol a un usuario
    /// </summary>
    public class RolAsignado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Rol asignado al usuario
        /// </summary>
        public RolUsuario Rol { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public RolAsignado(Guid usuarioId, RolUsuario rol)
        {
            UsuarioId = usuarioId;
            Rol = rol;
        }
    }
} 