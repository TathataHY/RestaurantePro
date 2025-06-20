namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para la gestión de productos
/// Contexto: Core
/// </summary>
[ApiController]
[Route("api/productos")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IMediator mediator, ILogger<ProductosController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los productos con paginación
    /// </summary>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="filtro">Filtro de búsqueda</param>
    /// <param name="categoriaId">ID de categoría (opcional)</param>
    /// <param name="soloActivos">Solo productos activos</param>
    /// <param name="orderBy">Campo de ordenamiento</param>
    /// <param name="orderDirection">Dirección de ordenamiento</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductoDto>>>> GetProductos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] string orderBy = "Nombre",
        [FromQuery] string orderDirection = "asc")
    {
        _logger.LogInformation("🛒 GET /api/productos - Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);

        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Filtro = filtro,
            CategoriaId = categoriaId,
            SoloActivos = soloActivos,
            OrderBy = orderBy,
            OrderDirection = orderDirection
        };

        var result = await _mediator.Send(query);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(result.Value, "Productos obtenidos exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Error al obtener productos",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> GetProducto(Guid id)
    {
        _logger.LogInformation("🛒 GET /api/productos/{Id}", id);

        var query = new ObtenerProductoPorIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<ProductoDto>.SuccessResponse(result.Value, "Producto obtenido exitosamente"));
        }

        return NotFound(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Producto no encontrado",
            StatusCodes.Status404NotFound));
    }

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="soloActivos">Solo productos activos</param>
    /// <param name="ordenarPorPopularidad">Ordenar por popularidad</param>
    [HttpGet("categoria/{categoriaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<ProductoDto>>>> GetProductosPorCategoria(
        Guid categoriaId,
        [FromQuery] bool soloActivos = true,
        [FromQuery] bool ordenarPorPopularidad = false)
    {
        _logger.LogInformation("🏷️ GET /api/productos/categoria/{CategoriaId}", categoriaId);

        var query = new ObtenerProductosPorCategoriaQuery(categoriaId, soloActivos, ordenarPorPopularidad);
        var result = await _mediator.Send(query);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<List<ProductoDto>>.SuccessResponse(result.Value, "Productos obtenidos exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Error al obtener productos por categoría",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="command">Datos del producto a crear</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> CrearProducto([FromBody] CrearProductoCommand command)
    {
        _logger.LogInformation("➕ POST /api/productos - Creando producto: {Nombre}", command.Nombre);

        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            var response = ApiResponse<ProductoDto>.SuccessResponse(result.Value, "Producto creado exitosamente");
            response.StatusCode = StatusCodes.Status201Created;
            
            return CreatedAtAction(
                nameof(GetProducto),
                new { id = result.Value.Id },
                response);
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Error al crear producto",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="command">Datos actualizados del producto</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ActualizarProducto(Guid id, [FromBody] ActualizarProductoCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/productos/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<object>.SuccessResponse(null, "Producto actualizado exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Error al actualizar producto",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> EliminarProducto(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/productos/{Id}", id);

        var command = new EliminarProductoCommand { Id = id };
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<object>.SuccessResponse(null, "Producto eliminado exitosamente"));
        }

        return NotFound(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Error al eliminar producto",
            StatusCodes.Status404NotFound));
    }
} 