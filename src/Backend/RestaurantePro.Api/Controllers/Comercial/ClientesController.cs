using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Comercial.Clientes.Commands.Create;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Comercial.Clientes.Queries.GetById;

namespace RestaurantePro.Api.Controllers.Comercial
{
    [ApiController]
    [Route("api/comercial/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>Información del cliente</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteDto>> GetById(int id)
        {
            var query = new GetClienteByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound();
                
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo cliente
        /// </summary>
        /// <param name="command">Datos del cliente a crear</param>
        /// <returns>ID del cliente creado</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Create(CreateClienteCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result }, result);
        }

        /// <summary>
        /// Actualiza los datos de un cliente existente
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="command">Datos actualizados del cliente</param>
        /// <returns>Sin contenido si tiene éxito</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, UpdateClienteCommand command)
        {
            if (id != command.Id)
                return BadRequest();
                
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Elimina un cliente
        /// </summary>
        /// <param name="id">ID del cliente a eliminar</param>
        /// <returns>Sin contenido si tiene éxito</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteClienteCommand { Id = id });
            return NoContent();
        }
    }
} 