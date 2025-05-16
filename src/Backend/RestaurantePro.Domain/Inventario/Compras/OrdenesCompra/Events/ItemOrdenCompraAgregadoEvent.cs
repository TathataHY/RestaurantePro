namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events
{
    /// <summary>
    /// Evento que se dispara cuando se agrega un item a una orden de compra
    /// </summary>
    public class ItemOrdenCompraAgregadoEvent : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;
        
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string NombreIngrediente { get; }
        
        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public decimal Cantidad { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad solicitada</param>
        public ItemOrdenCompraAgregadoEvent(Guid ordenCompraId, Guid ingredienteId, string nombreIngrediente, decimal cantidad)
        {
            OrdenCompraId = ordenCompraId;
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            Cantidad = cantidad;
        }
    }
} 