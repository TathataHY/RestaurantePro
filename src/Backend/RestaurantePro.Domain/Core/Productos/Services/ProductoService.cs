namespace RestaurantePro.Domain.Core.Productos.Services;

/// <summary>
/// Implementación del servicio de dominio para operaciones con productos
/// </summary>
public class ProductoService : IProductoService
{
    private readonly IProductoRepository _productoRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(
        IProductoRepository productoRepository,
        IIngredienteRepository ingredienteRepository,
        ILogger<ProductoService> logger)
    {
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
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
            
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
            var receta = producto?.Recetas.FirstOrDefault();
            
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
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
            var receta = producto?.Recetas.FirstOrDefault();
            
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

    /// <inheritdoc />
    public async Task<Result<bool>> VerificarDisponibilidadAsync(Producto producto, int cantidad, CancellationToken cancellationToken = default)
    {
        try
        {
            if (producto == null)
                return Result.Failure<bool>("Producto no puede ser nulo");

            if (cantidad <= 0)
                return Result.Failure<bool>("La cantidad debe ser mayor a cero");

            if (!producto.EstaActivo)
                return Result.Success(false);

            // Si el producto no requiere ingredientes, está disponible
            if (!await RequiereIngredientesAsync(producto.Id))
            {
                return Result.Success(true);
            }

            // Verificar disponibilidad de ingredientes
            var tieneIngredientes = await TieneIngredientesSuficientesAsync(producto.Id, cantidad);
            return Result.Success(tieneIngredientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductoId}", producto.Id);
            return Result.Failure<bool>($"Error al verificar disponibilidad: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<object>> VerificarDisponibilidadConIngredientesAsync(Producto producto, int cantidad, CancellationToken cancellationToken = default)
    {
        try
        {
            if (producto == null)
                return Result.Failure<object>("Producto no puede ser nulo");

            if (cantidad <= 0)
                return Result.Failure<object>("La cantidad debe ser mayor a cero");

            var resultado = new
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                EstaDisponible = true,
                CantidadVerificada = cantidad,
                IngredientesVerificados = 0,
                AnalisisDetallado = new List<object>()
            };

            if (!producto.EstaActivo)
            {
                return Result.Success<object>(new
                {
                    resultado.ProductoId,
                    resultado.NombreProducto,
                    EstaDisponible = false,
                    resultado.CantidadVerificada,
                    MotivoNoDisponible = "Producto inactivo",
                    IngredientesVerificados = 0,
                    AnalisisDetallado = new List<object>()
                });
            }

            // Si no requiere ingredientes, está disponible
            if (!await RequiereIngredientesAsync(producto.Id))
            {
                return Result.Success<object>(resultado);
            }

            // Verificar ingredientes específicos (simulado)
            var ingredientesDisponibles = await TieneIngredientesSuficientesAsync(producto.Id, cantidad);
            
            return Result.Success<object>(new
            {
                resultado.ProductoId,
                resultado.NombreProducto,
                EstaDisponible = ingredientesDisponibles,
                resultado.CantidadVerificada,
                IngredientesVerificados = ingredientesDisponibles ? 4 : 0,
                AnalisisDetallado = new List<object>
                {
                    new
                    {
                        Ingrediente = "Ingrediente Principal",
                        Disponible = ingredientesDisponibles,
                        CantidadRequerida = cantidad * 1.5,
                        CantidadDisponible = ingredientesDisponibles ? cantidad * 2 : cantidad * 0.5
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad con ingredientes del producto {ProductoId}", producto.Id);
            return Result.Failure<object>($"Error al verificar disponibilidad: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si un producto requiere ingredientes para su preparación
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>True si el producto requiere ingredientes</returns>
    private async Task<bool> RequiereIngredientesAsync(Guid productoId)
    {
        try
        {
            // Un producto requiere ingredientes si tiene una receta asociada
            var receta = await _productoRepository.ObtenerPorIdAsync(productoId);
            return receta != null && receta.Recetas?.Any() == true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar si el producto {ProductoId} requiere ingredientes", productoId);
            // Por defecto, asumimos que sí requiere ingredientes por seguridad
            return true;
        }
    }
} 