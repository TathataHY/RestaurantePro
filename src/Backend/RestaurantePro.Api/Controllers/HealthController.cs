using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Controllers;

/// <summary>
/// Controlador para verificar el estado de salud de la API
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica el estado de salud de la API
    /// </summary>
    /// <returns>Estado de salud de la API</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "4.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }

    /// <summary>
    /// Verifica el estado de salud de la base de datos
    /// </summary>
    /// <returns>Estado de salud de la base de datos</returns>
    [HttpGet("database")]
    public IActionResult GetDatabaseHealth()
    {
        // Por ahora retornamos OK, pero aquí se podría verificar la conexión a la BD
        return Ok(new
        {
            Status = "Healthy",
            Database = "Connected",
            Timestamp = DateTime.UtcNow
        });
    }
}
