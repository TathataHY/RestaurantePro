using Microsoft.AspNetCore.Identity;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Identity.Models;

namespace RestaurantePro.Infrastructure.Identity.Services;

/// <summary>
/// Implementación del servicio de usuarios de Identity
/// </summary>
public class IdentityUserService : IIdentityUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityUserService> _logger;

    public IdentityUserService(UserManager<ApplicationUser> userManager, ILogger<IdentityUserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> CreateUserAsync(
        string userName, 
        string email, 
        string nombre, 
        string apellidos, 
        string password, 
        string rol, 
        bool activo = true, 
        bool emailConfirmed = true)
    {
        try
        {
            // Verificar si el usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"El usuario con email {email} ya existe");
            }

            // Crear usuario en Identity
            var identityUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                Nombre = nombre,
                Apellidos = apellidos,
                FechaCreacion = DateTime.UtcNow,
                Activo = activo,
                EmailConfirmed = emailConfirmed,
                FotoPerfil = "",
                RefreshToken = ""
            };

            var result = await _userManager.CreateAsync(identityUser, password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("❌ Error creando usuario {Username} en Identity: {Errors}", userName, errors);
                throw new InvalidOperationException($"Error creando usuario en Identity: {errors}");
            }

            // Asignar rol
            var roleResult = await _userManager.AddToRoleAsync(identityUser, rol);
            
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("❌ Error asignando rol {Role} al usuario {Username}: {Errors}", rol, userName, errors);
                // No lanzar excepción aquí, el usuario ya se creó
            }

            _logger.LogInformation("✅ Usuario {Username} creado exitosamente en Identity con rol {Role}", userName, rol);
            return identityUser.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado creando usuario {Username} en Identity", userName);
            throw;
        }
    }

    public async Task<bool> VerifyPasswordAsync(string userName, string password)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return false;
            }

            return await _userManager.CheckPasswordAsync(user, password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error verificando contraseña para usuario {Username}", userName);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userName, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("⚠️ Usuario {Username} no encontrado para cambiar contraseña", userName);
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("❌ Error cambiando contraseña para usuario {Username}: {Errors}", userName, errors);
                return false;
            }

            _logger.LogInformation("✅ Contraseña cambiada exitosamente para usuario {Username}", userName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado cambiando contraseña para usuario {Username}", userName);
            return false;
        }
    }
}
