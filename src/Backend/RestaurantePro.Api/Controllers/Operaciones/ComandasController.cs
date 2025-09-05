using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.EliminarComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CambiarEstadoComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProducto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.RemoverProducto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AplicarDescuento;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;
using RestaurantePro.Application.Operaciones.Comandas.Commands.ProcesarPedidoCompleto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;
using RestaurantePro.Application.Operaciones.Commands.FinalizarServicioCompleto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarCantidadItem;
using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasPaginadas;
using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// Controlador para la gestión de comandas del restaurante
/// Endpoints para CRUD completo de comandas y operaciones de mesa
/// </summary>
[ApiController]
[Route("api/operaciones/comandas")]
[Produces("application/json")]
[Authorize]
public class ComandasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComandasController> _logger;

    public ComandasController(IMediator mediator, ILogger<ComandasController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todas las comandas con filtros opcionales
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ComandaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ComandaDto>>>> GetComandas(
        [FromQuery] ObtenerComandasPaginadasQuery query)
    {
        _logger.LogInformation("🍽️ GET /api/operaciones/comandas - Estado: {Estado}, Mesa: {MesaId}, Mesero: {MeseroId}", 
            query.Estado, query.MesaId, query.MeseroId);

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al obtener comandas",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
            result.Value, "Comandas obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una comanda específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> GetComanda(Guid id, [FromQuery] bool incluirItems = true)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/comandas/{Id} - IncluirItems: {IncluirItems}", id, incluirItems);

        var query = ObtenerComandaPorIdQuery.Create(id, incluirItems);
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Comanda no encontrada",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Comanda obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva comanda
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> PostComanda([FromBody] CrearComandaCommand command)
    {
        _logger.LogInformation("➕ POST /api/operaciones/comandas - Cliente: {ClienteId}, Mesa: {MesaId}", 
            command.ClienteId, command.MesaId);

        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            var response = ApiResponse<ComandaDto>.SuccessResponse(result.Value, "Comanda creada exitosamente");
            response.StatusCode = StatusCodes.Status201Created;
            
            return CreatedAtAction(
                nameof(GetComanda),
                new { id = result.Value.Id },
                response);
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error ?? "Error desconocido" },
            "Error al crear comanda",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Actualiza una comanda existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> PutComanda(Guid id, [FromBody] ActualizarComandaCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/operaciones/comandas/{Id}", id);

        // Crear una nueva instancia del comando con el ID de la URL
        var commandWithId = command.WithId(id);

        var result = await _mediator.Send(commandWithId);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al actualizar comanda", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Comanda actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cambia el estado de una comanda
    /// </summary>
    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> CambiarEstadoComanda(Guid id, [FromBody] CambiarEstadoComandaCommand command)
    {
        _logger.LogInformation("🔄 PATCH /api/operaciones/comandas/{Id}/estado - NuevoEstado: {NuevoEstado}", id, command.NuevoEstado);

        var commandWithId = command.WithComandaId(id);
        var result = await _mediator.Send(commandWithId);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al cambiar estado de comanda", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Estado de comanda cambiado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Agrega un producto a una comanda
    /// </summary>
    [HttpPost("{id:guid}/productos")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> AgregarProducto(Guid id, [FromBody] AgregarProductoCommand command)
    {
        _logger.LogInformation("➕ POST /api/operaciones/comandas/{Id}/productos - Producto: {ProductoId}, Cantidad: {Cantidad}", 
            id, command.ProductoId, command.Cantidad);

        var commandWithId = command.WithComandaId(id);
        var result = await _mediator.Send(commandWithId);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al agregar producto", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Producto agregado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Remueve un producto de una comanda
    /// </summary>
    [HttpDelete("{id:guid}/productos/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> RemoverProducto(Guid id, Guid itemId)
    {
        _logger.LogInformation("➖ DELETE /api/operaciones/comandas/{Id}/productos/{ItemId}", id, itemId);

        var command = new RemoverProductoCommand { ComandaId = id, ItemId = itemId };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al remover producto", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Producto removido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem de la comanda
    /// </summary>
    [HttpPut("{id:guid}/productos/{itemId:guid}/cantidad")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> ActualizarCantidad(Guid id, Guid itemId, [FromBody] ActualizarCantidadItemCommand body)
    {
        _logger.LogInformation("✏️ PUT /api/operaciones/comandas/{Id}/productos/{ItemId}/cantidad - NuevaCantidad: {Cantidad}", id, itemId, body.NuevaCantidad);

        var command = new ActualizarCantidadItemCommand
        {
            ComandaId = id,
            ItemId = itemId,
            NuevaCantidad = body.NuevaCantidad
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al actualizar cantidad de item",
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(result.Value, "Cantidad actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Aplica un descuento a una comanda
    /// </summary>
    [HttpPost("{id:guid}/descuento")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> AplicarDescuento(Guid id, [FromBody] AplicarDescuentoCommand command)
    {
        _logger.LogInformation("💰 POST /api/operaciones/comandas/{Id}/descuento - Porcentaje: {PorcentajeDescuento}%", 
            id, command.PorcentajeDescuento);

        var commandWithId = command.WithComandaId(id);
        var result = await _mediator.Send(commandWithId);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al aplicar descuento", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Descuento aplicado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cierra una comanda y genera la factura correspondiente
    /// </summary>
    [HttpPost("{id:guid}/cerrar")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> CerrarComanda(Guid id, [FromBody] CerrarComandaCommand command)
    {
        _logger.LogInformation("🏁 POST /api/operaciones/comandas/{Id}/cerrar - Iniciando cierre de comanda", id);
        _logger.LogInformation("📋 Comando recibido - ComandaId: {ComandaId}, MétodoPago: {MetodoPago}, MontoPagado: {MontoPagado}", 
            command.ComandaId, command.MetodoPago, command.MontoPagado);

        var commandWithId = command.WithComandaId(id);
        _logger.LogInformation("🔄 Comando actualizado - ComandaId: {ComandaId}", commandWithId.ComandaId);
        
        _logger.LogInformation("📤 Enviando comando al mediator...");
        var result = await _mediator.Send(commandWithId);
        _logger.LogInformation("📥 Resultado recibido del mediator - Success: {Success}, Error: {Error}", 
            result.Succeeded, result.Error);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            _logger.LogError("❌ Error al cerrar comanda {ComandaId}: {Error}", id, result.Error);
            
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al cerrar comanda", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        _logger.LogInformation("✅ Comanda {ComandaId} cerrada exitosamente. Estado en respuesta: {Estado}", 
            id, result.Value?.Estado);

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Comanda cerrada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Divide una comanda en múltiples comandas separadas
    /// </summary>
    [HttpPost("{id:guid}/dividir")]
    [ProducesResponseType(typeof(ApiResponse<DividirComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DividirComandaDto>>> DividirComanda(Guid id, [FromBody] DividirComandaCommand command)
    {
        _logger.LogInformation("✂️ POST /api/operaciones/comandas/{Id}/dividir - Tipo: {TipoDivision}, Motivo: {Motivo}", 
            id, command.TipoDivision, command.MotivoDivision);

        // Asignar el ID de la comanda original desde la URL
        command.ComandaOriginalId = id;
        
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al dividir comanda", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<DividirComandaDto>.SuccessResponse(
            result.Value, "Comanda dividida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Unifica múltiples comandas en una sola comanda
    /// </summary>
    [HttpPost("unificar")]
    [ProducesResponseType(typeof(ApiResponse<UnificarComandasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UnificarComandasDto>>> UnificarComandas([FromBody] UnificarComandasCommand command)
    {
        _logger.LogInformation("🔗 POST /api/operaciones/comandas/unificar - Comandas: {ComandasCount}, Mesa: {MesaId}, Motivo: {Motivo}", 
            command.ComandasIds.Count, command.MesaDestinoId, command.MotivoUnificacion);
        
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al unificar comandas", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<UnificarComandasDto>.SuccessResponse(
            result.Value, "Comandas unificadas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Procesa un pedido completo que cruza múltiples bounded contexts
    /// Orquesta: Comanda + Inventario + Promociones + Facturación + Fidelización
    /// </summary>
    [HttpPost("procesar-pedido-completo")]
    [ProducesResponseType(typeof(ApiResponse<PedidoCompletoResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PedidoCompletoResult>>> ProcesarPedidoCompleto([FromBody] ProcesarPedidoCompletoCommand command)
    {
        _logger.LogInformation("🚀 POST /api/operaciones/comandas/procesar-pedido-completo - Items: {ItemsCount}, Cliente: {ClienteId}, Mesa: {MesaId}", 
            command.Items.Count, command.ClienteId, command.MesaId);
        
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al procesar pedido completo", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PedidoCompletoResult>.SuccessResponse(
            result.Value, "Pedido completo procesado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Finaliza una comanda específica
    /// </summary>
    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(typeof(ApiResponse<ComandaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ComandaDto>>> FinalizarComanda(Guid id, [FromBody] FinalizarComandaCommand command)
    {
        _logger.LogInformation("🍽️ POST /api/operaciones/comandas/{Id}/finalizar - Usuario: {UsuarioId}", 
            id, command.UsuarioId);

        // Crear nueva instancia con el ID de la comanda desde la URL
        var finalizarCommand = new FinalizarComandaCommand
        {
            ComandaId = id,
            UsuarioId = command.UsuarioId,
            ObservacionesFinalizacion = command.ObservacionesFinalizacion,
            ValidarTodosItemsListos = command.ValidarTodosItemsListos,
            NotificarMesero = command.NotificarMesero,
            FechaFinalizacion = command.FechaFinalizacion
        };
        
        var result = await _mediator.Send(finalizarCommand);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al finalizar comanda", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ComandaDto>.SuccessResponse(
            result.Value, "Comanda finalizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Finaliza el servicio completo implementando patrón Saga
    /// Orquesta: Comanda + Facturación + Fidelización + Mesa + Notificaciones + Analytics
    /// </summary>
    [HttpPost("{id:guid}/finalizar-servicio-completo")]
    [ProducesResponseType(typeof(ApiResponse<ServicioCompletoResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServicioCompletoResult>>> FinalizarServicioCompleto(Guid id, [FromBody] FinalizarServicioCompletoCommand command)
    {
        _logger.LogInformation("🚀 POST /api/operaciones/comandas/{Id}/finalizar-servicio-completo - Tipo: {TipoFinalizacion}", 
            id, command.TipoFinalizacion);

        // Crear nueva instancia con el ID de la comanda desde la URL
        var finalizarCommand = new FinalizarServicioCompletoCommand
        {
            ComandaId = id,
            ClienteId = command.ClienteId,
            TipoFinalizacion = command.TipoFinalizacion,
            PropinaSugerida = command.PropinaSugerida,
            GenerarFacturaInmediata = command.GenerarFacturaInmediata,
            AplicarDescuentoFidelizacion = command.AplicarDescuentoFidelizacion,
            LiberarMesaAutomaticamente = command.LiberarMesaAutomaticamente,
            EnviarNotificacionCliente = command.EnviarNotificacionCliente,
            RegistrarEstadisticas = command.RegistrarEstadisticas,
            ObservacionesFinalizacion = command.ObservacionesFinalizacion
        };
        
        var result = await _mediator.Send(finalizarCommand);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrada") == true 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, 
                "Error al finalizar servicio completo", 
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ServicioCompletoResult>.SuccessResponse(
            result.Value, "Servicio completo finalizado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina una comanda
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarComanda(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/operaciones/comandas/{Id}", id);

        var command = EliminarComandaCommand.Create(id);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al eliminar comanda",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Comanda eliminada exitosamente");
        return Ok(response);
    }
} 