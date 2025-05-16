namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza la información de un proveedor
    /// </summary>
    public class ProveedorActualizado : DomainEvent
    {
        /// <summary>
        /// ID del proveedor actualizado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Nombre actualizado
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Teléfono actualizado
        /// </summary>
        public PhoneNumber Telefono { get; }

        /// <summary>
        /// Email actualizado
        /// </summary>
        public Email Email { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre actualizado</param>
        /// <param name="telefono">Teléfono actualizado</param>
        /// <param name="email">Email actualizado</param>
        public ProveedorActualizado(Guid proveedorId, string nombre, PhoneNumber telefono, Email email) 
            : base(DateTimeOffset.Now)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
            Telefono = telefono;
            Email = email;
        }
    }
} 