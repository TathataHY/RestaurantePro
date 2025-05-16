namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento que se dispara cuando se desactiva un proveedor
    /// </summary>
    public class ProveedorDesactivado : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        /// <summary>
        /// ID del proveedor desactivado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProveedorDesactivado(Guid proveedorId, string nombre)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
        }
    }
} 