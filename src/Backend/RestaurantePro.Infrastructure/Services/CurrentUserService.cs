using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Identity.Extensions;
using System.Security.Claims;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de usuario actual basado en HttpContext
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// ID del usuario actual
    /// </summary>
    public string? UserId => _httpContextAccessor.HttpContext?.User?.GetUserId();

    /// <summary>
    /// Nombre del usuario actual
    /// </summary>
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    /// <summary>
    /// Email del usuario actual
    /// </summary>
    public string? Email => _httpContextAccessor.HttpContext?.User?.GetEmail();

    /// <summary>
    /// Indica si el usuario está autenticado
    /// </summary>
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Roles del usuario actual
    /// </summary>
    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value) ?? Enumerable.Empty<string>();

    /// <summary>
    /// Rol principal del usuario actual (primer rol o null si no tiene roles)
    /// </summary>
    public string? Rol => Roles.FirstOrDefault();

    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    /// <param name="role">Rol a verificar</param>
    /// <returns>True si tiene el rol</returns>
    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
} 