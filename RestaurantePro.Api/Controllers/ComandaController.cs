using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Data;
using RestaurantePro.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ComandaController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComandaController(AppDbContext context)
    {
        _context = context;
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
    public async Task<ActionResult<Comanda>> PostComanda(Comanda comanda)
    {
        _context.Comandas.Add(comanda);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetComanda), new { id = comanda.Id }, comanda);
    }

    // PUT: api/Comanda/{id}/estado
    [Authorize(Roles = "Administrador,Cocinero")]
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] EstadoComandaRequest estado)
    {
        var comanda = await _context.Comandas.FindAsync(id);
        if (comanda == null)
        {
            return NotFound();
        }

        comanda.Estado = estado.Estado;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    public class EstadoComandaRequest
    {
        public EstadoComanda Estado { get; set; }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComanda(int id)
    {
        var comanda = await _context.Comandas.FindAsync(id);
        if (comanda == null)
        {
            return NotFound();
        }

        _context.Comandas.Remove(comanda);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ComandaExists(int id)
    {
        return _context.Comandas.Any(e => e.Id == id);
    }
}
