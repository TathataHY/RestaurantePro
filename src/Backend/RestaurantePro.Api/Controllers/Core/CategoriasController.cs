using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.Productos.Services;
using RestaurantePro.Domain.Core.Productos.Policies;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para gestionar categorías de productos
/// </summary>
[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IVisibilidadCategoriasPolicy _visibilidadPolicy;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(
        IProductoCategoriaRepository categoriaRepository,
        IProductoRepository productoRepository,
        IVisibilidadCategoriasPolicy visibilidadPolicy,
        ILogger<CategoriasController> logger)
    {
        _categoriaRepository = categoriaRepository;
        _productoRepository = productoRepository;
        _visibilidadPolicy = visibilidadPolicy;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las categorías de productos
    /// </summary>
    [HttpGet]
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
                Color = "#2196F3", // Color por defecto
                Icono = "🍽️", // Icono por defecto
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
} 