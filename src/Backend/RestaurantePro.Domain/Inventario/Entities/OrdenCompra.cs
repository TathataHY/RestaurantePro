namespace RestaurantePro.Domain.Inventario.Entities
{
    /// <summary>
    /// Entidad que representa una orden de compra a un proveedor
    /// </summary>
    public class OrdenCompra : EntityBase
    {
        /// <summary>
        /// ID del proveedor al que se realiza la orden
        /// </summary>
        public Guid ProveedorId { get; private set; }
        
        /// <summary>
        /// Fecha en que se emitió la orden
        /// </summary>
        public DateTime FechaEmision { get; private set; }
        
        /// <summary>
        /// Fecha estimada de entrega
        /// </summary>
        public DateTime FechaEntregaEstimada { get; private set; }
        
        /// <summary>
        /// Fecha en que se envió la orden al proveedor
        /// </summary>
        public DateTime? FechaEnvio { get; private set; }
        
        /// <summary>
        /// Fecha en que se recibió la orden
        /// </summary>
        public DateTime? FechaRecepcion { get; private set; }
        
        /// <summary>
        /// Fecha en que se canceló la orden
        /// </summary>
        public DateTime? FechaCancelacion { get; private set; }
        
        /// <summary>
        /// Observaciones generales para la orden
        /// </summary>
        public string Observaciones { get; private set; }
        
        /// <summary>
        /// Observaciones de la recepción
        /// </summary>
        public string ObservacionesRecepcion { get; private set; }
        
        /// <summary>
        /// Motivo de cancelación si la orden fue cancelada
        /// </summary>
        public string MotivoCancelacion { get; private set; }
        
        /// <summary>
        /// Estado actual de la orden
        /// </summary>
        public EstadoOrdenCompra Estado { get; private set; }
        
        /// <summary>
        /// Monto total de la orden
        /// </summary>
        public decimal Total { get; private set; }
        
        /// <summary>
        /// Items de la orden de compra
        /// </summary>
        private readonly List<ItemOrdenCompra> _items = new List<ItemOrdenCompra>();
        
        /// <summary>
        /// Acceso de solo lectura a los items de la orden
        /// </summary>
        public IReadOnlyCollection<ItemOrdenCompra> Items => _items.AsReadOnly();
        
        // Constructor privado para EF Core
        private OrdenCompra() { }
        
        /// <summary>
        /// Crea una nueva orden de compra
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEmision">Fecha de emisión</param>
        /// <param name="fechaEntregaEstimada">Fecha estimada de entrega</param>
        /// <param name="observaciones">Observaciones generales</param>
        /// <returns>Nueva orden de compra</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public static OrdenCompra Crear(Guid proveedorId, DateTime fechaEmision, DateTime fechaEntregaEstimada, string observaciones = null)
        {
            if (fechaEntregaEstimada < fechaEmision)
                throw new ArgumentException("La fecha de entrega estimada no puede ser anterior a la fecha de emisión", nameof(fechaEntregaEstimada));
                
            var ordenCompra = new OrdenCompra
            {
                ProveedorId = proveedorId,
                FechaEmision = fechaEmision,
                FechaEntregaEstimada = fechaEntregaEstimada,
                Observaciones = observaciones ?? string.Empty,
                Estado = EstadoOrdenCompra.Pendiente,
                Total = 0
            };
            
            ordenCompra.AddDomainEvent(new OrdenCompraCreada(
                ordenCompra.Id, 
                proveedorId, 
                fechaEmision, 
                fechaEntregaEstimada));
            
            return ordenCompra;
        }
        
        /// <summary>
        /// Agrega un item a la orden de compra
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad</param>
        /// <param name="precioUnitario">Precio unitario</param>
        /// <returns>El item agregado</returns>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Pendiente</exception>
        public ItemOrdenCompra AgregarItem(Guid ingredienteId, decimal cantidad, decimal precioUnitario)
        {
            if (Estado != EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException("No se pueden agregar items a una orden que no está en estado pendiente");
                
            var item = ItemOrdenCompra.Crear(Id, ingredienteId, cantidad, precioUnitario);
            _items.Add(item);
            
            // Recalcular el total
            RecalcularTotal();
            
            AddDomainEvent(new ItemOrdenCompraAgregado(
                Id,
                item.Id,
                ingredienteId,
                cantidad,
                precioUnitario,
                item.Subtotal));
                
            return item;
        }
        
        /// <summary>
        /// Elimina un item de la orden de compra
        /// </summary>
        /// <param name="itemId">ID del item a eliminar</param>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Pendiente</exception>
        /// <exception cref="ArgumentException">Si el item no existe</exception>
        public void EliminarItem(Guid itemId)
        {
            if (Estado != EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException("No se pueden eliminar items de una orden que no está en estado pendiente");
                
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new ArgumentException($"No existe un item con el ID {itemId} en esta orden", nameof(itemId));
                
            _items.Remove(item);
            
            // Recalcular el total
            RecalcularTotal();
            
            AddDomainEvent(new ItemOrdenCompraEliminado(
                Id,
                itemId,
                item.IngredienteId,
                Total));
        }
        
        /// <summary>
        /// Envía la orden de compra al proveedor
        /// </summary>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Pendiente o no tiene items</exception>
        public void Enviar()
        {
            if (Estado != EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException("No se puede enviar una orden que no está en estado pendiente");
                
            if (!_items.Any())
                throw new InvalidOperationException("No se puede enviar una orden sin items");
                
            Estado = EstadoOrdenCompra.Enviada;
            FechaEnvio = DateTime.Now;
            MarkAsModified();
            
            AddDomainEvent(new OrdenCompraEnviada(Id, FechaEnvio.Value, Total));
        }
        
        /// <summary>
        /// Registra la recepción de la orden de compra
        /// </summary>
        /// <param name="fechaRecepcion">Fecha de recepción</param>
        /// <param name="observaciones">Observaciones de la recepción</param>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Enviada</exception>
        public void Recibir(DateTime fechaRecepcion, string observaciones = null)
        {
            if (Estado != EstadoOrdenCompra.Enviada)
                throw new InvalidOperationException("No se puede recibir una orden que no está en estado enviada");
                
            Estado = EstadoOrdenCompra.Recibida;
            FechaRecepcion = fechaRecepcion;
            ObservacionesRecepcion = observaciones ?? string.Empty;
            MarkAsModified();
            
            AddDomainEvent(new OrdenCompraRecibida(Id, fechaRecepcion, ObservacionesRecepcion));
        }
        
        /// <summary>
        /// Cancela la orden de compra
        /// </summary>
        /// <param name="motivo">Motivo de la cancelación</param>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Pendiente o Enviada</exception>
        public void Cancelar(string motivo)
        {
            if (Estado != EstadoOrdenCompra.Pendiente && Estado != EstadoOrdenCompra.Enviada)
                throw new InvalidOperationException("La orden no puede cancelarse en su estado actual");
                
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de cancelación no puede estar vacío", nameof(motivo));
                
            Estado = EstadoOrdenCompra.Cancelada;
            FechaCancelacion = DateTime.Now;
            MotivoCancelacion = motivo;
            MarkAsModified();
            
            AddDomainEvent(new OrdenCompraCancelada(Id, FechaCancelacion.Value, motivo));
        }
        
        /// <summary>
        /// Recalcula el total de la orden
        /// </summary>
        private void RecalcularTotal()
        {
            Total = _items.Sum(i => i.Subtotal);
            MarkAsModified();
        }
    }
} 