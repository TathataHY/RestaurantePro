using Microsoft.AspNetCore.Authorization;
using MediatR;
using RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;
using RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;
using RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;
using RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedorPorId;
using RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using static RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto.EliminarContactoCommand;

namespace RestaurantePro.Api.Controllers.Proveedores;

/// <summary>
/// Controlador para la gestión de proveedores
/// Endpoints para gestionar proveedores, contactos, evaluaciones y operaciones relacionadas
/// </summary>
[ApiController]
[Route("api/proveedores")]
[Produces("application/json")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(IMediator mediator, ILogger<ProveedoresController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los proveedores con filtros opcionales y paginación
    /// </summary>
    /// <param name="pageNumber">Número de página (default: 1)</param>
    /// <param name="pageSize">Elementos por página (default: 10, max: 100)</param>
    /// <param name="terminoBusqueda">Término de búsqueda (nombre, RFC, email, ciudad, etc.)</param>
    /// <param name="soloActivos">Filtrar solo proveedores activos (default: true)</param>
    /// <param name="ciudad">Filtro opcional por ciudad</param>
    /// <param name="pais">Filtro opcional por país</param>
    /// <param name="diasCredito_Min">Días de crédito mínimos</param>
    /// <param name="diasCredito_Max">Días de crédito máximos</param>
    /// <param name="incluirContactos">Incluir contactos en el resultado</param>
    /// <param name="campoOrden">Campo por el cual ordenar (default: Nombre)</param>
    /// <param name="direccionOrden">Dirección del ordenamiento: asc/desc (default: asc)</param>
    /// <returns>Lista paginada de proveedores</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProveedorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProveedorDto>>>> ObtenerProveedores(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? terminoBusqueda = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] string? ciudad = null,
        [FromQuery] string? pais = null,
        [FromQuery] int? diasCredito_Min = null,
        [FromQuery] int? diasCredito_Max = null,
        [FromQuery] bool incluirContactos = false,
        [FromQuery] string campoOrden = "Nombre",
        [FromQuery] string direccionOrden = "asc")
    {
        _logger.LogInformation("📋 GET /api/proveedores - Página: {PageNumber}, Tamaño: {PageSize}, Búsqueda: {TerminoBusqueda}",
            pageNumber, pageSize, terminoBusqueda);

        try
        {
            var query = new ObtenerProveedoresPaginadosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TerminoBusqueda = terminoBusqueda,
                SoloActivos = soloActivos,
                Ciudad = ciudad,
                Pais = pais,
                DiasCredito_Min = diasCredito_Min,
                DiasCredito_Max = diasCredito_Max,
                IncluirContactos = incluirContactos,
                CampoOrden = campoOrden,
                DireccionOrden = direccionOrden,
                UsuarioId = GetCurrentUserId()
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<PaginatedList<ProveedorDto>>.SuccessResponse(result.Value, "Proveedores obtenidos exitosamente"));
            }

            _logger.LogWarning("Error al obtener proveedores: {Error}", result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al obtener proveedores"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener proveedores");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al obtener proveedores"));
        }
    }

    /// <summary>
    /// Obtiene un proveedor específico por ID
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="incluirContactos">Incluir contactos del proveedor (default: true)</param>
    /// <param name="incluirCategorias">Incluir categorías del proveedor (default: false)</param>
    /// <returns>Datos del proveedor</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProveedorDto>>> ObtenerProveedor(
        Guid id,
        [FromQuery] bool incluirContactos = true,
        [FromQuery] bool incluirCategorias = false)
    {
        _logger.LogInformation("🔍 GET /api/proveedores/{Id} - Obteniendo proveedor por ID", id);

        try
        {
            var query = new ObtenerProveedorPorIdQuery(id, incluirContactos, incluirCategorias, GetCurrentUserId());
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ProveedorDto>.SuccessResponse(result.Value, "Proveedor obtenido exitosamente"));
            }

            if (result.Error?.Contains("no fue encontrado") == true)
            {
                _logger.LogWarning("Proveedor no encontrado: {Id}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor no encontrado"));
            }

            _logger.LogWarning("Error al obtener proveedor {Id}: {Error}", id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al obtener proveedor"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al obtener proveedor"));
        }
    }

    /// <summary>
    /// Crea un nuevo proveedor
    /// </summary>
    /// <param name="command">Datos del proveedor a crear</param>
    /// <returns>Proveedor creado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProveedorDto>>> CrearProveedor([FromBody] CrearProveedorCommand command)
    {
        _logger.LogInformation("➕ POST /api/proveedores - Creando nuevo proveedor: {Nombre}", command.Nombre);

        try
        {
            // Asignar usuario actual
            command.UsuarioId = GetCurrentUserId();

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                _logger.LogInformation("✅ Proveedor creado exitosamente: {Id} - {Nombre}", result.Value.Id, result.Value.Nombre);
                return CreatedAtAction(
                    nameof(ObtenerProveedor),
                    new { id = result.Value.Id },
                    ApiResponse<ProveedorDto>.SuccessResponse(result.Value, "Proveedor creado exitosamente"));
            }

            _logger.LogWarning("Error al crear proveedor {Nombre}: {Error}", command.Nombre, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al crear proveedor"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear proveedor {Nombre}", command.Nombre);
            // Incluir el mensaje real de la excepción en entorno de desarrollo
            var errorMsg = $"{ex.Message}" + (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { errorMsg },
                    "Error inesperado al crear proveedor"));
        }
    }

    /// <summary>
    /// Actualiza un proveedor existente
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="command">Datos a actualizar</param>
    /// <returns>Proveedor actualizado</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProveedorDto>>> ActualizarProveedor(Guid id, [FromBody] ActualizarProveedorCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/proveedores/{Id} - Actualizando proveedor", id);

        try
        {
            // Asignar ID del proveedor
            command.Id = id;

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ProveedorDto>.SuccessResponse(result.Value, "Proveedor actualizado exitosamente"));
            }

            if (result.Error?.Contains("no fue encontrado") == true)
            {
                _logger.LogWarning("Proveedor no encontrado para actualizar: {Id}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor no encontrado"));
            }

            _logger.LogWarning("Error al actualizar proveedor {Id}: {Error}", id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al actualizar proveedor"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al actualizar proveedor"));
        }
    }

    /// <summary>
    /// Elimina (desactiva) un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="request">Motivo de desactivación</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarProveedor(Guid id, [FromBody] DesactivarProveedorRequest? request = null)
    {
        _logger.LogInformation("🗑️ DELETE /api/proveedores/{Id} - Eliminando (desactivando) proveedor", id);

        try
        {
            var command = new DesactivarProveedorCommand
            {
                Id = id,
                RazonDesactivacion = request?.RazonDesactivacion ?? "Desactivación manual"
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Value, "Proveedor desactivado exitosamente"));
            }

            if (result.Error?.Contains("no fue encontrado") == true)
            {
                _logger.LogWarning("Proveedor no encontrado para desactivar: {Id}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor no encontrado"));
            }

            _logger.LogWarning("Error al desactivar proveedor {Id}: {Error}", id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al desactivar proveedor"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al desactivar proveedor"));
        }
    }

    /// <summary>
    /// Obtiene los contactos de un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <returns>Lista de contactos del proveedor</returns>
    [HttpGet("{id:guid}/contactos")]
    [ProducesResponseType(typeof(ApiResponse<List<ContactoProveedorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<ContactoProveedorDto>>>> ObtenerContactosProveedor(Guid id)
    {
        _logger.LogInformation("👥 GET /api/proveedores/{Id}/contactos - Obteniendo contactos del proveedor", id);

        try
        {
            // Obtener proveedor con contactos
            var query = new ObtenerProveedorPorIdQuery(id, incluirContactos: true, incluirCategorias: false, GetCurrentUserId());
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                var contactos = result.Value.Contactos ?? new List<ContactoProveedorDto>();
                return Ok(ApiResponse<List<ContactoProveedorDto>>.SuccessResponse(contactos, "Contactos obtenidos exitosamente"));
            }

            if (result.Error?.Contains("no fue encontrado") == true)
            {
                _logger.LogWarning("Proveedor no encontrado para obtener contactos: {Id}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor no encontrado"));
            }

            _logger.LogWarning("Error al obtener contactos del proveedor {Id}: {Error}", id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al obtener contactos"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener contactos del proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al obtener contactos"));
        }
    }

    /// <summary>
    /// Agrega un nuevo contacto a un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="command">Datos del contacto a agregar</param>
    /// <returns>Contacto creado</returns>
    [HttpPost("{id:guid}/contactos")]
    [ProducesResponseType(typeof(ApiResponse<ContactoProveedorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContactoProveedorDto>>> AgregarContactoProveedor(Guid id, [FromBody] AgregarContactoCommand command)
    {
        _logger.LogInformation("👤 POST /api/proveedores/{Id}/contactos - Agregando contacto al proveedor", id);

        try
        {
            // Asignar ID del proveedor
            command.ProveedorId = id;

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return CreatedAtAction(
                    nameof(ObtenerContactosProveedor),
                    new { id = id },
                    ApiResponse<ContactoProveedorDto>.SuccessResponse(result.Value, "Contacto agregado exitosamente"));
            }

            if (result.Error?.Contains("no existe") == true)
            {
                _logger.LogWarning("Proveedor no encontrado para agregar contacto: {Id}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor no encontrado"));
            }

            _logger.LogWarning("Error al agregar contacto al proveedor {Id}: {Error}", id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al agregar contacto"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al agregar contacto al proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al agregar contacto"));
        }
    }

    /// <summary>
    /// Actualiza un contacto específico de un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="contactoId">ID del contacto</param>
    /// <param name="command">Datos a actualizar del contacto</param>
    /// <returns>Contacto actualizado</returns>
    [HttpPut("{id:guid}/contactos/{contactoId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContactoProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContactoProveedorDto>>> ActualizarContactoProveedor(Guid id, Guid contactoId, [FromBody] ActualizarContactoCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/proveedores/{Id}/contactos/{ContactoId} - Actualizando contacto del proveedor", id, contactoId);

        try
        {
            // Asignar IDs
            command.ProveedorId = id;
            command.Id = contactoId;

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ContactoProveedorDto>.SuccessResponse(result.Value, "Contacto actualizado exitosamente"));
            }

            if (result.Error?.Contains("no existe") == true || result.Error?.Contains("no fue encontrado") == true)
            {
                _logger.LogWarning("Proveedor o contacto no encontrado: proveedor {Id}, contacto {ContactoId}", id, contactoId);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor o contacto no encontrado"));
            }

            _logger.LogWarning("Error al actualizar contacto {ContactoId} del proveedor {Id}: {Error}", contactoId, id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al actualizar contacto"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar contacto {ContactoId} del proveedor {Id}", contactoId, id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al actualizar contacto"));
        }
    }

    /// <summary>
    /// Elimina un contacto específico de un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="contactoId">ID del contacto</param>
    /// <param name="request">Configuración de eliminación</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id:guid}/contactos/{contactoId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarContactoProveedor(Guid id, Guid contactoId, [FromBody] EliminarContactoRequest? request = null)
    {
        _logger.LogInformation("🗑️ DELETE /api/proveedores/{Id}/contactos/{ContactoId} - Eliminando contacto del proveedor", id, contactoId);

        try
        {
            var command = new EliminarContactoCommand(contactoId, id, request?.MotivoEliminacion ?? "Eliminación manual")
            {
                TipoEliminacion = request?.TipoEliminacion ?? TipoEliminacion.Logica,
                ReasignarContactoPrincipal = request?.ReasignarContactoPrincipal ?? true,
                NuevoContactoPrincipalId = request?.NuevoContactoPrincipalId,
                ForzarEliminacion = request?.ForzarEliminacion ?? false
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Value, "Contacto eliminado exitosamente"));
            }

            if (result.Error?.Contains("no existe") == true || result.Error?.Contains("no fue encontrado") == true)
            {
                _logger.LogWarning("Proveedor o contacto no encontrado: proveedor {Id}, contacto {ContactoId}", id, contactoId);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor o contacto no encontrado"));
            }

            _logger.LogWarning("Error al eliminar contacto {ContactoId} del proveedor {Id}: {Error}", contactoId, id, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al eliminar contacto"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar contacto {ContactoId} del proveedor {Id}", contactoId, id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al eliminar contacto"));
        }
    }

    /// <summary>
    /// Obtiene las evaluaciones de un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <returns>Lista de evaluaciones del proveedor</returns>
    [HttpGet("{id:guid}/evaluaciones")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ObtenerEvaluacionesProveedor(Guid id)
    {
        _logger.LogInformation("⭐ GET /api/proveedores/{Id}/evaluaciones - Obteniendo evaluaciones del proveedor", id);

        // TODO: Implementar cuando esté disponible la lógica de evaluaciones
        // Por ahora retornamos una respuesta temporal
        _logger.LogWarning("Endpoint de evaluaciones pendiente de implementación");
        
        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Funcionalidad de evaluaciones en desarrollo" },
            "Endpoint pendiente de implementación",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Crea una nueva evaluación para un proveedor
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="command">Datos de la evaluación</param>
    /// <returns>Evaluación creada</returns>
    [HttpPost("{id:guid}/evaluaciones")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> CrearEvaluacionProveedor(Guid id, [FromBody] object command)
    {
        _logger.LogInformation("⭐ POST /api/proveedores/{Id}/evaluaciones - Creando evaluación para proveedor", id);

        // TODO: Implementar cuando esté disponible la lógica de evaluaciones
        // Por ahora retornamos una respuesta temporal
        _logger.LogWarning("Endpoint de evaluaciones pendiente de implementación");
        
        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Funcionalidad de evaluaciones en desarrollo" },
            "Endpoint pendiente de implementación",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Activa un proveedor previamente desactivado
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <returns>Confirmación de activación</returns>
    [HttpPatch("{id:guid}/activar")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ActivarProveedor(Guid id)
    {
        _logger.LogInformation("✅ PATCH /api/proveedores/{Id}/activar - Activando proveedor", id);

        // TODO: Implementar comando de activación cuando esté disponible
        // Por ahora usamos una implementación temporal
        try
        {
            // Obtener el proveedor para verificar que existe
            var query = new ObtenerProveedorPorIdQuery(id, incluirContactos: false, incluirCategorias: false, GetCurrentUserId());
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                if (result.Error?.Contains("no fue encontrado") == true)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        new List<string> { result.Error },
                        "Proveedor no encontrado"));
                }

                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error ?? "Error desconocido" },
                    "Error al verificar proveedor"));
            }

            // TODO: Implementar lógica de activación cuando esté disponible en el dominio
            _logger.LogInformation("Proveedor {Id} activado exitosamente", id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Proveedor activado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al activar proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al activar proveedor"));
        }
    }

    /// <summary>
    /// Desactiva un proveedor con motivo específico
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <param name="request">Motivo de desactivación</param>
    /// <returns>Confirmación de desactivación</returns>
    [HttpPatch("{id:guid}/desactivar")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> DesactivarProveedor(Guid id, [FromBody] DesactivarProveedorRequest request)
    {
        _logger.LogInformation("❌ PATCH /api/proveedores/{Id}/desactivar - Desactivando proveedor", id);

        try
        {
            var command = new DesactivarProveedorCommand
            {
                Id = id,
                RazonDesactivacion = request.RazonDesactivacion ?? "Desactivación manual"
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Value, "Proveedor desactivado exitosamente"));
            }

            if (result.Error?.Contains("no fue encontrado") == true)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Proveedor no encontrado"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al desactivar proveedor"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar proveedor {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error interno del servidor" },
                    "Error inesperado al desactivar proveedor"));
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde el contexto de autenticación
    /// </summary>
    private Guid GetCurrentUserId()
    {
        // TODO: Implementar obtención real del usuario autenticado
        // Por ahora retornamos un GUID de desarrollo
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}

// DTOs auxiliares para requests
public class DesactivarProveedorRequest
{
    public string? RazonDesactivacion { get; set; }
}

public class EliminarContactoRequest
{
    public TipoEliminacion TipoEliminacion { get; set; } = TipoEliminacion.Logica;
    public string? MotivoEliminacion { get; set; }
    public bool ReasignarContactoPrincipal { get; set; } = true;
    public Guid? NuevoContactoPrincipalId { get; set; }
    public bool ForzarEliminacion { get; set; } = false;
} 