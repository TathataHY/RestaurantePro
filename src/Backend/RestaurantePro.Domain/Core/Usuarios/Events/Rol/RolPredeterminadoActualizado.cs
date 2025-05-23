namespace RestaurantePro.Domain.Core.Usuarios.Events.Rol
{
    /// <summary>
    /// Evento de dominio que se dispara cuando cambia el estado predeterminado de un rol
    /// </summary>
    public class RolPredeterminadoActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        public Guid RolId { get; }
        
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Indica si ahora el rol es predeterminado
        /// </summary>
        public bool EsPredeterminado { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RolPredeterminadoActualizado(Guid rolId, string nombre, bool esPredeterminado)
        {
            RolId = rolId;
            Nombre = nombre;
            EsPredeterminado = esPredeterminado;
        }
    }
} 