using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Usuarios.Commands.LoginUsuario;
using RestaurantePro.Application.Features.Usuarios.Commands.RegistrarUsuario;
using RestaurantePro.Application.Features.Usuarios.Queries.ObtenerPerfilUsuario;
using RestaurantePro.Application.Features.Usuarios.Queries.ObtenerPerfilUsuarioActual;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<int>> Registrar(RegistrarUsuarioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUsuarioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> ObtenerPerfil(string id)
        {
            var query = new ObtenerPerfilUsuarioQuery { UsuarioId = id };
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult> ObtenerPerfilActual()
        {
            var resultado = await _mediator.Send(new ObtenerPerfilUsuarioActualQuery());
            return Ok(resultado);
        }

        [HttpGet("admin/dashboard")]
        [Authorize(Policy = "RequiereAdministrador")]
        public ActionResult ObtenerEstadisticasAdmin()
        {
            return Ok(new { mensaje = "Datos de administrador cargados correctamente" });
        }

        [HttpGet("gerente/dashboard")]
        [Authorize(Policy = "RequiereGerente")]
        public ActionResult ObtenerEstadisticasGerente()
        {
            return Ok(new { mensaje = "Datos de gerente cargados correctamente" });
        }
    }
} 