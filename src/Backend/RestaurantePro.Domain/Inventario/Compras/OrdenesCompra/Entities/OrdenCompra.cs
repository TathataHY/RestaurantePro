namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities
{
    /// <summary>
    /// Agregado raíz que representa una orden de compra a un proveedor.
    /// 
    /// Invariantes:
    /// - Una orden de compra debe tener al menos un item para poder ser enviada
    /// - El total de la orden debe reflejar siempre la suma de los items
    /// - Una orden cancelada no puede cambiar a ningún otro estado
    /// - Solo una orden en estado pendiente puede modificar sus items
    /// - La orden debe tener un proveedor válido asignado
    /// 
    /// Ciclo de vida:
    /// - Creación → Pendiente → Enviada → Recibida → [Finalizada]
    ///                      ↘ Cancelada
    /// 
    /// Reglas de negocio:
    /// - Cuando una orden cambia de estado, se emite el evento correspondiente
    /// - Las órdenes automáticas se generan a partir de eventos de stock bajo
    /// - Los items de orden son entidades internas al agregado
    /// - Solo se puede cancelar una orden pendiente o enviada
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
        /// Items de la orden de compra.
        /// Colección interna del agregado.
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
        /// Crea una nueva orden de compra.
        /// Factory method que garantiza la creación de órdenes en estado válido.
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
        /// Agrega un item a la orden de compra.
        /// Si ya existe un item para el mismo ingrediente, aumenta su cantidad.
        /// Solo puede usarse en órdenes en estado Pendiente.
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad solicitada</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <returns>El item agregado o actualizado</returns>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Pendiente</exception>
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
                
                // Recalcular el total (que indirectamente llamará a ValidarInvariantes)
                RecalcularTotal();
                
                return itemExistente;
            }

            // Crear un nuevo item
            var nuevoItem = ItemOrdenCompra.Crear(Id, ingredienteId, nombre, cantidad, unidadMedida);
            _items.Add(nuevoItem);
            
            // Recalcular el total (que indirectamente llamará a ValidarInvariantes)
            RecalcularTotal();

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
            
            // Validar invariantes antes de emitir eventos
            ValidarInvariantes();
            
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
            
            // Validar invariantes antes de emitir eventos
            ValidarInvariantes();
            
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
            
            // Validar invariantes antes de emitir eventos
            ValidarInvariantes();
            
            AddDomainEvent(new OrdenCompraCancelada(Id, FechaCancelacion.Value, motivo));
        }
        
        /// <summary>
        /// Recalcula el total de la orden
        /// </summary>
        private void RecalcularTotal()
        {
            Total = _items.Sum(i => i.Subtotal);
            MarkAsModified();
            ValidarInvariantes();
        }
        
        /// <summary>
        /// Valida todas las invariantes del agregado OrdenCompra.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            // Validar que el proveedor esté asignado
            if (ProveedorId == Guid.Empty)
                throw new InvalidOperationException("La orden debe tener un proveedor asignado");
            
            // Validar que el estado sea válido
            if (!Enum.IsDefined(typeof(EstadoOrdenCompra), Estado))
                throw new InvalidOperationException($"Estado de orden no válido: {Estado}");
            
            // Validar que el total sea consistente con los items
            decimal totalCalculado = _items.Sum(i => i.Subtotal);
            if (Math.Abs(Total - totalCalculado) > 0.01m)
                throw new InvalidOperationException($"Inconsistencia en el total de la orden. Calculado: {totalCalculado}, Actual: {Total}");
            
            // Validar coherencia de fechas
            if (FechaEntregaEstimada < FechaEmision)
                throw new InvalidOperationException("La fecha de entrega estimada no puede ser anterior a la fecha de emisión");
            
            if (FechaEnvio.HasValue && FechaEnvio < FechaEmision)
                throw new InvalidOperationException("La fecha de envío no puede ser anterior a la fecha de emisión");
            
            if (FechaRecepcion.HasValue && !FechaEnvio.HasValue)
                throw new InvalidOperationException("No se puede registrar recepción sin haber enviado la orden");
            
            if (FechaRecepcion.HasValue && FechaRecepcion < FechaEnvio)
                throw new InvalidOperationException("La fecha de recepción no puede ser anterior a la fecha de envío");
                
            // Validar coherencia de la cancelación
            if (Estado == EstadoOrdenCompra.Cancelada && string.IsNullOrWhiteSpace(MotivoCancelacion))
                throw new InvalidOperationException("Una orden cancelada debe tener un motivo de cancelación");
                
            // Validar que una orden enviada tenga items
            if ((Estado == EstadoOrdenCompra.Enviada || Estado == EstadoOrdenCompra.Recibida) && !_items.Any())
                throw new InvalidOperationException("La orden debe tener al menos un item");
            
            // NUEVAS VALIDACIONES
            
            // Validar límites de valores para la orden
            if (Total < 0)
                throw new InvalidOperationException("El total de la orden no puede ser negativo");
                
            if (Total > 1000000m) // Ejemplo: un millón como límite superior razonable
                throw new InvalidOperationException("El total de la orden excede el límite máximo permitido");
                
            // Validar que los items tengan cantidades y precios válidos
            foreach (var item in _items)
            {
                if (item.Cantidad <= 0)
                    throw new InvalidOperationException($"El item {item.NombreIngrediente} (ID: {item.Id}) tiene una cantidad inválida: {item.Cantidad}");
                    
                if (item.PrecioUnitario < 0)
                    throw new InvalidOperationException($"El item {item.NombreIngrediente} (ID: {item.Id}) tiene un precio unitario inválido: {item.PrecioUnitario}");
                    
                if (item.Subtotal != item.Cantidad * item.PrecioUnitario)
                    throw new InvalidOperationException($"Inconsistencia en el subtotal del item {item.NombreIngrediente} (ID: {item.Id})");
                    
                // Validar límite máximo por item (ejemplo: 1000 unidades como límite razonable)
                if (item.Cantidad > 1000m)
                    throw new InvalidOperationException($"La cantidad del item {item.NombreIngrediente} (ID: {item.Id}) excede el límite máximo permitido");
            }
            
            // Validar las transiciones de estado
            if (Estado == EstadoOrdenCompra.Recibida && !FechaRecepcion.HasValue)
                throw new InvalidOperationException("Una orden en estado Recibida debe tener fecha de recepción");
                
            if (Estado == EstadoOrdenCompra.Enviada && !FechaEnvio.HasValue)
                throw new InvalidOperationException("Una orden en estado Enviada debe tener fecha de envío");
                
            if (Estado == EstadoOrdenCompra.Cancelada && !FechaCancelacion.HasValue)
                throw new InvalidOperationException("Una orden en estado Cancelada debe tener fecha de cancelación");
                
            // Validar longitud de campos de texto
            if (!string.IsNullOrEmpty(Observaciones) && Observaciones.Length > 500)
                throw new InvalidOperationException("Las observaciones no pueden exceder los 500 caracteres");
                
            if (!string.IsNullOrEmpty(ObservacionesRecepcion) && ObservacionesRecepcion.Length > 500)
                throw new InvalidOperationException("Las observaciones de recepción no pueden exceder los 500 caracteres");
                
            if (!string.IsNullOrEmpty(MotivoCancelacion) && MotivoCancelacion.Length > 500)
                throw new InvalidOperationException("El motivo de cancelación no puede exceder los 500 caracteres");
                
            // Validar que las órdenes canceladas no tengan fecha de recepción
            if (Estado == EstadoOrdenCompra.Cancelada && FechaRecepcion.HasValue)
                throw new InvalidOperationException("Una orden cancelada no puede tener fecha de recepción");
                
            // Validar coherencia entre estado y fechas
            if (FechaCancelacion.HasValue && Estado != EstadoOrdenCompra.Cancelada)
                throw new InvalidOperationException("Una orden con fecha de cancelación debe estar en estado Cancelada");
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
