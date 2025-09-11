using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
using RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;
using RestaurantePro.Application.Core.Productos.DTOs;

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

    public ProductosController(IMediator mediator, ILogger<ProductosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos con paginación
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductoDto>>>> GetProductos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? filtro = null,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] string orderBy = "Nombre",
        [FromQuery] string orderDirection = "asc")
    {
        _logger.LogInformation("📋 GET /api/core/productos");
        
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
        if (pagina < 1)
        {
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                new List<string> { "El número de página debe ser mayor a 0" }, 
                "Parámetros de paginación inválidos", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        if (tamanoPagina < 1 || tamanoPagina > 100)
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
            PageNumber = pagina,
            PageSize = tamanoPagina,
            Filtro = filtro,
            CategoriaId = categoriaId,
            SoloActivos = soloActivos,
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
} 