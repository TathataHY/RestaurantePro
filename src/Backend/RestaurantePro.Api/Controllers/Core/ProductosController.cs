using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
using RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerEstadisticasProductos;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para gestionar productos del menú
/// </summary>
[ApiController]
[Route("api/core/productos")]
[Produces("application/json")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductosController> _logger;
    private readonly IProductoRepository _productoRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ICacheService _cache;

    public ProductosController(
        IMediator mediator, 
        ILogger<ProductosController> logger,
        IProductoRepository productoRepository,
        IWebHostEnvironment environment,
        ICacheService cache)
    {
        _mediator = mediator;
        _logger = logger;
        _productoRepository = productoRepository;
        _environment = environment;
        _cache = cache;
    }

    /// <summary>
    /// Obtiene todos los productos con paginación
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductoDto>>>> GetProductos(
        [FromQuery] int PageNumber = 1,
        [FromQuery] int PageSize = 10,
        [FromQuery] int? tamanoPagina = null, // Compatibilidad con tests
        [FromQuery] int? pagina = null, // Compatibilidad con tests (número de página)
        [FromQuery] string? filtro = null,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] decimal? precioMinimo = null,
        [FromQuery] decimal? precioMaximo = null,
        [FromQuery] DateTime? fechaCreacionDesde = null,
        [FromQuery] DateTime? fechaCreacionHasta = null,
        [FromQuery] int? popularidadMinima = null,
        [FromQuery] int? popularidadMaxima = null,
        [FromQuery] string orderBy = "Nombre",
        [FromQuery] string orderDirection = "asc")
    {
        // Usar tamanoPagina si está presente, sino usar PageSize
        var pageSize = tamanoPagina ?? PageSize;
        // Usar pagina si está presente, sino usar PageNumber
        var pageNumber = pagina ?? PageNumber;
        
        _logger.LogInformation("🔍 Parámetros recibidos - PageNumber: {PageNumber}, pagina: {pagina}, PageSize: {PageSize}, tamanoPagina: {tamanoPagina}", 
            PageNumber, pagina, PageSize, tamanoPagina);
        _logger.LogInformation("📋 GET /api/core/productos - Filtro: '{Filtro}', CategoriaId: {CategoriaId}, SoloActivos: {SoloActivos}, PageNumber: {PageNumber}, PageSize: {PageSize}", 
            filtro, categoriaId, soloActivos, pageNumber, pageSize);
        
        // Validar que los parámetros de query string sean válidos
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();
                
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                errors, 
                "Parámetros de consulta inválidos", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        
        // Validar parámetros de paginación
        if (pageNumber < 1)
        {
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                new List<string> { "El número de página debe ser mayor a 0" }, 
                "Parámetros de paginación inválidos", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                new List<string> { "El tamaño de página debe estar entre 1 y 100" }, 
                "Parámetros de paginación inválidos", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        // Validar longitud del filtro
        if (!string.IsNullOrEmpty(filtro) && filtro.Length > 100)
        {
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                new List<string> { "El filtro no puede exceder 100 caracteres" }, 
                "Filtro de búsqueda inválido", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Filtro = filtro,
            CategoriaId = categoriaId,
            SoloActivos = soloActivos,
            PrecioMinimo = precioMinimo,
            PrecioMaximo = precioMaximo,
            FechaCreacionDesde = fechaCreacionDesde,
            FechaCreacionHasta = fechaCreacionHasta,
            PopularidadMinima = popularidadMinima,
            PopularidadMaxima = popularidadMaxima,
            OrderBy = orderBy,
            OrderDirection = orderDirection
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                result.Errors, "Error al obtener productos", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(
            result.Value, "Productos obtenidos exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene un producto específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> GetProducto(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/core/productos/{Id}", id);
        
        var query = new ObtenerProductoPorIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Producto no encontrado", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ProductoDto>.SuccessResponse(
            result.Value, "Producto obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    [HttpGet("categoria/{categoriaId:guid}")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<List<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProductoDto>>>> GetProductosPorCategoria(
        Guid categoriaId, [FromQuery] bool soloActivos = true, [FromQuery] bool ordenarPorPopularidad = false)
    {
        _logger.LogInformation("🏷️ GET /api/core/productos/categoria/{CategoriaId}", categoriaId);
        
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId, soloActivos, ordenarPorPopularidad);
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            // Si la categoría no existe, devolver 404
            if (result.Errors.Any(e => e.Contains("categoría") && e.Contains("no encontrada")))
            {
                var notFoundResponse = ApiResponse<List<ProductoDto>>.ErrorResponse(
                    result.Errors, "Categoría no encontrada", StatusCodes.Status404NotFound);
                return NotFound(notFoundResponse);
            }
            
            var errorResponse = ApiResponse<List<ProductoDto>>.ErrorResponse(
                result.Errors, "Error al obtener productos por categoría", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<ProductoDto>>.SuccessResponse(
            result.Value, "Productos por categoría obtenidos exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> CrearProducto(
        [FromBody] CrearProductoCommand command)
    {
        _logger.LogInformation("➕ POST /api/core/productos");
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al crear producto", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<ProductoDto>.SuccessResponse(
            result.Value, "Producto creado exitosamente");
            
        return CreatedAtAction(
            nameof(GetProducto),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> ActualizarProducto(
        Guid id, [FromBody] ActualizarProductoCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/core/productos/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrado")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al actualizar producto", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ProductoDto>.SuccessResponse(
            result.Value, "Producto actualizado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarProducto(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/productos/{Id}", id);

        var command = new EliminarProductoCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al eliminar producto", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Producto eliminado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cambia el estado activo/inactivo de un producto
    /// </summary>
    [HttpPost("{id:guid}/cambiar-estado")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> CambiarEstadoProducto(Guid id, [FromQuery] bool activo)
    {
        _logger.LogInformation("🔄 POST /api/core/productos/{Id}/cambiar-estado - Activo: {Activo}", id, activo);

        try
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Producto no encontrado", "No se encontró el producto especificado", StatusCodes.Status404NotFound);
                return NotFound(errorResponse);
            }

            // Cambiar el estado del producto
            if (activo)
            {
                producto.Activar();
                _logger.LogInformation("✅ Producto {Id} activado", id);
            }
            else
            {
                producto.Desactivar();
                _logger.LogInformation("❌ Producto {Id} desactivado", id);
            }

            await _productoRepository.ActualizarAsync(producto);

            // Limpiar cache relacionado
            await _cache.RemoveAsync($"productos_paginados_*");
            await _cache.RemoveAsync($"producto_{id}");
            await _cache.RemoveAsync("estadisticas_productos");

            var response = ApiResponse<bool>.SuccessResponse(
                true, $"Producto {(activo ? "activado" : "desactivado")} exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al cambiar estado del producto {Id}", id);
            var errorResponse = ApiResponse<object>.ErrorResponse(
                "Error interno del servidor", "Error al cambiar el estado del producto", StatusCodes.Status500InternalServerError);
            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene estadísticas de productos
    /// </summary>
    [HttpGet("estadisticas")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<EstadisticasProductosDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EstadisticasProductosDto>>> GetEstadisticas()
    {
        _logger.LogInformation("📊 GET /api/core/productos/estadisticas");
        
        var query = new ObtenerEstadisticasProductosQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<EstadisticasProductosDto>.ErrorResponse(
                result.Errors, "Error al obtener estadísticas de productos", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<EstadisticasProductosDto>.SuccessResponse(
            result.Value, "Estadísticas obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Sube una imagen para un producto específico
    /// </summary>
    [HttpPost("{productoId}/imagen")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> SubirImagen(
        Guid productoId, 
        IFormFile archivo)
    {
        _logger.LogInformation("📸 POST /api/core/productos/{ProductoId}/imagen", productoId);

        if (archivo == null)
        {
            var errorResponse = ApiResponse<string>.ErrorResponse(
                new List<string> { "No se ha proporcionado ningún archivo" }, 
                "Archivo requerido", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        try
        {
            // 1. Validar que el producto existe
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
            if (producto == null)
            {
                var errorResponse = ApiResponse<string>.ErrorResponse(
                    new List<string> { $"Producto con ID {productoId} no encontrado" }, 
                    "Producto no encontrado", 
                    StatusCodes.Status404NotFound);
                return NotFound(errorResponse);
            }

            // 2. Validar archivo
            if (!ValidarArchivo(archivo, out var errorValidacion))
            {
                var errorResponse = ApiResponse<string>.ErrorResponse(
                    new List<string> { errorValidacion }, 
                    "Archivo inválido", 
                    StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            // 3. Crear directorio si no existe
            var carpetaImagenes = "uploads/productos";
            var directorioImagenes = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, carpetaImagenes);
            
            // Log para debuggear
            _logger.LogInformation($"🔍 WebRootPath: {_environment.WebRootPath}");
            _logger.LogInformation($"🔍 ContentRootPath: {_environment.ContentRootPath}");
            _logger.LogInformation($"🔍 DirectorioImagenes: {directorioImagenes}");
            
            // Si aún es null, usar directorio temporal
            if (string.IsNullOrEmpty(directorioImagenes))
            {
                directorioImagenes = Path.Combine(Path.GetTempPath(), "restaurantepro", "uploads", "productos");
                _logger.LogWarning($"⚠️ Usando directorio temporal: {directorioImagenes}");
            }
            
            if (!Directory.Exists(directorioImagenes))
            {
                Directory.CreateDirectory(directorioImagenes);
                _logger.LogInformation($"✅ Directorio creado: {directorioImagenes}");
            }

            // 4. Generar nombre único para el archivo
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"{producto.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var rutaCompleta = Path.Combine(directorioImagenes, nombreArchivo);

            // 5. Guardar archivo
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // 6. Generar URL relativa
            var urlImagen = $"/{carpetaImagenes}/{nombreArchivo}";

            // 7. Actualizar producto con nueva imagen
            producto.ActualizarImagen(urlImagen);
            await _productoRepository.ActualizarAsync(producto);

            // 8. Invalidar caché
            try
            {
                _cache.InvalidatePattern($"productos:lista:*");
                _cache.InvalidatePattern($"producto:{producto.Id}");
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "No se pudo invalidar caché tras subir imagen para producto {Id}", producto.Id);
            }

            _logger.LogInformation("✅ Imagen subida exitosamente: {UrlImagen}", urlImagen);
            var response = ApiResponse<string>.SuccessResponse(urlImagen, "Imagen subida exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al subir imagen para producto: {ProductoId}", productoId);
            var errorResponse = ApiResponse<string>.ErrorResponse(
                new List<string> { $"Error interno del servidor: {ex.Message}" }, 
                "Error al subir imagen", 
                StatusCodes.Status500InternalServerError);
            return StatusCode(500, errorResponse);
        }
    }

    private bool ValidarArchivo(IFormFile archivo, out string error)
    {
        error = string.Empty;

        if (archivo == null || archivo.Length == 0)
        {
            error = "No se ha seleccionado ningún archivo";
            return false;
        }

        const long tamañoMaximo = 5 * 1024 * 1024; // 5MB
        if (archivo.Length > tamañoMaximo)
        {
            error = $"El archivo es demasiado grande. Tamaño máximo permitido: {tamañoMaximo / (1024 * 1024)}MB";
            return false;
        }

        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!extensionesPermitidas.Contains(extension))
        {
            error = $"Tipo de archivo no permitido. Extensiones permitidas: {string.Join(", ", extensionesPermitidas)}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Busca productos por texto
    /// </summary>
    [HttpGet("buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ProductoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<ProductoDto>>>> BuscarProductos(
        [FromQuery] string? texto = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] int limite = 100)
    {
        _logger.LogInformation("🔍 GET /api/core/productos/buscar - Texto: '{Texto}', SoloActivos: {SoloActivos}, CategoriaId: {CategoriaId}, Limite: {Limite}", 
            texto, soloActivos, categoriaId, limite);

        try
        {
            var query = new ObtenerProductosPaginadosQuery
            {
                Filtro = texto,
                SoloActivos = soloActivos,
                CategoriaId = categoriaId,
                PageNumber = 1,
                PageSize = limite,
                OrderBy = "Nombre",
                OrderDirection = "asc"
            };

            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                _logger.LogWarning("❌ Error al buscar productos: {Error}", result.Error);
                var errorResponse = ApiResponse<List<ProductoDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" },
                    "Error al buscar productos",
                    StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            _logger.LogInformation("✅ Búsqueda exitosa: {Count} productos encontrados", result.Value?.Items?.Count ?? 0);
            
            // Convertir PaginatedList<ProductoDto> a List<ProductoDto> para la respuesta
            var productos = result.Value?.Items?.ToList() ?? new List<ProductoDto>();
            
            var response = ApiResponse<List<ProductoDto>>.SuccessResponse(
                productos, 
                $"Búsqueda completada: {productos.Count} productos encontrados");
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error interno al buscar productos");
            var errorResponse = ApiResponse<List<ProductoDto>>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
} 