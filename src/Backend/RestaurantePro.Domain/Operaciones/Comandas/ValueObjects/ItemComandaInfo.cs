namespace RestaurantePro.Domain.Operaciones.Comandas.ValueObjects
{
    /// <summary>
    /// Información simplificada de un item de comanda para usar en eventos
    /// </summary>
    public class ItemComandaInfo
    {
        /// <summary>
        /// ID del item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;
        
        /// <summary>
        /// Cantidad del producto
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        public decimal Precio { get; set; }
        
        /// <summary>
        /// Observaciones del item
        /// </summary>
        public string Observaciones { get; set; } = string.Empty;
        
        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public ItemComandaInfo()
        {
        }
        
        /// <summary>
        /// Constructor con parámetros básicos
        /// </summary>
        public ItemComandaInfo(Guid itemId, Guid productoId, string nombreProducto, int cantidad, decimal precio)
        {
            ItemId = itemId;
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Cantidad = cantidad;
            Precio = precio;
        }
        
        /// <summary>
        /// Constructor con todos los parámetros
        /// </summary>
        public ItemComandaInfo(Guid itemId, Guid productoId, string nombreProducto, int cantidad, decimal precio, string observaciones)
            : this(itemId, productoId, nombreProducto, cantidad, precio)
        {
            Observaciones = observaciones;
        }
    }
} 