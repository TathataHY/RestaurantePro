using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Productos.Commands.ActualizarProducto;
using RestaurantePro.Application.Features.Productos.Commands.CambiarDisponibilidadProducto;
using RestaurantePro.Application.Features.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Features.Productos.Commands.EliminarProducto;
using RestaurantePro.Application.Features.Productos.Queries.ObtenerProductoPorId;
using RestaurantePro.Application.Features.Productos.Queries.ObtenerProductos;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerProductos([FromQuery] ObtenerProductosQuery query)
        {
            var productos = await _mediator.Send(query);
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> ObtenerProductoPorId(int id)
        {
            var query = new ObtenerProductoPorIdQuery { Id = id };
            var producto = await _mediator.Send(query);
            
            if (producto == null)
                return NotFound();
                
            return Ok(producto);
        }

        [HttpPost]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> CrearProducto(CrearProductoCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerProductoPorId), new { id }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> ActualizarProducto(int id, ActualizarProductoCommand command)
        {
            if (id != command.Id)
                return BadRequest();
                
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            var command = new EliminarProductoCommand { Id = id };
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }

        [HttpPatch("{id}/disponibilidad")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> CambiarDisponibilidad(int id, [FromBody] bool disponible)
        {
            var command = new CambiarDisponibilidadProductoCommand
            {
                Id = id,
                Disponible = disponible
            };
            
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }
    }
} 