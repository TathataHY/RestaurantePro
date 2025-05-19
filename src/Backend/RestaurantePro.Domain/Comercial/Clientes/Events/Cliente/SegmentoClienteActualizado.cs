namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento que se genera cuando se actualiza el segmento de un cliente
    /// en base a su comportamiento y patrones de consumo.
    /// </summary>
    public class SegmentoClienteActualizado : DomainEvent
    {
        /// <summary>
        /// ID del cliente cuyo segmento ha sido actualizado
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// Segmento anterior del cliente
        /// </summary>
        public SegmentoCliente SegmentoAnterior { get; }
        
        /// <summary>
        /// Nuevo segmento asignado al cliente
        /// </summary>
        public SegmentoCliente NuevoSegmento { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="segmentoAnterior">Segmento anterior</param>
        /// <param name="nuevoSegmento">Nuevo segmento</param>
        public SegmentoClienteActualizado(Guid clienteId, SegmentoCliente segmentoAnterior, SegmentoCliente nuevoSegmento) : base()
        {
            ClienteId = clienteId;
            SegmentoAnterior = segmentoAnterior;
            NuevoSegmento = nuevoSegmento;
        }
    }
} 