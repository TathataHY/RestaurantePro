using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;
using RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;
using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;
using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;
using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Api.Controllers.Comercial;

/// <summary>
/// Controlador para la gestión de clientes
/// Contexto: Comercial
/// </summary>
[ApiController]
[Route("api/comercial/clientes")]
[Produces("application/json")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IMediator mediator, ILogger<ClientesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los clientes con paginación
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrador,Gerente,Empleado")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ClienteSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ClienteSummaryDto>>>> GetClientes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filtroTexto = null,
        [FromQuery] bool? soloActivos = true,
        [FromQuery] string? segmento = null,
        [FromQuery] bool? soloConTarjetaFidelizacion = null,
        [FromQuery] bool soloClientesFrecuentes = false,
        [FromQuery] DateTime? fechaRegistroDesde = null,
        [FromQuery] DateTime? fechaRegistroHasta = null,
        [FromQuery] string orderBy = "FechaCreacion",
        [FromQuery] string orderDirection = "desc")
    {
        _logger.LogInformation("👥 GET /api/comercial/clientes - Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);

        var query = new ObtenerClientesPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            FiltroTexto = filtroTexto,
            SoloActivos = soloActivos,
            Segmento = segmento,
            SoloConTarjetaFidelizacion = soloConTarjetaFidelizacion,
            SoloClientesFrecuentes = soloClientesFrecuentes,
            FechaRegistroDesde = fechaRegistroDesde,
            FechaRegistroHasta = fechaRegistroHasta,
            OrdenarPor = orderBy,
            DireccionOrden = orderDirection
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener clientes", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(
            result.Value, "Clientes obtenidos exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene un cliente específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Gerente,Empleado")]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> GetCliente(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/comercial/clientes/{Id}", id);

        var query = new ObtenerClientePorIdQuery { ClienteId = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Cliente no encontrado", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ClienteDto>.SuccessResponse(
            result.Value, "Cliente obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente,Empleado")]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> CrearCliente([FromBody] CrearClienteCommand command)
    {
        _logger.LogInformation("➕ POST /api/comercial/clientes - Creando cliente: {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al crear cliente", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<ClienteDto>.SuccessResponse(
            result.Value, "Cliente creado exitosamente");

        return CreatedAtAction(
            nameof(GetCliente),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Gerente,Empleado")]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> ActualizarCliente(Guid id, [FromBody] ActualizarClienteCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/comercial/clientes/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errors = result.Errors ?? new List<string> { result.Error ?? "Error desconocido" };
            var statusCode = errors.Any(e => e.Contains("no encontrado"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            var errorResponse = ApiResponse<object>.ErrorResponse(
                errors, "Error al actualizar cliente", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ClienteDto>.SuccessResponse(
            result.Value, "Cliente actualizado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Desactiva un cliente (eliminación lógica)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarCliente(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/comercial/clientes/{Id}", id);

        var command = new DesactivarClienteCommand 
        { 
            ClienteId = id,
            MotivoDesactivacion = "Desactivación por API",
            DesactivadoPor = "Sistema",
            NotasAdicionales = "Cliente desactivado vía endpoint DELETE API"
        };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al desactivar cliente", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            true, "Cliente desactivado exitosamente");
        return Ok(response);
    }
} 