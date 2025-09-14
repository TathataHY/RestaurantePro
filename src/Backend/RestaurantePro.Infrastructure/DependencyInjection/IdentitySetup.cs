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
using RestaurantePro.Infrastructure.Identity.EventHandlers;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using RestaurantePro.Domain.Core.Usuarios.Events.Usuario;
using RestaurantePro.Infrastructure.Persistence;
using System;
using System.Text;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    public static class IdentitySetup
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Log para confirmar que se está ejecutando
            Console.WriteLine("🚀 [IDENTITY_SETUP] Iniciando configuración de Identity...");
            Console.WriteLine("🚀 [IDENTITY_SETUP] Iniciando configuración de Identity...");
            Console.WriteLine("🚀 [IDENTITY_SETUP] Iniciando configuración de Identity...");
            
            // Log adicional para debugging
            System.Diagnostics.Debug.WriteLine("🚀 [IDENTITY_SETUP] DEBUG: Iniciando configuración de Identity...");
            System.Console.Error.WriteLine("🚀 [IDENTITY_SETUP] ERROR: Iniciando configuración de Identity...");
            
            // Log extremo para debugging
            System.Console.Out.WriteLine("🚀 [IDENTITY_SETUP] OUT: Iniciando configuración de Identity...");
            System.Console.Error.WriteLine("🚀 [IDENTITY_SETUP] ERROR: Iniciando configuración de Identity...");
            System.Console.Out.WriteLine("🚀 [IDENTITY_SETUP] OUT: Iniciando configuración de Identity...");
            
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

            // Registrar manejadores de eventos de Identity
            services.AddScoped<IDomainEventHandler<UsuarioActivado>, UsuarioActivado_SincronizarConIdentityHandler>();
            services.AddScoped<IDomainEventHandler<UsuarioCreado>, UsuarioCreado_SincronizarConIdentityHandler>();
            
            // Registrar explícitamente el logger específico para los manejadores
            services.AddScoped<ILogger<UsuarioActivado_SincronizarConIdentityHandler>>(provider =>
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<UsuarioActivado_SincronizarConIdentityHandler>());
            services.AddScoped<ILogger<UsuarioCreado_SincronizarConIdentityHandler>>(provider =>
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<UsuarioCreado_SincronizarConIdentityHandler>());
            
            // Log para confirmar el registro
            Console.WriteLine("🔧 [IDENTITY_SETUP] UsuarioActivado_SincronizarConIdentityHandler registrado en el contenedor de dependencias");
            Console.WriteLine("🔧 [IDENTITY_SETUP] UsuarioCreado_SincronizarConIdentityHandler registrado en el contenedor de dependencias");
            Console.WriteLine("🔧 [IDENTITY_SETUP] ILogger<UsuarioActivado_SincronizarConIdentityHandler> registrado explícitamente");
            Console.WriteLine("🔧 [IDENTITY_SETUP] ILogger<UsuarioCreado_SincronizarConIdentityHandler> registrado explícitamente");
            
            // Verificar que las dependencias del manejador estén registradas
            var userManagerDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(UserManager<IdentityApplicationUser>));
            if (userManagerDescriptor != null)
            {
                Console.WriteLine($"✅ [IDENTITY_SETUP] UserManager<IdentityApplicationUser> encontrado: {userManagerDescriptor.ServiceType.Name}");
            }
            else
            {
                Console.WriteLine("❌ [IDENTITY_SETUP] UserManager<IdentityApplicationUser> NO encontrado");
            }
            
            var loggerDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ILogger<UsuarioActivado_SincronizarConIdentityHandler>));
            if (loggerDescriptor != null)
            {
                Console.WriteLine($"✅ [IDENTITY_SETUP] ILogger<UsuarioActivado_SincronizarConIdentityHandler> encontrado: {loggerDescriptor.ServiceType.Name}");
            }
            else
            {
                Console.WriteLine("❌ [IDENTITY_SETUP] ILogger<UsuarioActivado_SincronizarConIdentityHandler> NO encontrado");
            }
            
            // Verificar que el manejador se registró correctamente
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(UsuarioActivado_SincronizarConIdentityHandler));
            if (serviceDescriptor != null)
            {
                Console.WriteLine($"✅ [IDENTITY_SETUP] Manejador encontrado en servicios: {serviceDescriptor.ServiceType.Name}");
                Console.WriteLine($"✅ [IDENTITY_SETUP] Lifetime: {serviceDescriptor.Lifetime}");
            }
            else
            {
                Console.WriteLine("❌ [IDENTITY_SETUP] Manejador NO encontrado en servicios");
            }

            // Log para confirmar que se completó la configuración
            Console.WriteLine("✅ [IDENTITY_SETUP] Configuración de Identity completada exitosamente");
            Console.WriteLine("✅ [IDENTITY_SETUP] Configuración de Identity completada exitosamente");
            Console.WriteLine("✅ [IDENTITY_SETUP] Configuración de Identity completada exitosamente");
            
            // Log extremo para debugging
            System.Console.Out.WriteLine("✅ [IDENTITY_SETUP] OUT: Configuración de Identity completada exitosamente");
            System.Console.Error.WriteLine("✅ [IDENTITY_SETUP] ERROR: Configuración de Identity completada exitosamente");
            System.Console.Out.WriteLine("✅ [IDENTITY_SETUP] OUT: Configuración de Identity completada exitosamente");
            
            return services;
        }
    }
} 