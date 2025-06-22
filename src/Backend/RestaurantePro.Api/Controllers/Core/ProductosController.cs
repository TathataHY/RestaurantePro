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
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductoDto>>>> GetProductos(
        [FromQuery] ObtenerProductosPaginadosQuery query)
    {
        _logger.LogInformation("📋 GET /api/core/productos");
        
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
    [ProducesResponseType(typeof(ApiResponse<List<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProductoDto>>>> GetProductosPorCategoria(
        Guid categoriaId, [FromQuery] bool soloActivos = true, [FromQuery] bool ordenarPorPopularidad = false)
    {
        _logger.LogInformation("🏷️ GET /api/core/productos/categoria/{CategoriaId}", categoriaId);
        
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId, soloActivos, ordenarPorPopularidad);
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
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