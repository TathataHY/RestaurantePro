namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el nivel de fidelización
    /// </summary>
    public class NivelFidelizacionActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }
        
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nivel de fidelización anterior
        /// </summary>
        public NivelFidelizacion NivelAnterior { get; }

        /// <summary>
        /// Nuevo nivel de fidelización
        /// </summary>
        public NivelFidelizacion NuevoNivel { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NivelFidelizacionActualizado(Guid tarjetaId, Guid clienteId, NivelFidelizacion nivelAnterior, NivelFidelizacion nuevoNivel)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            NivelAnterior = nivelAnterior;
            NuevoNivel = nuevoNivel;
        }
    }
} 

