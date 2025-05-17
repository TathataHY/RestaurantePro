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
        /// Nivel de fidelización anterior
        /// </summary>
        public NivelFidelizacion NivelAnterior { get; }

        /// <summary>
        /// Nuevo nivel de fidelización
        /// </summary>
        public NivelFidelizacion NuevoNivel { get; }

        
        public NivelFidelizacionActualizado(Guid tarjetaId, NivelFidelizacion nivelAnterior, NivelFidelizacion nuevoNivel)
        {
            TarjetaId = tarjetaId;
            NivelAnterior = nivelAnterior;
            NuevoNivel = nuevoNivel;
        }
    }
} 

