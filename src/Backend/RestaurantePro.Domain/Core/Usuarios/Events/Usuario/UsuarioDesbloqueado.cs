namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se desbloquea un usuario
    /// </summary>
    public class UsuarioDesbloqueado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario desbloqueado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioDesbloqueado(Guid usuarioId)
        {
            UsuarioId = usuarioId;
        }
    }
} 