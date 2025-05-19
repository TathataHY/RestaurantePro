namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario
{
    /// <summary>
    /// Evento de dominio emitido cuando se crea un nuevo usuario
    /// </summary>
    public class UsuarioCreado : DomainEvent
    {
        /// <summary>
        /// Identificador del usuario creado
        /// </summary>
        public Guid UsuarioId { get; }
        
        /// <summary>
        /// Nombre de usuario para login
        /// </summary>
        public string NombreUsuario { get; }
        
        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Estado inicial del usuario
        /// </summary>
        public EstadoUsuario Estado { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioCreado(Guid usuarioId, string nombreUsuario, string email, EstadoUsuario estado)
        {
            UsuarioId = usuarioId;
            NombreUsuario = nombreUsuario;
            Email = email;
            Estado = estado;
        }
    }
} 