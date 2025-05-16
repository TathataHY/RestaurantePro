namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento que se dispara cuando se desactiva un proveedor
    /// </summary>
    public class ProveedorDesactivadoEvent : DomainEvent
    {
        /// <summary>
        /// ID del proveedor desactivado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Nombre del proveedor desactivado
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre del proveedor</param>
        public ProveedorDesactivadoEvent(Guid proveedorId, string nombre)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
        }
    }
} 