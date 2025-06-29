using MediatR;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.CrearEvaluacionProveedor;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.ActualizarEvaluacionProveedor;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.EliminarEvaluacionProveedor;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerTodasEvaluaciones;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerEvaluacionPorId;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerEvaluacionesPorProveedor;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerPromedioEvaluaciones;

namespace RestaurantePro.Api.Controllers.Proveedores;

[ApiController]
[Route("api/proveedores/evaluaciones")]
[Produces("application/json")]
[Authorize]
public class EvaluacionesProveedorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EvaluacionesProveedorController> _logger;

    public EvaluacionesProveedorController(IMediator mediator, ILogger<EvaluacionesProveedorController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las evaluaciones de proveedores
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EvaluacionProveedorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<EvaluacionProveedorDto>>>> GetEvaluaciones([FromQuery] bool soloActivas = true)
    {
        var result = await _mediator.Send(new ObtenerTodasEvaluacionesQuery { SoloActivas = soloActivas });
        if (result.IsSuccess())
            return Ok(ApiResponse<List<EvaluacionProveedorDto>>.SuccessResponse(result.Value, "Evaluaciones obtenidas"));
        return BadRequest(ApiResponse<List<EvaluacionProveedorDto>>.ErrorResponse(result.Error, "Error al obtener evaluaciones"));
    }

    /// <summary>
    /// Obtiene una evaluación específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EvaluacionProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EvaluacionProveedorDto>>> GetEvaluacion(Guid id)
    {
        var result = await _mediator.Send(new ObtenerEvaluacionPorIdQuery(id));
        if (result.IsSuccess())
            return Ok(ApiResponse<EvaluacionProveedorDto>.SuccessResponse(result.Value, "Evaluación obtenida"));
        return NotFound(ApiResponse<object>.ErrorResponse(result.Error, "Evaluación no encontrada"));
    }

    /// <summary>
    /// Crea una nueva evaluación de proveedor
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EvaluacionProveedorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EvaluacionProveedorDto>>> CrearEvaluacion([FromBody] CrearEvaluacionProveedorCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess())
            return CreatedAtAction(nameof(GetEvaluacion), new { id = result.Value.Id }, ApiResponse<EvaluacionProveedorDto>.SuccessResponse(result.Value, "Evaluación creada exitosamente"));
        return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al crear evaluación"));
    }

    /// <summary>
    /// Actualiza una evaluación de proveedor existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EvaluacionProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EvaluacionProveedorDto>>> ActualizarEvaluacion(Guid id, [FromBody] ActualizarEvaluacionProveedorCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.ErrorResponse("El ID de la ruta no coincide con el del cuerpo", "Error de validación"));
        var result = await _mediator.Send(command);
        if (result.IsSuccess())
            return Ok(ApiResponse<EvaluacionProveedorDto>.SuccessResponse(result.Value, "Evaluación actualizada exitosamente"));
        if (result.Error == "Evaluación no encontrada")
            return NotFound(ApiResponse<object>.ErrorResponse(result.Error, "Evaluación no encontrada"));
        return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al actualizar evaluación"));
    }

    /// <summary>
    /// Elimina una evaluación de proveedor
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarEvaluacion(Guid id)
    {
        var result = await _mediator.Send(new EliminarEvaluacionProveedorCommand { Id = id });
        if (result.IsSuccess())
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Evaluación eliminada exitosamente"));
        if (result.Error == "Evaluación no encontrada")
            return NotFound(ApiResponse<object>.ErrorResponse(result.Error, "Evaluación no encontrada"));
        return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al eliminar evaluación"));
    }

    /// <summary>
    /// Obtiene evaluaciones por proveedor específico
    /// </summary>
    [HttpGet("proveedor/{proveedorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<EvaluacionProveedorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<EvaluacionProveedorDto>>>> GetEvaluacionesPorProveedor(Guid proveedorId, [FromQuery] bool soloActivas = true)
    {
        var result = await _mediator.Send(new ObtenerEvaluacionesPorProveedorQuery(proveedorId) { SoloActivas = soloActivas });
        if (result.IsSuccess())
            return Ok(ApiResponse<List<EvaluacionProveedorDto>>.SuccessResponse(result.Value, "Evaluaciones por proveedor obtenidas"));
        return BadRequest(ApiResponse<List<EvaluacionProveedorDto>>.ErrorResponse(result.Error, "Error al obtener evaluaciones del proveedor"));
    }

    /// <summary>
    /// Obtiene el promedio de evaluaciones de un proveedor
    /// </summary>
    [HttpGet("promedio/{proveedorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PromedioEvaluacionesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PromedioEvaluacionesDto>>> GetPromedioEvaluaciones(Guid proveedorId, [FromQuery] bool soloActivas = true)
    {
        var result = await _mediator.Send(new ObtenerPromedioEvaluacionesQuery(proveedorId) { SoloActivas = soloActivas });
        if (result.IsSuccess())
            return Ok(ApiResponse<PromedioEvaluacionesDto>.SuccessResponse(result.Value, "Promedio de evaluaciones calculado"));
        return BadRequest(ApiResponse<PromedioEvaluacionesDto>.ErrorResponse(result.Error, "Error al calcular promedio de evaluaciones"));
    }
} 