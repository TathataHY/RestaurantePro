using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Application.Core.Productos.Commands.CrearCategoria;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarCategoria;
using RestaurantePro.Application.Core.Productos.Commands.EliminarCategoria;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.Productos.Services;
using RestaurantePro.Domain.Core.Productos.Policies;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para gestionar categorías de productos
/// </summary>
[ApiController]
[Route("api/core/categorias")]
[Produces("application/json")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IVisibilidadCategoriasPolicy _visibilidadPolicy;
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(
        IProductoCategoriaRepository categoriaRepository,
        IProductoRepository productoRepository,
        IVisibilidadCategoriasPolicy visibilidadPolicy,
        IMediator mediator,
        ILogger<CategoriasController> logger)
    {
        _categoriaRepository = categoriaRepository;
        _productoRepository = productoRepository;
        _visibilidadPolicy = visibilidadPolicy;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las categorías de productos
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CategoriaProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CategoriaProductoDto>>>> GetCategorias(
        [FromQuery] bool soloActivas = true,
        [FromQuery] bool ocultarVacias = true)
    {
        _logger.LogInformation("🏷️ GET /api/categorias - SoloActivas: {SoloActivas}, OcultarVacias: {OcultarVacias}", 
            soloActivas, ocultarVacias);

        try
        {
            List<Domain.Core.Productos.Entities.ProductoCategoria> categorias;

            if (ocultarVacias)
            {
                // Usar la política de visibilidad que filtra categorías vacías
                categorias = await _visibilidadPolicy.ObtenerCategoriasVisiblesAsync(ocultarVacias);
            }
            else
            {
                // Obtener todas las categorías según el filtro de activas
                categorias = soloActivas 
                    ? await _categoriaRepository.ObtenerActivasAsync()
                    : await _categoriaRepository.ObtenerTodasAsync();
            }

            // Mapear a DTOs con información adicional
            var categoriasDto = new List<CategoriaProductoDto>();
            
            foreach (var categoria in categorias)
            {
                var productosCategoria = await _productoRepository.ObtenerPorCategoriaAsync(
                    categoria.Id, soloActivas);
                
                var categoriaDto = new CategoriaProductoDto
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Color = "#2196F3", // Color por defecto
                    Icono = "🍽️", // Icono por defecto
                    Orden = categoria.Orden,
                    Activa = categoria.EstaActivo,
                    CantidadProductos = productosCategoria.Count,
                    ProductosDisponibles = productosCategoria.Count(p => p.EstaActivo),
                    FechaCreacion = categoria.FechaCreacion
                };
                
                categoriasDto.Add(categoriaDto);
            }

            // Ordenar por la propiedad Orden
            categoriasDto = categoriasDto.OrderBy(c => c.Orden).ToList();

            var response = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(
                categoriasDto, "Categorías obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener categorías");
            var errorResponse = ApiResponse<List<CategoriaProductoDto>>.ErrorResponse(
                new List<string> { "Error interno al obtener categorías" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene una categoría específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CategoriaProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoriaProductoDto>>> GetCategoria(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/categorias/{Id}", id);

        try
        {
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(id);
            
            if (categoria == null)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    new List<string> { "Categoría no encontrada" },
                    "Categoría no encontrada",
                    StatusCodes.Status404NotFound);
                return NotFound(errorResponse);
            }

            // Obtener productos de la categoría
            var productosCategoria = await _productoRepository.ObtenerPorCategoriaAsync(
                categoria.Id, true);

            var categoriaDto = new CategoriaProductoDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Color = categoria.Color,
                Icono = categoria.Icono,
                Orden = categoria.Orden,
                Activa = categoria.EstaActivo,
                CantidadProductos = productosCategoria.Count,
                ProductosDisponibles = productosCategoria.Count(p => p.EstaActivo),
                FechaCreacion = categoria.FechaCreacion
            };

            var response = ApiResponse<CategoriaProductoDto>.SuccessResponse(
                categoriaDto, "Categoría obtenida exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener categoría: {Id}", id);
            var errorResponse = ApiResponse<CategoriaProductoDto>.ErrorResponse(
                new List<string> { "Error interno al obtener categoría" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Busca categorías por nombre
    /// </summary>
    [HttpGet("buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CategoriaProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CategoriaProductoDto>>>> BuscarCategorias([FromQuery] string nombre)
    {
        _logger.LogInformation("🔍 GET /api/core/categorias/buscar - Nombre: {Nombre}", nombre);

        try
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                // Si no hay término de búsqueda, devolver todas las categorías activas
                return await GetCategorias(true, true);
            }

            // Buscar categorías que contengan el nombre
            var categorias = await _categoriaRepository.ObtenerActivasAsync();
            var categoriasFiltradas = categorias
                .Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Mapear a DTOs
            var categoriasDto = new List<CategoriaProductoDto>();
            
            foreach (var categoria in categoriasFiltradas)
            {
                var productosCategoria = await _productoRepository.ObtenerPorCategoriaAsync(
                    categoria.Id, true);
                
                var categoriaDto = new CategoriaProductoDto
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Color = "#2196F3", // Color por defecto
                    Icono = "🍽️", // Icono por defecto
                    Orden = categoria.Orden,
                    Activa = categoria.EstaActivo,
                    CantidadProductos = productosCategoria.Count,
                    ProductosDisponibles = productosCategoria.Count(p => p.EstaActivo),
                    FechaCreacion = categoria.FechaCreacion
                };
                
                categoriasDto.Add(categoriaDto);
            }

            // Ordenar por la propiedad Orden
            categoriasDto = categoriasDto.OrderBy(c => c.Orden).ToList();

            var response = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(
                categoriasDto, "Categorías encontradas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al buscar categorías: {Nombre}", nombre);
            var errorResponse = ApiResponse<List<CategoriaProductoDto>>.ErrorResponse(
                new List<string> { "Error interno al buscar categorías" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Crea una nueva categoría de productos
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoriaProductoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CategoriaProductoDto>>> CrearCategoria([FromBody] CrearCategoriaDto request)
    {
        _logger.LogInformation("➕ POST /api/core/categorias - Nombre: {Nombre}", request.Nombre);

        try
        {
            var command = new CrearCategoriaCommand
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Color = request.Color,
                Icono = request.Icono,
                Orden = request.Orden,
                Activa = request.Activa
            };

            var result = await _mediator.Send(command);

                    if (result.IsSuccess())
            {
                var response = ApiResponse<CategoriaProductoDto>.SuccessResponse(
                    result.Value, "Categoría creada exitosamente");
                return CreatedAtAction(nameof(GetCategoria), new { id = result.Value.Id }, response);
            }

            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors.ToList(), "Errores de validación", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al crear categoría: {Nombre}", request.Nombre);
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "Error interno al crear la categoría" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Actualiza una categoría de productos existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CategoriaProductoDto>>> ActualizarCategoria(
        Guid id, [FromBody] ActualizarCategoriaDto request)
    {
        _logger.LogInformation("✏️ PUT /api/core/categorias/{Id} - Nombre: {Nombre}", id, request.Nombre);

        try
        {
            // Validar que el ID de la URL coincida con el del body
            if (id != request.Id)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    new List<string> { "El ID de la URL no coincide con el ID del cuerpo de la petición" },
                    "Error de validación",
                    StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var command = new ActualizarCategoriaCommand
            {
                Id = request.Id,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Color = request.Color,
                Icono = request.Icono,
                Orden = request.Orden,
                Activa = request.Activa
            };

            var result = await _mediator.Send(command);

                    if (result.IsSuccess())
            {
                var response = ApiResponse<CategoriaProductoDto>.SuccessResponse(
                    result.Value, "Categoría actualizada exitosamente");
                return Ok(response);
            }

            if (result.Errors.Any(e => e.Contains("no encontrada")))
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    result.Errors.ToList(), "Categoría no encontrada", StatusCodes.Status404NotFound);
                return NotFound(errorResponse);
            }

            var validationErrorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors.ToList(), "Errores de validación", StatusCodes.Status400BadRequest);
            return BadRequest(validationErrorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar categoría: {Id}", id);
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "Error interno al actualizar la categoría" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Elimina una categoría de productos
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> EliminarCategoria(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/categorias/{Id}", id);

        try
        {
            var command = new EliminarCategoriaCommand { Id = id };
            var result = await _mediator.Send(command);

                    if (result.IsSuccess())
            {
                var response = ApiResponse<object>.SuccessResponse(
                    null, "Categoría eliminada exitosamente");
                return Ok(response);
            }

            if (result.Errors.Any(e => e.Contains("no encontrada")))
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    result.Errors.ToList(), "Categoría no encontrada", StatusCodes.Status404NotFound);
                return NotFound(errorResponse);
            }

            var validationErrorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors.ToList(), "Errores de validación", StatusCodes.Status400BadRequest);
            return BadRequest(validationErrorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al eliminar categoría: {Id}", id);
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "Error interno al eliminar la categoría" },
                "Error de servidor",
                StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
} 