using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using MediatR;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromociones;

namespace RestaurantePro.Api.Controllers.Public;

/// <summary>
/// Promociones públicas visibles (solo lectura)
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/promociones")]
[Produces("application/json")]
public class PromocionesPublicController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PromocionesPublicController> _logger;

    public PromocionesPublicController(IMediator mediator, ILogger<PromocionesPublicController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista promociones públicas vigentes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PromocionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PromocionDto>>>> Get(
        [FromQuery] bool soloVigentes = true,
        [FromQuery] string? ordenarPor = "FechaCreacion",
        [FromQuery] string? direccion = "desc",
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        _logger.LogInformation("🏷️ GET /api/public/promociones");

        var query = new ObtenerPromocionesQuery
        {
            SoloVigentes = soloVigentes,
            OrdenarPor = ordenarPor,
            DireccionOrdenamiento = direccion,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };

        var result = await _mediator.Send(query);
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<PromocionDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error" },
                "Error al obtener promociones");
            return BadRequest(errorResponse);
        }

        return Ok(ApiResponse<List<PromocionDto>>.SuccessResponse(result.Value, "Promociones obtenidas"));
    }
}


