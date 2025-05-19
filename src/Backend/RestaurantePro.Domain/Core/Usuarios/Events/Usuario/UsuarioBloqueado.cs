namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se bloquea un usuario
    /// </summary>
    public class UsuarioBloqueado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario bloqueado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Motivo por el que se bloqueó al usuario
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioBloqueado(Guid usuarioId, string motivo)
        {
            UsuarioId = usuarioId;
            Motivo = motivo;
        }
    }
} 