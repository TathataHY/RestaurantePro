using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Pagos.Commands.CancelarPago;
using RestaurantePro.Application.Features.Pagos.Commands.RegistrarPago;
using RestaurantePro.Application.Features.Pagos.Queries.ObtenerPagoPorId;
using RestaurantePro.Application.Features.Pagos.Queries.ObtenerPagos;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PagosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerPagos([FromQuery] ObtenerPagosQuery query)
        {
            var pagos = await _mediator.Send(query);
            return Ok(pagos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> ObtenerPagoPorId(int id)
        {
            var query = new ObtenerPagoPorIdQuery { Id = id };
            var pago = await _mediator.Send(query);
            
            if (pago == null)
                return NotFound();
                
            return Ok(pago);
        }

        [HttpPost]
        public async Task<ActionResult> RegistrarPago(RegistrarPagoCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerPagoPorId), new { id }, null);
        }

        [HttpPost("{id}/cancelar")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> CancelarPago(int id, CancelarPagoCommand command)
        {
            if (id != command.Id)
                return BadRequest();
                
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }

        [HttpGet("comanda/{comandaId}")]
        public async Task<ActionResult> ObtenerPagosPorComanda(int comandaId)
        {
            var query = new ObtenerPagosQuery { ComandaId = comandaId };
            var pagos = await _mediator.Send(query);
            return Ok(pagos);
        }
    }
} 