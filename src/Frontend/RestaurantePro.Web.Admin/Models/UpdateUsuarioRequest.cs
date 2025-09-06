using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

public class UpdateUsuarioRequest
{
    public Guid Id { get; set; }

    [StringLength(100)]
    public string? NombreCompleto { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? Rol { get; set; }
}


