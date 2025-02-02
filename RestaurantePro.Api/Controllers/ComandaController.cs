using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.Queries;
using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.Handlers;
using MediatR;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Interfaces;
using AutoMapper;
using RestaurantePro.Core.Exceptions;
using RestaurantePro.Core.Entities;
using RestaurantePro.Infrastructure.Data;

namespace RestaurantePro.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ComandaController : ControllerBase
{
    private readonly RestauranteContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ComandaController(RestauranteContext context, IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _mediator = mediator;
    }

    // GET: api/Comanda
    [Authorize(Roles = "Administrador,Mesero,Cocinero")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Comanda>>> GetComandas()
    {
        return await _context.Comandas
            .Include(c => c.Detalles)
            .Include(c => c.Mesa)
            .Include(c => c.Mesero)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Comanda>> GetComanda(int id)
    {
        var comanda = await _context.Comandas
            .Include(c => c.Detalles)
            .Include(c => c.Mesa)
            .Include(c => c.Mesero)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comanda == null)
        {
            return NotFound();
        }

        return comanda;
    }

    // POST: api/Comanda
    [Authorize(Roles = "Administrador,Mesero")]
    [HttpPost]
    public async Task<ActionResult<ComandaDto>> CreateComanda(CreateComandaCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetComanda), new { id = result.Id }, result);
    }

    // PUT: api/Comanda/{id}
    [Authorize(Roles = "Administrador,Cocinero")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComanda(int id, UpdateComandaCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        try 
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComanda(int id)
    {
        var handler = new DeleteComandaCommandHandler(_unitOfWork);
        await handler.Handle(new DeleteComandaCommand { Id = id });
        return NoContent();
    }

    private bool ComandaExists(int id)
    {
        return _context.Comandas.Any(e => e.Id == id);
    }
}
