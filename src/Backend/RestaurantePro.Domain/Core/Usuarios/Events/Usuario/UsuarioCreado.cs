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
        /// Tipo de usuario
        /// </summary>
        public TipoUsuario TipoUsuario { get; }
        
        /// <summary>
        /// Contraseña del usuario (para sincronización con Identity)
        /// </summary>
        public string Password { get; }
        
        /// <summary>
        /// Rol del usuario (para asignación en Identity)
        /// </summary>
        public string Rol { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public UsuarioCreado(Guid usuarioId, string nombreUsuario, string email, EstadoUsuario estado, string password, string rol, TipoUsuario tipoUsuario = TipoUsuario.Empleado)
        {
            UsuarioId = usuarioId;
            NombreUsuario = nombreUsuario;
            Email = email;
            Estado = estado;
            Password = password;
            Rol = rol;
            TipoUsuario = tipoUsuario;
        }
    }
} 