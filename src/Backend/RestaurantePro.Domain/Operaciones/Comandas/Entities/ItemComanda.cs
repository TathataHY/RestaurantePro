
namespace RestaurantePro.Domain.Operaciones.Comandas.Entities
{
    /// <summary>
    /// Representa un producto incluido en una comanda
    /// </summary>
    public class ItemComanda : EntityBase
    {
        /// <summary>
        /// ID de la comanda a la que pertenece este ítem
        /// </summary>
        public Guid ComandaId { get; private set; }

        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Cantidad solicitada del producto
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Precio unitario del producto al momento de crear la comanda
        /// </summary>
        public decimal PrecioUnitario { get; private set; }

        /// <summary>
        /// Subtotal (cantidad * precio unitario)
        /// </summary>
        public decimal Subtotal { get; private set; }

        /// <summary>
        /// Observaciones específicas para este ítem (e.g., "sin cebolla")
        /// </summary>
        public string Observaciones { get; private set; }

        // Constructor privado para EF Core
        private ItemComanda() { }

        /// <summary>
        /// Constructor principal
        /// </summary>
        public ItemComanda(Guid comandaId, Guid productoId, int cantidad, decimal precioUnitario, string observaciones = null)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            if (precioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(precioUnitario));

            Id = Guid.NewGuid();
            ComandaId = comandaId;
            ProductoId = productoId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = cantidad * precioUnitario;
            Observaciones = observaciones;
        }

        /// <summary>
        /// Actualiza la cantidad del producto en la comanda
        /// </summary>
        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));

            Cantidad = nuevaCantidad;
            RecalcularSubtotal();
        }

        /// <summary>
        /// Actualiza el precio unitario
        /// </summary>
        public void ActualizarPrecioUnitario(decimal nuevoPrecioUnitario)
        {
            if (nuevoPrecioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(nuevoPrecioUnitario));

            PrecioUnitario = nuevoPrecioUnitario;
            RecalcularSubtotal();
        }

        /// <summary>
        /// Actualiza las observaciones del ítem
        /// </summary>
        public void ActualizarObservaciones(string nuevasObservaciones)
        {
            Observaciones = nuevasObservaciones;
        }

        /// <summary>
        /// Recalcula el subtotal basado en cantidad y precio unitario
        /// </summary>
        private void RecalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;
        }
    }
}
