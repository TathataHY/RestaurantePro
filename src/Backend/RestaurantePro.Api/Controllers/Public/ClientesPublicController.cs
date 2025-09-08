using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using MediatR;
using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Api.Controllers.Public;

/// <summary>
/// Registro público de nuevos clientes (acceso anónimo)
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/clientes")]
[Produces("application/json")]
public class ClientesPublicController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientesPublicController> _logger;

    public ClientesPublicController(IMediator mediator, ILogger<ClientesPublicController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo cliente público
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClienteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> Registrar([FromBody] PublicClienteRegisterRequest request)
    {
        _logger.LogInformation("🧾 POST /api/public/clientes - Registro público: {Email}", request.Email);

        var command = new CrearClienteCommand
        {
            Nombre = request.Nombre,
            Email = request.Email,
            Telefono = request.Telefono,
            FechaNacimiento = request.FechaNacimiento,
            EstaActivo = true
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" },
                "Error al registrar cliente",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<ClienteDto>.SuccessResponse(result.Value, "Cliente registrado exitosamente");
        return CreatedAtAction(nameof(Registrar), new { id = result.Value.Id }, response);
    }
}

public class PublicClienteRegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}


