namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza la información de contacto
    /// </summary>
    public class InformacionContactoActualizada : IDomainEvent
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

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public InformacionContactoActualizada(Guid clienteId, string email, string telefono)
        {
            ClienteId = clienteId;
            Email = email;
            Telefono = telefono;
        }
    }
} 
