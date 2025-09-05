using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Identity.Configuration;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Identity.Services;
using RestaurantePro.Infrastructure.Persistence;
using System;
using System.Text;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    public static class IdentitySetup
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar opciones de JWT
            services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings"));
            services.AddSingleton<IConfigureOptions<JwtConfiguration>, JwtConfigurationSetup>();

            // Configurar Identity desde el archivo de configuración
            services.Configure<IdentityConfiguration>(configuration.GetSection("IdentitySettings"));
            var identitySettings = configuration.GetSection("IdentitySettings").Get<IdentityConfiguration>() ?? new IdentityConfiguration();

            services.AddIdentity<IdentityApplicationUser, ApplicationRole>(options =>
            {
                // Configuración de contraseñas
                options.Password.RequireDigit = identitySettings.PasswordSettings.RequireDigit;
                options.Password.RequireLowercase = identitySettings.PasswordSettings.RequireLowercase;
                options.Password.RequireUppercase = identitySettings.PasswordSettings.RequireUppercase;
                options.Password.RequireNonAlphanumeric = identitySettings.PasswordSettings.RequireNonAlphanumeric;
                options.Password.RequiredLength = identitySettings.PasswordSettings.RequiredLength;
                options.Password.RequiredUniqueChars = identitySettings.PasswordSettings.RequiredUniqueChars;

                // Configuración de bloqueo
                options.Lockout.DefaultLockoutTimeSpan = identitySettings.LockoutSettings.DefaultLockoutTimeSpan;
                options.Lockout.MaxFailedAccessAttempts = identitySettings.LockoutSettings.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = identitySettings.LockoutSettings.AllowedForNewUsers;

                // Configuración de usuario
                options.User.RequireUniqueEmail = identitySettings.UserSettings.RequireUniqueEmail;
                
                // Configuración de SignIn
                options.SignIn.RequireConfirmedAccount = identitySettings.UserSettings.RequireConfirmedAccount;
                options.SignIn.RequireConfirmedEmail = identitySettings.UserSettings.RequireConfirmedEmail;
                options.SignIn.RequireConfirmedPhoneNumber = identitySettings.UserSettings.RequireConfirmedPhoneNumber;
            })
            .AddEntityFrameworkStores<RestauranteProDbContext>()
            .AddDefaultTokenProviders()
            .AddRoles<ApplicationRole>() // Asegurarse que los roles son de tipo ApplicationRole
            .AddRoleManager<RoleManager<ApplicationRole>>()
            .AddRoleValidator<RoleValidator<ApplicationRole>>();

            // Configurar autenticación JWT
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtConfiguration>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };

                // Permitir leer el token desde un header personalizado para no tocar Authorization: Basic
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var tokenFromHeader = context.Request.Headers["X-Bearer-Token"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(tokenFromHeader))
                        {
                            context.Token = tokenFromHeader;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Registrar servicios de Identity
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserPermissionService, PermissionService>();

            return services;
        }
    }
} 