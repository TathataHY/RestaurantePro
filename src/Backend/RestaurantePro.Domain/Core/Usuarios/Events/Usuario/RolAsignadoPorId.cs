namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se asigna un rol a un usuario mediante su ID
    /// </summary>
    public class RolAsignadoPorId : DomainEvent
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Identificador único del rol asignado
        /// </summary>
        public Guid RolId { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RolAsignadoPorId(Guid usuarioId, Guid rolId)
        {
            UsuarioId = usuarioId;
            RolId = rolId;
        }
    }
} 