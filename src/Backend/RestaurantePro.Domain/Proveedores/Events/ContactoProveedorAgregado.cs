namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agrega un contacto a un proveedor
    /// </summary>
    public class ContactoProveedorAgregado : DomainEvent
    {
        /// <summary>
        /// ID del proveedor
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// ID del contacto agregado
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
        /// Constructor del evento
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="contactoId">ID del contacto</param>
        /// <param name="nombre">Nombre del contacto</param>
        /// <param name="cargo">Cargo del contacto</param>
        public ContactoProveedorAgregado(Guid proveedorId, Guid contactoId, string nombre, string cargo) 
            : base(DateTimeOffset.Now)
        {
            ProveedorId = proveedorId;
            ContactoId = contactoId;
            Nombre = nombre;
            Cargo = cargo;
        }
    }
} 