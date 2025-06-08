namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agrega una etiqueta a una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionEtiquetaAgregada : DomainEvent
    {
        public Guid TarjetaId { get; }
        public Guid ClienteId { get; }
        public string Etiqueta { get; }

        public TarjetaFidelizacionEtiquetaAgregada(Guid tarjetaId, Guid clienteId, string etiqueta)
            : base(Guid.NewGuid())
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Etiqueta = etiqueta;
        }
    }
} 