namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities
{
    /// <summary>
    /// Entidad que representa un ítem dentro de una orden de compra
    /// </summary>
    public class ItemOrdenCompra : EntityBase
    {
        /// <summary>
        /// ID de la orden de compra a la que pertenece este ítem
        /// </summary>
        public Guid OrdenCompraId { get; private set; }
        
        /// <summary>
        /// ID del ingrediente solicitado
        /// </summary>
        public Guid IngredienteId { get; private set; }
        
        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public decimal Cantidad { get; private set; }
        
        /// <summary>
        /// Precio unitario acordado
        /// </summary>
        public decimal PrecioUnitario { get; private set; }
        
        /// <summary>
        /// Subtotal (Cantidad * PrecioUnitario)
        /// </summary>
        public decimal Subtotal { get; private set; }
        
        // Constructor privado para EF Core
        private ItemOrdenCompra() { }
        
        /// <summary>
        /// Crea un nuevo ítem de orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad solicitada</param>
        /// <param name="precioUnitario">Precio unitario</param>
        /// <returns>Nuevo ítem de orden de compra</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        internal static ItemOrdenCompra Crear(Guid ordenCompraId, Guid ingredienteId, decimal cantidad, decimal precioUnitario)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            if (precioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(precioUnitario));
                
            var item = new ItemOrdenCompra
            {
                OrdenCompraId = ordenCompraId,
                IngredienteId = ingredienteId,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = cantidad * precioUnitario
            };
            
            return item;
        }
        
        /// <summary>
        /// Actualiza la cantidad y precio del ítem
        /// </summary>
        /// <param name="nuevaCantidad">Nueva cantidad</param>
        /// <param name="nuevoPrecioUnitario">Nuevo precio unitario</param>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public void Actualizar(decimal nuevaCantidad, decimal nuevoPrecioUnitario)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));
                
            if (nuevoPrecioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(nuevoPrecioUnitario));
                
            Cantidad = nuevaCantidad;
            PrecioUnitario = nuevoPrecioUnitario;
            Subtotal = nuevaCantidad * nuevoPrecioUnitario;
            MarkAsModified();
        }
    }
} 
