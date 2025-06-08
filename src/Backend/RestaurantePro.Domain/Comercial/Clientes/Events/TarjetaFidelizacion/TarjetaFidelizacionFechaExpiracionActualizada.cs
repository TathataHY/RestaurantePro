namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza la fecha de expiración de una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionFechaExpiracionActualizada : DomainEvent
    {
        public Guid TarjetaId { get; }
        public Guid ClienteId { get; }
        public DateTime FechaExpiracion { get; }

        public TarjetaFidelizacionFechaExpiracionActualizada(Guid tarjetaId, Guid clienteId, DateTime fechaExpiracion)
            : base(Guid.NewGuid())
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            FechaExpiracion = fechaExpiracion;
        }
    }
} 