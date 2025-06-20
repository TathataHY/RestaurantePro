using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Categorias.Commands.ActualizarCategoria;
using RestaurantePro.Application.Features.Categorias.Commands.CrearCategoria;
using RestaurantePro.Application.Features.Categorias.Commands.EliminarCategoria;
using RestaurantePro.Application.Features.Categorias.Queries.ObtenerCategoriaPorId;
using RestaurantePro.Application.Features.Categorias.Queries.ObtenerCategorias;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerCategorias([FromQuery] ObtenerCategoriasQuery query)
        {
            var categorias = await _mediator.Send(query);
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> ObtenerCategoriaPorId(int id)
        {
            var query = new ObtenerCategoriaPorIdQuery { Id = id };
            var categoria = await _mediator.Send(query);
            
            if (categoria == null)
                return NotFound();
                
            return Ok(categoria);
        }

        [HttpPost]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> CrearCategoria(CrearCategoriaCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { id }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<ActionResult> ActualizarCategoria(int id, ActualizarCategoriaCommand command)
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
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            var command = new EliminarCategoriaCommand { Id = id };
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound("La categoría no existe o no se puede eliminar porque tiene productos asociados.");
                
            return NoContent();
        }
    }
} 