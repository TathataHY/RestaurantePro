namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
    
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza la información de contacto
    /// </summary>
    public class InformacionContactoActualizada : DomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nuevo email del cliente
        /// </summary>
        public Email Email { get; }

        /// <summary>
        /// Nuevo teléfono del cliente
        /// </summary>
        public PhoneNumber Telefono { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public InformacionContactoActualizada(Guid clienteId, Email email, PhoneNumber telefono)
        {
            ClienteId = clienteId;
            Email = email;
            Telefono = telefono;
        }
    }
} 

