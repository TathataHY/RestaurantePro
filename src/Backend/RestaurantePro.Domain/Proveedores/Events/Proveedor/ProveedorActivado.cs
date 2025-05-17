namespace RestaurantePro.Domain.Proveedores.Events.Proveedor
{
    /// <summary>
    /// Evento que se dispara cuando se activa un proveedor
    /// </summary>
    public class ProveedorActivado : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        /// <summary>
        /// ID del proveedor activado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProveedorActivado(Guid proveedorId, string nombre)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
        }
    }
} 

