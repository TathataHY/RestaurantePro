namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se elimina un contacto de un proveedor
    /// </summary>
    public class ContactoProveedorEliminado : DomainEvent
    {
        /// <summary>
        /// ID del proveedor
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// ID del contacto eliminado
        /// </summary>
        public Guid ContactoId { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="contactoId">ID del contacto</param>
        public ContactoProveedorEliminado(Guid proveedorId, Guid contactoId) 
            : base(DateTimeOffset.Now)
        {
            ProveedorId = proveedorId;
            ContactoId = contactoId;
        }
    }
} 