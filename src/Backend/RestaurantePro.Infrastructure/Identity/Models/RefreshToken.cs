using Microsoft.AspNetCore.Identity;

namespace RestaurantePro.Infrastructure.Identity.Models;

/// <summary>
/// Entidad para manejar múltiples refresh tokens por usuario
/// Permite que un usuario tenga sesiones activas en múltiples dispositivos
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? DeviceId { get; set; } // Identificador del dispositivo
    public string? DeviceName { get; set; } // Nombre del dispositivo (opcional)
    public bool IsActive { get; set; } = true;
    
    // Navegación
    public virtual IdentityApplicationUser User { get; set; } = null!;
}
