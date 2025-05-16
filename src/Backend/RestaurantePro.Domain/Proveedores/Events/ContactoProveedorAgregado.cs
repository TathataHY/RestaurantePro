namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento que se dispara cuando se agrega un contacto a un proveedor
    /// </summary>
    public class ContactoProveedorAgregado : IDomainEvent
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
        /// Cargo del contacto
        /// </summary>
        public string Cargo { get; }

        /// <summary>
        /// Email del contacto
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Teléfono del contacto
        /// </summary>
        public string Telefono { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContactoProveedorAgregado(Guid proveedorId, Guid contactoId, string nombre, string cargo, string email, string telefono)
        {
            ProveedorId = proveedorId;
            ContactoId = contactoId;
            Nombre = nombre;
            Cargo = cargo;
            Email = email;
            Telefono = telefono;
        }
    }
} 