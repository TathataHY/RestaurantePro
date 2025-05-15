using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Reservaciones.Commands.CancelarReservacion;
using RestaurantePro.Application.Features.Reservaciones.Commands.CompletarReservacion;
using RestaurantePro.Application.Features.Reservaciones.Commands.ConfirmarReservacion;
using RestaurantePro.Application.Features.Reservaciones.Commands.CrearReservacion;
using RestaurantePro.Application.Features.Reservaciones.Commands.MarcarNoShow;
using RestaurantePro.Application.Features.Reservaciones.Commands.MarcarReservacionOcupada;
using RestaurantePro.Application.Features.Reservaciones.Queries.ObtenerReservacionPorId;
using RestaurantePro.Application.Features.Reservaciones.Queries.ObtenerReservaciones;
using System;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservacionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservacionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerReservaciones([FromQuery] ObtenerReservacionesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerReservacionPorId(int id)
        {
            var query = new ObtenerReservacionPorIdQuery { Id = id };
            var reservacion = await _mediator.Send(query);

            if (reservacion == null)
            {
                return NotFound();
            }

            return Ok(reservacion);
        }

        [HttpPost]
        public async Task<IActionResult> CrearReservacion([FromBody] CrearReservacionCommand command)
        {
            var reservacionId = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerReservacionPorId), new { id = reservacionId }, null);
        }

        [HttpPut("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarReservacion(int id, [FromBody] ConfirmarReservacionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarReservacion(int id, [FromBody] CancelarReservacionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/ocupar")]
        public async Task<IActionResult> MarcarComoOcupada(int id, [FromBody] MarcarReservacionOcupadaCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/completar")]
        public async Task<IActionResult> CompletarReservacion(int id, [FromBody] CompletarReservacionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/noshow")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> MarcarNoShow(int id, [FromBody] MarcarNoShowCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
} 