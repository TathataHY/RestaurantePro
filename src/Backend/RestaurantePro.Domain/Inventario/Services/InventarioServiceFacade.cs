namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Inventario
    /// </summary>
    public class InventarioServiceFacade : IInventarioServiceFacade
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly INotificationManager _notificationManager;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<OrdenCompraBuilder> _ordenCompraBuilderLogger;
        private readonly ILogger<IngredienteBuilder> _ingredienteBuilderLogger;
        private readonly IStockBajoPolicy _stockBajoPolicy;
        
        /// <summary>
        /// Constructor de la fachada de servicios de inventario
        /// </summary>
        public InventarioServiceFacade(
            IIngredienteRepository ingredienteRepository,
            IProveedorRepository proveedorRepository,
            IOrdenCompraRepository ordenCompraRepository,
            INotificationManager notificationManager,
            IDateTimeService dateTimeService,
            ILogger<OrdenCompraBuilder> ordenCompraBuilderLogger,
            ILogger<IngredienteBuilder> ingredienteBuilderLogger,
            IStockBajoPolicy stockBajoPolicy)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _ordenCompraBuilderLogger = ordenCompraBuilderLogger ?? throw new ArgumentNullException(nameof(ordenCompraBuilderLogger));
            _ingredienteBuilderLogger = ingredienteBuilderLogger ?? throw new ArgumentNullException(nameof(ingredienteBuilderLogger));
            _stockBajoPolicy = stockBajoPolicy ?? throw new ArgumentNullException(nameof(stockBajoPolicy));
        }
        
        private static string GetFullExceptionMessage(Exception ex)
        {
            if (ex == null) return string.Empty;
            var msg = ex.Message;
            if (ex.InnerException != null)
                msg += " | INNER: " + GetFullExceptionMessage(ex.InnerException);
            return msg;
        }
        
        #region Ingredientes
        
        /// <inheritdoc />
        public async Task<Result<Ingrediente>> RegistrarIngredienteAsync(
            string nombre, 
            string descripcion, 
            string unidadMedida, 
            decimal stockMinimo, 
            decimal stockActual,
            RotacionIngrediente rotacion = RotacionIngrediente.Media,
            TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño,
            decimal costo = 0,
            CancellationToken cancellationToken = default)
        {
            // Limpiar notificaciones previas
            _notificationManager.ClearErrors();
            
            try
            {
                // Convertir string unidadMedida a enum UnidadMedida
                if (!Enum.TryParse(unidadMedida, true, out UnidadMedida unidadMedidaEnum))
                {
                    return Result.Failure<Ingrediente>($"Unidad de medida no válida: {unidadMedida}");
                }
                
                // Generar código automático usando las primeras letras del nombre y un timestamp
                string codigo = $"{nombre.Substring(0, Math.Min(3, nombre.Length)).ToUpper()}-{DateTime.Now:yyyyMMddHHmmss}";
                
                // Usar IngredienteBuilder para crear el ingrediente con validaciones robustas
                var builder = new IngredienteBuilder(_notificationManager, _ingredienteBuilderLogger);
                
                var resultado = builder
                    .ConNombre(nombre)
                    .ConCodigo(codigo)
                    .ConDescripcion(descripcion)
                    .ConUnidadMedida(unidadMedidaEnum)
                    .ConStockMinimo(stockMinimo)
                    .ConStockActual(stockActual)
                    .ConRotacion(rotacion)
                    .ConTemporada(temporada)
                    .ConCostoPromedio(costo)
                    .Construir();
                
                if (!resultado.Succeeded)
                {
                    return Result.Failure<Ingrediente>($"Error al construir ingrediente: {string.Join(", ", resultado.Errors ?? new List<string>())}");
                }
                
                var ingrediente = resultado.Value;
                
                // Persistir el ingrediente
                await _ingredienteRepository.AgregarAsync(ingrediente);
                await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(ingrediente);
            }
            catch (Exception ex)
            {
                var message = $"Error al registrar el ingrediente: {GetFullExceptionMessage(ex)}";
                _notificationManager.AddError(message);
                return Result.Failure<Ingrediente>(message);
            }
        }
        
        /// <summary>
        /// Registra un nuevo ingrediente usando el IngredienteBuilder con opciones avanzadas
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="descripcion">Descripción del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <param name="stockMinimo">Stock mínimo</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="codigo">Código personalizado (opcional - se genera automático si no se proporciona)</param>
        /// <param name="proveedorPrincipalId">ID del proveedor principal (opcional)</param>
        /// <param name="rotacion">Nivel de rotación del ingrediente</param>
        /// <param name="temporada">Temporada del ingrediente</param>
        /// <param name="costo">Costo promedio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el ingrediente registrado</returns>
        public async Task<Result<Ingrediente>> RegistrarIngredienteAvanzadoAsync(
            string nombre,
            string descripcion,
            UnidadMedida unidadMedida,
            decimal stockMinimo,
            decimal stockActual,
            string? codigo = null,
            Guid? proveedorPrincipalId = null,
            RotacionIngrediente rotacion = RotacionIngrediente.Media,
            TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño,
            decimal costo = 0,
            CancellationToken cancellationToken = default)
        {
            // Limpiar notificaciones previas
            _notificationManager.ClearErrors();
            
            try
            {
                // Generar código automático si no se proporciona
                var codigoFinal = codigo ?? $"{nombre.Substring(0, Math.Min(3, nombre.Length)).ToUpper()}-{DateTime.Now:yyyyMMddHHmmss}";
                
                // Usar IngredienteBuilder para crear el ingrediente con validaciones robustas
                var builder = new IngredienteBuilder(_notificationManager, _ingredienteBuilderLogger);
                
                builder
                    .ConNombre(nombre)
                    .ConCodigo(codigoFinal)
                    .ConDescripcion(descripcion)
                    .ConUnidadMedida(unidadMedida)
                    .ConStockMinimo(stockMinimo)
                    .ConStockActual(stockActual)
                    .ConRotacion(rotacion)
                    .ConTemporada(temporada)
                    .ConCostoPromedio(costo);
                
                // Agregar proveedor principal si se proporciona
                if (proveedorPrincipalId.HasValue)
                {
                    builder.ConProveedorPrincipal(proveedorPrincipalId.Value);
                }
                
                var resultado = builder.Construir();
                
                if (!resultado.Succeeded)
                {
                    return Result.Failure<Ingrediente>($"Error al construir ingrediente: {string.Join(", ", resultado.Errors ?? new List<string>())}");
                }
                
                var ingrediente = resultado.Value;
                
                // Verificar si el proveedor existe (si se especificó)
                if (proveedorPrincipalId.HasValue)
                {
                    var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorPrincipalId.Value, cancellationToken);
                    if (proveedor == null)
                    {
                        return Result.Failure<Ingrediente>($"No se encontró el proveedor con ID {proveedorPrincipalId.Value}");
                    }
                }
                
                // Persistir el ingrediente
                await _ingredienteRepository.AgregarAsync(ingrediente);
                await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(ingrediente);
            }
            catch (Exception ex)
            {
                var message = $"Error al registrar el ingrediente avanzado: {ex.Message}";
                _notificationManager.AddError(message);
                return Result.Failure<Ingrediente>(message);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<Ingrediente>> ActualizarStockIngredienteAsync(
            Guid ingredienteId, 
            decimal cantidad, 
            TipoMovimientoInventario tipoMovimiento, 
            string? referencia = null, 
            string? observacion = null, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(ingredienteId != Guid.Empty, "El ID del ingrediente es requerido", propertyName: nameof(ingredienteId));
            _notificationManager.CurrentNotification.Require(cantidad > 0, "La cantidad debe ser mayor que cero", propertyName: nameof(cantidad));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Ingrediente>(null);
            }
            
            try
            {
                // Obtener el ingrediente
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    _notificationManager.AddError($"No se encontró el ingrediente con ID {ingredienteId}", propertyName: nameof(ingredienteId));
                    return _notificationManager.ToResult<Ingrediente>(null);
                }
                
                // Preparar el motivo para el movimiento
                string motivo = string.IsNullOrEmpty(observacion) 
                    ? $"Movimiento de {tipoMovimiento} - {referencia ?? "N/A"}" 
                    : observacion;
                
                // Crear el movimiento según el tipo
                switch (tipoMovimiento)
                {
                    case TipoMovimientoInventario.Ingreso:
                        ingrediente.IncrementarStock(cantidad, motivo);
                        break;
                    case TipoMovimientoInventario.Egreso:
                        if (cantidad > ingrediente.Stock)
                        {
                            _notificationManager.AddError($"Stock insuficiente. Stock actual: {ingrediente.Stock}, Cantidad solicitada: {cantidad}", propertyName: nameof(cantidad));
                            return _notificationManager.ToResult<Ingrediente>(null);
                        }
                        ingrediente.DecrementarStock(cantidad, motivo);
                        break;
                    case TipoMovimientoInventario.Ajuste:
                        if (cantidad > ingrediente.Stock)
                        {
                            // Incremento (ajuste positivo)
                            ingrediente.IncrementarStock(cantidad - ingrediente.Stock, $"Ajuste positivo - {motivo}");
                        }
                        else if (cantidad < ingrediente.Stock)
                        {
                            // Decremento (ajuste negativo)
                            ingrediente.DecrementarStock(ingrediente.Stock - cantidad, $"Ajuste negativo - {motivo}");
                        }
                        break;
                    default:
                        _notificationManager.AddError($"Tipo de movimiento no soportado: {tipoMovimiento}", propertyName: nameof(tipoMovimiento));
                        return _notificationManager.ToResult<Ingrediente>(null);
                }
                
                // Persistir cambios
                await _ingredienteRepository.ActualizarAsync(ingrediente);
                await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(ingrediente);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar el stock: {ex.Message}");
                return _notificationManager.ToResult<Ingrediente>(null);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<IEnumerable<Ingrediente>>> VerificarIngredientesStockBajoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var ingredientes = await _ingredienteRepository.ObtenerIngredientesConStockBajoAsync(cancellationToken);
                return Result.Success(ingredientes);
            }
            catch (Exception ex)
            {
                _notificationManager.CreateNewNotification();
                _notificationManager.AddError($"Error al verificar ingredientes con stock bajo: {ex.Message}");
                return _notificationManager.ToResult<IEnumerable<Ingrediente>>(new List<Ingrediente>());
            }
        }
        
        #endregion
        
        #region Órdenes de Compra
        
        /// <inheritdoc />
        public async Task<Result<OrdenCompra>> CrearOrdenCompraAsync(
            Guid proveedorId, 
            DateTime fechaEntregaEstimada, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(proveedorId != Guid.Empty, "El ID del proveedor es requerido", propertyName: nameof(proveedorId));
            _notificationManager.CurrentNotification.Require(fechaEntregaEstimada > _dateTimeService.Now, "La fecha de entrega estimada debe ser posterior a la fecha actual", propertyName: nameof(fechaEntregaEstimada));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<OrdenCompra>(null);
            }
            
            try
            {
                // Verificar que exista el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                if (proveedor == null)
                {
                    _notificationManager.AddError($"No se encontró el proveedor con ID {proveedorId}", propertyName: nameof(proveedorId));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                if (!proveedor.Activo)
                {
                    _notificationManager.AddError($"El proveedor con ID {proveedorId} no está activo", propertyName: nameof(proveedorId));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                // Usar OrdenCompraBuilder para crear con validaciones robustas
                var builder = new OrdenCompraBuilder(_notificationManager, _ordenCompraBuilderLogger);
                
                var resultadoOrden = builder
                    .ParaProveedor(proveedorId)
                    .ConFechaEmision(_dateTimeService.Now)
                    .ConFechaEntregaEstimada(fechaEntregaEstimada)
                    .ConObservaciones(observaciones)
                    .Construir();
                
                if (!resultadoOrden.Succeeded)
                {
                    return resultadoOrden; // Ya tiene los errores del builder
                }
                
                var ordenCompra = resultadoOrden.Value!;
                
                // Persistir la orden
                await _ordenCompraRepository.AgregarAsync(ordenCompra);
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(ordenCompra);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear la orden de compra: {ex.Message}");
                return _notificationManager.ToResult<OrdenCompra>(null);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<OrdenCompra>> AgregarItemOrdenCompraAsync(
            Guid ordenCompraId, 
            Guid ingredienteId, 
            decimal cantidad, 
            decimal precioUnitario, 
            string observacion = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(ordenCompraId != Guid.Empty, "El ID de la orden de compra es requerido", propertyName: nameof(ordenCompraId));
            _notificationManager.CurrentNotification.Require(ingredienteId != Guid.Empty, "El ID del ingrediente es requerido", propertyName: nameof(ingredienteId));
            _notificationManager.CurrentNotification.Require(cantidad > 0, "La cantidad debe ser mayor que cero", propertyName: nameof(cantidad));
            _notificationManager.CurrentNotification.Require(precioUnitario >= 0, "El precio unitario no puede ser negativo", propertyName: nameof(precioUnitario));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<OrdenCompra>(null);
            }
            
            try
            {
                // Obtener la orden de compra
                var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
                if (ordenCompra == null)
                {
                    _notificationManager.AddError($"No se encontró la orden de compra con ID {ordenCompraId}", propertyName: nameof(ordenCompraId));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                // Verificar que la orden esté en estado borrador
                if (ordenCompra.Estado != EstadoOrdenCompra.Borrador)
                {
                    _notificationManager.AddError($"No se pueden agregar items a una orden que no esté en estado Borrador. Estado actual: {ordenCompra.Estado}", propertyName: nameof(ordenCompra.Estado));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                // Verificar que exista el ingrediente
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    _notificationManager.AddError($"No se encontró el ingrediente con ID {ingredienteId}", propertyName: nameof(ingredienteId));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                // Agregar el ítem (ajustar según la firma del método real en OrdenCompra)
                ordenCompra.AgregarItem(ingredienteId, ingrediente.Nombre, cantidad, ingrediente.UnidadMedida);
                
                // Persistir cambios
                await _ordenCompraRepository.ActualizarAsync(ordenCompra);
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(ordenCompra);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar el ítem a la orden de compra: {ex.Message}");
                return _notificationManager.ToResult<OrdenCompra>(null);
            }
        }
        
        /// <summary>
        /// Crea una orden de compra usando el OrdenCompraBuilder con múltiples ítems
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEntregaEstimada">Fecha estimada de entrega</param>
        /// <param name="items">Lista de ítems a incluir en la orden</param>
        /// <param name="observaciones">Observaciones (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la orden de compra creada</returns>
        public async Task<Result<OrdenCompra>> CrearOrdenCompraAvanzadaAsync(
            Guid proveedorId,
            DateTime fechaEntregaEstimada,
            List<(Guid ingredienteId, decimal cantidad, decimal precioUnitario)> items,
            string observaciones = "",
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros básicos
            _notificationManager.CurrentNotification.Require(proveedorId != Guid.Empty, "El ID del proveedor es requerido", propertyName: nameof(proveedorId));
            _notificationManager.CurrentNotification.Require(fechaEntregaEstimada > _dateTimeService.Now, "La fecha de entrega estimada debe ser posterior a la fecha actual", propertyName: nameof(fechaEntregaEstimada));
            _notificationManager.CurrentNotification.Require(items != null && items.Count > 0, "Debe especificar al menos un ítem", propertyName: nameof(items));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<OrdenCompra>(null);
            }
            
            try
            {
                // Verificar que exista el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                if (proveedor == null)
                {
                    _notificationManager.AddError($"No se encontró el proveedor con ID {proveedorId}", propertyName: nameof(proveedorId));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                if (!proveedor.Activo)
                {
                    _notificationManager.AddError($"El proveedor con ID {proveedorId} no está activo", propertyName: nameof(proveedorId));
                    return _notificationManager.ToResult<OrdenCompra>(null);
                }
                
                // Usar OrdenCompraBuilder para crear con validaciones robustas
                var builder = new OrdenCompraBuilder(_notificationManager, _ordenCompraBuilderLogger);
                
                var builderResult = builder
                    .ParaProveedor(proveedorId)
                    .ConFechaEmision(_dateTimeService.Now)
                    .ConFechaEntregaEstimada(fechaEntregaEstimada)
                    .ConObservaciones(observaciones);
                
                // Agregar todos los ítems usando el builder
                foreach (var (ingredienteId, cantidad, precioUnitario) in items)
                {
                    // Obtener información del ingrediente
                    var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                    if (ingrediente == null)
                    {
                        _notificationManager.AddError($"No se encontró el ingrediente con ID {ingredienteId}", propertyName: nameof(items));
                        return _notificationManager.ToResult<OrdenCompra>(null);
                    }
                    
                    // Agregar ítem al builder
                    builderResult = builderResult.AgregarItem(
                        ingredienteId,
                        ingrediente.Nombre,
                        cantidad,
                        ingrediente.UnidadMedida,
                        precioUnitario);
                }
                
                // Construir la orden final
                var resultadoOrden = builderResult.Construir();
                
                if (!resultadoOrden.Succeeded)
                {
                    return resultadoOrden; // Ya tiene los errores del builder
                }
                
                var ordenCompra = resultadoOrden.Value!;
                
                // Persistir la orden
                await _ordenCompraRepository.AgregarAsync(ordenCompra);
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(ordenCompra);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear la orden de compra avanzada: {ex.Message}");
                return _notificationManager.ToResult<OrdenCompra>(null);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<bool>> ActualizarEstadoOrdenCompraAsync(
            Guid ordenCompraId, 
            EstadoOrdenCompra nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(ordenCompraId != Guid.Empty, "El ID de la orden de compra es requerido", propertyName: nameof(ordenCompraId));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la orden de compra
                var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
                if (ordenCompra == null)
                {
                    _notificationManager.AddError($"No se encontró la orden de compra con ID {ordenCompraId}", propertyName: nameof(ordenCompraId));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Actualizar el estado según el tipo (ajustar según la implementación real)
                switch (nuevoEstado)
                {
                    case EstadoOrdenCompra.Enviada:
                        ordenCompra.Enviar();
                        break;
                    case EstadoOrdenCompra.Cancelada:
                        ordenCompra.Cancelar("Cancelada desde servicio de inventario");
                        break;
                    case EstadoOrdenCompra.Recibida:
                        ordenCompra.Recibir(_dateTimeService.Now, "Recibida desde servicio de inventario");
                        break;
                    default:
                        _notificationManager.AddError($"Estado no soportado para actualización manual: {nuevoEstado}", propertyName: nameof(nuevoEstado));
                        return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _ordenCompraRepository.ActualizarAsync(ordenCompra);
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar el estado de la orden de compra: {ex.Message}");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<bool>> RecibirOrdenCompraCompletaAsync(
            Guid ordenCompraId, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(ordenCompraId != Guid.Empty, "El ID de la orden de compra es requerido", propertyName: nameof(ordenCompraId));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la orden de compra
                var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
                if (ordenCompra == null)
                {
                    _notificationManager.AddError($"No se encontró la orden de compra con ID {ordenCompraId}", propertyName: nameof(ordenCompraId));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar que esté en estado Enviada
                if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
                {
                    _notificationManager.AddError($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}", propertyName: nameof(ordenCompra.Estado));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Marcar como recibida
                ordenCompra.Recibir(_dateTimeService.Now, observaciones);
                
                // Registrar entrada de stock para cada ítem
                foreach (var item in ordenCompra.Items)
                {
                    var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                    if (ingrediente != null)
                    {
                        ingrediente.IncrementarStock(
                            item.Cantidad,
                            $"Recepción de orden de compra #{ordenCompraId}");
                        
                        await _ingredienteRepository.ActualizarAsync(ingrediente);
                    }
                }
                
                // Persistir cambios
                await _ordenCompraRepository.ActualizarAsync(ordenCompra);
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al recibir la orden de compra: {ex.Message}");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<bool>> RecibirOrdenCompraParcialAsync(
            Guid ordenCompraId, 
            Dictionary<Guid, decimal> itemsRecibidos, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(ordenCompraId != Guid.Empty, "El ID de la orden de compra es requerido", propertyName: nameof(ordenCompraId));
            _notificationManager.CurrentNotification.Require(itemsRecibidos != null && itemsRecibidos.Count > 0, "Se debe recibir al menos un ítem", propertyName: nameof(itemsRecibidos));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la orden de compra
                var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
                if (ordenCompra == null)
                {
                    _notificationManager.AddError($"No se encontró la orden de compra con ID {ordenCompraId}", propertyName: nameof(ordenCompraId));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar que esté en estado Enviada
                if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
                {
                    _notificationManager.AddError($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}", propertyName: nameof(ordenCompra.Estado));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Validar que todos los items especificados existen en la orden
                foreach (var itemId in itemsRecibidos.Keys)
                {
                    if (!ordenCompra.Items.Any(i => i.Id == itemId))
                    {
                        _notificationManager.AddError($"El item con ID {itemId} no existe en esta orden", propertyName: nameof(itemsRecibidos));
                        return _notificationManager.ToResult<bool>(false);
                    }
                }
                
                // La orden se considera como recibida de forma parcial pero el estado actual será Recibida
                ordenCompra.Recibir(_dateTimeService.Now, $"{observaciones} (Recepción parcial)");
                
                // Registrar entrada de stock para los items recibidos
                foreach (var kvp in itemsRecibidos)
                {
                    var itemId = kvp.Key;
                    var cantidadRecibida = kvp.Value;
                    
                    var item = ordenCompra.Items.FirstOrDefault(i => i.Id == itemId);
                    if (item != null && cantidadRecibida > 0)
                    {
                        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                        if (ingrediente != null)
                        {
                            ingrediente.IncrementarStock(
                                cantidadRecibida, 
                                $"Recepción parcial de orden de compra #{ordenCompraId}");
                            
                            await _ingredienteRepository.ActualizarAsync(ingrediente);
                        }
                    }
                }
                
                // Persistir cambios
                await _ordenCompraRepository.ActualizarAsync(ordenCompra);
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al recibir parcialmente la orden de compra: {ex.Message}");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<IEnumerable<OrdenCompra>>> GenerarOrdenesCompraAutomaticasAsync(CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Ejecutar política de stock bajo para obtener ingredientes priorizados
                var resultado = await _stockBajoPolicy.EjecutarAsync(cancellationToken);
                
                var ordenesCompra = new List<OrdenCompra>();
                
                // Verificar que el resultado fue exitoso
                if (!resultado.Succeeded || resultado.Value == null)
                {
                    return Result.Success<IEnumerable<OrdenCompra>>(ordenesCompra);
                }
                
                // Agrupar ingredientes por proveedor
                var ingredientesPorProveedor = resultado.Value.IngredientesPriorizados
                    .Select(async ip => {
                        // Obtener el ingrediente completo con su proveedor
                        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ip.IngredienteId, cancellationToken);
                        return new { Ingrediente = ingrediente, Prioridad = ip.Prioridad };
                    })
                    .Select(t => t.Result)
                    .Where(t => t.Ingrediente != null && t.Ingrediente.ProveedorPrincipalId.HasValue)
                    .GroupBy(t => t.Ingrediente.ProveedorPrincipalId.Value);
                
                // Generar una orden por cada proveedor
                foreach (var grupo in ingredientesPorProveedor)
                {
                    var proveedorId = grupo.Key;
                    
                    // Verificar si existe el proveedor
                    var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                    if (proveedor == null || !proveedor.Activo)
                        continue;
                        
                    // Usar OrdenCompraBuilder para crear la orden automática con validaciones robustas
                    var builder = new OrdenCompraBuilder(_notificationManager, _ordenCompraBuilderLogger);
                    
                    var observacionesAutomaticas = $"Orden automática por stock bajo - {_dateTimeService.Now:dd/MM/yyyy}";
                    var fechaEntregaEstimada = _dateTimeService.Now.AddDays(3);
                    
                    var resultadoOrden = builder
                        .ParaProveedor(proveedorId)
                        .ConFechaEmision(_dateTimeService.Now)
                        .ConFechaEntregaEstimada(fechaEntregaEstimada)
                        .ConObservaciones(observacionesAutomaticas);
                    
                    // Agregar items a la orden usando el builder
                    foreach (var item in grupo.OrderByDescending(g => g.Prioridad))
                    {
                        var ingrediente = item.Ingrediente;
                        
                        // Calcular cantidad a pedir
                        decimal cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
                        decimal cantidadPedir = Math.Max(1, Math.Ceiling(cantidadFaltante * 1.2m));
                        
                        // Agregar a la orden usando el builder
                        resultadoOrden = resultadoOrden.AgregarItem(
                            ingrediente.Id,
                            ingrediente.Nombre,
                            cantidadPedir,
                            ingrediente.UnidadMedida);
                    }
                    
                    // Construir la orden final
                    var resultadoFinal = resultadoOrden.Construir();
                    
                    if (!resultadoFinal.Succeeded)
                    {
                        // Log del error pero continúa con el siguiente proveedor
                        _notificationManager.AddError($"Error al crear orden automática para proveedor {proveedorId}: {resultadoFinal.Error}", "OrdenAutomatica");
                        continue;
                    }
                    
                    var orden = resultadoFinal.Value!;
                    
                    // Persistir la orden
                    await _ordenCompraRepository.AgregarAsync(orden);
                    ordenesCompra.Add(orden);
                }
                
                // Guardar todos los cambios
                await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success<IEnumerable<OrdenCompra>>(ordenesCompra);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al generar órdenes de compra automáticas: {ex.Message}");
                return _notificationManager.ToResult<IEnumerable<OrdenCompra>>(new List<OrdenCompra>());
            }
        }
        
        #endregion
    }
} 