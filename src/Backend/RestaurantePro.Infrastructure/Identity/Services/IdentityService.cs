using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityApplicationUser> _userManager;
        private readonly SignInManager<IdentityApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenService _jwtTokenService;

        public IdentityService(
            UserManager<IdentityApplicationUser> userManager,
            SignInManager<IdentityApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return (Result.Failure(new List<string> { "El email ya está en uso" }), string.Empty);
            }

            existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
            {
                return (Result.Failure(new List<string> { "El nombre de usuario ya está en uso" }), string.Empty);
            }

            var user = new IdentityApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return (Result.Failure(result.Errors.Select(e => e.Description).ToList()), string.Empty);
            }

            return (Result.Success(), user.Id);
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var authResult = await AuthenticateAsync(email, password);
            
            if (!authResult.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = authResult.Errors?.FirstOrDefault() ?? "Error de autenticación"
                };
            }
            
            return authResult.Value;
        }

        public async Task<Result<AuthResponse>> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                return Result.Failure<AuthResponse>(new List<string> { "Usuario o contraseña incorrectos" });
            }

            if (!user.Activo)
            {
                return Result.Failure<AuthResponse>(new List<string> { "Usuario desactivado" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (!result.Succeeded)
            {
                return Result.Failure<AuthResponse>(new List<string> { "Usuario o contraseña incorrectos" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _jwtTokenService.GenerateToken(user.Id, user.UserName, user.Email, roles);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                Success = true,
                UserId = user.Id,
                UserName = user.UserName,
                Token = tokenResponse.AccessToken,
                Expiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                Roles = roles.ToList(),
                Message = "Autenticación exitosa"
            });
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        {
            try {
                // Para implementar después ya que requiere extender la interfaz IJwtTokenService
                // con los métodos necesarios
                throw new NotImplementedException("Esta funcionalidad será implementada próximamente");
            } 
            catch (Exception ex)
            {
                return Result.Failure<AuthResponse>(new List<string> { $"Error al renovar el token: {ex.Message}" });
            }
        }

        public async Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Result.Failure<string>(new List<string> { "El email ya está en uso" });
            }

            existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
            {
                return Result.Failure<string>(new List<string> { "El nombre de usuario ya está en uso" });
            }

            // Verificar si el rol existe
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
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return Result.Failure<string>(result.Errors.Select(e => e.Description).ToList());
            }

            // Asignar rol
            result = await _userManager.AddToRoleAsync(user, rol);

            if (!result.Succeeded)
            {
                // Si no se pudo asignar el rol, eliminar el usuario creado
                await _userManager.DeleteAsync(user);
                return Result.Failure<string>(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success<string>(user.Id);
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

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            };
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }

        public async Task<Result> AddUserToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return Result.Failure(new List<string> { $"El rol '{role}' no existe" });
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }
    }
} 
