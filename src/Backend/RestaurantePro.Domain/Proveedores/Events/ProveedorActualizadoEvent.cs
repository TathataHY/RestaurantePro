namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza un proveedor
    /// </summary>
    public class ProveedorActualizadoEvent : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        /// <summary>
        /// ID del proveedor actualizado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Nombre del proveedor actualizado
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre del proveedor</param>
        public ProveedorActualizadoEvent(Guid proveedorId, string nombre)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
        }
    }
} 