using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenService _jwtTokenService;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<AuthResponse>> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                return Result<AuthResponse>.Failure(new List<string> { "Usuario o contraseña incorrectos" });
            }

            if (!user.Activo)
            {
                return Result<AuthResponse>.Failure(new List<string> { "Usuario desactivado" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (!result.Succeeded)
            {
                return Result<AuthResponse>.Failure(new List<string> { "Usuario o contraseña incorrectos" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _jwtTokenService.GenerateToken(user, roles);

            // Actualizar el refresh token del usuario
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = tokenResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Nombre = user.Nombre,
                Apellidos = user.Apellidos,
                Token = tokenResponse.Token,
                RefreshToken = tokenResponse.RefreshToken,
                Roles = roles.ToList()
            });
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return Result<AuthResponse>.Failure(new List<string> { "Token inválido" });
            }

            var userId = principal.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<AuthResponse>.Failure(new List<string> { "Token inválido" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<AuthResponse>.Failure(new List<string> { "Refresh token inválido o expirado" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _jwtTokenService.GenerateToken(user, roles);

            // Actualizar el refresh token del usuario
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = tokenResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Nombre = user.Nombre,
                Apellidos = user.Apellidos,
                Token = tokenResponse.Token,
                RefreshToken = tokenResponse.RefreshToken,
                Roles = roles.ToList()
            });
        }

        public async Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Result<string>.Failure(new List<string> { "El email ya está en uso" });
            }

            existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
            {
                return Result<string>.Failure(new List<string> { "El nombre de usuario ya está en uso" });
            }

            // Verificar si el rol existe
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                return Result<string>.Failure(new List<string> { $"El rol '{rol}' no existe" });
            }

            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                Nombre = nombre,
                Apellidos = apellidos,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            // Asignar rol
            result = await _userManager.AddToRoleAsync(user, rol);

            if (!result.Succeeded)
            {
                // Si no se pudo asignar el rol, eliminar el usuario creado
                await _userManager.DeleteAsync(user);
                return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result<string>.Success(user.Id);
        }

        public async Task<Result> UpdateUserAsync(string id, string nombre, string apellidos, string email, string username)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            // Verificar que el email no esté en uso por otro usuario
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != id)
            {
                return Result.Failure(new List<string> { "El email ya está en uso" });
            }

            // Verificar que el username no esté en uso por otro usuario
            existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null && existingUser.Id != id)
            {
                return Result.Failure(new List<string> { "El nombre de usuario ya está en uso" });
            }

            user.Nombre = nombre;
            user.Apellidos = apellidos;
            user.Email = email;
            user.UserName = username;
            user.UltimaModificacion = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            // Soft delete
            user.Activo = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(string id, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result<UserDto>.Failure(new List<string> { "Usuario no encontrado" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Result<UserDto>.Success(new UserDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellidos = user.Apellidos,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToList(),
                Activo = user.Activo
            });
        }

        public async Task<Result<List<UserDto>>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellidos = user.Apellidos,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    Activo = user.Activo
                });
            }

            return Result<List<UserDto>>.Success(result);
        }
    }
} 