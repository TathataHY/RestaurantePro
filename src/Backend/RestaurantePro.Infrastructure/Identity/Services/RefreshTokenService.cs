using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Identity.Services;

/// <summary>
/// Servicio para manejar múltiples refresh tokens por usuario
/// Permite sesiones simultáneas en múltiples dispositivos
/// </summary>
public class RefreshTokenService
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(UserManager<IdentityApplicationUser> userManager, ILogger<RefreshTokenService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo refresh token para un usuario
    /// </summary>
    public async Task<string> CreateRefreshTokenAsync(Guid userId, string? deviceId = null, string? deviceName = null)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("Usuario no encontrado");
            }

            // Generar nuevo refresh token
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = userId,
                ExpiryTime = DateTime.UtcNow.AddDays(7),
                DeviceId = deviceId,
                DeviceName = deviceName,
                IsActive = true
            };

            // Agregar a la colección
            user.RefreshTokens.Add(refreshToken);

            // Limpiar tokens expirados del usuario
            await CleanupExpiredTokensAsync(user);

            // Guardar cambios
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("🔄 Refresh token creado para usuario {UserId}, Device: {DeviceId}", userId, deviceId);
            
            return refreshToken.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando refresh token para usuario {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Valida un refresh token y devuelve el usuario asociado
    /// </summary>
        public async Task<IdentityApplicationUser?> ValidateRefreshTokenAsync(string token)
    {
        try
        {
            var refreshToken = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .Where(u => u.RefreshTokens.Any(rt => rt.Token == token && rt.IsActive))
                .SelectMany(u => u.RefreshTokens)
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsActive);

            if (refreshToken == null)
            {
                _logger.LogWarning("❌ Refresh token no encontrado o inactivo: {Token}", token);
                return null;
            }

            if (refreshToken.ExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("❌ Refresh token expirado: {Token}, Expiry: {ExpiryTime}", token, refreshToken.ExpiryTime);
                // Marcar como inactivo
                refreshToken.IsActive = false;
                await _userManager.UpdateAsync(refreshToken.User);
                return null;
            }

            _logger.LogInformation("✅ Refresh token válido para usuario {UserId}", refreshToken.UserId);
            return refreshToken.User;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando refresh token: {Token}", token);
            return null;
        }
    }

    /// <summary>
    /// Revoca un refresh token específico
    /// </summary>
    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        try
        {
            var refreshToken = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .Where(u => u.RefreshTokens.Any(rt => rt.Token == token))
                .SelectMany(u => u.RefreshTokens)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
            {
                return false;
            }

            refreshToken.IsActive = false;
            await _userManager.UpdateAsync(refreshToken.User);

            _logger.LogInformation("🔄 Refresh token revocado: {Token}", token);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revocando refresh token: {Token}", token);
            return false;
        }
    }

    /// <summary>
    /// Revoca todos los refresh tokens de un usuario
    /// </summary>
    public async Task<bool> RevokeAllRefreshTokensAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
            {
                token.IsActive = false;
            }

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("🔄 Todos los refresh tokens revocados para usuario {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revocando todos los refresh tokens para usuario {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Limpia tokens expirados de un usuario
    /// </summary>
    private async Task CleanupExpiredTokensAsync(IdentityApplicationUser user)
    {
        var expiredTokens = user.RefreshTokens
            .Where(rt => rt.ExpiryTime <= DateTime.UtcNow)
            .ToList();

        foreach (var token in expiredTokens)
        {
            token.IsActive = false;
        }

        if (expiredTokens.Any())
        {
            _logger.LogInformation("🧹 Limpiando {Count} tokens expirados para usuario {UserId}", 
                expiredTokens.Count, user.Id);
        }
    }

    /// <summary>
    /// Obtiene información de tokens activos de un usuario
    /// </summary>
    public async Task<List<RefreshToken>> GetActiveTokensAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new List<RefreshToken>();
            }

            return user.RefreshTokens
                .Where(rt => rt.IsActive && rt.ExpiryTime > DateTime.UtcNow)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo tokens activos para usuario {UserId}", userId);
            return new List<RefreshToken>();
        }
    }
}
