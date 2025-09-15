using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerPlanoMesas;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesaPorId;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerEstadoMesas;
using RestaurantePro.Application.Operaciones.Mesas.Commands.CrearMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.ActualizarMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.CambiarEstadoMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarCliente;
using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.ReservarMesa;
using RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
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
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> ObtenerMesa(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/mesas/{Id}", id);

        var query = new ObtenerMesaPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no existe")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status500InternalServerError;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al obtener la mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(
            result.Value, "Mesa obtenida exitosamente");
        return Ok(response);
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
    public async Task<ActionResult<ApiResponse<object>>> AsignarMesa(Guid id, [FromBody] AsignarMesaCommand request)
    {
        _logger.LogInformation("🪑 POST /api/operaciones/mesas/{Id}/asignar", id);

        // Asignar el ID de la URL al comando
        request.MesaId = id;

        var result = await _mediator.Send(request);
        
        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al asignar la mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<object>.SuccessResponse(
            new { message = "Mesa asignada exitosamente" }, "Mesa asignada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Libera una mesa (la marca como disponible)
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="command">Datos de la liberación</param>
    [HttpPost("{id:guid}/liberar")]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> LiberarMesa(Guid id, [FromBody] LiberarMesaCommand command)
    {
        _logger.LogInformation("🆓 POST /api/operaciones/mesas/{Id}/liberar", id);
        command.MesaId = id;
        
        _logger.LogInformation("🔄 Ejecutando comando LiberarMesaCommand para mesa {MesaId}", id);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            _logger.LogWarning("⚠️ Comando LiberarMesaCommand falló para mesa {MesaId}: {Error}", id, result.Error);
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al liberar la mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        _logger.LogInformation("✅ Comando LiberarMesaCommand exitoso para mesa {MesaId}. Estado final: {Estado}", 
            id, result.Value.Estado);

        var response = ApiResponse<MesaDto>.SuccessResponse(result.Value, "Mesa liberada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Reserva una mesa para una fecha y hora específica
    /// </summary>
    /// <param name="command">Datos de la reserva</param>
    [HttpPost("reservar")]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> ReservarMesa([FromBody] ReservarMesaCommand command)
    {
        _logger.LogInformation("📅 POST /api/operaciones/mesas/reservar");
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no existe"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al reservar la mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        // Obtener la mesa actualizada para devolver el DTO
        var mesaActualizadaResult = await _mediator.Send(new ObtenerMesaPorIdQuery { Id = command.MesaId });
        if (!mesaActualizadaResult.Succeeded || mesaActualizadaResult.Value == null)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(new List<string> { "No se pudo obtener la mesa actualizada" }, "Error al obtener la mesa actualizada", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(mesaActualizadaResult.Value, "Mesa reservada exitosamente");
        return Ok(response);
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
    public async Task<ActionResult<ApiResponse<object>>> MarcarFueraDeServicio(Guid id, [FromBody] MarcarFueraDeServicioRequest request)
    {
        _logger.LogInformation("🚫 POST /api/operaciones/mesas/{Id}/fuera-servicio", id);

        var command = CambiarEstadoMesaCommand.MarcarFueraDeServicio(
            id, 
            request.Motivo, 
            null, 
            request.Observaciones);

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al marcar la mesa como fuera de servicio", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<object>.SuccessResponse(
            new { message = "Mesa marcada como fuera de servicio exitosamente" }, 
            "Mesa marcada como fuera de servicio exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Request model para marcar mesa fuera de servicio
    /// </summary>
    public class MarcarFueraDeServicioRequest
    {
        /// <summary>
        /// Motivo por el cual la mesa está fuera de servicio
        /// </summary>
        public string Motivo { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Obtiene mesas disponibles
    /// </summary>
    /// <param name="capacidadMinima">Capacidad mínima requerida</param>
    /// <param name="ubicacion">Ubicación preferida</param>
    [HttpGet("disponibles")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<MesaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PaginatedList<MesaDto>>>> ObtenerMesasDisponibles(
        [FromQuery] int? capacidadMinima = null,
        [FromQuery] string? ubicacion = null)
    {
        _logger.LogInformation("✅ GET /api/operaciones/mesas/disponibles - CapacidadMinima={CapacidadMinima}, Ubicacion={Ubicacion}", 
            capacidadMinima, ubicacion);

        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = capacidadMinima,
            Zona = ubicacion,
            Pagina = 1,
            TamanoPagina = 50,
            SoloActivas = true,
            OrdenarPorNumero = true
        };

        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al obtener mesas disponibles", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(
            result.Value, "Mesas disponibles obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el estado de ocupación del restaurante
    /// </summary>
    [HttpGet("estado-ocupacion")]
    [ProducesResponseType(typeof(ApiResponse<EstadoMesasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EstadoMesasDto>>> ObtenerEstadoOcupacion()
    {
        _logger.LogInformation("📊 GET /api/operaciones/mesas/estado-ocupacion");

        var query = new ObtenerEstadoMesasQuery
        {
            SoloActivas = true,
            IncluirEstadisticas = true,
            IncluirComandas = false
        };

        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al obtener estado de ocupación", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<EstadoMesasDto>.SuccessResponse(
            result.Value, "Estado de ocupación obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Busca la mejor mesa disponible para los criterios especificados
    /// </summary>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="ubicacionPreferida">Ubicación preferida</param>
    [HttpGet("buscar-mejor")]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> BuscarMejorMesa(
        [FromQuery] int numeroPersonas,
        [FromQuery] string? ubicacionPreferida = null)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/mesas/buscar-mejor - NumeroPersonas={NumeroPersonas}, UbicacionPreferida={UbicacionPreferida}", 
            numeroPersonas, ubicacionPreferida);

        if (numeroPersonas <= 0)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "El número de personas debe ser mayor a 0" },
                "Parámetros inválidos", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = numeroPersonas,
            Zona = ubicacionPreferida,
            Pagina = 1,
            TamanoPagina = 10,
            SoloActivas = true,
            OrdenarPorNumero = true
        };

        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al buscar mesa", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        // Buscar la mejor mesa (la más pequeña que cumpla con la capacidad)
        var mejorMesa = result.Value.Items
            .Where(m => m.Capacidad >= numeroPersonas)
            .OrderBy(m => m.Capacidad)
            .ThenBy(m => m.Numero)
            .FirstOrDefault();

        if (mejorMesa == null)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "No se encontró una mesa disponible para el número de personas especificado" },
                "Mesa no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(
            mejorMesa, "Mejor mesa encontrada exitosamente");
        return Ok(response);
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

    /// <summary>
    /// Asigna un cliente a una mesa
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <param name="command">Datos de la asignación</param>
    [HttpPost("{id:guid}/asignar-cliente")]
    [ProducesResponseType(typeof(ApiResponse<MesaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MesaDto>>> AsignarClienteAMesa(Guid id, [FromBody] AsignarClienteAMesaCommand command)
    {
        _logger.LogInformation("👤 POST /api/operaciones/mesas/{Id}/asignar-cliente", id);
        command.MesaId = id;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada") || e.Contains("no encontrado"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al asignar cliente a la mesa", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        // Obtener la mesa actualizada para devolver el DTO
        var mesaActualizadaResult = await _mediator.Send(new ObtenerMesaPorIdQuery { Id = id });
        if (!mesaActualizadaResult.Succeeded || mesaActualizadaResult.Value == null)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(new List<string> { "No se pudo obtener la mesa actualizada" }, "Error al obtener la mesa actualizada", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<MesaDto>.SuccessResponse(mesaActualizadaResult.Value, "Cliente asignado a la mesa exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el plano completo de mesas del restaurante
    /// </summary>
    /// <param name="query">Filtros para el plano</param>
    [HttpGet("plano")]
    [ProducesResponseType(typeof(ApiResponse<PlanoMesasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PlanoMesasDto>>> ObtenerPlanoMesas([FromQuery] ObtenerPlanoMesasQuery query)
    {
        _logger.LogInformation("🗺️ GET /api/operaciones/mesas/plano");
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(result.Errors, "Error al obtener el plano de mesas", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PlanoMesasDto>.SuccessResponse(result.Value, "Plano de mesas obtenido exitosamente");
        return Ok(response);
    }
} 