namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza el nombre de un cliente
    /// </summary>
    public class NombreClienteActualizado : DomainEvent
    {
        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// Nombre anterior del cliente
        /// </summary>
        public string NombreAnterior { get; }
        
        /// <summary>
        /// Nuevo nombre del cliente
        /// </summary>
        public string NuevoNombre { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="nombreAnterior">Nombre anterior</param>
        /// <param name="nuevoNombre">Nuevo nombre</param>
        public NombreClienteActualizado(Guid clienteId, string nombreAnterior, string nuevoNombre)
        {
            ClienteId = clienteId;
            NombreAnterior = nombreAnterior;
            NuevoNombre = nuevoNombre;
        }
    }
} 