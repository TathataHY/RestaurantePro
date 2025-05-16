namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se activa un proveedor
    /// </summary>
    public class ProveedorActivado : DomainEvent
    {
        /// <summary>
        /// ID del proveedor activado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        public ProveedorActivado(Guid proveedorId) 
            : base(DateTimeOffset.Now)
        {
            ProveedorId = proveedorId;
        }
    }
} 