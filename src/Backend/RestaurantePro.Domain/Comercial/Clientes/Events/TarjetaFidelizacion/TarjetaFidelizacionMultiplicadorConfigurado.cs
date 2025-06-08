namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se configura un multiplicador de puntos en una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionMultiplicadorConfigurado : DomainEvent
    {
        public Guid TarjetaId { get; }
        public Guid ClienteId { get; }
        public decimal Multiplicador { get; }

        public TarjetaFidelizacionMultiplicadorConfigurado(Guid tarjetaId, Guid clienteId, decimal multiplicador)
            : base(Guid.NewGuid())
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Multiplicador = multiplicador;
        }
    }
} 