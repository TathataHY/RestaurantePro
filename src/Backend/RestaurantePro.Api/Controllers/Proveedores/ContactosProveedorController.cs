using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerContactoPorId;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerContactosPorProveedor;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerTodosContactos;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Controllers.Proveedores;

[ApiController]
[Route("api/proveedores/contactos")]
[Produces("application/json")]
[Authorize]
public class ContactosProveedorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContactosProveedorController> _logger;

    public ContactosProveedorController(IMediator mediator, ILogger<ContactosProveedorController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los contactos de proveedores
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ContactoProveedorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<ContactoProveedorDto>>>> GetContactos(
        [FromQuery] string? terminoBusqueda = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] Guid? proveedorId = null,
        [FromQuery] bool? esPrincipal = null,
        [FromQuery] string campoOrden = "Nombre",
        [FromQuery] string direccionOrden = "asc")
    {
        _logger.LogInformation("📞 GET /api/proveedores/contactos - Termino: {Termino}, SoloActivos: {SoloActivos}, ProveedorId: {ProveedorId}", 
            terminoBusqueda, soloActivos, proveedorId);

        try
        {
            var query = new ObtenerTodosContactosQuery
            {
                TerminoBusqueda = terminoBusqueda,
                SoloActivos = soloActivos,
                ProveedorId = proveedorId,
                EsPrincipal = esPrincipal,
                CampoOrden = campoOrden,
                DireccionOrden = direccionOrden,
                UsuarioId = Guid.NewGuid() // TODO: Obtener del usuario autenticado
            };

            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al obtener contactos: {Error}", result.Error);
                return BadRequest(ApiResponse<List<ContactoProveedorDto>>.ErrorResponse(
                    result.Error ?? "Error al obtener contactos", 
                    "Error al obtener contactos"));
            }

            var response = ApiResponse<List<ContactoProveedorDto>>.SuccessResponse(result.Value, "Contactos obtenidos exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener contactos");
            return StatusCode(500, ApiResponse<List<ContactoProveedorDto>>.ErrorResponse(
                "Error interno del servidor", 
                "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene un contacto específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContactoProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContactoProveedorDto>>> GetContacto(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/proveedores/contactos/{Id}", id);

        try
        {
            var query = new ObtenerContactoPorIdQuery(id, Guid.NewGuid()); // TODO: Obtener del usuario autenticado
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Contacto no encontrado: {Id}", id);
                return NotFound(ApiResponse<ContactoProveedorDto>.ErrorResponse(
                    result.Error ?? "Contacto no encontrado", 
                    "Contacto no encontrado"));
            }

            var response = ApiResponse<ContactoProveedorDto>.SuccessResponse(result.Value, "Contacto obtenido exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener contacto {Id}", id);
            return StatusCode(500, ApiResponse<ContactoProveedorDto>.ErrorResponse(
                "Error interno del servidor", 
                "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Crea un nuevo contacto de proveedor
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ContactoProveedorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContactoProveedorDto>>> CrearContacto(
        [FromBody] AgregarContactoCommand command)
    {
        _logger.LogInformation("➕ POST /api/proveedores/contactos - ProveedorId: {ProveedorId}, Nombre: {Nombre}", 
            command.ProveedorId, command.Nombre);

        try
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al crear contacto: {Error}", result.Error);
                return BadRequest(ApiResponse<ContactoProveedorDto>.ErrorResponse(
                    result.Error ?? "Error al crear contacto", 
                    "Error al crear contacto"));
            }

            var response = ApiResponse<ContactoProveedorDto>.SuccessResponse(result.Value, "Contacto creado exitosamente");
            return CreatedAtAction(nameof(GetContacto), new { id = result.Value.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear contacto");
            return StatusCode(500, ApiResponse<ContactoProveedorDto>.ErrorResponse(
                "Error interno del servidor", 
                "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Actualiza un contacto de proveedor existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContactoProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContactoProveedorDto>>> ActualizarContacto(
        Guid id, [FromBody] ActualizarContactoCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/proveedores/contactos/{Id} - Nombre: {Nombre}", id, command.Nombre);

        try
        {
            command.Id = id; // Asegurar que el ID coincida
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al actualizar contacto {Id}: {Error}", id, result.Error);
                return NotFound(ApiResponse<ContactoProveedorDto>.ErrorResponse(
                    result.Error ?? "Error al actualizar contacto", 
                    "Error al actualizar contacto"));
            }

            var response = ApiResponse<ContactoProveedorDto>.SuccessResponse(result.Value, "Contacto actualizado exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar contacto {Id}", id);
            return StatusCode(500, ApiResponse<ContactoProveedorDto>.ErrorResponse(
                "Error interno del servidor", 
                "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Elimina un contacto de proveedor
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarContacto(Guid id, [FromQuery] Guid proveedorId)
    {
        _logger.LogInformation("🗑️ DELETE /api/proveedores/contactos/{Id}", id);

        try
        {
            var command = new EliminarContactoCommand(id, proveedorId, "Eliminación solicitada por API");
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al eliminar contacto {Id}: {Error}", id, result.Error);
                return NotFound(ApiResponse<bool>.ErrorResponse(
                    result.Error ?? "Error al eliminar contacto", 
                    "Error al eliminar contacto"));
            }

            var response = ApiResponse<bool>.SuccessResponse(result.Value, "Contacto eliminado exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar contacto {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Error interno del servidor", 
                "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene contactos por proveedor específico
    /// </summary>
    [HttpGet("proveedor/{proveedorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ContactoProveedorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<ContactoProveedorDto>>>> GetContactosPorProveedor(
        Guid proveedorId,
        [FromQuery] bool soloActivos = true,
        [FromQuery] bool? esPrincipal = null)
    {
        _logger.LogInformation("🏢 GET /api/proveedores/contactos/proveedor/{ProveedorId} - SoloActivos: {SoloActivos}", 
            proveedorId, soloActivos);

        try
        {
            var query = new ObtenerContactosPorProveedorQuery
            {
                ProveedorId = proveedorId,
                SoloActivos = soloActivos,
                EsPrincipal = esPrincipal,
                UsuarioId = Guid.NewGuid() // TODO: Obtener del usuario autenticado
            };

            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al obtener contactos del proveedor {ProveedorId}: {Error}", proveedorId, result.Error);
                return NotFound(ApiResponse<List<ContactoProveedorDto>>.ErrorResponse(
                    result.Error ?? "Error al obtener contactos del proveedor", 
                    "Error al obtener contactos del proveedor"));
            }

            var response = ApiResponse<List<ContactoProveedorDto>>.SuccessResponse(result.Value, "Contactos del proveedor obtenidos exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener contactos del proveedor {ProveedorId}", proveedorId);
            return StatusCode(500, ApiResponse<List<ContactoProveedorDto>>.ErrorResponse(
                "Error interno del servidor", 
                "Error interno del servidor"));
        }
    }
} 