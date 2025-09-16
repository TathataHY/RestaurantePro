using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerHistorialComandas;
using RestaurantePro.Api.Common;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// 📋 Controlador para gestionar el historial de comandas
/// </summary>
[ApiController]
[Route("api/operaciones/historial-comandas")]
[Authorize]
public class HistorialComandasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HistorialComandasController> _logger;

    public HistorialComandasController(IMediator mediator, ILogger<HistorialComandasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 🔍 Obtener historial de comandas con filtros avanzados
    /// </summary>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 20)</param>
    /// <param name="fechaDesde">Fecha desde (opcional)</param>
    /// <param name="fechaHasta">Fecha hasta (opcional)</param>
    /// <param name="mesaId">ID de la mesa (opcional)</param>
    /// <param name="meseroId">ID del mesero (opcional)</param>
    /// <param name="clienteId">ID del cliente (opcional)</param>
    /// <param name="estado">Estado de la comanda (opcional)</param>
    /// <param name="montoMinimo">Monto mínimo (opcional)</param>
    /// <param name="montoMaximo">Monto máximo (opcional)</param>
    /// <param name="terminoBusqueda">Término de búsqueda (opcional)</param>
    /// <param name="incluirCanceladas">Incluir comandas canceladas (por defecto: false)</param>
    /// <param name="soloFinalizadas">Solo comandas finalizadas (por defecto: true)</param>
    /// <param name="ordenarPor">Campo de ordenamiento (por defecto: FechaMasReciente)</param>
    /// <returns>Lista paginada de comandas del historial</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ComandaSummaryDto>>>> ObtenerHistorialComandas(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] Guid? mesaId = null,
        [FromQuery] Guid? meseroId = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] string? estado = null,
        [FromQuery] decimal? montoMinimo = null,
        [FromQuery] decimal? montoMaximo = null,
        [FromQuery] string? terminoBusqueda = null,
        [FromQuery] bool incluirCanceladas = false,
        [FromQuery] bool soloFinalizadas = true,
        [FromQuery] string ordenarPor = "FechaMasReciente")
    {
        try
        {
            _logger.LogInformation("📋 GET /api/operaciones/historial-comandas - Página: {PageNumber}, Tamaño: {PageSize}, Mesa: {MesaId}", 
                pageNumber, pageSize, mesaId);

            // Parsear estado si se proporciona
            Domain.Operaciones.Comandas.Enums.EstadoComanda? estadoEnum = null;
            if (!string.IsNullOrWhiteSpace(estado) && 
                Enum.TryParse<Domain.Operaciones.Comandas.Enums.EstadoComanda>(estado, true, out var estadoParsed))
            {
                estadoEnum = estadoParsed;
            }

            // Parsear ordenamiento
            if (!Enum.TryParse<OrdenHistorial>(ordenarPor, true, out var ordenarPorEnum))
            {
                ordenarPorEnum = OrdenHistorial.FechaMasReciente;
            }

            var query = new ObtenerHistorialComandasQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                MesaId = mesaId,
                MeseroId = meseroId,
                ClienteId = clienteId,
                Estado = estadoEnum,
                MontoMinimo = montoMinimo,
                MontoMaximo = montoMaximo,
                TerminoBusqueda = terminoBusqueda,
                IncluirCanceladas = incluirCanceladas,
                SoloFinalizadas = soloFinalizadas,
                OrdenarPor = ordenarPorEnum
            };

            var result = await _mediator.Send(query);

            if (result.IsSuccess())
            {
                _logger.LogInformation("✅ Historial de comandas obtenido exitosamente. Total: {Total}", 
                    result.Value?.TotalCount ?? 0);
                
                return Ok(ApiResponse<PaginatedList<ComandaSummaryDto>>.SuccessResponse(result.Value!));
            }

            _logger.LogWarning("⚠️ Error al obtener historial de comandas: {Error}", result.Error);
            return BadRequest(ApiResponse<PaginatedList<ComandaSummaryDto>>.ErrorResponse(result.Error, "Error al obtener historial de comandas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener historial de comandas");
            return StatusCode(500, ApiResponse<PaginatedList<ComandaSummaryDto>>.ErrorResponse(
                "Error interno del servidor al obtener historial de comandas", "Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// 🔍 Obtener historial de comandas por mesa específica
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="fechaDesde">Fecha desde (opcional, por defecto: últimos 7 días)</param>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <returns>Lista paginada de comandas de la mesa</returns>
    [HttpGet("mesa/{mesaId:guid}")]
    public async Task<ActionResult<ApiResponse<PaginatedList<ComandaSummaryDto>>>> ObtenerHistorialPorMesa(
        [FromRoute] Guid mesaId,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] int pageNumber = 1)
    {
        try
        {
            _logger.LogInformation("📋 GET /api/operaciones/historial-comandas/mesa/{MesaId} - Página: {PageNumber}", 
                mesaId, pageNumber);

            var query = ObtenerHistorialComandasQuery.PorMesa(mesaId, fechaDesde, pageNumber);
            var result = await _mediator.Send(query);

            if (result.IsSuccess())
            {
                _logger.LogInformation("✅ Historial de mesa {MesaId} obtenido exitosamente. Total: {Total}", 
                    mesaId, result.Value?.TotalCount ?? 0);
                
                return Ok(ApiResponse<PaginatedList<ComandaSummaryDto>>.SuccessResponse(result.Value!));
            }

            _logger.LogWarning("⚠️ Error al obtener historial de mesa {MesaId}: {Error}", mesaId, result.Error);
            return BadRequest(ApiResponse<PaginatedList<ComandaSummaryDto>>.ErrorResponse(result.Error, "Error al obtener historial de la mesa"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener historial de mesa {MesaId}", mesaId);
            return StatusCode(500, ApiResponse<PaginatedList<ComandaSummaryDto>>.ErrorResponse(
                "Error interno del servidor al obtener historial de la mesa", "Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// 🔍 Obtener historial básico (últimos 30 días)
    /// </summary>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 20)</param>
    /// <returns>Lista paginada de comandas del historial básico</returns>
    [HttpGet("basico")]
    public async Task<ActionResult<ApiResponse<PaginatedList<ComandaSummaryDto>>>> ObtenerHistorialBasico(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("📋 GET /api/operaciones/historial-comandas/basico - Página: {PageNumber}, Tamaño: {PageSize}", 
                pageNumber, pageSize);

            var query = ObtenerHistorialComandasQuery.Basico(pageNumber, pageSize);
            var result = await _mediator.Send(query);

            if (result.IsSuccess())
            {
                _logger.LogInformation("✅ Historial básico obtenido exitosamente. Total: {Total}", 
                    result.Value?.TotalCount ?? 0);
                
                return Ok(ApiResponse<PaginatedList<ComandaSummaryDto>>.SuccessResponse(result.Value!));
            }

            _logger.LogWarning("⚠️ Error al obtener historial básico: {Error}", result.Error);
            return BadRequest(ApiResponse<PaginatedList<ComandaSummaryDto>>.ErrorResponse(result.Error, "Error al obtener historial básico"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener historial básico");
            return StatusCode(500, ApiResponse<PaginatedList<ComandaSummaryDto>>.ErrorResponse(
                "Error interno del servidor al obtener historial básico", "Error interno del servidor", 500));
        }
    }
}
