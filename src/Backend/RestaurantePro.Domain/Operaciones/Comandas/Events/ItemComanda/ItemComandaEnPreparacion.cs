namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento que se dispara cuando un ítem de comanda pasa a estado en preparación
    /// </summary>
    public class ItemComandaEnPreparacion : DomainEvent
    {
        /// <summary>
        /// ID del ítem de comanda
        /// </summary>
        public Guid ItemComandaId { get; }
        
        /// <summary>
        /// ID del producto asociado al ítem
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// ID de la comanda a la que pertenece el ítem
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ItemComandaEnPreparacion(Guid itemComandaId, Guid productoId, Guid comandaId)
        {
            ItemComandaId = itemComandaId;
            ProductoId = productoId;
            ComandaId = comandaId;
        }
    }
} 