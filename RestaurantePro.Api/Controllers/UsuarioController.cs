using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Core.Identity;
using RestaurantePro.Core.Enums;
using RestaurantePro.Infrastructure.Services;

namespace RestaurantePro.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;

    public UsuarioController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsuarios()
    {
        return await _userManager.Users.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationUser>> GetUsuario(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        return usuario;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.NombreUsuario);
        if (user == null || !user.IsActive)
        {
            return Unauthorized("Usuario no encontrado o inactivo");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Contraseña, false);
        if (!result.Succeeded)
        {
            return Unauthorized("Contraseña incorrecta");
        }

        var token = _tokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            Usuario = new UserResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                NombreUsuario = user.UserName,
                Rol = user.Rol,
                IsActive = user.IsActive
            }
        };
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult<ApplicationUser>> PostUsuario(ApplicationUser usuario)
    {
        var existingUser = await _userManager.FindByNameAsync(usuario.UserName);

        if (existingUser != null)
        {
            return BadRequest("El nombre de usuario ya está en uso");
        }

        await _userManager.CreateAsync(usuario, "Admin123!");

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUsuario(string id, ApplicationUser usuario)
    {
        if (id != usuario.Id)
        {
            return BadRequest();
        }

        await _userManager.UpdateAsync(usuario);

        try
        {
            await _userManager.UpdateAsync(usuario);
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
    public async Task<IActionResult> DeleteUsuario(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        // Verificar si el usuario es el último administrador
        if (usuario.Rol == RolUsuario.Administrador)
        {
            var adminCount = await _userManager.Users.CountAsync(u => u.Rol == RolUsuario.Administrador && u.Id != id);

            if (adminCount == 0)
            {
                return BadRequest("No se puede eliminar el último usuario administrador");
            }
        }

        await _userManager.DeleteAsync(usuario);

        return NoContent();
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeDefaultUsers()
    {
        // Crear usuario administrador por defecto
        if (await _userManager.FindByNameAsync("admin") == null)
        {
            var defaultAdmin = new ApplicationUser
            {
                UserName = "admin",
                Nombre = "Administrador",
                Apellido = "Sistema",
                Email = "admin@restaurante.com",
                Rol = RolUsuario.Administrador,
                IsActive = true
            };
            await _userManager.CreateAsync(defaultAdmin, "Admin123!");
        }

        // Crear usuario mesero por defecto
        if (await _userManager.FindByNameAsync("mesero") == null)
        {
            var defaultMesero = new ApplicationUser
            {
                UserName = "mesero",
                Nombre = "Mesero",
                Apellido = "Sistema",
                Email = "mesero@restaurante.com",
                Rol = RolUsuario.Mesero,
                IsActive = true
            };
            await _userManager.CreateAsync(defaultMesero, "Mesero123!");
        }

        // Crear usuario cocinero por defecto
        if (await _userManager.FindByNameAsync("cocinero") == null)
        {
            var defaultCocinero = new ApplicationUser
            {
                UserName = "cocinero",
                Nombre = "Cocinero",
                Apellido = "Sistema",
                Email = "cocinero@restaurante.com",
                Rol = RolUsuario.Cocinero,
                IsActive = true
            };
            await _userManager.CreateAsync(defaultCocinero, "Cocinero123!");
        }

        return Ok("Usuarios por defecto creados exitosamente");
    }

    [HttpGet("authorized")]
    public async Task<ActionResult<bool>> IsUserAuthorized([FromQuery] string nombreUsuario, [FromQuery] RolUsuario[] roles)
    {
        var usuario = await _userManager.FindByNameAsync(nombreUsuario);

        if (usuario == null)
        {
            return false;
        }

        return roles.Contains(usuario.Rol);
    }

    private bool UsuarioExists(string id)
    {
        return _userManager.Users.Any(e => e.Id == id);
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
    public UserResponse Usuario { get; set; }
}

public class UserResponse
{
    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string NombreUsuario { get; set; }
    public RolUsuario Rol { get; set; }
    public bool IsActive { get; set; }
} 