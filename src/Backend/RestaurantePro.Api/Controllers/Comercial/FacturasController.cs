using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.ActualizarFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.RegistrarPagoFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.EliminarFactura;
using RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;
using RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturas;
using RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturasPorCliente;
using RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturasPorComanda;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Api.Models.Requests;

namespace RestaurantePro.Api.Controllers.Comercial;

/// <summary>
/// Controlador para la gestión de facturación comercial
/// Endpoints para CRUD completo de facturas y operaciones fiscales
/// </summary>
[ApiController]
[Route("api/comercial/facturas")]
[Produces("application/json")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FacturasController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public FacturasController(IMediator mediator, ILogger<FacturasController> logger, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Obtiene todas las facturas con filtros opcionales
    /// </summary>
    /// <param name="estado">Filtrar por estado de factura</param>
    /// <param name="clienteId">Filtrar por cliente específico</param>
    /// <param name="fechaDesde">Fecha desde para filtrar</param>
    /// <param name="fechaHasta">Fecha hasta para filtrar</param>
    /// <returns>Lista de facturas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FacturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<FacturaDto>>>> GetFacturas(
        [FromQuery] string? estado = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        _logger.LogInformation("📋 GET /api/comercial/facturas");
        var query = new ObtenerFacturasQuery
        {
            Estado = estado,
            ClienteId = clienteId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };
        var result = await _mediator.Send(query);
        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error al obtener facturas" },
                "Error al obtener facturas",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(
            result.Value, "Facturas obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una factura específica por ID
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <returns>Datos de la factura</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FacturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FacturaDto>>> GetFactura(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/comercial/facturas/{Id}", id);
        var query = ObtenerFacturaPorIdQuery.ConsultaBasica(id);
        var result = await _mediator.Send(query);
        if (!result.IsSuccess())
        {
            var errors = result.Errors ?? new List<string>();
            _logger.LogInformation("🔍 GetFactura - Errores recibidos: {@Errors}", errors);
            _logger.LogWarning("[DEBUG] Errores recibidos para determinar statusCode: {Errors}", string.Join(" | ", errors));
            var statusCode = errors.Any(e => e != null && (e.ToLower().Contains("no encontrada") || e.ToLower().Contains("no existe")))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            _logger.LogInformation("🔍 GetFactura - StatusCode determinado: {StatusCode}", statusCode);
            var errorResponse = ApiResponse<object>.ErrorResponse(
                errors.Count > 0 ? errors : new List<string> { "Factura no encontrada" },
                "Factura no encontrada",
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }
        var response = ApiResponse<FacturaDto>.SuccessResponse(
            result.Value, "Factura obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva factura para comandas específicas
    /// </summary>
    /// <param name="command">Datos de la factura a crear</param>
    /// <returns>Factura creada</returns>
    [HttpPost]
    [Authorize(Roles = "Administrador,Cajero,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<FacturaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FacturaDto>>> CrearFactura(
        [FromBody] CrearFacturaCommand command)
    {
        _logger.LogInformation("➕ POST /api/comercial/facturas");
        var result = await _mediator.Send(command);
        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error al crear factura" },
                "Error al crear factura",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        var response = ApiResponse<FacturaDto>.SuccessResponse(
            result.Value, "Factura creada exitosamente");
        return CreatedAtAction(nameof(GetFactura), new { id = result.Value.Id }, response);
    }

    /// <summary>
    /// Actualiza una factura existente
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <param name="command">Datos de actualización</param>
    /// <returns>Factura actualizada</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Cajero,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<FacturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FacturaDto>>> ActualizarFactura(
        Guid id, [FromBody] ActualizarFacturaCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/comercial/facturas/{Id}", id);
        command.Id = id;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess())
        {
            var statusCode = result.Errors != null && result.Errors.Any(e => e.Contains("no encontrada") || e.Contains("no existe"))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error al actualizar factura" },
                "Error al actualizar factura",
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }
        var response = ApiResponse<FacturaDto>.SuccessResponse(
            result.Value, "Factura actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene las facturas de un cliente específico
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="soloActivas">Filtrar solo facturas activas</param>
    /// <returns>Lista de facturas del cliente</returns>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FacturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<FacturaDto>>>> GetFacturasPorCliente(
        Guid clienteId, [FromQuery] bool soloActivas = true)
    {
        _logger.LogInformation("📄 GET /api/comercial/facturas/cliente/{ClienteId}", clienteId);
        var query = new ObtenerFacturasPorClienteQuery { ClienteId = clienteId, SoloActivas = soloActivas };
        var result = await _mediator.Send(query);
        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error desconocido" },
                "No se encontraron facturas para el cliente",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }
        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(
            result.Value,
            "Facturas del cliente obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene las facturas asociadas a una comanda
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <returns>Lista de facturas de la comanda</returns>
    [HttpGet("comanda/{comandaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FacturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<FacturaDto>>>> GetFacturasPorComanda(
        Guid comandaId)
    {
        _logger.LogInformation("📄 GET /api/comercial/facturas/comanda/{ComandaId}", comandaId);
        var query = new ObtenerFacturasPorComandaQuery { ComandaId = comandaId };
        var result = await _mediator.Send(query);
        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error desconocido" },
                "No se encontraron facturas para la comanda",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }
        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(
            result.Value,
            "Facturas de la comanda obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Genera el PDF de una factura para descarga
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <returns>Archivo PDF de la factura</returns>
    [HttpGet("{id:guid}/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DescargarFacturaPdf(Guid id)
    {
        _logger.LogInformation("📄 GET /api/comercial/facturas/{Id}/pdf - INICIO", id);

        // Primero verificar si la factura existe
        var facturaQuery = ObtenerFacturaPorIdQuery.ConsultaBasica(id);
        _logger.LogInformation("📄 GET /api/comercial/facturas/{Id}/pdf - Enviando query al mediator", id);
        var facturaResult = await _mediator.Send(facturaQuery);
        _logger.LogInformation("📄 GET /api/comercial/facturas/{Id}/pdf - Resultado recibido: IsSuccess={IsSuccess}", id, facturaResult.IsSuccess());
        
        if (!facturaResult.IsSuccess())
        {
            _logger.LogInformation("📄 GET /api/comercial/facturas/{Id}/pdf - Factura no encontrada, devolviendo 404", id);
            var errorsPdf = facturaResult.Errors ?? new List<string>();
            var statusCodePdf = errorsPdf.Any(e => e != null && 
                (e.ToLower().Contains("no encontrada") || 
                 e.ToLower().Contains("no existe")))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var notFoundResponse = ApiResponse<object>.ErrorResponse(
                errorsPdf.Count > 0 ? errorsPdf : new List<string> { "Factura no encontrada" },
                "Factura no encontrada",
                statusCodePdf);
            return StatusCode(statusCodePdf, notFoundResponse);
        }

        // Aquí deberías invocar una Query/Handler que genere el PDF y devuelva el archivo o un error
        // Por ahora, devolvemos 501 NotImplemented
        var notImplementedResponse = ApiResponse<object>.ErrorResponse(
            new List<string> { "Funcionalidad no implementada" }, 
            "Descarga de PDF no implementada", 
            StatusCodes.Status501NotImplemented);
        return StatusCode(StatusCodes.Status501NotImplemented, notImplementedResponse);
    }

    /// <summary>
    /// Obtiene facturas pendientes de pago
    /// </summary>
    /// <param name="diasVencimiento">Filtrar por días de vencimiento</param>
    /// <returns>Lista de facturas pendientes</returns>
    [HttpGet("pendientes")]
    [Authorize(Roles = "Administrador,Gerente,Cajero")]
    [ProducesResponseType(typeof(ApiResponse<List<FacturaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FacturaDto>>>> GetFacturasPendientes(
        [FromQuery] int? diasVencimiento = null)
    {
        _logger.LogInformation("📄 GET /api/comercial/facturas/pendientes?diasVencimiento={DiasVencimiento}", diasVencimiento);

        var query = new ObtenerFacturasQuery
        {
            SoloPendientesPago = true,
            DiasVencimiento = diasVencimiento
        };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error desconocido" }, 
                "Error al obtener facturas pendientes", 
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(
            result.Value, "Facturas pendientes obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Registra un pago para una factura
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <param name="command">Datos del pago</param>
    /// <returns>Confirmación del pago</returns>
    [HttpPost("{id:guid}/pagar")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> RegistrarPago(
        Guid id, [FromBody] RegistrarPagoFacturaCommand command)
    {
        _logger.LogInformation("💳 POST /api/comercial/facturas/{Id}/pagar", id);
        command.FacturaId = id;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error al registrar el pago" },
                "No se pudo registrar el pago",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        var response = ApiResponse<bool>.SuccessResponse(
            result.Value,
            "Pago registrado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Anula una factura existente
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <returns>Confirmación de anulación</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Cajero,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> AnularFactura(Guid id)
    {
        _logger.LogInformation("❌ DELETE /api/comercial/facturas/{Id}", id);
        
        // Obtener el usuario actual para la autorización
        var usuarioActualId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(usuarioActualId))
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "Usuario no autenticado" },
                "Error de autenticación",
                StatusCodes.Status401Unauthorized);
            return Unauthorized(errorResponse);
        }
        
        // Convertir el string UserId a Guid
        if (!Guid.TryParse(usuarioActualId, out var usuarioId))
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "ID de usuario inválido" },
                "Error de autenticación",
                StatusCodes.Status401Unauthorized);
            return Unauthorized(errorResponse);
        }
        
        var command = new EliminarFacturaCommand
        {
            FacturaId = id,
            Motivo = "Anulación solicitada desde API",
            UsuarioAutorizaId = usuarioId
        };
        
        var result = await _mediator.Send(command);
        if (!result.IsSuccess())
        {
            var errors = result.Errors ?? new List<string>();
            var statusCode = errors.Any(e => e != null && (e.ToLower().Contains("no encontrada") || e.ToLower().Contains("no existe")))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(
                errors.Count > 0 ? errors : new List<string> { "Error al anular factura" },
                "Error al anular factura",
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }
        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Factura anulada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Anula una factura existente (método alternativo con PATCH)
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <param name="request">Datos de la anulación</param>
    /// <returns>Confirmación de anulación</returns>
    [HttpPatch("{id:guid}/anular")]
    [Authorize(Roles = "Administrador,Cajero,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> AnularFacturaPatch(
        Guid id, [FromBody] AnularFacturaRequest request)
    {
        _logger.LogInformation("🔄 PATCH /api/comercial/facturas/{Id}/anular", id);

        var command = new AnularFacturaCommand
        {
            FacturaId = id,
            Motivo = request.Motivo,
            DescripcionDetallada = request.DescripcionDetallada,
            UsuarioAutorizaId = Guid.TryParse(_currentUserService.UserId, out var userId) ? userId : Guid.Empty,
            TipoAnulacion = request.TipoAnulacion,
            ObservacionesAdicionales = request.ObservacionesAdicionales,
            GenerarNotaCredito = request.GenerarNotaCredito,
            NotificarCliente = request.NotificarCliente,
            ProcesarDevolucionPago = request.ProcesarDevolucionPago,
            MetodoDevolucion = request.MetodoDevolucion,
            RevertirInventario = request.RevertirInventario,
            CancelarPuntosFidelizacion = request.CancelarPuntosFidelizacion,
            Prioridad = 2 // Prioridad normal por defecto
        };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess())
        {
            var errors = result.Errors ?? new List<string>();
            var statusCode = errors.Any(e => e != null && (e.ToLower().Contains("no encontrada") || e.ToLower().Contains("no existe")))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            var errorResponse = ApiResponse<object>.ErrorResponse(
                errors.Count > 0 ? errors : new List<string> { "Error al anular factura" },
                "Error al anular factura",
                statusCode);
            return StatusCode(statusCode, errorResponse);
        }
        
        var response = ApiResponse<bool>.SuccessResponse(
            true, "Factura anulada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Envía una factura por email al cliente
    /// </summary>
    /// <param name="id">ID de la factura</param>
    /// <param name="emailDestino">Email de destino (opcional, usa el del cliente si no se especifica)</param>
    /// <returns>Confirmación del envío</returns>
    [HttpPost("{id:guid}/enviar-email")]
    [Authorize(Roles = "Administrador,Cajero,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EnviarFacturaPorEmail(
        Guid id, [FromBody] string? emailDestino = null)
    {
        _logger.LogInformation("📧 POST /api/comercial/facturas/{Id}/enviar-email", id);

        // Por ahora, devolvemos 501 NotImplemented ya que no tenemos el servicio de email implementado
        var errorResponse = ApiResponse<object>.ErrorResponse(
            new List<string> { "Funcionalidad no implementada" }, 
            "Envío de email no implementado", 
            StatusCodes.Status501NotImplemented);
        return StatusCode(StatusCodes.Status501NotImplemented, errorResponse);
    }

    /// <summary>
    /// Busca facturas por criterios específicos
    /// </summary>
    /// <param name="numeroFactura">Número de factura</param>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="estado">Estado de la factura</param>
    /// <param name="tipoFactura">Tipo de factura</param>
    /// <param name="fechaDesde">Fecha desde</param>
    /// <param name="fechaHasta">Fecha hasta</param>
    /// <param name="montoMinimo">Monto mínimo</param>
    /// <param name="montoMaximo">Monto máximo</param>
    /// <returns>Lista de facturas que coinciden con los criterios</returns>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<List<FacturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<FacturaDto>>>> BuscarFacturas(
        [FromQuery] string? numeroFactura = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? tipoFactura = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] decimal? montoMinimo = null,
        [FromQuery] decimal? montoMaximo = null)
    {
        _logger.LogInformation("🔍 GET /api/comercial/facturas/buscar");
        
        var query = new ObtenerFacturasQuery
        {
            Estado = estado,
            ClienteId = clienteId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            TipoFactura = tipoFactura,
            // TODO: Agregar filtros adicionales cuando se implementen en el Query
        };
        
        var result = await _mediator.Send(query);
        if (!result.IsSuccess())
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { "Error al buscar facturas" },
                "Error al buscar facturas",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        
        // Filtrar por número de factura si se especifica
        var facturasFiltradas = result.Value;
        if (!string.IsNullOrWhiteSpace(numeroFactura))
        {
            facturasFiltradas = facturasFiltradas.Where(f => f.Numero.Contains(numeroFactura)).ToList();
        }
        
        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(
            facturasFiltradas, "Búsqueda de facturas completada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Genera un reporte de facturas según criterios específicos
    /// </summary>
    /// <param name="tipoReporte">Tipo de reporte (ventas, clientes, productos, etc.)</param>
    /// <param name="fechaDesde">Fecha desde</param>
    /// <param name="fechaHasta">Fecha hasta</param>
    /// <param name="formato">Formato del reporte (pdf, excel, json)</param>
    /// <returns>Reporte generado</returns>
    [HttpGet("reporte")]
    [Authorize(Roles = "Administrador,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> GenerarReporteFacturas(
        [FromQuery] string tipoReporte = "ventas",
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] string formato = "json")
    {
        _logger.LogInformation("📊 GET /api/comercial/facturas/reporte?tipoReporte={TipoReporte}", tipoReporte);

        // Por ahora, devolvemos 501 NotImplemented ya que no tenemos el servicio de reportes implementado
        var errorResponse = ApiResponse<object>.ErrorResponse(
            new List<string> { "Funcionalidad no implementada" }, 
            "Generación de reportes no implementada", 
            StatusCodes.Status501NotImplemented);
        return StatusCode(StatusCodes.Status501NotImplemented, errorResponse);
    }
} 