using System;
using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.Identity.Configuration
{
    /// <summary>
    /// Configuración para Identity
    /// </summary>
    public class IdentityConfiguration
    {
        /// <summary>
        /// Configuración de contraseñas
        /// </summary>
        public PasswordSettings PasswordSettings { get; set; } = new PasswordSettings();

        /// <summary>
        /// Configuración de bloqueo de cuentas
        /// </summary>
        public LockoutSettings LockoutSettings { get; set; } = new LockoutSettings();

        /// <summary>
        /// Configuración de usuarios
        /// </summary>
        public UserSettings UserSettings { get; set; } = new UserSettings();

        /// <summary>
        /// Configuración de cookies
        /// </summary>
        public CookieSettings CookieSettings { get; set; } = new CookieSettings();

        /// <summary>
        /// Roles predeterminados del sistema
        /// </summary>
        public List<DefaultRole> DefaultRoles { get; set; } = new List<DefaultRole>();
    }

    /// <summary>
    /// Configuración de contraseñas
    /// </summary>
    public class PasswordSettings
    {
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public int RequiredLength { get; set; } = 8;
        public int RequiredUniqueChars { get; set; } = 1;
    }

    /// <summary>
    /// Configuración de bloqueo de cuentas
    /// </summary>
    public class LockoutSettings
    {
        public bool AllowedForNewUsers { get; set; } = true;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Configuración de usuarios
    /// </summary>
    public class UserSettings
    {
        public bool RequireUniqueEmail { get; set; } = true;
        public bool RequireConfirmedEmail { get; set; } = true;
        public bool RequireConfirmedPhoneNumber { get; set; } = false;
        public bool RequireConfirmedAccount { get; set; } = true;
    }

    /// <summary>
    /// Configuración de cookies
    /// </summary>
    public class CookieSettings
    {
        public string ApplicationCookie { get; set; } = "RestaurantePro.Identity";
        public bool HttpOnly { get; set; } = true;
        public bool SecurePolicy { get; set; } = true;
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromDays(14);
        public string LoginPath { get; set; } = "/Account/Login";
        public string LogoutPath { get; set; } = "/Account/Logout";
        public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";
    }

    /// <summary>
    /// Rol predeterminado del sistema
    /// </summary>
    public class DefaultRole
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; } = true;
        public List<string> Permissions { get; set; } = new List<string>();
    }
} 