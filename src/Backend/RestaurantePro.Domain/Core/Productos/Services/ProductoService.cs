namespace RestaurantePro.Domain.Core.Productos.Services;

/// <summary>
/// Implementación del servicio de dominio para operaciones con productos
/// </summary>
public class ProductoService : IProductoService
{
    private readonly IProductoRepository _productoRepository;
    private readonly IRecetaRepository _recetaRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(
        IProductoRepository productoRepository,
        IRecetaRepository recetaRepository,
        IIngredienteRepository ingredienteRepository,
        ILogger<ProductoService> logger)
    {
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        _recetaRepository = recetaRepository ?? throw new ArgumentNullException(nameof(recetaRepository));
        _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Producto encontrado o null</returns>
    public async Task<Producto?> ObtenerPorIdAsync(Guid productoId)
    {
        try
        {
            _logger.LogDebug("Obteniendo producto por ID: {ProductoId}", productoId);
            
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
            
            if (producto == null)
            {
                _logger.LogWarning("Producto no encontrado: {ProductoId}", productoId);
            }
            
            return producto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {ProductoId}", productoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la receta de un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Receta del producto o null</returns>
    public async Task<Receta?> ObtenerRecetaPorProductoIdAsync(Guid productoId)
    {
        try
        {
            _logger.LogDebug("Obteniendo receta para producto: {ProductoId}", productoId);
            
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId);
            
            if (receta == null)
            {
                _logger.LogInformation("No se encontró receta para el producto: {ProductoId}", productoId);
            }
            
            return receta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener receta del producto {ProductoId}", productoId);
            throw;
        }
    }

    /// <summary>
    /// Verifica si un producto está disponible
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>True si está disponible</returns>
    public async Task<bool> EstaDisponibleAsync(Guid productoId)
    {
        try
        {
            _logger.LogDebug("Verificando disponibilidad del producto: {ProductoId}", productoId);
            
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
            
            if (producto == null)
            {
                _logger.LogWarning("Producto no encontrado para verificar disponibilidad: {ProductoId}", productoId);
                return false;
            }

            // El producto debe estar activo
            if (!producto.EstaActivo)
            {
                _logger.LogDebug("Producto inactivo: {ProductoId}", productoId);
                return false;
            }

            // Para productos, verificamos si tiene ingredientes suficientes
            return await TieneIngredientesSuficientesAsync(productoId, 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductoId}", productoId);
            return false;
        }
    }

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <returns>Lista de productos</returns>
    public async Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId)
    {
        try
        {
            _logger.LogDebug("Obteniendo productos por categoría: {CategoriaId}", categoriaId);
            
            var productos = await _productoRepository.ObtenerPorCategoriaAsync(categoriaId);
            
            _logger.LogDebug("Se encontraron {Cantidad} productos en la categoría {CategoriaId}", 
                productos.Count(), categoriaId);
            
            return productos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos por categoría {CategoriaId}", categoriaId);
            throw;
        }
    }

    /// <summary>
    /// Verifica la disponibilidad de ingredientes para un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="cantidad">Cantidad solicitada</param>
    /// <returns>True si hay ingredientes suficientes</returns>
    public async Task<bool> TieneIngredientesSuficientesAsync(Guid productoId, int cantidad = 1)
    {
        try
        {
            _logger.LogDebug("Verificando ingredientes suficientes para producto: {ProductoId}, Cantidad: {Cantidad}", 
                productoId, cantidad);

            // Obtener la receta del producto
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId);
            
            if (receta == null)
            {
                _logger.LogInformation("Producto sin receta, se considera disponible: {ProductoId}", productoId);
                return true; // Si no hay receta, no hay restricciones de ingredientes
            }

            // Verificar cada ingrediente de la receta
            foreach (var ingredienteReceta in receta.Ingredientes)
            {
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteReceta.IngredienteId);
                
                if (ingrediente == null)
                {
                    _logger.LogWarning("Ingrediente no encontrado: {IngredienteId} para producto {ProductoId}", 
                        ingredienteReceta.IngredienteId, productoId);
                    return false;
                }

                var cantidadNecesaria = ingredienteReceta.Cantidad * cantidad;
                
                if (ingrediente.Stock < cantidadNecesaria)
                {
                    _logger.LogDebug("Stock insuficiente del ingrediente {IngredienteId}. Necesario: {Necesario}, Disponible: {Disponible}", 
                        ingredienteReceta.IngredienteId, cantidadNecesaria, ingrediente.Stock);
                    return false;
                }
            }

            _logger.LogDebug("Todos los ingredientes están disponibles para producto: {ProductoId}", productoId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar ingredientes del producto {ProductoId}", productoId);
            return false;
        }
    }
} 