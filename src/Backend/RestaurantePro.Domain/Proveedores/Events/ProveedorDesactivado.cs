namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se desactiva un proveedor
    /// </summary>
    public class ProveedorDesactivado : DomainEvent
    {
        /// <summary>
        /// ID del proveedor desactivado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        public ProveedorDesactivado(Guid proveedorId) 
            : base(DateTimeOffset.Now)
        {
            ProveedorId = proveedorId;
        }
    }
} 