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
        /// Aprueba la orden de compra
        /// </summary>
        /// <param name="dateTimeService">Servicio de fecha/hora</param>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Pendiente o no tiene items</exception>
        public void Aprobar(IDateTimeService dateTimeService)
        {
            if (Estado != EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException("No se puede aprobar una orden que no está en estado pendiente");
                
            if (!_items.Any())
                throw new InvalidOperationException("No se puede aprobar una orden sin items");
            
            // La orden se considera aprobada pero aún no enviada
            // En un flujo real, primero se aprueba y luego se envía
            var fechaAprobacion = dateTimeService.Now;
            
            Estado = EstadoOrdenCompra.Confirmada;
            // Emitir evento de aprobación
            AddDomainEvent(new OrdenCompraAprobada(Id, ProveedorId, fechaAprobacion, Total));
            
            MarkAsModified();
            ValidarInvariantes();
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
        /// Recibe la orden de compra
        /// </summary>
        /// <param name="fechaRecepcion">Fecha de recepción</param>
        /// <param name="observaciones">Observaciones de la recepción</param>
        /// <exception cref="InvalidOperationException">Si la orden no está en estado Enviada</exception>
        public void Recibir(DateTime fechaRecepcion, string? observaciones = null)
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

        /// <summary>
        /// Valida que la orden de compra cumpla con todas las reglas de negocio
        /// </summary>
        /// <param name="notification">Gestor de notificaciones para acumular errores</param>
        /// <returns>True si la orden es válida, False si tiene errores</returns>
        public bool Validar(INotificationManager notification)
        {
            notification
                .RequireNotNull(ProveedorId, "El proveedor es obligatorio", propertyName: "ProveedorId")
                .Require(Items != null && Items.Count > 0, "La orden debe tener al menos un ítem", propertyName: "Items")
                .Require(FechaEmision <= FechaEntregaEstimada, "La fecha de entrega esperada debe ser posterior a la fecha de emisión", propertyName: "FechaEntregaEsperada");
                
            // Si el estado es Completada, debe tener fecha de entrega real
            if (Estado == EstadoOrdenCompra.Completada)
            {
                notification.Require(FechaRecepcion.HasValue, "Una orden completada debe tener fecha de recepción", propertyName: "FechaRecepcion");
            }
            
            // Validar cada ítem
            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    notification
                        .Require(item.IngredienteId != Guid.Empty, "El ingrediente del ítem es obligatorio", propertyName: "Item.IngredienteId")
                        .Require(item.Cantidad > 0, "La cantidad debe ser mayor a cero", propertyName: "Item.Cantidad")
                        .Require(item.PrecioUnitario >= 0, "El precio unitario no puede ser negativo", propertyName: "Item.PrecioUnitario");
                }
            }
            
            return !notification.HasErrors;
        }
        
        /// <summary>
        /// Método para crear una orden de compra con validaciones y Result
        /// </summary>
        public static Result<OrdenCompra> Crear(
            Guid proveedorId,
            List<ItemOrdenCompra> items,
            DateTime fechaEmision,
            DateTime fechaEntregaEsperada,
            string usuario,
            INotificationManager notification)
        {
            notification.CreateNewNotification();
            
            // Validar el proveedor
            notification.RequireNotNull(proveedorId, "El proveedor es requerido", propertyName: "ProveedorId");
            notification.Require(proveedorId != Guid.Empty, "El ID del proveedor no puede estar vacío", propertyName: "ProveedorId");
            
            // Validar la fecha de emisión
            notification.Require(fechaEmision != DateTime.MinValue, "La fecha de emisión es requerida", propertyName: "FechaEmision");
            
            // Validar la fecha de entrega esperada
            notification.Require(fechaEntregaEsperada > fechaEmision, "La fecha de entrega esperada debe ser posterior a la fecha de emisión", propertyName: "FechaEntregaEsperada");
            
            // Validar los items
            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    // Validar propiedades de ItemOrdenCompra
                    notification.Require(item.IngredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", propertyName: "IngredienteId");
                    notification.Require(item.Cantidad > 0, "La cantidad debe ser mayor que cero", propertyName: "Cantidad");
                    notification.Require(!string.IsNullOrWhiteSpace(item.NombreIngrediente), "El nombre del ingrediente es requerido", propertyName: "NombreIngrediente");
                }
            }
            
            if (notification.HasErrors)
            {
                return notification.ToResult<OrdenCompra>(null);
            }
            
            // Crear la orden
            var orden = new OrdenCompra
            {
                ProveedorId = proveedorId,
                FechaEmision = fechaEmision,
                FechaEntregaEstimada = fechaEntregaEsperada,
                Estado = EstadoOrdenCompra.Pendiente,
                Observaciones = "Orden creada por " + usuario
            };
            
            // Agregar los items
            if (items != null)
            {
                foreach (var item in items)
                {
                    // Crear un nuevo item para esta orden
                    var nuevoItem = ItemOrdenCompra.Crear(
                        orden.Id,
                        item.IngredienteId,
                        item.NombreIngrediente,
                        item.Cantidad,
                        item.UnidadMedida);
                        
                    orden._items.Add(nuevoItem);
                }
            }
            
            // Calcular el total
            orden.RecalcularTotal();
            
            // Emitir evento de creación
            orden.AddDomainEvent(new OrdenCompraCreada(orden.Id, proveedorId, fechaEmision));
            
            return Result.Success(orden);
        }
        
        /// <summary>
        /// Recibir la orden con validaciones y Result
        /// </summary>
        public Result RecibirOrden(DateTime fechaEntregaReal, string usuario, INotificationManager notification)
        {
            notification.CreateNewNotification();
            
            // Validar el estado actual
            notification.Require(Estado == EstadoOrdenCompra.Enviada, 
                "Solo se pueden recibir órdenes en estado Enviada. Estado actual: " + Estado,
                propertyName: "Estado");
                
            // Validar la fecha de entrega
            notification.Require(fechaEntregaReal != DateTime.MinValue, 
                "La fecha de entrega real es requerida",
                propertyName: "FechaEntregaReal");
                
            // Validar que hay items
            notification.Require(_items.Any(), 
                "No se puede recibir una orden sin items",
                propertyName: "Items");
                
            if (notification.HasErrors)
            {
                return notification.ToResult();
            }
            
            // Actualizar el estado
            Estado = EstadoOrdenCompra.Recibida;
            FechaRecepcion = fechaEntregaReal;
            ObservacionesRecepcion = "Recibido por " + usuario;
            MarkAsModified();
            
            // Emitir evento
            AddDomainEvent(new OrdenCompraRecibida(Id, fechaEntregaReal, ObservacionesRecepcion));
            
            return Result.Success();
        }
        
        /// <summary>
        /// Cancela una orden de compra
        /// </summary>
        /// <param name="motivo">Motivo de la cancelación</param>
        /// <param name="usuario">Usuario que cancela la orden</param>
        /// <param name="notification">Gestor de notificaciones</param>
        /// <returns>Resultado de la operación</returns>
        public Result CancelarOrden(string motivo, string usuario, INotificationManager notification)
        {
            notification.CreateNewNotification();
            
            notification
                .Require(Estado == EstadoOrdenCompra.Pendiente, "Solo se pueden cancelar órdenes pendientes", propertyName: "Estado")
                .RequireNotEmpty(motivo, "El motivo de cancelación es obligatorio", propertyName: "Motivo")
                .RequireNotEmpty(usuario, "El usuario es obligatorio", propertyName: "Usuario");
                
            if (notification.HasErrors)
                return notification.ToResult();
                
            Estado = EstadoOrdenCompra.Cancelada;
            FechaCancelacion = DateTime.Now;
            MotivoCancelacion = motivo;
            MarkAsModified();
            
            AddDomainEvent(new OrdenCompraCancelada(Id, FechaCancelacion.Value, motivo));
            
            return Result.Success();
        }
    }
} 
