namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se actualizan los datos de un usuario
    /// </summary>
    public class UsuarioActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario actualizado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Nombre completo actualizado
        /// </summary>
        public string NombreCompleto { get; }
        
        /// <summary>
        /// Email actualizado
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioActualizado(Guid usuarioId, string nombreCompleto, string email)
        {
            UsuarioId = usuarioId;
            NombreCompleto = nombreCompleto;
            Email = email;
        }
    }
} 