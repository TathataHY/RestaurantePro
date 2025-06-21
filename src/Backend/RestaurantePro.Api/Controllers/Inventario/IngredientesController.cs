using Microsoft.AspNetCore.Authorization;
using MediatR;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Api.Controllers.Inventario;

/// <summary>
/// Controlador para la gestión de ingredientes y su inventario
/// Endpoints para gestionar ingredientes, stock, movimientos y proveedores asociados
/// </summary>
[ApiController]
[Route("api/inventario/ingredientes")]
[Produces("application/json")]
// [Authorize] // Temporalmente comentado para facilitar las pruebas
public class IngredientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IngredientesController> _logger;

    public IngredientesController(IMediator mediator, ILogger<IngredientesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los ingredientes con filtros opcionales
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<IngredienteSummaryDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ObtenerIngredientes([FromQuery] ObtenerIngredientesPaginadosQuery query)
    {
        _logger.LogInformation("➡️ Obteniendo todos los ingredientes...");
        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            return Ok(ApiResponse<PaginatedList<IngredienteSummaryDto>>.SuccessResponse(result.Value, "Ingredientes obtenidos"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al obtener ingredientes"));
    }

    /// <summary>
    /// Obtiene un ingrediente por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<IngredienteDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ObtenerIngredientePorId(Guid id)
    {
        _logger.LogInformation("➡️ Obteniendo ingrediente por ID: {Id}", id);
        var query = new ObtenerIngredientePorIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<IngredienteDto>.SuccessResponse(result.Value));
        }
        
        return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Ingrediente no encontrado", 404));
    }

    /// <summary>
    /// Crea un nuevo ingrediente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IngredienteDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CrearIngrediente([FromBody] CrearIngredienteCommand command)
    {
        _logger.LogInformation("➡️ Creando nuevo ingrediente...");
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            var response = ApiResponse<IngredienteDto>.SuccessResponse(result.Value, "Ingrediente creado exitosamente");
            return CreatedAtAction(nameof(ObtenerIngredientePorId), new { id = result.Value.Id }, response);
        }
        
        return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al crear ingrediente"));
    }

    /// <summary>
    /// Actualiza un ingrediente existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 501)]
    [ProducesResponseType(401)]
    public IActionResult ActualizarIngrediente(Guid id, [FromBody] object ingredienteDto)
    {
        _logger.LogInformation("➡️ Actualizando ingrediente: {Id}", id);
        return StatusCode(501, ApiResponse<object>.ErrorResponse(new List<string> { "Endpoint no implementado" }, "Endpoint no implementado", 501));
    }

    /// <summary>
    /// Elimina un ingrediente (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 501)]
    [ProducesResponseType(401)]
    public IActionResult EliminarIngrediente(Guid id)
    {
        _logger.LogInformation("➡️ Eliminando ingrediente: {Id}", id);
        return StatusCode(501, ApiResponse<object>.ErrorResponse(new List<string> { "Endpoint no implementado" }, "Endpoint no implementado", 501));
    }

    /// <summary>
    /// Obtiene los movimientos de inventario para un ingrediente
    /// </summary>
    [HttpGet("{id}/movimientos")]
    [ProducesResponseType(typeof(ApiResponse<object>), 501)]
    [ProducesResponseType(401)]
    public IActionResult ObtenerMovimientosDeIngrediente(Guid id)
    {
        _logger.LogInformation("➡️ Obteniendo movimientos para el ingrediente: {Id}", id);
        return StatusCode(501, ApiResponse<object>.ErrorResponse(new List<string> { "Endpoint no implementado" }, "Endpoint no implementado", 501));
    }

    /// <summary>
    /// Registra un nuevo movimiento de inventario (ajuste manual)
    /// </summary>
    [HttpPost("{id}/movimientos")]
    [ProducesResponseType(typeof(ApiResponse<object>), 501)]
    [ProducesResponseType(401)]
    public IActionResult RegistrarMovimiento(Guid id, [FromBody] object movimientoDto)
    {
        _logger.LogInformation("➡️ Registrando movimiento para el ingrediente: {Id}", id);
        return StatusCode(501, ApiResponse<object>.ErrorResponse(new List<string> { "Endpoint no implementado" }, "Endpoint no implementado", 501));
    }

    /// <summary>
    /// Obtiene la lista de ingredientes con bajo stock
    /// </summary>
    [HttpGet("bajo-stock")]
    [ProducesResponseType(typeof(ApiResponse<List<IngredienteSummaryDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ObtenerIngredientesBajoStock([FromQuery] ObtenerIngredientesBajoStockQuery query)
    {
        _logger.LogInformation("➡️ Obteniendo ingredientes con bajo stock...");
        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            return Ok(ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(result.Value, "Ingredientes con bajo stock obtenidos"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al obtener ingredientes con bajo stock"));
    }
    
    /// <summary>
    /// Asocia un proveedor principal a un ingrediente
    /// </summary>
    [HttpPost("{id}/asociar-proveedor/{proveedorId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 501)]
    [ProducesResponseType(401)]
    public IActionResult AsociarProveedor(Guid id, Guid proveedorId)
    {
        _logger.LogInformation("➡️ Asociando proveedor {ProveedorId} a ingrediente {Id}", proveedorId, id);
        return StatusCode(501, ApiResponse<object>.ErrorResponse(new List<string> { "Endpoint no implementado" }, "Endpoint no implementado", 501));
    }
    
    /// <summary>
    /// Genera un reporte de valoración de inventario para los ingredientes
    /// </summary>
    [HttpGet("reporte/valoracion")]
    [ProducesResponseType(typeof(ApiResponse<object>), 501)]
    [ProducesResponseType(401)]
    public IActionResult GenerarReporteValoracion()
    {
        _logger.LogInformation("➡️ Generando reporte de valoración de ingredientes...");
        return StatusCode(501, ApiResponse<object>.ErrorResponse(new List<string> { "Endpoint no implementado" }, "Endpoint no implementado", 501));
    }
} 