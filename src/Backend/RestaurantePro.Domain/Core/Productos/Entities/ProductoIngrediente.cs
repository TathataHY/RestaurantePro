namespace RestaurantePro.Domain.Core.Productos.Entities
{
    /// <summary>
    /// Representa la relación entre un producto y sus ingredientes
    /// </summary>
    public class ProductoIngrediente : EntityBase
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; private set; }

        /// <summary>
        /// Cantidad del ingrediente utilizada en el producto
        /// </summary>
        public decimal Cantidad { get; private set; }

        /// <summary>
        /// Indica si el ingrediente es opcional en el producto
        /// </summary>
        public bool EsOpcional { get; private set; }

        // Constructor privado para EF Core
        private ProductoIngrediente() { }

        /// <summary>
        /// Constructor principal
        /// </summary>
        public ProductoIngrediente(Guid productoId, Guid ingredienteId, decimal cantidad, bool esOpcional = false)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            Id = Guid.NewGuid();
            ProductoId = productoId;
            IngredienteId = ingredienteId;
            Cantidad = cantidad;
            EsOpcional = esOpcional;
        }

        /// <summary>
        /// Actualiza la cantidad de ingrediente utilizada
        /// </summary>
        public void ActualizarCantidad(decimal nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));

            Cantidad = nuevaCantidad;
        }

        /// <summary>
        /// Cambia el estado opcional/requerido del ingrediente
        /// </summary>
        public void CambiarEstadoOpcional(bool esOpcional)
        {
            EsOpcional = esOpcional;
        }
    }
} 