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

        /// <summary>
        /// Estado actual del ítem
        /// </summary>
        public EstadoItemComanda Estado { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'EnPreparacion'
        /// </summary>
        public DateTime? FechaPreparacion { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'Listo'
        /// </summary>
        public DateTime? FechaListo { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'Entregado'
        /// </summary>
        public DateTime? FechaEntrega { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'Cancelado'
        /// </summary>
        public DateTime? FechaCancelacion { get; private set; }

        /// <summary>
        /// Motivo de cancelación (si aplica)
        /// </summary>
        public string MotivoCancelacion { get; private set; }

        // Constructor privado para EF Core
        private ItemComanda() { }

        /// <summary>
        /// Constructor principal
        /// </summary>
        public ItemComanda(Guid comandaId, Guid productoId, int cantidad, decimal precioUnitario, string observaciones = "")
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
            Observaciones = observaciones ?? string.Empty;
            Estado = EstadoItemComanda.Pendiente;
            
            // No emitimos evento de creación, la comanda ya lo hace
        }

        /// <summary>
        /// Marca el ítem como "En Preparación"
        /// </summary>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda MarcarEnPreparacion()
        {
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se puede marcar como en preparación un ítem con estado {Estado}");
                
            Estado = EstadoItemComanda.EnPreparacion;
            FechaPreparacion = DateTime.Now;
            
            AddDomainEvent(new ItemComandaEnPreparacion(Id, ProductoId, ComandaId));
            
            return this;
        }
        
        /// <summary>
        /// Marca el ítem como "Listo" para ser entregado
        /// </summary>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda MarcarComoListo()
        {
            if (Estado != EstadoItemComanda.EnPreparacion)
                throw new InvalidOperationException($"No se puede marcar como listo un ítem que no está en preparación");
                
            Estado = EstadoItemComanda.Listo;
            FechaListo = DateTime.Now;
            
            AddDomainEvent(new ItemComandaListo(Id, ProductoId, ComandaId));
            
            return this;
        }
        
        /// <summary>
        /// Marca el ítem como "Entregado" al cliente
        /// Esto desencadena la actualización del inventario
        /// </summary>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda MarcarComoEntregado()
        {
            if (Estado != EstadoItemComanda.Listo)
                throw new InvalidOperationException($"No se puede entregar un ítem que no está listo");
                
            Estado = EstadoItemComanda.Entregado;
            FechaEntrega = DateTime.Now;
            
            AddDomainEvent(new ItemComandaEntregado(Id, ProductoId, ComandaId, Cantidad));
            
            return this;
        }
        
        /// <summary>
        /// Cancela el ítem con un motivo
        /// </summary>
        /// <param name="motivo">Razón por la que se cancela el ítem</param>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda Cancelar(string motivo)
        {
            if (Estado == EstadoItemComanda.Entregado || Estado == EstadoItemComanda.Cancelado)
                throw new InvalidOperationException($"No se puede cancelar un ítem con estado {Estado}");
                
            Estado = EstadoItemComanda.Cancelado;
            FechaCancelacion = DateTime.Now;
            MotivoCancelacion = motivo;
            
            // Agregamos el motivo a las observaciones para referencia histórica
            Observaciones = string.IsNullOrEmpty(Observaciones) 
                ? $"Cancelado: {motivo}" 
                : $"{Observaciones} | Cancelado: {motivo}";
                
            AddDomainEvent(new ItemComandaCancelado(Id, ProductoId, ComandaId, motivo));
            
            return this;
        }

        /// <summary>
        /// Actualiza la cantidad del producto en la comanda
        /// Solo permitido en estado Pendiente
        /// </summary>
        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));
                
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se puede modificar la cantidad de un ítem con estado {Estado}");

            Cantidad = nuevaCantidad;
            RecalcularSubtotal();
        }

        /// <summary>
        /// Actualiza el precio unitario
        /// Solo permitido en estado Pendiente
        /// </summary>
        public void ActualizarPrecioUnitario(decimal nuevoPrecioUnitario)
        {
            if (nuevoPrecioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(nuevoPrecioUnitario));
                
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se puede modificar el precio de un ítem con estado {Estado}");

            PrecioUnitario = nuevoPrecioUnitario;
            RecalcularSubtotal();
        }

        /// <summary>
        /// Actualiza las observaciones del ítem
        /// Solo permitido en estado Pendiente o EnPreparacion
        /// </summary>
        public void ActualizarObservaciones(string nuevasObservaciones)
        {
            if (Estado != EstadoItemComanda.Pendiente && Estado != EstadoItemComanda.EnPreparacion)
                throw new InvalidOperationException($"No se pueden modificar las observaciones de un ítem con estado {Estado}");
                
            Observaciones = nuevasObservaciones ?? string.Empty;
        }

        /// <summary>
        /// Recalcula el subtotal basado en cantidad y precio unitario
        /// </summary>
        private void RecalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;
        }

        /// <summary>
        /// Método para obtener el ID del producto.
        /// Facilita el mockeo de la clase.
        /// </summary>
        public virtual Guid ObtenerProductoId()
        {
            return ProductoId;
        }
        
        /// <summary>
        /// Método para obtener la cantidad.
        /// Facilita el mockeo de la clase.
        /// </summary>
        public virtual int ObtenerCantidad()
        {
            return Cantidad;
        }
    }
}
