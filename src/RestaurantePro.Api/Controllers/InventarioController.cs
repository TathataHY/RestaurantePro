using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Inventario.Commands.ActualizarInventario;
using RestaurantePro.Application.Features.Inventario.Commands.AjustarInventario;
using RestaurantePro.Application.Features.Inventario.Commands.RegistrarMovimiento;
using RestaurantePro.Application.Features.Inventario.Queries.ObtenerInventario;
using RestaurantePro.Application.Features.Inventario.Queries.ObtenerInventarioPorId;
using RestaurantePro.Application.Features.Inventario.Queries.ObtenerMovimientos;
using RestaurantePro.Application.Features.Inventario.Queries.ObtenerEstadisticasInventario;
using RestaurantePro.Application.Features.Inventario.Queries.ObtenerSugerenciasCompra;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequiereEmpleado")]
    public class InventarioController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventarioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerInventario([FromQuery] ObtenerInventarioQuery query)
        {
            var inventario = await _mediator.Send(query);
            return Ok(inventario);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerInventarioPorId(int id)
        {
            var query = new ObtenerInventarioPorIdQuery { Id = id };
            var inventario = await _mediator.Send(query);

            if (inventario == null)
            {
                return NotFound();
            }

            return Ok(inventario);
        }

        [HttpPut("{id}/actualizar")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ActualizarInventario(int id, [FromBody] ActualizarInventarioCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID no coincide");
            }

            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpPost("ajustar")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> AjustarInventario([FromBody] AjustarInventarioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpPost("movimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoCommand command)
        {
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpGet("movimientos")]
        public async Task<IActionResult> ObtenerMovimientos([FromQuery] ObtenerMovimientosQuery query)
        {
            var movimientos = await _mediator.Send(query);
            return Ok(movimientos);
        }

        [HttpGet("alertas")]
        public async Task<IActionResult> ObtenerAlertasStock()
        {
            var query = new ObtenerInventarioQuery { SoloConAlertas = true };
            var alertas = await _mediator.Send(query);
            return Ok(alertas);
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ObtenerDashboardInventario([FromQuery] int periodoD = 30)
        {
            var query = new ObtenerEstadisticasInventarioQuery { PeriodoDias = periodoD };
            var estadisticas = await _mediator.Send(query);
            return Ok(estadisticas);
        }

        [HttpGet("sugerencias-compra")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ObtenerSugerenciasCompra([FromQuery] ObtenerSugerenciasCompraQuery query)
        {
            var sugerencias = await _mediator.Send(query);
            return Ok(sugerencias);
        }
    }
} 