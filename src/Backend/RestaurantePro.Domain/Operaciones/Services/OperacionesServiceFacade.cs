using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Operaciones
    /// </summary>
    public class OperacionesServiceFacade : IOperacionesServiceFacade
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IServicioPreparaciones _servicioPreparaciones;
        private readonly INotificationManager _notificationManager;
        private readonly ILogger<ComandaBuilder> _comandaBuilderLogger;
        private readonly ILogger<ReservacionBuilder> _reservacionBuilderLogger;
        private readonly ILogger<MesaBuilder> _mesaBuilderLogger;
        
        public OperacionesServiceFacade(
            IComandaRepository comandaRepository,
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IProductoRepository productoRepository,
            IServicioPreparaciones servicioPreparaciones,
            INotificationManager notificationManager,
            ILogger<ComandaBuilder> comandaBuilderLogger,
            ILogger<ReservacionBuilder> reservacionBuilderLogger,
            ILogger<MesaBuilder> mesaBuilderLogger)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _servicioPreparaciones = servicioPreparaciones ?? throw new ArgumentNullException(nameof(servicioPreparaciones));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _comandaBuilderLogger = comandaBuilderLogger ?? throw new ArgumentNullException(nameof(comandaBuilderLogger));
            _reservacionBuilderLogger = reservacionBuilderLogger ?? throw new ArgumentNullException(nameof(reservacionBuilderLogger));
            _mesaBuilderLogger = mesaBuilderLogger ?? throw new ArgumentNullException(nameof(mesaBuilderLogger));
        }
        
        #region Comandas

        /// <inheritdoc />
        public async Task<Result<Comanda>> CrearNuevaComandaAsync(
            Guid? clienteId, 
            Guid? mesaId, 
            Guid meseroId, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(meseroId != Guid.Empty, "El ID del mesero no puede estar vacío", "MeseroId");
            
            if (mesaId.HasValue)
            {
                _notificationManager.Require(mesaId.Value != Guid.Empty, "El ID de la mesa no puede estar vacío", "MesaId");
            }
            
            if (clienteId.HasValue)
            {
                _notificationManager.Require(clienteId.Value != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            }
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // Usar ComandaBuilder para crear la comanda con validaciones robustas
                var builder = new ComandaBuilder(_notificationManager, _comandaBuilderLogger);
                
                builder.ConMesero(meseroId);
                
                if (clienteId.HasValue)
                {
                    builder.ConCliente(clienteId.Value);
                }
                
                if (mesaId.HasValue)
                {
                    builder.EnMesa(mesaId.Value);
                }
                
                if (!string.IsNullOrWhiteSpace(observaciones))
                {
                    builder.ConObservaciones(observaciones);
                }
                
                // Construir la comanda
                var resultadoComanda = builder.Construir();
                if (!resultadoComanda.Succeeded)
                {
                    return resultadoComanda; // Ya tiene los errores del builder
                }
                
                var comanda = resultadoComanda.Value!;
                
                // Persistir la comanda
                await _comandaRepository.AgregarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear comanda: {ex.Message}", "CrearComanda");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Comanda>> AgregarProductoAComandaAsync(
            Guid comandaId, 
            Guid productoId, 
            int cantidad, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor a cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Obtener el producto
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                if (producto == null)
                {
                    _notificationManager.AddError($"No se encontró el producto con ID {productoId}", "ProductoId");
                    return _notificationManager.ToResult<Comanda>(null);
                }

                // 🍳 FLUJO HÍBRIDO: Verificar preparaciones primero y agregar producto
                var resultadoAgregar = await AgregarProductoConFlujoPrepararcionesAsync(
                    comanda, producto, cantidad, observaciones);
                
                if (!resultadoAgregar.Succeeded)
                {
                    _notificationManager.AddError("Error al agregar producto con flujo de preparaciones", "AgregarProducto");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar producto a comanda: {ex.Message}", "AgregarProducto");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        /// <summary>
        /// 🍳 Flujo híbrido: Verifica preparaciones primero, luego inventario si es necesario
        /// </summary>
        private async Task<Result> AgregarProductoConFlujoPrepararcionesAsync(
            Comanda comanda, 
            Producto producto, 
            int cantidad, 
            string observaciones)
        {
            bool tomarDePreparaciones = false;
            string observacionesCompletas = observaciones;
            
            try
            {
                // 🔍 Paso 1: Verificar disponibilidad en preparaciones diarias
                var disponibilidadResult = await _servicioPreparaciones.VerificarDisponibilidadAsync(
                    producto.Id, cantidad);
                
                if (disponibilidadResult.Succeeded && disponibilidadResult.Value)
                {
                    // ✅ Paso 2: Consumir de preparaciones diarias
                    var consumoResult = await _servicioPreparaciones.ConsumirPreparacionAsync(
                        producto.Id, cantidad);
                    
                    if (consumoResult.Succeeded)
                    {
                        tomarDePreparaciones = true;
                        _notificationManager.AddInformation(
                            $"✅ Producto '{producto.Nombre}' (cantidad: {cantidad}) tomado de preparaciones diarias", 
                            "FlujoPreparariones");
                        
                        // Agregar información a las observaciones
                        observacionesCompletas = string.IsNullOrWhiteSpace(observaciones) 
                            ? "🍳 Preparación diaria" 
                            : $"{observaciones} (🍳 Preparación diaria)";
                    }
                    else
                    {
                        _notificationManager.AddError(
                            $"⚠️ No se pudo consumir preparación para '{producto.Nombre}'. Motivo: {consumoResult.Error}", 
                            "FlujoPreparariones");
                    }
                }
                else
                {
                    _notificationManager.AddInformation(
                        $"ℹ️ Producto '{producto.Nombre}' no disponible en preparaciones, se preparará al momento", 
                        "FlujoPreparariones");
                }
            }
            catch (Exception ex)
            {
                _notificationManager.AddError(
                    $"⚠️ Error verificando preparaciones para '{producto.Nombre}': {ex.Message}. Continuando con flujo normal.", 
                    "FlujoPreparariones");
            }

            // 🥘 Paso 3: Si no se tomó de preparaciones, verificar inventario
            if (!tomarDePreparaciones)
            {
                // TODO: Aquí se integraría con el verificador de inventario/ingredientes
                // Por ahora solo agregamos información
                observacionesCompletas = string.IsNullOrWhiteSpace(observaciones) 
                    ? "🥘 Preparación al momento" 
                    : $"{observaciones} (🥘 Preparación al momento)";
                    
                _notificationManager.AddInformation(
                    $"🥘 Producto '{producto.Nombre}' será preparado al momento", 
                    "FlujoPreparariones");
            }

            // 📝 Paso 4: Agregar el producto a la comanda
            try
            {
                comanda.AgregarItem(producto.Id, producto.Nombre, cantidad, producto.Precio.Valor, observacionesCompletas);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error agregando ítem a comanda: {ex.Message}", "AgregarItem");
                return Result.Failure($"Error agregando ítem a comanda: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AgregarPersonalizacionExtraAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            decimal cantidad, 
            decimal precioAdicional = 0, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(itemId != Guid.Empty, "El ID del ítem no puede estar vacío", "ItemId");
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngrediente), "El nombre del ingrediente no puede estar vacío", "NombreIngrediente");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor a cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Agregar personalización de tipo "extra"
                var personalizado = comanda.AgregarPersonalizacionExtra(
                    itemId, 
                    ingredienteId, 
                    nombreIngrediente, 
                    cantidad, 
                    precioAdicional);
                
                if (!personalizado)
                {
                    _notificationManager.AddError($"No se pudo agregar la personalización al ítem con ID {itemId}", "ItemId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar personalización: {ex.Message}", "AgregarPersonalizacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AgregarPersonalizacionQuitarAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(itemId != Guid.Empty, "El ID del ítem no puede estar vacío", "ItemId");
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngrediente), "El nombre del ingrediente no puede estar vacío", "NombreIngrediente");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Agregar personalización de tipo "quitar"
                var personalizado = comanda.AgregarPersonalizacionQuitar(
                    itemId, 
                    ingredienteId, 
                    nombreIngrediente);
                
                if (!personalizado)
                {
                    _notificationManager.AddError($"No se pudo agregar la personalización al ítem con ID {itemId}", "ItemId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar personalización: {ex.Message}", "AgregarPersonalizacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AgregarPersonalizacionSustituirAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            Guid ingredienteSustitucionId, 
            string nombreIngredienteSustitucion, 
            decimal cantidad = 1, 
            decimal precioAdicional = 0, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(itemId != Guid.Empty, "El ID del ítem no puede estar vacío", "ItemId");
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngrediente), "El nombre del ingrediente no puede estar vacío", "NombreIngrediente");
            _notificationManager.Require(ingredienteSustitucionId != Guid.Empty, "El ID del ingrediente de sustitución no puede estar vacío", "IngredienteSustitucionId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngredienteSustitucion), "El nombre del ingrediente de sustitución no puede estar vacío", "NombreIngredienteSustitucion");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor a cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Agregar personalización de tipo "sustituir"
                var personalizado = comanda.AgregarPersonalizacionSustituir(
                    itemId, 
                    ingredienteId, 
                    nombreIngrediente, 
                    ingredienteSustitucionId, 
                    nombreIngredienteSustitucion, 
                    cantidad, 
                    precioAdicional);
                
                if (!personalizado)
                {
                    _notificationManager.AddError($"No se pudo agregar la personalización al ítem con ID {itemId}", "ItemId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar personalización: {ex.Message}", "AgregarPersonalizacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ActualizarEstadoComandaAsync(
            Guid comandaId, 
            EstadoComanda nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(Enum.IsDefined(typeof(EstadoComanda), nuevoEstado), $"El estado '{nuevoEstado}' no es válido", "NuevoEstado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Actualizar estado según el tipo
                try
                {
                    switch (nuevoEstado)
                    {
                        case EstadoComanda.EnProceso:
                            comanda.MarcarEnPreparacion();
                            break;
                        case EstadoComanda.Lista:
                            comanda.MarcarLista();
                            break;
                        case EstadoComanda.Entregada:
                            comanda.MarcarEntregada();
                            break;
                        case EstadoComanda.Finalizada:
                            comanda.MarcarPagada();
                            break;
                        case EstadoComanda.Cancelada:
                            comanda.Cancelar("Cancelada desde servicio de operaciones");
                            break;
                        default:
                            _notificationManager.AddError($"No se puede actualizar al estado {nuevoEstado}", "NuevoEstado");
                            return _notificationManager.ToResult<bool>(false);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "ActualizarEstado");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar estado de comanda: {ex.Message}", "ActualizarEstadoComanda");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AplicarDescuentoComandaAsync(
            Guid comandaId, 
            decimal montoDescuento, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(montoDescuento > 0, "El monto del descuento debe ser mayor a cero", "MontoDescuento");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(motivo), "El motivo del descuento no puede estar vacío", "Motivo");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                try
                {
                    // Aplicar descuento
                    comanda.AplicarDescuento(montoDescuento, motivo);
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "AplicarDescuento");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al aplicar descuento a comanda: {ex.Message}", "AplicarDescuentoComanda");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <summary>
        /// 🍳 Crea una nueva comanda con productos y flujo híbrido de preparaciones
        /// </summary>
        public async Task<Result<Comanda>> CrearComandaConProductosAsync(
            Guid? clienteId,
            Guid? mesaId,
            Guid meseroId,
            IEnumerable<(Guid ProductoId, int Cantidad, string Observaciones)> productos,
            string observacionesComanda = "",
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros básicos
            _notificationManager.Require(meseroId != Guid.Empty, "El ID del mesero no puede estar vacío", "MeseroId");
            _notificationManager.Require(productos?.Any() == true, "Debe incluir al menos un producto", "Productos");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // 🏗️ Paso 1: Crear comanda base
                var resultadoComandaBase = await CrearNuevaComandaAsync(
                    clienteId, mesaId, meseroId, observacionesComanda, cancellationToken);
                
                if (!resultadoComandaBase.Succeeded)
                {
                    return resultadoComandaBase;
                }
                
                var comanda = resultadoComandaBase.Value!;
                
                // 🍳 Paso 2: Agregar productos con flujo híbrido de preparaciones
                var productosAgregados = 0;
                var productosDePreparaciones = 0;
                var productosAlMomento = 0;
                
                foreach (var (productoId, cantidad, observaciones) in productos)
                {
                    // Validar producto
                    _notificationManager.Require(productoId != Guid.Empty, $"ID de producto no válido: {productoId}", "ProductoId");
                    _notificationManager.Require(cantidad > 0, $"Cantidad debe ser mayor a 0: {cantidad}", "Cantidad");
                    
                    if (_notificationManager.HasErrors)
                    {
                        continue; // Saltar este producto y continuar
                    }
                    
                    // Obtener producto
                    var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                    if (producto == null)
                    {
                        _notificationManager.AddError($"Producto no encontrado: {productoId}", "ProductoId");
                        continue;
                    }
                    
                    // Agregar con flujo híbrido
                    var resultadoAgregar = await AgregarProductoConFlujoPrepararcionesAsync(
                        comanda, producto, cantidad, observaciones);
                    
                    if (resultadoAgregar.Succeeded)
                    {
                        productosAgregados++;
                        
                        // Contar estadísticas del flujo
                        if (observaciones.Contains("🍳 Preparación diaria"))
                            productosDePreparaciones++;
                        else
                            productosAlMomento++;
                    }
                }
                
                // ✅ Paso 3: Validar que se agregó al menos un producto
                if (productosAgregados == 0)
                {
                    _notificationManager.AddError("No se pudo agregar ningún producto a la comanda", "Productos");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // 📊 Paso 4: Log estadísticas del flujo híbrido
                var porcentajePreparaciones = (productosDePreparaciones * 100.0) / productosAgregados;
                _notificationManager.AddInformation(
                    $"📊 Comanda creada con {productosAgregados} productos: " +
                    $"{productosDePreparaciones} de preparaciones ({porcentajePreparaciones:F1}%), " +
                    $"{productosAlMomento} al momento", 
                    "EstadisticasComanda");
                
                // Persistir cambios finales
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error creando comanda con productos: {ex.Message}", "CrearComandaConProductos");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        #endregion
        
        #region Reservaciones

        /// <inheritdoc />
        public async Task<Result<Reservacion>> CrearReservacionAsync(
            Guid clienteId, 
            DateTime fecha, 
            int cantidadPersonas, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Buscar mesa disponible primero
                var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                    fecha.Date, 
                    fecha.TimeOfDay, 
                    cantidadPersonas, 
                    90, // Duración predeterminada en minutos
                    cancellationToken);
                
                if (!mesasDisponibles.Any())
                {
                    _notificationManager.AddError("No hay mesas disponibles para la fecha y cantidad de personas especificadas", "Disponibilidad");
                    return _notificationManager.ToResult<Reservacion>(null);
                }
                
                // Seleccionar la primera mesa disponible
                var mesaId = mesasDisponibles.First();
                
                // Usar ReservacionBuilder para crear la reservación con validaciones robustas
                var builder = new ReservacionBuilder(_notificationManager, _reservacionBuilderLogger);
                
                var resultadoReservacion = builder
                    .ParaCliente(clienteId)
                    .ParaMesa(mesaId)
                    .ParaFecha(fecha.Date)
                    .AHora(fecha.TimeOfDay)
                    .ConDuracion(TimeSpan.FromMinutes(90)) // Duración predeterminada
                    .ParaPersonas(cantidadPersonas)
                    .ConTelefono("000-000-0000") // Teléfono temporal (deberá actualizarse)
                    .ConEmail("temp@restaurant.com") // Email temporal (deberá actualizarse)
                    .ConObservaciones(observaciones)
                    .Construir();
                
                if (!resultadoReservacion.Succeeded)
                {
                    return resultadoReservacion; // Ya tiene los errores del builder
                }
                
                var reservacion = resultadoReservacion.Value!;
                
                // Persistir la reservación
                await _reservacionRepository.AgregarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(reservacion);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear reservación: {ex.Message}", "CrearReservacion");
                return _notificationManager.ToResult<Reservacion>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Reservacion>> ObtenerReservacionAsync(Guid reservacionId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Reservacion>(null);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<Reservacion>(null);
                }
                
                return Result.Success(reservacion);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener la reservación: {ex.Message}", "ObtenerReservacion");
                return _notificationManager.ToResult<Reservacion>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AsignarMesaAReservacionAsync(
            Guid reservacionId, 
            Guid mesaId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            _notificationManager.Require(mesaId != Guid.Empty, "El ID de la mesa no puede estar vacío", "MesaId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar que la mesa exista
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _notificationManager.AddError($"No se encontró la mesa con ID {mesaId}", "MesaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar disponibilidad
                var disponible = await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                    mesaId,
                    reservacion.FechaReservacion,
                    reservacion.FechaReservacion.TimeOfDay,
                    (int)reservacion.DuracionEstimada.TotalMinutes,
                    cancellationToken);
                
                if (!disponible)
                {
                    _notificationManager.AddError("La mesa seleccionada no está disponible en el horario de la reservación", "Disponibilidad");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Asignar mesa
                reservacion.CambiarMesa(mesaId);
                
                // Persistir cambios
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al asignar mesa a reservación: {ex.Message}", "AsignarMesa");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ActualizarEstadoReservacionAsync(
            Guid reservacionId, 
            EstadoReservacion nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            _notificationManager.Require(Enum.IsDefined(typeof(EstadoReservacion), nuevoEstado), $"El estado '{nuevoEstado}' no es válido", "NuevoEstado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                try
                {
                    // Actualizar estado según el tipo
                    switch (nuevoEstado)
                    {
                        case EstadoReservacion.Confirmada:
                            reservacion.Confirmar();
                            break;
                        case EstadoReservacion.Completada:
                            reservacion.Completar();
                            break;
                        case EstadoReservacion.Cancelada:
                            reservacion.Cancelar("Cancelada desde servicio de operaciones");
                            break;
                        case EstadoReservacion.NoShow:
                            reservacion.MarcarNoAsistio();
                            break;
                        default:
                            _notificationManager.AddError($"No se puede actualizar al estado {nuevoEstado}", "NuevoEstado");
                            return _notificationManager.ToResult<bool>(false);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "ActualizarEstado");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar estado de reservación: {ex.Message}", "ActualizarEstadoReservacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<Guid>>> VerificarDisponibilidadMesasAsync(
            DateTime fecha, 
            int cantidadPersonas, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(fecha > DateTime.Now, "La fecha debe ser posterior a la fecha actual", "Fecha");
            _notificationManager.Require(cantidadPersonas > 0, "La cantidad de personas debe ser mayor a cero", "CantidadPersonas");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<IEnumerable<Guid>>(Array.Empty<Guid>());
            }
            
            try
            {
                // Obtener mesas disponibles
                var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                    fecha.Date,
                    fecha.TimeOfDay,
                    cantidadPersonas,
                    90, // Duración predeterminada en minutos
                    cancellationToken);
                
                return Result.Success(mesasDisponibles);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al verificar disponibilidad de mesas: {ex.Message}", "VerificarDisponibilidad");
                return _notificationManager.ToResult<IEnumerable<Guid>>(Array.Empty<Guid>());
            }
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<Reservacion>>> ObtenerReservacionesPorRangoFechasAsync(
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(fechaInicio <= fechaFin, "La fecha de inicio debe ser anterior o igual a la fecha fin", "FechaInicio");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<IEnumerable<Reservacion>>(Array.Empty<Reservacion>());
            }
            
            try
            {
                var reservaciones = await _reservacionRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, cancellationToken);
                return Result.Success(reservaciones);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener reservaciones: {ex.Message}", "ObtenerReservaciones");
                return _notificationManager.ToResult<IEnumerable<Reservacion>>(Array.Empty<Reservacion>());
            }
        }

        /// <inheritdoc />
        public async Task<Result<Comanda>> ConvertirReservacionAComandaAsync(
            Guid reservacionId, 
            Guid meseroId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            _notificationManager.Require(meseroId != Guid.Empty, "El ID del mesero no puede estar vacío", "MeseroId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Verificar que la reservación esté confirmada
                if (reservacion.Estado != EstadoReservacion.Confirmada)
                {
                    _notificationManager.AddError("Solo se pueden convertir a comanda las reservaciones confirmadas", "Estado");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Usar ComandaBuilder para crear la comanda con validaciones robustas
                var builder = new ComandaBuilder(_notificationManager, _comandaBuilderLogger);
                
                var observacionesComanda = $"Comanda generada desde reservación #{reservacionId}";
                
                var resultadoComanda = builder
                    .ConMesero(meseroId)
                    .ConCliente(reservacion.ClienteId)
                    .EnMesa(reservacion.MesaId)
                    .ConObservaciones(observacionesComanda)
                    .Construir();
                
                if (!resultadoComanda.Succeeded)
                {
                    return resultadoComanda; // Ya tiene los errores del builder
                }
                
                var comanda = resultadoComanda.Value!;
                
                // Marcar la reservación como completada
                reservacion.Completar();
                
                // Persistir cambios
                await _comandaRepository.AgregarAsync(comanda);
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al convertir reservación a comanda: {ex.Message}", "ConvertirReservacion");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        #endregion
        
        #region Mesas
        
        /// <inheritdoc />
        public async Task<Result<Mesa>> RegistrarMesaAsync(
            int numero, 
            int capacidad, 
            string ubicacion, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Verificar que no exista una mesa con el mismo número
                var mesaExistente = await _mesaRepository.ObtenerPorNumeroAsync(numero);
                if (mesaExistente != null)
                {
                    _notificationManager.AddError($"Ya existe una mesa con el número {numero}", nameof(numero));
                    return _notificationManager.ToResult<Mesa>(null);
                }
                
                // Usar MesaBuilder para crear la mesa con validaciones robustas
                var builder = new MesaBuilder(_notificationManager, _mesaBuilderLogger);
                
                var resultadoMesa = builder
                    .ConNumero(numero)
                    .ConCapacidad(capacidad)
                    .EnUbicacion(ubicacion)
                    .Construir();
                
                if (!resultadoMesa.Succeeded)
                {
                    return resultadoMesa; // Ya tiene los errores del builder
                }
                
                var mesa = resultadoMesa.Value!;
                
                // Persistir la mesa
                await _mesaRepository.AgregarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();
                
                return Result.Success(mesa);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al registrar mesa: {ex.Message}", "RegistrarMesa");
                return _notificationManager.ToResult<Mesa>(null);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<Mesa>> ActualizarMesaAsync(
            Guid mesaId, 
            int capacidad, 
            string ubicacion, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // NOTA: Este método no está implementado porque la entidad Mesa no permite
            // actualizar capacidad ni ubicación por reglas de negocio.
            // Una mesa debe mantener su configuración original.
            _notificationManager.AddError("No se permite actualizar la capacidad o ubicación de una mesa existente", "ActualizarMesa");
            return _notificationManager.ToResult<Mesa>(null);
        }
        
        /// <inheritdoc />
        public async Task<Result<bool>> CambiarEstadoMesaAsync(
            Guid mesaId, 
            EstadoMesa nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(mesaId != Guid.Empty, "El ID de la mesa es requerido", nameof(mesaId));
            _notificationManager.CurrentNotification.Require(Enum.IsDefined(typeof(EstadoMesa), nuevoEstado), $"El estado '{nuevoEstado}' no es válido", nameof(nuevoEstado));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la mesa
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _notificationManager.AddError($"No se encontró la mesa con ID {mesaId}", nameof(mesaId));
                    return _notificationManager.ToResult<bool>(false);
                }

                // Validar que la mesa no tenga comandas activas antes de cambiar estado
                var comandasActivas = await _comandaRepository.ObtenerComandasPorMesaAsync(mesaId, cancellationToken);
                var comandasNoFinalizadas = comandasActivas.Where(c => 
                    c.Estado != EstadoComanda.Finalizada && 
                    c.Estado != EstadoComanda.Cancelada && 
                    c.Estado != EstadoComanda.Dividida);

                if (comandasNoFinalizadas.Any())
                {
                    var estadosComandas = string.Join(", ", comandasNoFinalizadas.Select(c => c.Estado.ToString()));
                    _notificationManager.AddError(
                        $"No se puede cambiar el estado de la mesa {mesa.Numero} porque tiene comandas activas en estados: {estadosComandas}. " +
                        "Debe finalizar o cancelar las comandas primero.", 
                        "ComandasActivas");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Cambiar estado usando los métodos correctos de la entidad
                try
                {
                    switch (nuevoEstado)
                    {
                        case EstadoMesa.Disponible:
                            mesa.MarcarComoDisponible();
                            break;
                        case EstadoMesa.Ocupada:
                            mesa.MarcarComoOcupada();
                            break;
                        case EstadoMesa.FueraDeServicio:
                            mesa.MarcarComoFueraDeServicio("Cambiado desde servicio de operaciones");
                            break;
                        case EstadoMesa.Reservada:
                            mesa.MarcarComoReservada();
                            break;
                        default:
                            _notificationManager.AddError($"No se puede cambiar al estado {nuevoEstado}", nameof(nuevoEstado));
                            return _notificationManager.ToResult<bool>(false);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "CambiarEstado");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _mesaRepository.ActualizarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al cambiar estado de mesa: {ex.Message}", "CambiarEstadoMesa");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<bool>> PonerMesaFueraDeServicioAsync(
            Guid mesaId, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(mesaId != Guid.Empty, "El ID de la mesa es requerido", nameof(mesaId));
            _notificationManager.CurrentNotification.Require(!string.IsNullOrWhiteSpace(motivo), "El motivo es requerido", nameof(motivo));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la mesa
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _notificationManager.AddError($"No se encontró la mesa con ID {mesaId}", nameof(mesaId));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Poner fuera de servicio con motivo específico
                try
                {
                    mesa.MarcarComoFueraDeServicio(motivo.Trim());
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "PonerFueraDeServicio");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _mesaRepository.ActualizarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al poner mesa fuera de servicio: {ex.Message}", "PonerMesaFueraDeServicio");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<bool>> LiberarMesaAsync(
            Guid mesaId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(mesaId != Guid.Empty, "El ID de la mesa es requerido", nameof(mesaId));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la mesa
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _notificationManager.AddError($"No se encontró la mesa con ID {mesaId}", nameof(mesaId));
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Liberar mesa (marcar como disponible)
                try
                {
                    mesa.MarcarComoDisponible();
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "LiberarMesa");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _mesaRepository.ActualizarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al liberar mesa: {ex.Message}", "LiberarMesa");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<IEnumerable<Mesa>>> ObtenerMesasDisponiblesAsync(
            int capacidadMinima = 1, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.CurrentNotification.Require(capacidadMinima > 0, "La capacidad mínima debe ser mayor a cero", nameof(capacidadMinima));
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<IEnumerable<Mesa>>(Array.Empty<Mesa>());
            }
            
            try
            {
                // Obtener todas las mesas disponibles
                var mesasDisponibles = await _mesaRepository.ObtenerMesasDisponiblesAsync();
                
                // Filtrar por capacidad mínima
                var mesasFiltradas = mesasDisponibles.Where(m => m.Capacidad >= capacidadMinima);
                
                return Result.Success(mesasFiltradas);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener mesas disponibles: {ex.Message}", "ObtenerMesasDisponibles");
                return _notificationManager.ToResult<IEnumerable<Mesa>>(Array.Empty<Mesa>());
            }
        }
        
        #endregion

        #region Preparaciones Diarias

        /// <summary>
        /// Prepara un producto con una cantidad específica para el día
        /// </summary>
        public async Task<Result<PreparacionDiaria>> PrepararProductoAsync(
            Guid productoId,
            int cantidad,
            Guid chefId,
            DateTime fechaVencimiento,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();

            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor que cero", "Cantidad");
            _notificationManager.Require(chefId != Guid.Empty, "El ID del chef no puede estar vacío", "ChefId");

            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<PreparacionDiaria>(null);
            }

            try
            {
                // Verificar que el producto existe
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                if (producto == null)
                {
                    _notificationManager.AddError($"No se encontró el producto con ID {productoId}", "ProductoId");
                    return _notificationManager.ToResult<PreparacionDiaria>(null);
                }

                // Preparar el producto usando el servicio de preparaciones
                var resultado = await _servicioPreparaciones.PrepararProductoAsync(
                    productoId, 
                    cantidad, 
                    chefId, 
                    fechaVencimiento, 
                    observaciones);

                if (!resultado.Succeeded)
                {
                    _notificationManager.AddError($"Error al preparar producto: {string.Join(", ", resultado.Errors)}", "PrepararProducto");
                    return _notificationManager.ToResult<PreparacionDiaria>(null);
                }

                return Result.Success(resultado.Value!);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al preparar producto: {ex.Message}", "PrepararProducto");
                return _notificationManager.ToResult<PreparacionDiaria>(null);
            }
        }

        /// <summary>
        /// Obtiene las preparaciones del día actual
        /// </summary>
        public async Task<Result<IEnumerable<PreparacionDiaria>>> ObtenerPreparacionesDelDiaAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var resultado = await _servicioPreparaciones.ObtenerPreparacionesDelDiaAsync();

                if (!resultado.Succeeded)
                {
                    _notificationManager.AddError($"Error al obtener preparaciones: {string.Join(", ", resultado.Errors)}", "ObtenerPreparaciones");
                    return _notificationManager.ToResult<IEnumerable<PreparacionDiaria>>(Array.Empty<PreparacionDiaria>());
                }

                return Result.Success(resultado.Value!.AsEnumerable());
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener preparaciones del día: {ex.Message}", "ObtenerPreparaciones");
                return _notificationManager.ToResult<IEnumerable<PreparacionDiaria>>(Array.Empty<PreparacionDiaria>());
            }
        }

        /// <summary>
        /// Obtiene las preparaciones de un producto específico
        /// </summary>
        public async Task<Result<IEnumerable<PreparacionDiaria>>> ObtenerPreparacionesPorProductoAsync(
            Guid productoId,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();

            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");

            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<IEnumerable<PreparacionDiaria>>(Array.Empty<PreparacionDiaria>());
            }

            try
            {
                var resultado = await _servicioPreparaciones.ObtenerPreparacionesPorProductoAsync(productoId);

                if (!resultado.Succeeded)
                {
                    _notificationManager.AddError($"Error al obtener preparaciones del producto: {string.Join(", ", resultado.Errors)}", "ObtenerPreparacionesProducto");
                    return _notificationManager.ToResult<IEnumerable<PreparacionDiaria>>(Array.Empty<PreparacionDiaria>());
                }

                return Result.Success(resultado.Value!.AsEnumerable());
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener preparaciones por producto: {ex.Message}", "ObtenerPreparacionesProducto");
                return _notificationManager.ToResult<IEnumerable<PreparacionDiaria>>(Array.Empty<PreparacionDiaria>());
            }
        }

        /// <summary>
        /// Verifica si hay suficiente cantidad preparada de un producto
        /// </summary>
        public async Task<Result<bool>> VerificarDisponibilidadPreparacionAsync(
            Guid productoId,
            int cantidadRequerida,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();

            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            _notificationManager.Require(cantidadRequerida > 0, "La cantidad requerida debe ser mayor que cero", "CantidadRequerida");

            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }

            try
            {
                var resultado = await _servicioPreparaciones.VerificarDisponibilidadAsync(productoId, cantidadRequerida);

                if (!resultado.Succeeded)
                {
                    _notificationManager.AddError($"Error al verificar disponibilidad: {string.Join(", ", resultado.Errors)}", "VerificarDisponibilidad");
                    return _notificationManager.ToResult<bool>(false);
                }

                return Result.Success(resultado.Value);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al verificar disponibilidad de preparación: {ex.Message}", "VerificarDisponibilidad");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        #endregion
    }
} 