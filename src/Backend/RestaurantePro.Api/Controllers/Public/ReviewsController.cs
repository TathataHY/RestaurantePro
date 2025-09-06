using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;

namespace RestaurantePro.Api.Controllers.Public;

/// <summary>
/// Controlador público para reseñas de clientes (demo en memoria)
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/reviews")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private static readonly List<ReviewDto> Reviews = new()
    {
        new ReviewDto
        {
            Id = Guid.NewGuid(),
            Nombre = "María",
            Comentario = "Excelente atención y sabores auténticos.",
            Calificacion = 5,
            Fecha = DateTime.UtcNow.AddDays(-1)
        }
    };

    /// <summary>
    /// Obtiene todas las reseñas públicas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ReviewDto>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<List<ReviewDto>>> Get()
    {
        var ordered = Reviews.OrderByDescending(r => r.Fecha).ToList();
        return Ok(ApiResponse<List<ReviewDto>>.SuccessResponse(ordered, "Reseñas obtenidas"));
    }

    /// <summary>
    /// Crea una nueva reseña pública
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<ReviewDto>> Post([FromBody] CrearReviewRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Comentario))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Datos inválidos", "Nombre y comentario son obligatorios"));
        }
        if (request.Calificacion < 1 || request.Calificacion > 5)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Calificación fuera de rango", "Calificación debe ser entre 1 y 5"));
        }

        var review = new ReviewDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Comentario = request.Comentario.Trim(),
            Calificacion = request.Calificacion,
            Fecha = DateTime.UtcNow
        };
        Reviews.Add(review);
        var response = ApiResponse<ReviewDto>.SuccessResponse(review, "Reseña creada");
        return CreatedAtAction(nameof(Get), new { id = review.Id }, response);
    }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public DateTime Fecha { get; set; }
}

public class CrearReviewRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Calificacion { get; set; }
}


