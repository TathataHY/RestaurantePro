using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Usuarios.Events.Usuario;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Domain.Core.Base.Events.Handlers;

namespace RestaurantePro.Infrastructure.Identity.EventHandlers;

/// <summary>
/// Manejador de eventos que sincroniza automáticamente con Identity cuando se activa un usuario
/// </summary>
public class UsuarioActivado_SincronizarConIdentityHandler : IDomainEventHandler<UsuarioActivado>
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly ILogger<UsuarioActivado_SincronizarConIdentityHandler> _logger;

    public UsuarioActivado_SincronizarConIdentityHandler(
        UserManager<IdentityApplicationUser> userManager,
        ILogger<UsuarioActivado_SincronizarConIdentityHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Maneja el evento de activación de usuario sincronizando automáticamente con Identity
    /// </summary>
    public async Task Handle(UsuarioActivado domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Sincronizando usuario activado con Identity: {UsuarioId}", domainEvent.UsuarioId);

            // Buscar el usuario en Identity por ID del dominio
            // Nota: Aquí necesitamos obtener el email del usuario desde el dominio
            // Para simplificar, vamos a buscar por ID directamente en Identity
            
            var usuarios = _userManager.Users.ToList();
            var usuarioIdentity = usuarios.FirstOrDefault(u => u.Id.ToString() == domainEvent.UsuarioId.ToString());
            
            if (usuarioIdentity == null)
            {
                _logger.LogWarning("⚠️ Usuario {UsuarioId} no encontrado en Identity para sincronización", domainEvent.UsuarioId);
                return;
            }

            // Verificar si necesita actualización
            if (usuarioIdentity.Activo == true)
            {
                _logger.LogInformation("✅ Usuario {UsuarioId} ya está activo en Identity", domainEvent.UsuarioId);
                return;
            }

            // Activar el usuario en Identity
            usuarioIdentity.Activo = true;
            usuarioIdentity.EmailConfirmed = true; // También confirmar el email cuando se activa

            var result = await _userManager.UpdateAsync(usuarioIdentity);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("✅ Usuario {UsuarioId} activado exitosamente en Identity", domainEvent.UsuarioId);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("❌ Error activando usuario {UsuarioId} en Identity: {Errors}", 
                    domainEvent.UsuarioId, errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado sincronizando usuario {UsuarioId} con Identity", 
                domainEvent.UsuarioId);
        }
    }
}
