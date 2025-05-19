namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando un usuario accede al sistema
    /// </summary>
    public class UsuarioAccedio : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario que accedió
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Fecha y hora del acceso
        /// </summary>
        public DateTime FechaAcceso { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioAccedio(Guid usuarioId, DateTime fechaAcceso)
        {
            UsuarioId = usuarioId;
            FechaAcceso = fechaAcceso;
        }
    }
} 