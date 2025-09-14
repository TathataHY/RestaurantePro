using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Usuarios.Events.Usuario;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Identity.EventHandlers;

/// <summary>
/// Manejador de eventos que sincroniza automáticamente con Identity cuando se activa un usuario
/// </summary>
public class UsuarioActivado_SincronizarConIdentityHandler : IDomainEventHandler<UsuarioActivado>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UsuarioActivado_SincronizarConIdentityHandler> _logger;
    private readonly RestauranteProDbContext _dbContext;

    public UsuarioActivado_SincronizarConIdentityHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<UsuarioActivado_SincronizarConIdentityHandler> logger,
        RestauranteProDbContext dbContext)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        
    }

    /// <summary>
    /// Maneja el evento de activación de usuario sincronizando automáticamente con Identity
    /// </summary>
    public async Task Handle(UsuarioActivado domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Sincronizando usuario activado con Identity: {UsuarioId}", domainEvent.UsuarioId);

            // Primero obtener el email del usuario desde la base de datos del dominio
            var usuarioDominio = await _dbContext.Usuarios.FindAsync(domainEvent.UsuarioId);
            if (usuarioDominio == null)
            {
                _logger.LogWarning("⚠️ Usuario {UsuarioId} no encontrado en la base de datos del dominio", domainEvent.UsuarioId);
                return;
            }

            _logger.LogInformation("📧 Email del usuario encontrado: {Email}", usuarioDominio.Email);

            // Buscar el usuario en Identity por EMAIL (campo común entre dominio e Identity)
            // El ID del dominio es diferente al ID de Identity, pero el email es el mismo
            
            var usuarioIdentity = await _userManager.FindByEmailAsync(usuarioDominio.Email);
            
            if (usuarioIdentity == null)
            {
                _logger.LogWarning("⚠️ Usuario {Email} no encontrado en Identity para sincronización", usuarioDominio.Email);
                return;
            }

            // Verificar si necesita actualización
            if (usuarioIdentity.Activo == true)
            {
                _logger.LogInformation("✅ Usuario {Email} ya está activo en Identity", usuarioDominio.Email);
                return;
            }

            // Activar el usuario en Identity
            usuarioIdentity.Activo = true;
            usuarioIdentity.EmailConfirmed = true; // También confirmar el email cuando se activa

            var result = await _userManager.UpdateAsync(usuarioIdentity);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("✅ Usuario {Email} activado exitosamente en Identity", usuarioDominio.Email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("❌ Error activando usuario {Email} en Identity: {Errors}", 
                    usuarioDominio.Email, errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado sincronizando usuario {UsuarioId} con Identity", 
                domainEvent.UsuarioId);
        }
    }
}
