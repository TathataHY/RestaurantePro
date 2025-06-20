using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Proveedores.Commands.CrearProveedor;
using RestaurantePro.Application.Features.Proveedores.Commands.ActualizarProveedor;
using RestaurantePro.Application.Features.Proveedores.Commands.EliminarProveedor;
using RestaurantePro.Application.Features.Proveedores.Queries.ObtenerProveedores;
using RestaurantePro.Application.Features.Proveedores.Queries.ObtenerProveedorPorId;
using RestaurantePro.Application.Features.Proveedores.Queries.ObtenerProveedoresPorCategoria;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequiereEmpleado")]
    public class ProveedoresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProveedoresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProveedores([FromQuery] ObtenerProveedoresQuery query)
        {
            var proveedores = await _mediator.Send(query);
            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerProveedorPorId(int id)
        {
            var query = new ObtenerProveedorPorIdQuery { Id = id };
            var proveedor = await _mediator.Send(query);

            if (proveedor == null)
            {
                return NotFound();
            }

            return Ok(proveedor);
        }

        [HttpGet("categoria/{categoriaId}")]
        public async Task<IActionResult> ObtenerProveedoresPorCategoria(int categoriaId)
        {
            var query = new ObtenerProveedoresPorCategoriaQuery { CategoriaId = categoriaId };
            var proveedores = await _mediator.Send(query);
            return Ok(proveedores);
        }

        [HttpPost]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> CrearProveedor([FromBody] CrearProveedorCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerProveedorPorId), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] ActualizarProveedorCommand command)
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
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var command = new EliminarProveedorCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("{id}/ingredientes")]
        public async Task<IActionResult> ObtenerIngredientesDeProveedor(int id)
        {
            var query = new ObtenerProveedorPorIdQuery { Id = id, IncluirIngredientes = true };
            var proveedor = await _mediator.Send(query);

            if (proveedor == null)
            {
                return NotFound();
            }

            return Ok(proveedor.Ingredientes);
        }
    }
} 