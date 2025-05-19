namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se confirma la cuenta de un usuario
    /// </summary>
    public class UsuarioConfirmado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario confirmado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioConfirmado(Guid usuarioId)
        {
            UsuarioId = usuarioId;
        }
    }
} 