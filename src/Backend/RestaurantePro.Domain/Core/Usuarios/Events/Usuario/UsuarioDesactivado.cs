namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se desactiva un usuario
    /// </summary>
    public class UsuarioDesactivado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario desactivado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioDesactivado(Guid usuarioId)
        {
            UsuarioId = usuarioId;
        }
    }
} 