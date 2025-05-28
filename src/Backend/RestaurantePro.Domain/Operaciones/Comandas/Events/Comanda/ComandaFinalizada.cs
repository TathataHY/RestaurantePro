namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se finaliza una comanda
    /// </summary>
    public class ComandaFinalizada : DomainEvent
    {
        /// <summary>
        /// ID de la comanda finalizada
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Total de la comanda (incluyendo impuestos)
        /// </summary>
        public decimal Total { get; }
        
        /// <summary>
        /// Items de la comanda
        /// </summary>
        public IReadOnlyCollection<ValueObjects.ItemComandaInfo> Items { get; }
        
        /// <summary>
        /// Fecha de finalización
        /// </summary>
        public DateTime FechaFinalizacion { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaFinalizada(Guid comandaId, decimal total)
        {
            ComandaId = comandaId;
            Total = total;
            Items = new List<ValueObjects.ItemComandaInfo>();
            FechaFinalizacion = DateTime.Now;
        }
        
        /// <summary>
        /// Constructor con información completa
        /// </summary>
        public ComandaFinalizada(
            Guid comandaId, 
            IEnumerable<ValueObjects.ItemComandaInfo> items,
            DateTime fechaFinalizacion)
        {
            ComandaId = comandaId;
            Items = items.ToList().AsReadOnly();
            FechaFinalizacion = fechaFinalizacion;
            Total = CalcularTotal(items);
        }
        
        private decimal CalcularTotal(IEnumerable<ValueObjects.ItemComandaInfo> items)
        {
            return items.Sum(i => i.Precio * i.Cantidad);
        }
    }
}


