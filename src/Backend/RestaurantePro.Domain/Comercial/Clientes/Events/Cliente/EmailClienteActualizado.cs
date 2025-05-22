namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
    
    /// <summary>
    /// Evento que se dispara cuando se actualiza el email de un cliente
    /// </summary>
    public class EmailClienteActualizado : DomainEvent
    {
        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// Email anterior del cliente
        /// </summary>
        public Email EmailAnterior { get; }
        
        /// <summary>
        /// Nuevo email del cliente
        /// </summary>
        public Email NuevoEmail { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="emailAnterior">Email anterior</param>
        /// <param name="nuevoEmail">Nuevo email</param>
        public EmailClienteActualizado(Guid clienteId, Email emailAnterior, Email nuevoEmail)
        {
            ClienteId = clienteId;
            EmailAnterior = emailAnterior;
            NuevoEmail = nuevoEmail;
        }
    }
} 