using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RestaurantePro.Infrastructure.Identity.Extensions
{
    /// <summary>
    /// Extensiones para ClaimsPrincipal para facilitar el acceso a claims comunes
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Obtiene el ID del usuario desde los claims
        /// </summary>
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
        }

        /// <summary>
        /// Obtiene el nombre completo del usuario desde los claims
        /// </summary>
        public static string GetFullName(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var firstName = principal.FindFirst("FirstName")?.Value;
            var lastName = principal.FindFirst("LastName")?.Value;

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                return principal.Identity?.Name;

            return $"{firstName} {lastName}".Trim();
        }

        /// <summary>
        /// Obtiene el email del usuario desde los claims
        /// </summary>
        public static string GetEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Verifica si el usuario tiene un rol específico
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal principal, string role)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.IsInRole(role);
        }

        /// <summary>
        /// Obtiene todos los roles del usuario desde los claims
        /// </summary>
        public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
        }

        /// <summary>
        /// Verifica si el usuario tiene un permiso específico
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.Claims
                .Any(c => c.Type == "Permission" && c.Value == permission);
        }

        /// <summary>
        /// Obtiene todos los permisos del usuario desde los claims
        /// </summary>
        public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value);
        }
    }
} 