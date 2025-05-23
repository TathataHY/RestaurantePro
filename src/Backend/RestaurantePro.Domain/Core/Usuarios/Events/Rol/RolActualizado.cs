namespace RestaurantePro.Domain.Core.Usuarios.Events.Rol
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza un rol existente
    /// </summary>
    public class RolActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        public Guid RolId { get; }
        
        /// <summary>
        /// Nombre anterior del rol
        /// </summary>
        public string NombreAnterior { get; }
        
        /// <summary>
        /// Nuevo nombre del rol
        /// </summary>
        public string NuevoNombre { get; }
        
        /// <summary>
        /// Tipo de usuario asociado con este rol
        /// </summary>
        public TipoUsuario TipoUsuario { get; }
        
        /// <summary>
        /// Indica si el rol es asignable a usuarios
        /// </summary>
        public bool EsAsignable { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RolActualizado(Guid rolId, string nombreAnterior, string nuevoNombre, TipoUsuario tipoUsuario, bool esAsignable)
        {
            RolId = rolId;
            NombreAnterior = nombreAnterior;
            NuevoNombre = nuevoNombre;
            TipoUsuario = tipoUsuario;
            EsAsignable = esAsignable;
        }
    }
} 