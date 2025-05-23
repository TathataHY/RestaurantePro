namespace RestaurantePro.Domain.Proveedores.Events.Proveedor
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza un proveedor
    /// </summary>
    public class ProveedorActualizado : DomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        
        /// <summary>
        /// ID del proveedor actualizado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Nombre actualizado del proveedor
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Email actualizado del proveedor
        /// </summary>
        public Email Email { get; }

        /// <summary>
        /// Teléfono actualizado del proveedor
        /// </summary>
        public PhoneNumber Telefono { get; }

        /// <summary>
        /// Dirección actualizada del proveedor
        /// </summary>
        public string Direccion { get; }

        /// <summary>
        /// Constructor simplificado
        /// </summary>
        public ProveedorActualizado(Guid proveedorId, string nombre)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
            Email = Email.Empty;
            Telefono = PhoneNumber.Empty;
            Direccion = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProveedorActualizado(Guid proveedorId, string nombre, Email email, PhoneNumber telefono, string direccion)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
            Email = email;
            Telefono = telefono;
            Direccion = direccion;
        }
    }
} 



