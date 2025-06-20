using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.OrdenesCompra.Commands.CrearOrdenCompra;
using RestaurantePro.Application.Features.OrdenesCompra.Commands.ActualizarOrdenCompra;
using RestaurantePro.Application.Features.OrdenesCompra.Commands.EliminarOrdenCompra;
using RestaurantePro.Application.Features.OrdenesCompra.Commands.RecibirOrdenCompra;
using RestaurantePro.Application.Features.OrdenesCompra.Queries.ObtenerOrdenesCompra;
using RestaurantePro.Application.Features.OrdenesCompra.Queries.ObtenerOrdenCompraPorId;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequiereEmpleado")]
    public class OrdenesCompraController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdenesCompraController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerOrdenesCompra([FromQuery] ObtenerOrdenesCompraQuery query)
        {
            var ordenes = await _mediator.Send(query);
            return Ok(ordenes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerOrdenCompraPorId(int id)
        {
            var query = new ObtenerOrdenCompraPorIdQuery { Id = id };
            var orden = await _mediator.Send(query);

            if (orden == null)
            {
                return NotFound();
            }

            return Ok(orden);
        }

        [HttpPost]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> CrearOrdenCompra([FromBody] CrearOrdenCompraCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerOrdenCompraPorId), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ActualizarOrdenCompra(int id, [FromBody] ActualizarOrdenCompraCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID no coincide");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequiereAdministrador")]
        public async Task<IActionResult> EliminarOrdenCompra(int id)
        {
            var command = new EliminarOrdenCompraCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/recibir")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> RecibirOrdenCompra(int id, [FromBody] RecibirOrdenCompraCommand command)
        {
            if (id != command.OrdenCompraId)
            {
                return BadRequest("El ID no coincide");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("pendientes")]
        public async Task<IActionResult> ObtenerOrdenesPendientes()
        {
            var query = new ObtenerOrdenesCompraQuery { EstadoPendiente = true };
            var ordenes = await _mediator.Send(query);
            return Ok(ordenes);
        }

        [HttpGet("proveedor/{proveedorId}")]
        public async Task<IActionResult> ObtenerOrdenesPorProveedor(int proveedorId)
        {
            var query = new ObtenerOrdenesCompraQuery { ProveedorId = proveedorId };
            var ordenes = await _mediator.Send(query);
            return Ok(ordenes);
        }
    }
} 