namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un cliente
    /// </summary>
    public class ClienteCreado : DomainEvent
    {
        /// <summary>
        /// Identificador del cliente creado
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public string NombreCompleto { get; }
        
        /// <summary>
        /// Email del cliente
        /// </summary>
        public Email Email { get; }
        
        /// <summary>
        /// Teléfono del cliente
        /// </summary>
        public PhoneNumber Telefono { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="nombreCompleto">Nombre completo del cliente</param>
        /// <param name="email">Email del cliente</param>
        /// <param name="telefono">Teléfono del cliente</param>
        public ClienteCreado(Guid clienteId, string nombreCompleto, Email email, PhoneNumber telefono)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
            Email = email;
            Telefono = telefono;
        }
    }
} 
