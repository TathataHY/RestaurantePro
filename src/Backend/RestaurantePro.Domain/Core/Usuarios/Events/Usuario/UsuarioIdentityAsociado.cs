namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se asocia un usuario con una cuenta de Identity
    /// </summary>
    public class UsuarioIdentityAsociado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Identificador de la cuenta de Identity
        /// </summary>
        public string IdentityId { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioIdentityAsociado(Guid usuarioId, string identityId)
        {
            UsuarioId = usuarioId;
            IdentityId = identityId;
        }
    }
} 