namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se configura un límite mensual de puntos en una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionLimiteMensualConfigurado : DomainEvent
    {
        public Guid TarjetaId { get; }
        public Guid ClienteId { get; }
        public int LimiteMensual { get; }

        public TarjetaFidelizacionLimiteMensualConfigurado(Guid tarjetaId, Guid clienteId, int limiteMensual)
            : base(Guid.NewGuid())
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            LimiteMensual = limiteMensual;
        }
    }
} 