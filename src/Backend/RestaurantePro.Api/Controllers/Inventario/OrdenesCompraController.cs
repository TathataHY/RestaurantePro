using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands;
using RestaurantePro.Application.Inventario.OrdenesCompra.Queries;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.CrearOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.ActualizarOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.AprobarOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RechazarOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenesCompraPaginadas;
using RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenCompraPorId;
using RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenesCompraPendientes;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Api.Controllers.Inventario;

/// <summary>
/// Controlador para gestión de órdenes de compra de inventario
/// </summary>
[ApiController]
[Route("api/inventario/ordenes-compra")]
[Produces("application/json")]
public class OrdenesCompraController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdenesCompraController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public OrdenesCompraController(
        IMediator mediator, 
        ILogger<OrdenesCompraController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    private Guid? GetCurrentUserId()
    {
        if (Guid.TryParse(_currentUserService.UserId, out var guid))
            return guid;
        return null;
    }

    /// <summary>
    /// Obtiene todas las órdenes de compra con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<OrdenCompraDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<OrdenCompraDto>>>> GetOrdenesCompra(
        [FromQuery] ObtenerOrdenesCompraPaginadasQuery query)
    {
        _logger.LogInformation("📋 GET /api/inventario/ordenes-compra");
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<PaginatedList<OrdenCompraDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener órdenes de compra", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<OrdenCompraDto>>.SuccessResponse(
            result.Value, "Órdenes de compra obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una orden de compra específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> GetOrdenCompra(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/inventario/ordenes-compra/{Id}", id);
        
        var query = new ObtenerOrdenCompraPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Orden de compra no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<OrdenCompraDto>.SuccessResponse(
            result.Value, "Orden de compra obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva orden de compra
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> CrearOrdenCompra(
        [FromBody] CrearOrdenCompraCommand command)
    {
        _logger.LogInformation("➕ POST /api/inventario/ordenes-compra");
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al crear orden de compra", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<OrdenCompraDto>.SuccessResponse(
            result.Value, "Orden de compra creada exitosamente");
            
        return CreatedAtAction(
            nameof(GetOrdenCompra),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza una orden de compra existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> ActualizarOrdenCompra(
        Guid id, [FromBody] ActualizarOrdenCompraCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/inventario/ordenes-compra/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al actualizar orden de compra", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<OrdenCompraDto>.SuccessResponse(
            result.Value, "Orden de compra actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Aprueba una orden de compra
    /// </summary>
    [HttpPost("{id:guid}/aprobar")]
    [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> AprobarOrdenCompra(Guid id)
    {
        _logger.LogInformation("✅ POST /api/inventario/ordenes-compra/{Id}/aprobar", id);

        var command = new AprobarOrdenCompraCommand 
        { 
            Id = id,
            UsuarioId = GetCurrentUserId()
        };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al aprobar orden de compra", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<OrdenCompraDto>.SuccessResponse(
            result.Value, "Orden de compra aprobada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Rechaza una orden de compra
    /// </summary>
    [HttpPost("{id:guid}/rechazar")]
    [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> RechazarOrdenCompra(
        Guid id, [FromBody] RechazarOrdenCompraCommand command)
    {
        _logger.LogInformation("❌ POST /api/inventario/ordenes-compra/{Id}/rechazar", id);

        command.Id = id;
        command.UsuarioId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al rechazar orden de compra", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<OrdenCompraDto>.SuccessResponse(
            result.Value, "Orden de compra rechazada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Recibe una orden de compra
    /// </summary>
    [HttpPost("{id:guid}/recibir")]
    [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> RecibirOrdenCompra(
        Guid id, [FromBody] RecibirOrdenCompraCommand command)
    {
        _logger.LogInformation("📦 POST /api/inventario/ordenes-compra/{Id}/recibir", id);

        command.Id = id;
        command.UsuarioId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al recibir orden de compra", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<OrdenCompraDto>.SuccessResponse(
            result.Value, "Orden de compra recibida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene las órdenes de compra pendientes
    /// </summary>
    [HttpGet("pendientes")]
    [ProducesResponseType(typeof(ApiResponse<List<OrdenCompraDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OrdenCompraDto>>>> ObtenerOrdenesPendientes()
    {
        _logger.LogInformation("📋 GET /api/inventario/ordenes-compra/pendientes");
        
        var query = new ObtenerOrdenesCompraPendientesQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<OrdenCompraDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener órdenes pendientes", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<OrdenCompraDto>>.SuccessResponse(
            result.Value, "Órdenes pendientes obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene las órdenes de compra por proveedor
    /// </summary>
    [HttpGet("proveedor/{proveedorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<OrdenCompraDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OrdenCompraDto>>>> ObtenerOrdenesPorProveedor(Guid proveedorId)
    {
        _logger.LogInformation("📋 GET /api/inventario/ordenes-compra/proveedor/{ProveedorId}", proveedorId);
        
        var query = new ObtenerOrdenesCompraPendientesQuery { ProveedorId = proveedorId };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<OrdenCompraDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener órdenes por proveedor", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<OrdenCompraDto>>.SuccessResponse(
            result.Value, "Órdenes por proveedor obtenidas exitosamente");
        return Ok(response);
    }
} 