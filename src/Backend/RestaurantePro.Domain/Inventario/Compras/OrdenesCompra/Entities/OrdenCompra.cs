namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities
{
    /// <summary>
    /// Entidad que representa una orden de compra a un proveedor
    /// </summary>
    public class OrdenCompra : EntityBase, IAggregateRoot
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
        
        /// <summary>
        /// Indica si todos los items de la orden han sido recibidos completamente
        /// </summary>
        public bool TodosLosItemsRecibidos => Estado == EstadoOrdenCompra.Recibida && 
                                             (_items.Count == 0 || _items.All(i => i.EstaCompletoEnRecepcion));
        
        // Constructor privado para EF Core
        private OrdenCompra() { }
        
        /// <summary>
        /// Crea una nueva orden de compra
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="observaciones">Observaciones de la orden</param>
        /// <param name="fechaEmision">Fecha de emisión de la orden</param>
        /// <returns>Nueva instancia de orden de compra</returns>
        public static OrdenCompra Crear(Guid proveedorId, string observaciones, DateTime fechaEmision)
        {
            var ordenCompra = new OrdenCompra
            {
                ProveedorId = proveedorId,
                FechaEmision = fechaEmision,
                Estado = EstadoOrdenCompra.Pendiente,
                Observaciones = observaciones ?? "Orden de compra automática"
            };

            ordenCompra.AddDomainEvent(new OrdenCompraCreada(ordenCompra.Id, proveedorId, fechaEmision));

            return ordenCompra;
        }
        
        /// <summary>
        /// Agrega un item a la orden de compra
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad solicitada</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <returns>El item agregado</returns>
        public ItemOrdenCompra AgregarItem(Guid ingredienteId, string nombre, decimal cantidad, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida)
        {
            if (Estado != EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException("No se pueden agregar items a una orden que no está en estado pendiente");

            // Verificar si ya existe un item para este ingrediente
            var itemExistente = Items.FirstOrDefault(i => i.IngredienteId == ingredienteId);
            if (itemExistente != null)
            {
                // Actualizar la cantidad del item existente
                itemExistente.AumentarCantidad(cantidad);
                return itemExistente;
            }

            // Crear un nuevo item
            var nuevoItem = ItemOrdenCompra.Crear(Id, ingredienteId, nombre, cantidad, unidadMedida);
            _items.Add(nuevoItem);

            AddDomainEvent(new ItemOrdenCompraAgregado(Id, ingredienteId, nombre, cantidad));

            return nuevoItem;
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
        
        /// <summary>
        /// Establece la fecha estimada de entrega
        /// </summary>
        /// <param name="fechaEntrega">Fecha de entrega estimada</param>
        /// <exception cref="ArgumentException">Si la fecha es anterior a la fecha de emisión</exception>
        public void EstablecerFechaEntrega(DateTime fechaEntrega)
        {
            if (fechaEntrega < FechaEmision)
                throw new ArgumentException("La fecha de entrega no puede ser anterior a la fecha de emisión", nameof(fechaEntrega));
                
            FechaEntregaEstimada = fechaEntrega;
            MarkAsModified();
        }
    }
} 
