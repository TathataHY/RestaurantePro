namespace RestaurantePro.Domain.Core.Services
{
    /// <summary>
    /// Servicio de integración entre los contextos Core y Operaciones.
    /// Implementa el patrón Anticorruption Layer para la comunicación entre el catálogo de productos y las comandas.
    /// </summary>
    public class CoreOperacionesIntegrationService : ICoreOperacionesIntegrationService
    {
        private readonly Productos.Interfaces.IProductoRepository _productoRepository;
        private readonly Productos.Services.IRecetaService _recetaService;
        private readonly SharedKernel.Validation.INotificationManager _notificationManager;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public CoreOperacionesIntegrationService(
            Productos.Interfaces.IProductoRepository productoRepository,
            Productos.Services.IRecetaService recetaService,
            SharedKernel.Validation.INotificationManager notificationManager,
            IDateTimeService dateTimeService)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _recetaService = recetaService ?? throw new ArgumentNullException(nameof(recetaService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <summary>
        /// Verifica la disponibilidad de un conjunto de productos para una comanda
        /// </summary>
        /// <param name="productosIdCantidad">Diccionario con IDs de productos y sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de disponibilidad</returns>
        public async Task<Result<DisponibilidadProductosResult>> VerificarDisponibilidadProductosAsync(
            Dictionary<Guid, int> productosIdCantidad,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                var resultado = new DisponibilidadProductosResult
                {
                    TodosDisponibles = true,
                    ProductosNoDisponibles = new Dictionary<Guid, string>(),
                    IngredientesFaltantes = new Dictionary<Guid, decimal>()
                };
                
                // Verificar cada producto
                foreach (var kvp in productosIdCantidad)
                {
                    var productoId = kvp.Key;
                    var cantidad = kvp.Value;
                    
                    // Obtener el producto
                    var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                    if (producto == null)
                    {
                        resultado.TodosDisponibles = false;
                        resultado.ProductosNoDisponibles[productoId] = "Producto no encontrado";
                        continue;
                    }
                    
                    // Verificar que el producto esté activo
                    if (!producto.EstaActivo)
                    {
                        resultado.TodosDisponibles = false;
                        resultado.ProductosNoDisponibles[productoId] = "Producto no disponible";
                        continue;
                    }
                    
                    // Verificar disponibilidad de ingredientes
                    var disponibilidadResult = await _recetaService.VerificarDisponibilidadIngredientesAsync(
                        productoId, cantidad, cancellationToken);
                        
                    if (!disponibilidadResult.Succeeded || !disponibilidadResult.Value)
                    {
                        resultado.TodosDisponibles = false;
                        resultado.ProductosNoDisponibles[productoId] = "Ingredientes insuficientes";
                        
                        // Obtener ingredientes faltantes
                        var ingredientesFaltantesResult = await _recetaService.ObtenerIngredientesFaltantesAsync(
                            productoId, cantidad, cancellationToken);
                            
                        if (ingredientesFaltantesResult.Succeeded && ingredientesFaltantesResult.Value.Any())
                        {
                            // Agregar ingredientes faltantes al resultado
                            foreach (var ingrediente in ingredientesFaltantesResult.Value)
                            {
                                if (resultado.IngredientesFaltantes.ContainsKey(ingrediente.Key))
                                {
                                    resultado.IngredientesFaltantes[ingrediente.Key] += ingrediente.Value;
                                }
                                else
                                {
                                    resultado.IngredientesFaltantes[ingrediente.Key] = ingrediente.Value;
                                }
                            }
                        }
                    }
                }
                
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al verificar disponibilidad de productos: {ex.Message}", "VerificarDisponibilidad");
                return _notificationManager.ToResult<DisponibilidadProductosResult>(null);
            }
        }
        
        /// <summary>
        /// Calcula el precio total de un conjunto de productos para una comanda
        /// </summary>
        /// <param name="productosIdCantidad">Diccionario con IDs de productos y sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de precios</returns>
        public async Task<Result<CalculoPreciosResult>> CalcularPreciosTotalesAsync(
            Dictionary<Guid, int> productosIdCantidad,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                var resultado = new CalculoPreciosResult
                {
                    PrecioTotal = 0,
                    PreciosUnitarios = new Dictionary<Guid, decimal>(),
                    PreciosPorProducto = new Dictionary<Guid, decimal>(),
                    ProductosNoEncontrados = new List<Guid>()
                };
                
                // Calcular precio total
                foreach (var kvp in productosIdCantidad)
                {
                    var productoId = kvp.Key;
                    var cantidad = kvp.Value;
                    
                    // Obtener el producto
                    var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                    if (producto == null)
                    {
                        resultado.ProductosNoEncontrados.Add(productoId);
                        continue;
                    }
                    
                    // Calcular precio unitario y subtotal
                    var precioUnitario = producto.Precio.Valor;
                    var subtotal = precioUnitario * cantidad;
                    
                    resultado.PreciosUnitarios[productoId] = precioUnitario;
                    resultado.PreciosPorProducto[productoId] = subtotal;
                    resultado.PrecioTotal += subtotal;
                }
                
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al calcular precios totales: {ex.Message}", "CalcularPrecios");
                return _notificationManager.ToResult<CalculoPreciosResult>(null);
            }
        }
        
        /// <summary>
        /// Actualiza la información de productos en stock basado en una comanda finalizada
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="productosIdCantidad">Diccionario con IDs de productos y sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del procesamiento</returns>
        public async Task<Result<bool>> ProcesarComandaFinalizadaAsync(
            Guid comandaId,
            Dictionary<Guid, int> productosIdCantidad,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Aquí solo realizamos acciones relacionadas con el contexto Core
                // La reducción de inventario se maneja en el contexto de Inventario
                
                // Podríamos registrar estadísticas de productos, actualizar popularidad, etc.
                
                // Por ahora, solo verificamos que todos los productos existan
                foreach (var kvp in productosIdCantidad)
                {
                    var productoId = kvp.Key;
                    var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                    
                    if (producto == null)
                    {
                        _notificationManager.AddError($"El producto con ID {productoId} no existe", "ProductoNoEncontrado");
                    }
                }
                
                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult<bool>(false);
                }
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al procesar comanda finalizada: {ex.Message}", "ProcesarComanda");
                return _notificationManager.ToResult<bool>(false);
            }
        }
    }
    
    /// <summary>
    /// Resultado de la verificación de disponibilidad de productos
    /// </summary>
    public class DisponibilidadProductosResult
    {
        /// <summary>
        /// Indica si todos los productos están disponibles
        /// </summary>
        public bool TodosDisponibles { get; set; }
        
        /// <summary>
        /// Diccionario con productos no disponibles y la razón
        /// </summary>
        public Dictionary<Guid, string> ProductosNoDisponibles { get; set; }
        
        /// <summary>
        /// Diccionario con ingredientes faltantes y la cantidad necesaria
        /// </summary>
        public Dictionary<Guid, decimal> IngredientesFaltantes { get; set; }
    }
    
    /// <summary>
    /// Resultado del cálculo de precios de productos
    /// </summary>
    public class CalculoPreciosResult
    {
        /// <summary>
        /// Precio total de todos los productos
        /// </summary>
        public decimal PrecioTotal { get; set; }
        
        /// <summary>
        /// Diccionario con precios unitarios por producto
        /// </summary>
        public Dictionary<Guid, decimal> PreciosUnitarios { get; set; }
        
        /// <summary>
        /// Diccionario con precios totales por producto (cantidad * precio unitario)
        /// </summary>
        public Dictionary<Guid, decimal> PreciosPorProducto { get; set; }
        
        /// <summary>
        /// Lista de productos no encontrados
        /// </summary>
        public List<Guid> ProductosNoEncontrados { get; set; }
    }
} 