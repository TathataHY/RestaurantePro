using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Data;
using RestaurantePro.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PlatoController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlatoController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Administrador,Mesero,Cocinero")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plato>>> GetPlatos()
    {
        return await _context.Platos.ToListAsync();
    }

    [Authorize(Roles = "Administrador,Mesero")]
    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<Plato>>> GetPlatosDisponibles()
    {
        return await _context.Platos
            .Where(p => p.Disponible && p.Stock > 0)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Plato>> GetPlato(int id)
    {
        var plato = await _context.Platos.FindAsync(id);

        if (plato == null)
        {
            return NotFound();
        }

        return plato;
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult<Plato>> PostPlato(Plato plato)
    {
        _context.Platos.Add(plato);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlato), new { id = plato.Id }, plato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlato(int id, Plato plato)
    {
        if (id != plato.Id)
        {
            return BadRequest();
        }

        _context.Entry(plato).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlatoExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlato(int id)
    {
        var plato = await _context.Platos.FindAsync(id);
        if (plato == null)
        {
            return NotFound();
        }

        // Verificar si el plato está siendo utilizado en alguna comanda
        var platoEnUso = await _context.ComandaDetalles
            .AnyAsync(cd => cd.PlatoId == id);

        if (platoEnUso)
        {
            return BadRequest("No se puede eliminar el plato porque está siendo utilizado en comandas.");
        }

        _context.Platos.Remove(plato);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Administrador,Cocinero")]
    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int cantidad)
    {
        var plato = await _context.Platos.FindAsync(id);
        if (plato == null)
        {
            return NotFound();
        }

        plato.Stock = cantidad;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PlatoExists(int id)
    {
        return _context.Platos.Any(e => e.Id == id);
    }
} 