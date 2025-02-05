using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Core.DTOs.Estadisticas;
using RestaurantePro.Core.DTOs.Plato;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadisticasController : ControllerBase
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly ILogger<EstadisticasController> _logger;

        public EstadisticasController(
            IComandaRepository comandaRepository,
            ILogger<EstadisticasController> logger)
        {
            _comandaRepository = comandaRepository;
            _logger = logger;
        }

        [HttpGet("ventas-diarias")]
        public async Task<ActionResult<List<VentasDiariasDto>>> GetVentasDiarias([FromQuery] DateTime fecha)
        {
            try
            {
                var ventas = await _comandaRepository.GetVentasDiariasAsync(fecha);
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas diarias");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("platos-populares")]
        public async Task<ActionResult<List<PlatoPopularDto>>> GetPlatosPopulares(
            [FromQuery] DateTime desde = default,
            [FromQuery] DateTime hasta = default)
        {
            try
            {
                if (desde == default) desde = DateTime.Today.AddDays(-30);
                if (hasta == default) hasta = DateTime.Today;

                var platos = await _comandaRepository.GetPlatosPopularesAsync(desde, hasta);
                return Ok(platos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener platos populares");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 