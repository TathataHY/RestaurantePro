namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
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
        public string Email { get; }

        /// <summary>
        /// Nuevo teléfono del cliente
        /// </summary>
        public string Telefono { get; }

        
        public InformacionContactoActualizada(Guid clienteId, string email, string telefono)
        {
            ClienteId = clienteId;
            Email = email;
            Telefono = telefono;
        }
    }
} 

