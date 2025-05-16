namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento que se dispara cuando se elimina un contacto de un proveedor
    /// </summary>
    public class ContactoProveedorEliminado : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        /// <summary>
        /// ID del proveedor
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// ID del contacto
        /// </summary>
        public Guid ContactoId { get; }

        /// <summary>
        /// Nombre del contacto
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContactoProveedorEliminado(Guid proveedorId, Guid contactoId, string nombre)
        {
            ProveedorId = proveedorId;
            ContactoId = contactoId;
            Nombre = nombre;
        }
    }
} 