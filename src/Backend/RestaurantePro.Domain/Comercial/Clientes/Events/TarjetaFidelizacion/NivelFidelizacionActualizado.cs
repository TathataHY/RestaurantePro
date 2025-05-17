namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el nivel de fidelización
    /// </summary>
    public class NivelFidelizacionActualizado : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Nivel de fidelización anterior
        /// </summary>
        public NivelFidelizacion NivelAnterior { get; }

        /// <summary>
        /// Nuevo nivel de fidelización
        /// </summary>
        public NivelFidelizacion NuevoNivel { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public NivelFidelizacionActualizado(Guid tarjetaId, NivelFidelizacion nivelAnterior, NivelFidelizacion nuevoNivel)
        {
            TarjetaId = tarjetaId;
            NivelAnterior = nivelAnterior;
            NuevoNivel = nuevoNivel;
        }
    }
} 
