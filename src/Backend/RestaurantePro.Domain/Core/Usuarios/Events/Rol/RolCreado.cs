namespace RestaurantePro.Domain.Core.Usuarios.Events.Rol
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un nuevo rol
    /// </summary>
    public class RolCreado : DomainEvent
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        public Guid RolId { get; }
        
        /// <summary>
        /// Nombre del rol creado
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Tipo de usuario asociado con este rol
        /// </summary>
        public TipoUsuario TipoUsuario { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RolCreado(Guid rolId, string nombre, TipoUsuario tipoUsuario)
        {
            RolId = rolId;
            Nombre = nombre;
            TipoUsuario = tipoUsuario;
        }
    }
} 