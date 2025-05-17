namespace RestaurantePro.Domain.Proveedores.Events.Proveedor
{
    /// <summary>
    /// Evento que se dispara cuando se registra un nuevo proveedor
    /// </summary>
    public class ProveedorRegistrado : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        /// <summary>
        /// ID del proveedor registrado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Nombre del proveedor registrado
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre del proveedor</param>
        public ProveedorRegistrado(Guid proveedorId, string nombre)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
        }
    }
}

