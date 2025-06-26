using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.AsignarProductos;
using RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.EliminarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.QuitarProductos;
using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromocionPorId;
using RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromociones;
using RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromocionesAplicables;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Api.Common;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Api.Controllers.Comercial;

/// <summary>
/// Controlador para la gestión de promociones y descuentos
/// Endpoints para crear, gestionar y aplicar promociones comerciales
/// </summary>
[ApiController]
[Route("api/comercial/promociones")]
[Produces("application/json")]
[Authorize]
public class PromocionesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PromocionesController> _logger;

    public PromocionesController(IMediator mediator, ILogger<PromocionesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las promociones con filtros opcionales
    /// </summary>
    /// <param name="estado">Filtro por estado</param>
    /// <param name="tipo">Filtro por tipo</param>
    /// <returns>Lista de promociones</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PromocionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PromocionDto>>>> ObtenerPromociones([FromQuery] string? estado, [FromQuery] string? tipo)
    {
        _logger.LogInformation("📋 GET /api/comercial/promociones - Obteniendo promociones con filtros: estado={Estado}, tipo={Tipo}", estado, tipo);
        
        var query = new ObtenerPromocionesQuery();
        
        // Convertir string a enum si se proporciona
        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPromocion>(estado, true, out var estadoEnum))
        {
            query.Estado = estadoEnum;
        }
        
        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoPromocion>(tipo, true, out var tipoEnum))
        {
            query.Tipo = tipoEnum;
        }
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<PromocionDto>>.ErrorResponse(
                result.Errors, "Error al obtener promociones", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<PromocionDto>>.SuccessResponse(
            result.Value, "Promociones obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una promoción específica por ID
    /// </summary>
    /// <param name="id">ID de la promoción</param>
    /// <returns>Datos de la promoción</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> ObtenerPromocion(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/comercial/promociones/{Id} - Obteniendo promoción por ID", id);
        
        var query = new ObtenerPromocionPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Promoción no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Promoción obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva promoción
    /// </summary>
    /// <param name="command">Datos de la promoción a crear</param>
    /// <returns>Promoción creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> CrearPromocion([FromBody] CrearPromocionCommand command)
    {
        _logger.LogInformation("➕ POST /api/comercial/promociones - Creando nueva promoción");
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al crear promoción", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Promoción creada exitosamente");
            
        return CreatedAtAction(
            nameof(ObtenerPromocion),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza una promoción existente
    /// </summary>
    /// <param name="id">ID de la promoción</param>
    /// <param name="command">Datos a actualizar</param>
    /// <returns>Promoción actualizada</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> ActualizarPromocion(Guid id, [FromBody] ActualizarPromocionCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/comercial/promociones/{Id} - Actualizando promoción", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al actualizar promoción", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Promoción actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina (cancela) una promoción
    /// </summary>
    /// <param name="id">ID de la promoción a cancelar</param>
    /// <returns>Confirmación de cancelación</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarPromocion(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/comercial/promociones/{Id} - Eliminando (cancelando) promoción", id);

        var command = new EliminarPromocionCommand 
        { 
            Id = id,
            Motivo = "Eliminada vía API DELETE endpoint"
        };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al eliminar promoción", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Promoción eliminada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Activa una promoción
    /// </summary>
    /// <param name="id">ID de la promoción a activar</param>
    /// <returns>Confirmación de activación</returns>
    [HttpPatch("{id:guid}/activar")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> ActivarPromocion(Guid id)
    {
        _logger.LogInformation("✅ PATCH /api/comercial/promociones/{Id}/activar - Activando promoción", id);

        var command = new ActivarPromocionCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al activar promoción", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Promoción activada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Pausa una promoción activa
    /// </summary>
    /// <param name="id">ID de la promoción a pausar</param>
    /// <returns>Confirmación de pausa</returns>
    [HttpPatch("{id:guid}/pausar")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> PausarPromocion(Guid id)
    {
        _logger.LogInformation("⏸️ PATCH /api/comercial/promociones/{Id}/pausar - Pausando promoción", id);

        var command = new PausarPromocionCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al pausar promoción", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Promoción pausada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Aplica una promoción a un monto específico
    /// </summary>
    /// <param name="request">Datos para aplicar la promoción</param>
    /// <returns>Resultado de la aplicación de la promoción</returns>
    [HttpPost("aplicar")]
    [ProducesResponseType(typeof(ApiResponse<AplicarPromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AplicarPromocionDto>>> AplicarPromocion([FromBody] AplicarPromocionRequest request)
    {
        _logger.LogInformation("🎁 POST /api/comercial/promociones/aplicar - Aplicando promoción");

        var command = new AplicarPromocionCommand
        {
            PromocionId = request.PromocionId,
            ClienteId = request.ClienteId,
            ComandaId = request.ComandaId,
            ProductosIds = request.ProductosIds,
            TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos
        };

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al aplicar promoción", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<AplicarPromocionDto>.SuccessResponse(
            result.Value, "Promoción aplicada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene las promociones aplicables para una comanda/cliente
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="monto">Monto de la comanda</param>
    /// <returns>Lista de promociones válidas</returns>
    [HttpGet("aplicables")]
    [ProducesResponseType(typeof(ApiResponse<List<PromocionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PromocionDto>>>> ObtenerPromocionesAplicables([FromQuery] Guid clienteId, [FromQuery] decimal monto)
    {
        _logger.LogInformation("💡 GET /api/comercial/promociones/aplicables - Obteniendo promociones aplicables para cliente {ClienteId} con monto {Monto}", clienteId, monto);
        
        var query = new ObtenerPromocionesAplicablesQuery
        {
            ClienteId = clienteId,
            Monto = monto
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<PromocionDto>>.ErrorResponse(
                result.Errors, "Error al obtener promociones aplicables", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<PromocionDto>>.SuccessResponse(
            result.Value, "Promociones aplicables obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Asigna productos a una promoción
    /// </summary>
    /// <param name="id">ID de la promoción</param>
    /// <param name="productosIds">Lista de IDs de productos a asignar</param>
    /// <returns>Confirmación</returns>
    [HttpPost("{id:guid}/productos")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> AsignarProductos(Guid id, [FromBody] List<Guid> productosIds)
    {
        _logger.LogInformation("📦 POST /api/comercial/promociones/{Id}/productos - Asignando productos a la promoción", id);

        var command = new AsignarProductosCommand
        {
            PromocionId = id,
            ProductosIds = productosIds
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al asignar productos", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Productos asignados exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Quita productos de una promoción
    /// </summary>
    /// <param name="id">ID de la promoción</param>
    /// <param name="productosIds">Lista de IDs de productos a quitar</param>
    /// <returns>Confirmación</returns>
    [HttpDelete("{id:guid}/productos")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> QuitarProductos(Guid id, [FromBody] List<Guid> productosIds)
    {
        _logger.LogInformation("📦 DELETE /api/comercial/promociones/{Id}/productos - Quitando productos de la promoción", id);

        var command = new QuitarProductosCommand
        {
            PromocionId = id,
            ProductosIds = productosIds
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al quitar productos", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PromocionDto>.SuccessResponse(
            result.Value, "Productos quitados exitosamente");
        return Ok(response);
    }
} 