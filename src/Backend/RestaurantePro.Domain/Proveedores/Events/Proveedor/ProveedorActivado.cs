namespace RestaurantePro.Domain.Proveedores.Events.Proveedor
{
    /// <summary>
    /// Evento que se dispara cuando se activa un proveedor
    /// </summary>
    public class ProveedorActivado : DomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        
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



