using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Mesas.Commands.ActualizarMesa;
using RestaurantePro.Application.Features.Mesas.Commands.CrearMesa;
using RestaurantePro.Application.Features.Mesas.Queries.ObtenerMesaPorId;
using RestaurantePro.Application.Features.Mesas.Queries.ObtenerMesas;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MesasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MesasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerMesas([FromQuery] ObtenerMesasQuery query)
        {
            var mesas = await _mediator.Send(query);
            return Ok(mesas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> ObtenerMesaPorId(int id)
        {
            var query = new ObtenerMesaPorIdQuery { Id = id };
            var mesa = await _mediator.Send(query);
            
            if (mesa == null)
                return NotFound();
                
            return Ok(mesa);
        }

        [HttpPost]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> CrearMesa(CrearMesaCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerMesaPorId), new { id }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> ActualizarMesa(int id, ActualizarMesaCommand command)
        {
            if (id != command.Id)
                return BadRequest();
                
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }

        [HttpPatch("{id}/estado")]
        [Authorize]
        public async Task<ActionResult> CambiarEstadoMesa(int id, [FromBody] ActualizarMesaCommand command)
        {
            command.Id = id;
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }
    }
} 