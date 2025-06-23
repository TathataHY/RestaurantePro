using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;
using RestaurantePro.Application.Operaciones.Mesas.Commands.CrearMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.ActualizarMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.CambiarEstadoMesa;
using RestaurantePro.Api.Common;
using AutoMapper;
using MediatR;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// Controlador para la gestión de mesas del restaurante
/// Endpoints para gestionar mesas, estados, asignaciones y disponibilidad
/// </summary>
[ApiController]
[Route("api/operaciones/mesas")]
[Produces("application/json")]
[Authorize]
public class MesasController : ControllerBase
{
    private readonly ILogger<MesasController> _logger;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public MesasController(ILogger<MesasController> logger, IMediator mediator, IMapper mapper)
    {
        _logger = logger;
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene todas las mesas del restaurante
    /// </summary>
    /// <param name="estado">Filtro opcional por estado de mesa</param>
    /// <param name="ubicacion">Filtro opcional por ubicación</param>
    /// <param name="capacidadMinima">Filtro opcional por capacidad mínima</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<MesaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<MesaDto>>>> ObtenerMesas(
        [FromQuery] string? estado = null,
        [FromQuery] string? ubicacion = null,
        [FromQuery] int? capacidadMinima = null)
    {
        _logger.LogInformation("🍽️ GET /api/operaciones/mesas - Filtros: Estado={Estado}, Ubicacion={Ubicacion}, CapacidadMinima={CapacidadMinima}", 
            estado, ubicacion, capacidadMinima);

        var query = new ObtenerMesasQuery 
        { 
            Estado = estado, 
            Ubicacion = ubicacion, 
            CapacidadMinima = capacidadMinima 
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<MesaDto>>.ErrorResponse(
                result.Errors, "Error al obtener mesas", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<List<MesaDto>>.SuccessResponse(result.Value, "Mesas obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una mesa específica por ID
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ObtenerMesa(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/mesas/{Id}", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Crea una nueva mesa
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> CrearMesa([FromBody] CrearMesaCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("Ya existe una mesa"))
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al crear mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(result.Value, "Mesa creada exitosamente");
        return CreatedAtAction(nameof(ObtenerMesa), new { id = result.Value.Id }, response);
    }

    /// <summary>
    /// Actualiza una mesa existente
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="command">Datos actualizados de la mesa</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> ActualizarMesa(Guid id, [FromBody] ActualizarMesaCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/operaciones/mesas/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al actualizar mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(result.Value, "Mesa actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina una mesa por ID
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarMesa(Guid id)
    {
        var command = new Application.Operaciones.Mesas.Commands.EliminarMesa.EliminarMesaCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al eliminar mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(true, "Mesa eliminada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Asigna (ocupa) una mesa
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="request">Datos de la asignación</param>
    [HttpPost("{id:guid}/asignar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> AsignarMesa(Guid id, [FromBody] object request)
    {
        _logger.LogInformation("🪑 POST /api/operaciones/mesas/{Id}/asignar", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Libera una mesa ocupada
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="request">Datos de la liberación</param>
    [HttpPost("{id:guid}/liberar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> LiberarMesa(Guid id, [FromBody] object request)
    {
        _logger.LogInformation("🔓 POST /api/operaciones/mesas/{Id}/liberar", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Reserva una mesa
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="request">Datos de la reservación</param>
    [HttpPost("{id:guid}/reservar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ReservarMesa(Guid id, [FromBody] object request)
    {
        _logger.LogInformation("📅 POST /api/operaciones/mesas/{Id}/reservar", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Marca una mesa como fuera de servicio
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="request">Motivo y datos del mantenimiento</param>
    [HttpPost("{id:guid}/fuera-servicio")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> MarcarFueraDeServicio(Guid id, [FromBody] object request)
    {
        _logger.LogInformation("🚫 POST /api/operaciones/mesas/{Id}/fuera-servicio", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Obtiene mesas disponibles
    /// </summary>
    /// <param name="capacidadMinima">Capacidad mínima requerida</param>
    /// <param name="ubicacion">Ubicación preferida</param>
    [HttpGet("disponibles")]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<object>>>> ObtenerMesasDisponibles(
        [FromQuery] int? capacidadMinima = null,
        [FromQuery] string? ubicacion = null)
    {
        _logger.LogInformation("✅ GET /api/operaciones/mesas/disponibles - CapacidadMinima={CapacidadMinima}, Ubicacion={Ubicacion}", 
            capacidadMinima, ubicacion);

        var response = ApiResponse<List<object>>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Obtiene el estado de ocupación del restaurante
    /// </summary>
    [HttpGet("estado-ocupacion")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ObtenerEstadoOcupacion()
    {
        _logger.LogInformation("📊 GET /api/operaciones/mesas/estado-ocupacion");

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Busca la mejor mesa disponible para los criterios especificados
    /// </summary>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="ubicacionPreferida">Ubicación preferida</param>
    [HttpGet("buscar-mejor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> BuscarMejorMesa(
        [FromQuery] int numeroPersonas,
        [FromQuery] string? ubicacionPreferida = null)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/mesas/buscar-mejor - NumeroPersonas={NumeroPersonas}, UbicacionPreferida={UbicacionPreferida}", 
            numeroPersonas, ubicacionPreferida);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado" },
            "Esta funcionalidad estará disponible próximamente",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Cambia el estado de una mesa
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="command">Datos del cambio de estado</param>
    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> CambiarEstadoMesa(Guid id, [FromBody] CambiarEstadoMesaCommand command)
    {
        _logger.LogInformation("🔄 PUT /api/operaciones/mesas/{Id}/estado", id);
        command.MesaId = id;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al cambiar estado de la mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        // Obtener la mesa actualizada para devolver el DTO
        var mesaActualizadaResult = await _mediator.Send(new ObtenerMesaPorIdQuery { Id = id });
        if (!mesaActualizadaResult.Succeeded || mesaActualizadaResult.Value == null)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(new List<string> { "No se pudo obtener la mesa actualizada" }, "Error al obtener la mesa actualizada", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(mesaActualizadaResult.Value, "Estado de la mesa actualizado exitosamente");
        return Ok(response);
    }
} 