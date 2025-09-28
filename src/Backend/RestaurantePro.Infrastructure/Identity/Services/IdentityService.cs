using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Infrastructure.Identity.Extensions;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityApplicationUser> _userManager;
        private readonly SignInManager<IdentityApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<IdentityService> _logger;
        private readonly CoreDbContext _coreDbContext;
        private readonly RefreshTokenService _refreshTokenService;

        public IdentityService(
            UserManager<IdentityApplicationUser> userManager,
            SignInManager<IdentityApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtTokenService jwtTokenService,
            ILogger<IdentityService> logger,
            CoreDbContext coreDbContext,
            RefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _coreDbContext = coreDbContext;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol)
        {
            // Validar email duplicado
            var existingUserByEmail = await _userManager.FindByEmailAsync(email);
            if (existingUserByEmail != null)
            {
                return Result.Failure<string>(new List<string> { "El email ya está en uso" });
            }

            // Validar username duplicado
            var existingUserByUsername = await _userManager.FindByNameAsync(username);
            if (existingUserByUsername != null)
            {
                return Result.Failure<string>(new List<string> { "El nombre de usuario ya está en uso" });
            }

            // Validar que el rol existe
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                return Result.Failure<string>(new List<string> { $"El rol '{rol}' no existe" });
            }

            var user = new IdentityApplicationUser
            {
                UserName = username,
                Email = email,
                Nombre = nombre,
                Apellidos = apellidos,
                FechaCreacion = DateTime.UtcNow,
                Activo = true,
                FotoPerfil = "",
                // RefreshToken removido - ahora se maneja en tabla separada
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return Result.Failure<string>(result.GetErrors().ToList());
            }

            result = await _userManager.AddToRoleAsync(user, rol);
            if (!result.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return Result.Failure<string>(result.GetErrors().ToList());
            }

            return Result.Success<string>(user.Id.ToString());
        }

        public async Task<Result> CreateRoleAsync(string roleName, string description, bool isSystemRole)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return Result.Failure($"El rol '{roleName}' ya existe.");
            }

            var newRole = new ApplicationRole(roleName)
            {
                Description = description,
                IsSystemRole = isSystemRole,
                CreatedOn = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(newRole);
            return result.ToResult();
        }

        public async Task<Result> AddUserToRoleAsync(string userId, string roleName)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure(new List<string> { "ID de usuario inválido." });
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado." });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return Result.Failure(new List<string> { $"El rol '{roleName}' no existe." });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.ToResult();
        }

        public async Task<Result> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure(new List<string> { "ID de usuario inválido." });
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado." });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return Result.Failure(new List<string> { $"El rol '{roleName}' no existe." });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.ToResult();
        }

        public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
        {
            var user = new IdentityApplicationUser
            {
                UserName = userName,
                Email = email,
                FotoPerfil = "",
                // RefreshToken removido - ahora se maneja en tabla separada
            };
            var result = await _userManager.CreateAsync(user, password);
            return (result.ToResult(), user.Id.ToString());
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var authResult = await AuthenticateAsync(email, password);
            if (!authResult.Succeeded)
            {
                return new AuthResponse { Success = false, Message = authResult.Errors.FirstOrDefault() };
            }
            return authResult.Value;
        }

        public async Task<Result<AuthResponse>> AuthenticateAsync(string email, string password, bool recordarme = false)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.Activo)
            {
                return Result.Failure<AuthResponse>("Usuario o contraseña incorrectos");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return Result.Failure<AuthResponse>("Usuario o contraseña incorrectos");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // 🔍 DEBUG: Ver exactamente qué roles se obtienen
            _logger.LogInformation("🔍🏷️ [AuthenticateAsync] Usuario: {Email}", user.Email);
            _logger.LogInformation("🔍🏷️ [AuthenticateAsync] Roles obtenidos: [{Roles}]", string.Join(", ", roles));
            _logger.LogInformation("🔍🏷️ [AuthenticateAsync] Total roles: {Count}", roles.Count);
            
            var tokenResponse = _jwtTokenService.GenerateToken(user.Id.ToString(), user.UserName, user.Email, roles);
            
            // Buscar el ID del dominio del usuario
            var domainUserId = await ObtenerDomainUserIdAsync(user.Email);
            _logger.LogInformation("🔍 DomainUserId obtenido para {Email}: {DomainUserId}", user.Email, domainUserId);
            
            // Si el usuario quiere que se recuerde, generar un refresh token
            string? refreshToken = null;
            if (recordarme)
            {
                refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);
                _logger.LogInformation("🔄 Refresh token creado para usuario {Email}", user.Email);
            }
            
            // 🌍 Calcular la hora de Chile para la expiración del token
            TimeZoneInfo chileTimeZone;
            try
            {
                chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"); // Windows
            }
            catch
            {
                try
                {
                    chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"); // Linux
                }
                catch
                {
                    chileTimeZone = TimeZoneInfo.Utc; // Fallback
                }
            }
            
            var utcNow = DateTime.UtcNow;
            var chileNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, chileTimeZone);
            var tokenExpiration = chileNow.AddSeconds(tokenResponse.ExpiresIn);
            
            _logger.LogInformation("🕐 Hora UTC: {UtcTime}", utcNow);
            _logger.LogInformation("🕐 Hora Chile: {ChileTime}", chileNow);
            _logger.LogInformation("🕐 Token expira (Chile): {ExpirationTime}", tokenExpiration);
            
            return Result.Success(new AuthResponse
            {
                Success = true,
                Token = tokenResponse.AccessToken,
                RefreshToken = recordarme ? refreshToken : null,
                Expiration = tokenExpiration,
                Message = "Autenticación exitosa",
                
                // 🎯 Objeto User completo para el frontend
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    UserName = user.UserName,
                    Nombre = user.Nombre ?? string.Empty,
                    Apellido = user.Apellidos ?? string.Empty,
                    Activo = user.Activo,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                },
                
                // 🔄 Campos legacy para compatibilidad
                UserId = user.Id.ToString(),
                DomainUserId = domainUserId,
                UserName = user.UserName,
                Roles = roles.ToList()
            });
        }
        
        public async Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                _logger.LogInformation("🔄 Iniciando renovación de token con refresh token: {RefreshToken}", refreshToken);

                // Validar refresh token usando el nuevo servicio
                var user = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);

                if (user == null || !user.Activo)
                {
                    _logger.LogWarning("❌ Refresh token inválido o usuario inactivo: {RefreshToken}", refreshToken);
                    return Result.Failure<AuthResponse>("Refresh token inválido o expirado");
                }

                _logger.LogInformation("✅ Refresh token válido para usuario {UserId}", user.Id);

                // Generar nuevo token
                var roles = await _userManager.GetRolesAsync(user);
                var tokenResponse = _jwtTokenService.GenerateToken(user.Id.ToString(), user.UserName, user.Email, roles);

                // Generar nuevo refresh token (mantener el dispositivo actual)
                var newRefreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);
                
                // Revocar el refresh token anterior
                await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);

                _logger.LogInformation("🔄 Nuevo refresh token generado para usuario {UserId}", user.Id);

                return Result.Success(new AuthResponse
                {
                    Success = true,
                    Token = tokenResponse.AccessToken,
                    RefreshToken = newRefreshToken,
                    Expiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    Message = "Token renovado exitosamente",
                    
                    // 🎯 Objeto User completo para el frontend
                    User = new UserDto
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email,
                        UserName = user.UserName,
                        Nombre = user.Nombre ?? string.Empty,
                        Apellido = user.Apellidos ?? string.Empty,
                        Activo = user.Activo,
                        EmailConfirmed = user.EmailConfirmed,
                        Roles = roles.ToList()
                    },
                    
                    // 🔄 Campos legacy para compatibilidad
                    UserId = user.Id.ToString(),
                    UserName = user.UserName,
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renovando token para usuario");
                return Result.Failure<AuthResponse>("Error interno durante la renovación del token");
            }
        }
        
        public async Task<Result> UpdateUserAsync(string id, string nombre, string apellidos, string email, string username)
        {
            if (!Guid.TryParse(id, out var userGuid))
            {
                return Result.Failure("ID de usuario inválido");
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            user.Nombre = nombre;
            user.Apellidos = apellidos;
            user.Email = email;
            user.UserName = username;

            var result = await _userManager.UpdateAsync(user);
            return result.ToResult();
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure("ID de usuario inválido");
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            var result = await _userManager.DeleteAsync(user);
            return result.ToResult();
        }

        public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? confirmNewPassword = null)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure("ID de usuario inválido");
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return Result.Failure("Usuario no encontrado");

            // Validar confirmación si se provee
            if (confirmNewPassword != null && newPassword != confirmNewPassword)
            {
                return Result.Failure("La confirmación de la nueva contraseña no coincide");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.ToResult();
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return null;
            }

            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Email = user.Email,
                Nombre = user.Nombre ?? string.Empty,
                Apellido = user.Apellidos ?? string.Empty,
                Activo = user.Activo,
                EmailConfirmed = user.EmailConfirmed,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await _userManager.Users
                .Select(user => new UserDto
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    Email = user.Email,
                    Nombre = user.Nombre ?? string.Empty,
                    Apellido = user.Apellidos ?? string.Empty,
                    Activo = user.Activo,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                }).ToListAsync();
        }

        /// <summary>
        /// Obtiene el ID del dominio del usuario basado en su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>ID del dominio del usuario o null si no se encuentra</returns>
        private async Task<string?> ObtenerDomainUserIdAsync(string email)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando usuario en dominio por email: {Email}", email);
                
                // Verificar si hay usuarios en la tabla
                var totalUsuarios = await _coreDbContext.Usuarios.CountAsync();
                _logger.LogInformation("🔍 Total de usuarios en dominio: {Total}", totalUsuarios);
                
                var usuarioDominio = await _coreDbContext.Usuarios
                    .Where(u => u.Email == email)
                    .Select(u => new { u.Id })
                    .FirstOrDefaultAsync();

                _logger.LogInformation("🔍 Usuario encontrado en dominio: {Usuario}", usuarioDominio?.Id);
                
                return usuarioDominio?.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el ID del dominio para el usuario {Email}", email);
                return null;
            }
        }
    }
} 
