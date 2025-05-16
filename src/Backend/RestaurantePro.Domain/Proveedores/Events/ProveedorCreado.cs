namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento que se dispara cuando se crea un nuevo proveedor
    /// </summary>
    public class ProveedorCreado : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        /// <summary>
        /// ID del proveedor creado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Email del proveedor
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Teléfono del proveedor
        /// </summary>
        public string Telefono { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProveedorCreado(Guid proveedorId, string nombre, string email, string telefono)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
            Email = email;
            Telefono = telefono;
        }
    }
} 