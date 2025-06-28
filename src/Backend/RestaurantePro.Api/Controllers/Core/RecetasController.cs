using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;
using RestaurantePro.Application.Core.Recetas.Commands.ActualizarReceta;
using RestaurantePro.Application.Core.Recetas.Commands.EliminarReceta;
using RestaurantePro.Application.Core.Recetas.Queries.ObtenerRecetas;
using RestaurantePro.Application.Core.Recetas.Queries.ObtenerRecetaPorId;
using RestaurantePro.Application.Core.Recetas.Queries.ObtenerRecetasPorProducto;
using RestaurantePro.Application.Core.Recetas.Queries.CalcularCostoReceta;
using RestaurantePro.Application.Core.Recetas.Queries.VerificarDisponibilidadReceta;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para gestión de recetas de productos
/// Endpoints para CRUD completo de recetas y gestión de ingredientes
/// </summary>
[ApiController]
[Route("api/core/recetas")]
[Produces("application/json")]
[Authorize]
public class RecetasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecetasController> _logger;

    public RecetasController(IMediator mediator, ILogger<RecetasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las recetas disponibles con paginación
    /// </summary>
    /// <param name="pageNumber">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página (máximo 100)</param>
    /// <param name="soloActivas">Filtrar solo recetas activas</param>
    /// <param name="productoId">Filtrar por producto específico</param>
    /// <param name="filtroTexto">Filtro de texto libre</param>
    /// <param name="ordenarPor">Campo de ordenamiento</param>
    /// <param name="direccionOrden">Dirección del ordenamiento (Asc/Desc)</param>
    /// <returns>Lista paginada de recetas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<RecetaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<RecetaDto>>>> GetRecetas(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? soloActivas = null,
        [FromQuery] Guid? productoId = null,
        [FromQuery] string? filtroTexto = null,
        [FromQuery] string ordenarPor = "FechaCreacion",
        [FromQuery] string direccionOrden = "Desc")
    {
        _logger.LogInformation("🍕 GET /api/core/recetas?pageNumber={PageNumber}&pageSize={PageSize}&soloActivas={SoloActivas}&productoId={ProductoId}&filtroTexto={FiltroTexto}&ordenarPor={OrdenarPor}&direccionOrden={DireccionOrden}", 
            pageNumber, pageSize, soloActivas, productoId, filtroTexto, ordenarPor, direccionOrden);

        try
        {
            // Validar parámetros de paginación
            if (pageNumber < 1)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "El número de página debe ser mayor a 0" },
                    "Parámetros de paginación inválidos"));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "El tamaño de página debe estar entre 1 y 100" },
                    "Parámetros de paginación inválidos"));
            }

            var query = new ObtenerRecetasQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SoloActivas = soloActivas,
                ProductoId = productoId,
                FiltroTexto = filtroTexto,
                OrdenarPor = ordenarPor,
                DireccionOrden = direccionOrden
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<PaginatedList<RecetaDto>>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al obtener recetas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener recetas");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al obtener recetas"));
        }
    }

    /// <summary>
    /// Obtiene una receta específica por ID
    /// </summary>
    /// <param name="id">ID de la receta</param>
    /// <returns>Receta encontrada</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RecetaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RecetaDto>>> GetReceta(Guid id)
    {
        _logger.LogInformation("🍕 GET /api/core/recetas/{Id}", id);

        try
        {
            var query = new ObtenerRecetaPorIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<RecetaDto>.SuccessResponse(result.Value));
            }

            if (result.Error.Contains("no encontrada"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Receta no encontrada"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al obtener la receta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener receta {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al obtener la receta"));
        }
    }

    /// <summary>
    /// Crea una nueva receta para un producto
    /// </summary>
    /// <param name="command">Datos de la receta a crear</param>
    /// <returns>Receta creada</returns>
    [HttpPost]
    [Authorize(Roles = "Administrador,Chef")]
    [ProducesResponseType(typeof(ApiResponse<RecetaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RecetaDto>>> CrearReceta(
        [FromBody] CrearRecetaCommand command)
    {
        _logger.LogInformation("➕ POST /api/core/recetas - ProductoId: {ProductoId}", command?.ProductoId);

        try
        {
            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetReceta), new { id = result.Value.Id },
                    ApiResponse<RecetaDto>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al crear la receta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear receta {ProductoId}", command?.ProductoId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al crear la receta"));
        }
    }

    /// <summary>
    /// Actualiza una receta existente
    /// </summary>
    /// <param name="id">ID de la receta</param>
    /// <param name="command">Datos actualizados de la receta</param>
    /// <returns>Receta actualizada</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Chef")]
    [ProducesResponseType(typeof(ApiResponse<RecetaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RecetaDto>>> ActualizarReceta(
        Guid id, [FromBody] ActualizarRecetaCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/core/recetas/{Id}", id);

        try
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<RecetaDto>.SuccessResponse(result.Value));
            }

            if (result.Error.Contains("no encontrada"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Receta no encontrada"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al actualizar la receta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar receta {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al actualizar la receta"));
        }
    }

    /// <summary>
    /// Elimina una receta
    /// </summary>
    /// <param name="id">ID de la receta a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarReceta(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/recetas/{Id}", id);

        try
        {
            var command = new EliminarRecetaCommand(id);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Value));
            }

            if (result.Error.Contains("no encontrada"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Receta no encontrada"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al eliminar la receta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar receta {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al eliminar la receta"));
        }
    }

    /// <summary>
    /// Obtiene las recetas asociadas a un producto específico
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Lista de recetas del producto</returns>
    [HttpGet("producto/{productoId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<RecetaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<RecetaDto>>>> GetRecetasPorProducto(Guid productoId)
    {
        _logger.LogInformation("🍕 GET /api/core/recetas/producto/{ProductoId}", productoId);

        try
        {
            var query = new ObtenerRecetasPorProductoQuery(productoId);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<List<RecetaDto>>.SuccessResponse(result.Value));
            }

            if (result.Error.Contains("no encontrado"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Producto no encontrado"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al obtener las recetas del producto"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener recetas del producto {ProductoId}", productoId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al obtener las recetas del producto"));
        }
    }

    /// <summary>
    /// Calcula el costo de ingredientes de una receta
    /// </summary>
    /// <param name="id">ID de la receta</param>
    /// <returns>Costo total de ingredientes</returns>
    [HttpGet("{id:guid}/costo")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<decimal>>> CalcularCostoReceta(Guid id)
    {
        _logger.LogInformation("💰 GET /api/core/recetas/{Id}/costo", id);

        try
        {
            var query = new CalcularCostoRecetaQuery(id);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<decimal>.SuccessResponse(result.Value));
            }

            if (result.Error.Contains("no encontrada"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Receta no encontrada"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al calcular el costo de la receta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al calcular costo de receta {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al calcular el costo de la receta"));
        }
    }

    /// <summary>
    /// Verifica disponibilidad de ingredientes para una receta
    /// </summary>
    /// <param name="id">ID de la receta</param>
    /// <param name="cantidad">Cantidad de porciones a verificar</param>
    /// <returns>Estado de disponibilidad</returns>
    [HttpGet("{id:guid}/disponibilidad")]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadRecetaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DisponibilidadRecetaDto>>> VerificarDisponibilidad(
        Guid id, [FromQuery] int cantidad = 1)
    {
        _logger.LogInformation("✅ GET /api/core/recetas/{Id}/disponibilidad?cantidad={Cantidad}", id, cantidad);

        try
        {
            var query = new VerificarDisponibilidadRecetaQuery(id, cantidad);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<DisponibilidadRecetaDto>.SuccessResponse(result.Value));
            }

            if (result.Error.Contains("no encontrada"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Receta no encontrada"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error },
                "Error al verificar disponibilidad de la receta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al verificar disponibilidad de receta {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al verificar disponibilidad de la receta"));
        }
    }
} 