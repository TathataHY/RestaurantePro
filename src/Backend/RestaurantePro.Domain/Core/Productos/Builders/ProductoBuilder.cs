namespace RestaurantePro.Domain.Core.Productos.Builders;

/// <summary>
/// Builder para construir instancias de Producto paso a paso con validaciones fluidas.
/// Permite crear productos complejos de manera segura y legible.
/// </summary>
public class ProductoBuilder
{
    private string? _nombre;
    private string? _descripcion;
    private decimal? _precio;
    private Guid? _categoriaId;
    private string? _categoriaNombre;
    private int? _popularidadInicial;
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<ProductoBuilder> _logger;

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public ProductoBuilder(INotificationManager notificationManager, ILogger<ProductoBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Establece el nombre del producto
    /// </summary>
    /// <param name="nombre">Nombre del producto</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProductoBuilder ConNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            _notificationManager.AddError("El nombre del producto no puede estar vacío", "Nombre");
            return this;
        }

        if (nombre.Length > 100)
        {
            _notificationManager.AddError("El nombre del producto no puede exceder 100 caracteres", "Nombre");
            return this;
        }

        _nombre = nombre.Trim();
        _logger.LogDebug("Nombre del producto establecido: {Nombre}", _nombre);
        return this;
    }

    /// <summary>
    /// Establece la descripción del producto
    /// </summary>
    /// <param name="descripcion">Descripción del producto</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProductoBuilder ConDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            _notificationManager.AddError("La descripción del producto no puede estar vacía", "Descripcion");
            return this;
        }

        if (descripcion.Length > 500)
        {
            _notificationManager.AddError("La descripción del producto no puede exceder 500 caracteres", "Descripcion");
            return this;
        }

        _descripcion = descripcion.Trim();
        _logger.LogDebug("Descripción del producto establecida");
        return this;
    }

    /// <summary>
    /// Establece el precio del producto
    /// </summary>
    /// <param name="precio">Precio del producto</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProductoBuilder ConPrecio(decimal precio)
    {
        if (precio <= 0)
        {
            _notificationManager.AddError("El precio del producto debe ser mayor que cero", "Precio");
            return this;
        }

        if (precio > 1000000m)
        {
            _notificationManager.AddError("El precio del producto no puede exceder $1,000,000", "Precio");
            return this;
        }

        _precio = precio;
        _logger.LogDebug("Precio del producto establecido: ${Precio:F2}", _precio);
        return this;
    }

    /// <summary>
    /// Establece la categoría del producto por ID
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="categoriaNombre">Nombre de la categoría (opcional)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProductoBuilder EnCategoria(Guid categoriaId, string? categoriaNombre = null)
    {
        if (categoriaId == Guid.Empty)
        {
            _notificationManager.AddError("El ID de categoría no puede estar vacío", "CategoriaId");
            return this;
        }

        _categoriaId = categoriaId;
        _categoriaNombre = categoriaNombre ?? "Sin categoría";
        _logger.LogDebug("Categoría del producto establecida: {CategoriaId} - {CategoriaNombre}", _categoriaId, _categoriaNombre);
        return this;
    }

    /// <summary>
    /// Establece la categoría del producto por nombre (busca automáticamente el ID)
    /// </summary>
    /// <param name="nombreCategoria">Nombre de la categoría</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProductoBuilder EnCategoria(string nombreCategoria)
    {
        if (string.IsNullOrWhiteSpace(nombreCategoria))
        {
            _notificationManager.AddError("El nombre de la categoría no puede estar vacío", "CategoriaNombre");
            return this;
        }

        // Para esta versión, vamos a generar un ID temporal y usar el nombre
        // En una implementación real, esto consultaría el repositorio de categorías
        _categoriaId = Guid.NewGuid(); // Temporal - en producción buscaría en repository
        _categoriaNombre = nombreCategoria.Trim();
        _logger.LogDebug("Categoría del producto establecida por nombre: {CategoriaNombre}", _categoriaNombre);
        
        // Agregar una notificación informativa
        _notificationManager.AddInformation($"Categoría '{nombreCategoria}' será creada si no existe", "CategoriaAutomatica");
        return this;
    }

    /// <summary>
    /// Establece la popularidad inicial del producto
    /// </summary>
    /// <param name="popularidad">Popularidad inicial (0-10)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProductoBuilder ConPopularidadInicial(int popularidad)
    {
        if (popularidad < 0 || popularidad > 10)
        {
            _notificationManager.AddError("La popularidad inicial debe estar entre 0 y 10", "PopularidadInicial");
            return this;
        }

        _popularidadInicial = popularidad;
        _logger.LogDebug("Popularidad inicial establecida: {Popularidad}", _popularidadInicial);
        return this;
    }

    /// <summary>
    /// Construye la instancia final del Producto
    /// </summary>
    /// <returns>Resultado con el producto creado o errores de validación</returns>
    public Result<Producto> Construir()
    {
        _logger.LogDebug("Iniciando construcción del producto");

        // Limpiar notificaciones previas del contexto de construcción
        _notificationManager.ClearErrors();

        try
        {
            // Validaciones finales obligatorias
            var hayErrores = false;

            if (string.IsNullOrWhiteSpace(_nombre))
            {
                _notificationManager.AddError("El nombre del producto es obligatorio", "Nombre");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(_descripcion))
            {
                _notificationManager.AddError("La descripción del producto es obligatoria", "Descripcion");
                hayErrores = true;
            }

            if (!_precio.HasValue)
            {
                _notificationManager.AddError("El precio del producto es obligatorio", "Precio");
                hayErrores = true;
            }

            if (!_categoriaId.HasValue)
            {
                _notificationManager.AddError("La categoría del producto es obligatoria", "CategoriaId");
                hayErrores = true;
            }

            // Si hay errores críticos, no continuar
            if (hayErrores)
            {
                _logger.LogWarning("Construcción del producto falló debido a errores de validación");
                return _notificationManager.ToResult<Producto>(null);
            }

            // Crear el value object PrecioProducto
            var precioProducto = new PrecioProducto(_precio!.Value);

            // Crear el producto usando el factory method de la entidad
            var producto = Producto.Crear(
                _nombre!,
                _descripcion!,
                precioProducto,
                _categoriaId!.Value,
                _categoriaNombre);

            // Establecer popularidad inicial si se especificó
            if (_popularidadInicial.HasValue && _popularidadInicial.Value > 0)
            {
                producto.ActualizarPopularidad(_popularidadInicial.Value);
            }

            _logger.LogInformation("Producto construido exitosamente: {ProductoId} - {Nombre}", producto.Id, producto.Nombre);
            return Result.Success(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la construcción del producto");
            _notificationManager.AddError($"Error interno: {ex.Message}", "ConstruccionProducto");
            return _notificationManager.ToResult<Producto>(null);
        }
    }

    /// <summary>
    /// Reinicia el builder para permitir reutilización
    /// </summary>
    /// <returns>Builder reiniciado</returns>
    public ProductoBuilder Reset()
    {
        _nombre = null;
        _descripcion = null;
        _precio = null;
        _categoriaId = null;
        _categoriaNombre = null;
        _popularidadInicial = null;

        // Limpiar notificaciones del contexto del builder
        _notificationManager.ClearErrors();

        _logger.LogDebug("ProductoBuilder reiniciado");
        return this;
    }

    /// <summary>
    /// Método de conveniencia para crear un builder configurado
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger</param>
    /// <returns>Nuevo builder configurado</returns>
    public static ProductoBuilder Nuevo(INotificationManager notificationManager, ILogger<ProductoBuilder> logger)
    {
        return new ProductoBuilder(notificationManager, logger);
    }
} 