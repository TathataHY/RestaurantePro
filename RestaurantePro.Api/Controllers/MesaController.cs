using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Data;
using RestaurantePro.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Controllers;

[Authorize]  // Requiere autenticación para todos los endpoints
[Route("api/[controller]")]
[ApiController]
public class MesaController : ControllerBase
{
    private readonly AppDbContext _context;

    public MesaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Mesa
    [Authorize(Roles = "Administrador,Mesero")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mesa>>> GetMesas()
    {
        return await _context.Mesas.ToListAsync();
    }

    // GET: api/Mesa/disponibles
    [Authorize(Roles = "Administrador,Mesero")]
    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<Mesa>>> GetMesasDisponibles()
    {
        return await _context.Mesas
            .Where(m => m.Estado == EstadoMesa.Disponible)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Mesa>> GetMesa(int id)
    {
        var mesa = await _context.Mesas.FindAsync(id);

        if (mesa == null)
        {
            return NotFound();
        }

        return mesa;
    }

    // POST: api/Mesa
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult<Mesa>> PostMesa(Mesa mesa)
    {
        _context.Mesas.Add(mesa);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMesa), new { id = mesa.Id }, mesa);
    }

    // PUT: api/Mesa/5
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMesa(int id, Mesa mesa)
    {
        if (id != mesa.Id)
        {
            return BadRequest();
        }

        _context.Entry(mesa).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MesaExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Mesa/5
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMesa(int id)
    {
        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa == null)
        {
            return NotFound();
        }

        // Verificar si la mesa está siendo utilizada en alguna comanda
        var mesaEnUso = await _context.Comandas
            .AnyAsync(c => c.MesaId == id && c.Estado != EstadoComanda.Entregada && c.Estado != EstadoComanda.Cancelada);

        if (mesaEnUso)
        {
            return BadRequest("No se puede eliminar la mesa porque está siendo utilizada en una comanda activa.");
        }

        _context.Mesas.Remove(mesa);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MesaExists(int id)
    {
        return _context.Mesas.Any(e => e.Id == id);
    }
} 