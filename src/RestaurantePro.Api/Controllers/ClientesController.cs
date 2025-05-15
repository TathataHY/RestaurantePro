using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Clientes.Commands.RegistrarCliente;
using RestaurantePro.Application.Features.Clientes.Queries.ObtenerClientes;
using RestaurantePro.Application.Features.Clientes.Queries.ObtenerClientePorId;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> ObtenerClientes([FromQuery] ObtenerClientesQuery query)
        {
            var clientes = await _mediator.Send(query);
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> ObtenerClientePorId(int id)
        {
            var query = new ObtenerClientePorIdQuery { Id = id };
            var cliente = await _mediator.Send(query);

            if (cliente == null)
            {
                return NotFound();
            }

            return Ok(cliente);
        }

        [HttpPost]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> RegistrarCliente([FromBody] RegistrarClienteCommand command)
        {
            var clienteId = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerClientePorId), new { id = clienteId }, null);
        }

        [HttpGet("{id}/tarjeta")]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> ObtenerTarjetaCliente(int id)
        {
            var query = new ObtenerTarjetaClienteQuery { ClienteId = id };
            var tarjeta = await _mediator.Send(query);

            if (tarjeta == null)
            {
                return NotFound("El cliente no tiene tarjeta de fidelización");
            }

            return Ok(tarjeta);
        }

        [HttpPost("{id}/activar-tarjeta")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ActivarTarjeta(int id)
        {
            var command = new ActivarTarjetaCommand { ClienteId = id };
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpPost("{id}/acumular-puntos")]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> AcumularPuntos(int id, [FromBody] AcumularPuntosCommand command)
        {
            if (id != command.ClienteId)
            {
                return BadRequest("El ID del cliente no coincide");
            }

            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpPost("{id}/canjear-puntos")]
        [Authorize(Policy = "RequiereEmpleado")]
        public async Task<IActionResult> CanjearPuntos(int id, [FromBody] CanjearPuntosCommand command)
        {
            if (id != command.ClienteId)
            {
                return BadRequest("El ID del cliente no coincide");
            }

            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }
    }
} 