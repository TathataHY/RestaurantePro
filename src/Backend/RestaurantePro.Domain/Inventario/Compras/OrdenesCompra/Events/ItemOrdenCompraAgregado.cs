namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events
{
    /// <summary>
    /// Evento que se dispara cuando se agrega un item a una orden de compra
    /// </summary>
    public class ItemOrdenCompraAgregado : Core.Base.Interfaces.IDomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// ID del item agregado
        /// </summary>
        public Guid ItemId { get; }
        
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Cantidad del ingrediente
        /// </summary>
        public decimal Cantidad { get; }
        
        /// <summary>
        /// Precio unitario
        /// </summary>
        public decimal PrecioUnitario { get; }
        
        /// <summary>
        /// Subtotal del item
        /// </summary>
        public decimal Subtotal { get; }
        
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;
        
        /// <summary>
        /// Constructor simplificado para uso inicial
        /// </summary>
        public ItemOrdenCompraAgregado(Guid ordenCompraId, Guid ingredienteId, string nombre, decimal cantidad)
        {
            OrdenCompraId = ordenCompraId;
            ItemId = Guid.NewGuid();
            IngredienteId = ingredienteId;
            Cantidad = cantidad;
            PrecioUnitario = 0;
            Subtotal = 0;
        }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="itemId">ID del item agregado</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad</param>
        /// <param name="precioUnitario">Precio unitario</param>
        /// <param name="subtotal">Subtotal</param>
        public ItemOrdenCompraAgregado(Guid ordenCompraId, Guid itemId, Guid ingredienteId, 
            decimal cantidad, decimal precioUnitario, decimal subtotal)
        {
            OrdenCompraId = ordenCompraId;
            ItemId = itemId;
            IngredienteId = ingredienteId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = subtotal;
        }
    }
} 
