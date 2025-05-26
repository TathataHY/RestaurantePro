/*
 * TODO: Plan de Adaptación a Patrón Result/Notification (Julio 2025)
 * 
 * Esta clase debe ser adaptada para mantener la consistencia con IRecetaService
 * que ya implementa los patrones Result y Notification.
 * 
 * Cambios requeridos:
 * 1. Actualizar los métodos para que mantengan el mismo comportamiento:
 *    - Preservar el retorno de Result<T> de _recetaServiceOriginal
 *    - Asegurar que la caché almacene y recupere correctamente los objetos Result<T>
 * 
 * 2. Garantizar que los métodos de invalidación de caché funcionen correctamente
 *    con el nuevo patrón:
 *    - Actualizar las claves de caché si es necesario
 * 
 * 3. Incluir manejo de errores específico de caché:
 *    - Añadir try/catch para errores de caché
 *    - Delegar al servicio original en caso de error en la caché
 * 
 * 4. Actualizar pruebas unitarias para verificar:
 *    - Recuperación correcta de Result desde caché
 *    - Almacenamiento correcto de Result en caché
 *    - Comportamiento ante fallos de caché
 */

namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Implementación del servicio de recetas con soporte de caché para mejorar el rendimiento
    /// </summary>
    public class RecetaServiceCached : IRecetaServiceCached
    {
        private readonly IRecetaService _recetaServiceOriginal;
        private readonly ICacheService _cacheService;
        private readonly INotificationManager _notificationManager;
        private const string CacheKeyPrefix = "RecetaService_";
        private const int CacheDurationMinutes = 60; // Caché de 1 hora para datos de recetas
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RecetaServiceCached(
            IRecetaService recetaService,
            ICacheService cacheService,
            INotificationManager notificationManager)
        {
            _recetaServiceOriginal = recetaService ?? throw new ArgumentNullException(nameof(recetaService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }
        
        /// <inheritdoc/>
        public async Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Dictionary<Guid, decimal>>(null);
            }
            
            var cacheKey = $"{CacheKeyPrefix}ObtenerIngredientesParaProducto_{productoId}";
            
            try
            {
                return await _cacheService.GetOrAddAsync(
                    cacheKey,
                    async (ct) => await _recetaServiceOriginal.ObtenerIngredientesParaProductoAsync(productoId, ct),
                    CacheDurationMinutes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener ingredientes desde caché: {ex.Message}", "Cache");
                
                try
                {
                    // Si hay un problema con la caché, intentamos obtener directamente
                    return await _recetaServiceOriginal.ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
                }
                catch (Exception innerEx)
                {
                    _notificationManager.AddError($"Error al obtener ingredientes: {innerEx.Message}", "RecetaService");
                    return _notificationManager.ToResult<Dictionary<Guid, decimal>>(null);
                }
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId")
                .Require(cantidad > 0, "La cantidad debe ser mayor que cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // No cachear este método porque la disponibilidad puede cambiar frecuentemente
                // y necesitamos datos en tiempo real
                return await _recetaServiceOriginal.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, cancellationToken);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al verificar disponibilidad: {ex.Message}", "RecetaService");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesFaltantesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId")
                .Require(cantidad > 0, "La cantidad debe ser mayor que cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Dictionary<Guid, decimal>>(null);
            }
            
            try
            {
                // No cachear este método porque los faltantes pueden cambiar frecuentemente
                // y necesitamos datos en tiempo real
                return await _recetaServiceOriginal.ObtenerIngredientesFaltantesAsync(productoId, cantidad, cancellationToken);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener ingredientes faltantes: {ex.Message}", "RecetaService");
                return _notificationManager.ToResult<Dictionary<Guid, decimal>>(null);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<decimal>(0);
            }
            
            var cacheKey = $"{CacheKeyPrefix}CalcularCostoReceta_{productoId}";
            
            try
            {
                return await _cacheService.GetOrAddAsync(
                    cacheKey,
                    async (ct) => await _recetaServiceOriginal.CalcularCostoRecetaAsync(productoId, ct),
                    CacheDurationMinutes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener costo de receta desde caché: {ex.Message}", "Cache");
                
                try
                {
                    // Si hay un problema con la caché, intentamos obtener directamente
                    return await _recetaServiceOriginal.CalcularCostoRecetaAsync(productoId, cancellationToken);
                }
                catch (Exception innerEx)
                {
                    _notificationManager.AddError($"Error al calcular costo de receta: {innerEx.Message}", "RecetaService");
                    return _notificationManager.ToResult<decimal>(0);
                }
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<ValueObjects.RentabilidadProducto>> CalcularRentabilidadProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<ValueObjects.RentabilidadProducto>(null);
            }
            
            var cacheKey = $"{CacheKeyPrefix}CalcularRentabilidadProducto_{productoId}";
            
            try
            {
                return await _cacheService.GetOrAddAsync(
                    cacheKey,
                    async (ct) => await _recetaServiceOriginal.CalcularRentabilidadProductoAsync(productoId, ct),
                    CacheDurationMinutes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener rentabilidad desde caché: {ex.Message}", "Cache");
                
                try
                {
                    // Si hay un problema con la caché, intentamos obtener directamente
                    return await _recetaServiceOriginal.CalcularRentabilidadProductoAsync(productoId, cancellationToken);
                }
                catch (Exception innerEx)
                {
                    _notificationManager.AddError($"Error al calcular rentabilidad: {innerEx.Message}", "RecetaService");
                    return _notificationManager.ToResult<ValueObjects.RentabilidadProducto>(null);
                }
            }
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
        
        /// <inheritdoc/>
        public void InvalidarCacheProducto(Guid productoId)
        {
            if (productoId == Guid.Empty)
            {
                return;
            }
            
            // Invalidar todas las entradas de caché relacionadas con este producto
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}ObtenerIngredientesParaProducto_{productoId}");
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}CalcularCostoReceta_{productoId}");
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}CalcularRentabilidadProducto_{productoId}");
        }
    }
} 