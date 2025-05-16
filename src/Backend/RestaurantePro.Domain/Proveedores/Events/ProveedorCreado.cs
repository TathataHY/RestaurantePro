namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un nuevo proveedor
    /// </summary>
    public class ProveedorCreado : DomainEvent
    {
        /// <summary>
        /// ID del proveedor creado
        /// </summary>
        public Guid ProveedorId { get; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// RFC del proveedor
        /// </summary>
        public string RFC { get; }

        /// <summary>
        /// Email del proveedor
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="rfc">RFC del proveedor</param>
        /// <param name="email">Email del proveedor</param>
        public ProveedorCreado(Guid proveedorId, string nombre, string rfc, Email email) 
            : base(DateTimeOffset.Now)
        {
            ProveedorId = proveedorId;
            Nombre = nombre;
            RFC = rfc;
            Email = email;
        }
    }
} 