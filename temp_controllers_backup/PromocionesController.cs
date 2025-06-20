using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Features.Promociones.Commands.ActualizarPromocion;
using RestaurantePro.Application.Features.Promociones.Commands.DesactivarPromocion;
using RestaurantePro.Application.Features.Promociones.Queries.ObtenerPromociones;
using RestaurantePro.Application.Features.Promociones.Queries.ObtenerPromocionPorId;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequiereGerente")]
    public class PromocionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PromocionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPromociones([FromQuery] ObtenerPromocionesQuery query)
        {
            var promociones = await _mediator.Send(query);
            return Ok(promociones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPromocionPorId(int id)
        {
            var query = new ObtenerPromocionPorIdQuery { Id = id };
            var promocion = await _mediator.Send(query);

            if (promocion == null)
            {
                return NotFound();
            }

            return Ok(promocion);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPromocion([FromBody] CrearPromocionCommand command)
        {
            var promocionId = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerPromocionPorId), new { id = promocionId }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPromocion(int id, [FromBody] ActualizarPromocionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la promoción no coincide");
            }

            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DesactivarPromocion(int id)
        {
            var command = new DesactivarPromocionCommand { Id = id };
            var resultado = await _mediator.Send(command);

            if (!resultado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("activas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerPromocionesActivas()
        {
            var query = new ObtenerPromocionesQuery { SoloActivas = true };
            var promociones = await _mediator.Send(query);
            return Ok(promociones);
        }

        [HttpGet("cliente/{clienteId}")]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> ObtenerPromocionesParaCliente(int clienteId)
        {
            var query = new ObtenerPromocionesClienteQuery { ClienteId = clienteId };
            var promociones = await _mediator.Send(query);
            return Ok(promociones);
        }
    }
} 