using Microsoft.AspNetCore.Authorization;
using MediatR;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.EliminarIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.AsociarProveedor;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerMovimientosIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerReporteValoracion;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

namespace RestaurantePro.Api.Controllers.Inventario;

/// <summary>
/// Controlador para la gestión de ingredientes y su inventario
/// Endpoints para gestionar ingredientes, stock, movimientos y proveedores asociados
/// </summary>
[ApiController]
[Route("api/inventario/ingredientes")]
[Produces("application/json")]
[Authorize]
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
    /// Obtiene todos los ingredientes con filtros opcionales (paginado)
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
    /// Obtiene todos los ingredientes como lista simple (para frontend móvil)
    /// </summary>
    [HttpGet("lista")]
    [ProducesResponseType(typeof(ApiResponse<List<IngredienteSummaryDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ObtenerIngredientesLista([FromQuery] bool soloActivos = true)
    {
        _logger.LogInformation("➡️ Obteniendo lista de ingredientes (soloActivos: {SoloActivos})", soloActivos);
        
        var query = new ObtenerIngredientesPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 100, // Usar el máximo permitido
            SoloActivos = soloActivos
        };
        
        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            var lista = result.Value.Items.ToList();
            return Ok(ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(lista, "Ingredientes obtenidos"));
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
    [ProducesResponseType(typeof(ApiResponse<IngredienteDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ActualizarIngrediente(Guid id, [FromBody] ActualizarIngredienteCommand command)
    {
        _logger.LogInformation("➡️ Actualizando ingrediente: {Id}", id);
        
        // Asignar el ID de la URL al comando
        command = command with { Id = id };
        
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<IngredienteDto>.SuccessResponse(result.Value, "Ingrediente actualizado exitosamente"));
        }
        
        var statusCode = result.Error?.Contains("no encontrado") == true ? 404 : 400;
        return StatusCode(statusCode, ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al actualizar ingrediente", statusCode));
    }

    /// <summary>
    /// Elimina un ingrediente (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> EliminarIngrediente(Guid id)
    {
        _logger.LogInformation("➡️ Eliminando ingrediente: {Id}", id);
        
        var command = new EliminarIngredienteCommand { Id = id };
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return NoContent();
        }
        
        return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al eliminar ingrediente", 404));
    }

    /// <summary>
    /// Obtiene los movimientos de inventario para un ingrediente
    /// </summary>
    [HttpGet("{id}/movimientos")]
    [ProducesResponseType(typeof(ApiResponse<List<MovimientoInventarioDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ObtenerMovimientosDeIngrediente(Guid id, [FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta, [FromQuery] int? limite)
    {
        _logger.LogInformation("➡️ Obteniendo movimientos para el ingrediente: {Id}", id);
        
        var query = new ObtenerMovimientosIngredienteQuery
        {
            IngredienteId = id,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Limite = limite
        };
        
        var result = await _mediator.Send(query);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<List<MovimientoInventarioDto>>.SuccessResponse(result.Value, "Movimientos obtenidos exitosamente"));
        }
        
        return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al obtener movimientos", 404));
    }

    /// <summary>
    /// Registra un nuevo movimiento de inventario (ajuste manual)
    /// </summary>
    [HttpPost("{id}/movimientos")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RegistrarMovimiento(Guid id, [FromBody] RegistrarMovimientoCommand command)
    {
        _logger.LogInformation("➡️ Registrando movimiento para el ingrediente: {Id}", id);
        
        // Asignar el ID del ingrediente al comando
        command = command with { IngredienteId = id };
        
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            var response = ApiResponse<Guid>.SuccessResponse(result.Value, "Movimiento registrado exitosamente");
            return CreatedAtAction(nameof(ObtenerMovimientosDeIngrediente), new { id }, response);
        }
        
        var statusCode = result.Error?.Contains("no encontrado") == true ? 404 : 400;
        return StatusCode(statusCode, ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al registrar movimiento", statusCode));
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
    /// Consume stock de un ingrediente (egreso)
    /// </summary>
    [HttpPost("{id}/consumir")]
    [ProducesResponseType(typeof(ApiResponse<IngredienteDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ConsumirStock(Guid id, [FromBody] ConsumirStockCommand command)
    {
        _logger.LogInformation("➡️ Consumiendo stock del ingrediente: {Id}", id);
        
        // Asignar el ID del ingrediente al comando
        command.IngredienteId = id;
        
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<IngredienteDto>.SuccessResponse(result.Value, "Stock consumido exitosamente"));
        }
        
        var statusCode = result.Error?.Contains("no encontrado") == true ? 404 : 400;
        return StatusCode(statusCode, ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al consumir stock", statusCode));
    }

    /// <summary>
    /// Registra un lote con fecha de vencimiento para un ingrediente
    /// </summary>
    [HttpPost("{id}/lotes")]
    [ProducesResponseType(typeof(ApiResponse<IngredienteDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RegistrarLote(Guid id, [FromBody] RegistrarLoteCommand command)
    {
        _logger.LogInformation("➡️ Registrando lote para el ingrediente: {Id}", id);
        
        // Asignar el ID del ingrediente al comando
        command.IngredienteId = id;
        
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            var response = ApiResponse<IngredienteDto>.SuccessResponse(result.Value, "Lote registrado exitosamente");
            return CreatedAtAction(nameof(ObtenerIngredientePorId), new { id }, response);
        }
        
        var statusCode = result.Error?.Contains("no encontrado") == true ? 404 : 400;
        return StatusCode(statusCode, ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al registrar lote", statusCode));
    }
    
    /// <summary>
    /// Asocia un proveedor principal a un ingrediente
    /// </summary>
    [HttpPost("{id}/asociar-proveedor/{proveedorId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AsociarProveedor(Guid id, Guid proveedorId, [FromBody] AsociarProveedorCommand? command = null)
    {
        _logger.LogInformation("➡️ Asociando proveedor {ProveedorId} a ingrediente {Id}", proveedorId, id);
        
        var asociarCommand = command ?? new AsociarProveedorCommand
        {
            IngredienteId = id,
            ProveedorId = proveedorId,
            EsProveedorPrincipal = true
        };
        
        var result = await _mediator.Send(asociarCommand);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<bool>.SuccessResponse(result.Value, "Proveedor asociado exitosamente"));
        }
        
        var statusCode = result.Error?.Contains("no encontrado") == true ? 404 : 400;
        return StatusCode(statusCode, ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al asociar proveedor", statusCode));
    }
    
    /// <summary>
    /// Genera un reporte de valoración de inventario para los ingredientes
    /// </summary>
    [HttpGet("reporte/valoracion")]
    [ProducesResponseType(typeof(ApiResponse<ReporteValoracionDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GenerarReporteValoracion([FromQuery] ObtenerReporteValoracionQuery query)
    {
        _logger.LogInformation("➡️ Generando reporte de valoración de ingredientes...");
        
        var result = await _mediator.Send(query);

        if (result.Succeeded)
        {
            return Ok(ApiResponse<ReporteValoracionDto>.SuccessResponse(result.Value, "Reporte de valoración generado exitosamente"));
        }
        
        return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al generar reporte de valoración"));
    }

    /// <summary>
    /// Obtiene estadísticas de ingredientes
    /// </summary>
    [HttpGet("estadisticas")]
    [ProducesResponseType(typeof(ApiResponse<EstadisticasIngredientesDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ObtenerEstadisticas()
    {
        _logger.LogInformation("➡️ Obteniendo estadísticas de ingredientes...");
        
        try
        {
            // Obtener todos los ingredientes para calcular estadísticas
            var query = new ObtenerIngredientesPaginadosQuery
            {
                PageNumber = 1,
                PageSize = 100,
                SoloActivos = true
            };
            
            var result = await _mediator.Send(query);
            
            if (result.Succeeded)
            {
                var ingredientes = result.Value.Items;
                var estadisticas = new EstadisticasIngredientesDto
                {
                    TotalIngredientes = ingredientes.Count,
                    IngredientesActivos = ingredientes.Count(i => i.Activo),
                    IngredientesBajoStock = ingredientes.Count(i => i.StockActual <= i.StockMinimo),
                    IngredientesSinStock = ingredientes.Count(i => i.StockActual <= 0),
                    ValorTotalInventario = ingredientes.Sum(i => i.ValorStock),
                    IngredientesPorCategoria = ingredientes.GroupBy(i => i.Categoria)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
                
                return Ok(ApiResponse<EstadisticasIngredientesDto>.SuccessResponse(estadisticas, "Estadísticas obtenidas exitosamente"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { result.Error }, "Error al obtener estadísticas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de ingredientes");
            return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { "Error interno del servidor" }, "Error al obtener estadísticas"));
        }
    }

    /// <summary>
    /// Busca ingredientes por término y filtros opcionales
    /// </summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<List<IngredienteSummaryDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> BuscarIngredientes([FromQuery] string termino, [FromQuery] string? categoria = null, [FromQuery] bool? soloDisponibles = null)
    {
        _logger.LogInformation("🔍 GET /api/inventario/ingredientes/buscar - Término: {Termino}, Categoría: {Categoria}, SoloDisponibles: {SoloDisponibles}", termino, categoria, soloDisponibles);

        try
        {
            // Obtener todos los ingredientes para buscar
            var query = new ObtenerIngredientesPaginadosQuery
            {
                PageNumber = 1,
                PageSize = 100,
                SoloActivos = true
            };
            
            var result = await _mediator.Send(query);
            
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(
                    new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener ingredientes", 400));
            }

            var ingredientes = result.Value.Items.AsEnumerable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(termino))
            {
                ingredientes = ingredientes.Where(i => 
                    i.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    i.Categoria.Contains(termino, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                ingredientes = ingredientes.Where(i => i.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
            }

            if (soloDisponibles.HasValue && soloDisponibles.Value)
            {
                ingredientes = ingredientes.Where(i => i.StockActual > 0);
            }

            var ingredientesFiltrados = ingredientes.ToList();

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(
                ingredientesFiltrados, "Ingredientes encontrados exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al buscar ingredientes: {Termino}", termino);
            var errorResponse = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(
                new List<string> { "Error interno al buscar ingredientes" }, "Error de servidor", 500);
            return StatusCode(500, errorResponse);
        }
    }
} 