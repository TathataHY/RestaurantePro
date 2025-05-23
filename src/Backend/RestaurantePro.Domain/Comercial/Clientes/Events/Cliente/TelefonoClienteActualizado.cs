namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza el teléfono de un cliente
    /// </summary>
    public class TelefonoClienteActualizado : DomainEvent
    {
        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// Teléfono anterior del cliente
        /// </summary>
        public PhoneNumber TelefonoAnterior { get; }
        
        /// <summary>
        /// Nuevo teléfono del cliente
        /// </summary>
        public PhoneNumber NuevoTelefono { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="telefonoAnterior">Teléfono anterior</param>
        /// <param name="nuevoTelefono">Nuevo teléfono</param>
        public TelefonoClienteActualizado(Guid clienteId, PhoneNumber telefonoAnterior, PhoneNumber nuevoTelefono)
        {
            ClienteId = clienteId;
            TelefonoAnterior = telefonoAnterior;
            NuevoTelefono = nuevoTelefono;
        }
    }
} 