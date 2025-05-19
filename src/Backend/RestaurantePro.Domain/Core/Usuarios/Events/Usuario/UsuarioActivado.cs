namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se activa un usuario
    /// </summary>
    public class UsuarioActivado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario activado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioActivado(Guid usuarioId)
        {
            UsuarioId = usuarioId;
        }
    }
} 