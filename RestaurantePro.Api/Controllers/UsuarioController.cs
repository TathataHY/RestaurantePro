using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Data;
using RestaurantePro.Api.Models;
using RestaurantePro.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public UsuarioController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
    {
        return await _context.Usuarios.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        return usuario;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && 
                                    u.Contraseña == request.Contraseña);

        if (usuario == null)
        {
            return NotFound("Usuario o contraseña incorrectos");
        }

        if (!usuario.Activo)
        {
            return BadRequest("Usuario inactivo");
        }

        var token = _tokenService.GenerateToken(usuario);

        return new LoginResponse
        {
            Token = token,
            Usuario = usuario
        };
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
    {
        var existingUser = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == usuario.NombreUsuario);

        if (existingUser != null)
        {
            return BadRequest("El nombre de usuario ya está en uso");
        }

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
    {
        if (id != usuario.Id)
        {
            return BadRequest();
        }

        _context.Entry(usuario).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UsuarioExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        // Verificar si el usuario es el último administrador
        if (usuario.Rol == RolUsuario.Administrador)
        {
            var adminCount = await _context.Usuarios
                .CountAsync(u => u.Rol == RolUsuario.Administrador && u.Id != id);

            if (adminCount == 0)
            {
                return BadRequest("No se puede eliminar el último usuario administrador");
            }
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeDefaultUsers()
    {
        // Crear usuario administrador por defecto
        var adminExists = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == "admin");
        if (!adminExists)
        {
            var defaultAdmin = new Usuario
            {
                Nombre = "Administrador",
                NombreUsuario = "admin",
                Contraseña = "admin123",
                Rol = RolUsuario.Administrador,
                Activo = true
            };
            _context.Usuarios.Add(defaultAdmin);
        }

        // Crear usuario mesero por defecto
        var meseroExists = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == "mesero");
        if (!meseroExists)
        {
            var defaultMesero = new Usuario
            {
                Nombre = "Mesero",
                NombreUsuario = "mesero",
                Contraseña = "mesero123",
                Rol = RolUsuario.Mesero,
                Activo = true
            };
            _context.Usuarios.Add(defaultMesero);
        }

        // Crear usuario cocinero por defecto
        var cocineroExists = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == "cocinero");
        if (!cocineroExists)
        {
            var defaultCocinero = new Usuario
            {
                Nombre = "Cocinero",
                NombreUsuario = "cocinero",
                Contraseña = "cocinero123",
                Rol = RolUsuario.Cocinero,
                Activo = true
            };
            _context.Usuarios.Add(defaultCocinero);
        }

        await _context.SaveChangesAsync();
        return Ok("Usuarios por defecto creados exitosamente");
    }

    [HttpGet("authorized")]
    public async Task<ActionResult<bool>> IsUserAuthorized([FromQuery] string nombreUsuario, [FromQuery] RolUsuario[] roles)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

        if (usuario == null)
        {
            return false;
        }

        return roles.Contains(usuario.Rol);
    }

    private bool UsuarioExists(int id)
    {
        return _context.Usuarios.Any(e => e.Id == id);
    }
}

public class LoginRequest
{
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; }
    public Usuario Usuario { get; set; }
} 