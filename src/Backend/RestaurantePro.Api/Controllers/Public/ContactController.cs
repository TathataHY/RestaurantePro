using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;

namespace RestaurantePro.Api.Controllers.Public;

/// <summary>
/// Controlador público para mensajes de contacto (demo en memoria)
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/contact")]
[Produces("application/json")]
public class ContactController : ControllerBase
{
    private static readonly List<ContactMessageDto> Messages = new();
    private readonly ILogger<ContactController> _logger;

    public ContactController(ILogger<ContactController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista los mensajes recibidos (demo)
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(typeof(ApiResponse<List<ContactMessageDto>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<List<ContactMessageDto>>> GetMessages()
    {
        _logger.LogInformation("📬 GET /api/public/contact/messages");
        var ordered = Messages.OrderByDescending(x => x.Fecha).ToList();
        return Ok(ApiResponse<List<ContactMessageDto>>.SuccessResponse(ordered));
    }

    /// <summary>
    /// Envía un nuevo mensaje de contacto
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ApiResponse<ContactMessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<ContactMessageDto>> Create([FromBody] CreateContactMessageRequest request)
    {
        _logger.LogInformation("📝 POST /api/public/contact/messages - From: {Nombre} <{Email}>", request.Nombre, request.Email);

        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Mensaje))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Nombre, Email y Mensaje son obligatorios" },
                "Validación fallida",
                StatusCodes.Status400BadRequest));
        }

        var msg = new ContactMessageDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Email = request.Email.Trim(),
            Asunto = request.Asunto?.Trim() ?? string.Empty,
            Mensaje = request.Mensaje.Trim(),
            Fecha = DateTime.UtcNow
        };

        Messages.Add(msg);

        var response = ApiResponse<ContactMessageDto>.SuccessResponse(msg, "Mensaje recibido");
        return Created($"/api/public/contact/messages/{msg.Id}", response);
    }

    public class ContactMessageDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class CreateContactMessageRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}


