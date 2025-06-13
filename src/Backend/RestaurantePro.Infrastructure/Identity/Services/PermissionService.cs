using Microsoft.AspNetCore.Identity;
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
    public class PermissionService : IUserPermissionService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityApplicationUser> _userManager;

        public PermissionService(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> UsuarioTienePermisoAsync(Guid usuarioId, string permiso)
        {
            // Implementación básica - en un escenario real, verificaríamos permisos específicos
            var user = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (user == null)
            {
                return false;
            }

            // Verificar si el usuario es administrador (tiene todos los permisos)
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return true;
            }

            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);
            
            // Verificar si algún rol tiene el permiso
            foreach (var role in roles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role));
                if (roleClaims.Any(c => c.Type == "Permission" && c.Value == permiso))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> UsuarioTieneRolAsync(Guid usuarioId, string rol)
        {
            var user = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, rol);
        }

        public async Task<bool> UsuarioTieneNivelAccesoAsync(Guid usuarioId, int nivelRequerido)
        {
            // Implementación básica - en un escenario real, cada rol tendría un nivel de acceso
            var user = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (user == null)
            {
                return false;
            }

            // Asignamos niveles ficticios a roles comunes
            var roles = await _userManager.GetRolesAsync(user);
            
            int nivelMaximo = 0;
            
            foreach (var role in roles)
            {
                int nivelRol = role.ToLower() switch
                {
                    "admin" => 10,
                    "manager" => 8,
                    "supervisor" => 6,
                    "employee" => 4,
                    "user" => 2,
                    _ => 1
                };
                
                nivelMaximo = Math.Max(nivelMaximo, nivelRol);
            }
            
            return nivelMaximo >= nivelRequerido;
        }

        public async Task<List<string>> ObtenerPermisosUsuarioAsync(Guid usuarioId)
        {
            var user = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permisos = new List<string>();
            
            foreach (var role in roles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role));
                permisos.AddRange(roleClaims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value));
            }
            
            return permisos.Distinct().ToList();
        }

        public Task<List<Guid>> ObtenerSubordinadosAsync(Guid supervisorId)
        {
            // Implementación simulada - en un escenario real, consultaríamos la estructura organizacional
            return Task.FromResult(new List<Guid>());
        }

        public Task<bool> PuedeSupervisarAsync(Guid supervisorId, Guid subordinadoId)
        {
            // Implementación simulada - en un escenario real, verificaríamos la jerarquía organizacional
            return Task.FromResult(true);
        }

        public Task<List<Guid>> ObtenerUsuariosMismoDepartamentoAsync(Guid usuarioId)
        {
            // Implementación simulada - en un escenario real, consultaríamos la estructura organizacional
            return Task.FromResult(new List<Guid>());
        }

        public async Task<bool> EsAdministradorAsync(Guid usuarioId)
        {
            var user = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, "Admin");
        }

        // Métodos internos para gestión de roles y permisos
        public async Task<Result<List<RoleDto>>> GetRolesAsync()
        {
            var roles = await Task.FromResult(_roleManager.Roles.ToList());
            var roleDtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims
                    .Where(c => c.Type == CustomClaimTypes.Permission)
                    .Select(c => c.Value)
                    .ToList();

                roleDtos.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Permissions = permissions
                });
            }

            return Result.Success<List<RoleDto>>(roleDtos);
        }

        public async Task<Result<RoleDto>> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Result.Failure<RoleDto>(new List<string> { "Rol no encontrado" });
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims
                .Where(c => c.Type == CustomClaimTypes.Permission)
                .Select(c => c.Value)
                .ToList();

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = permissions
            };

            return Result.Success<RoleDto>(roleDto);
        }

        public async Task<Result<string>> CreateRoleAsync(string name, List<string> permissions)
        {
            // Verificar que el nombre no esté en uso
            var existingRole = await _roleManager.FindByNameAsync(name);
            if (existingRole != null)
            {
                return Result.Failure<string>(new List<string> { "Ya existe un rol con este nombre" });
            }

            var role = new IdentityRole(name);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return Result.Failure<string>(result.Errors.Select(e => e.Description).ToList());
            }

            // Agregar permisos
            foreach (var permission in permissions)
            {
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            }

            return Result.Success<string>(role.Id);
        }

        public async Task<Result<string>> UpdateRoleAsync(string id, string name, List<string> permissions)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Result.Failure<string>(new List<string> { "Rol no encontrado" });
            }

            // Verificar que el nombre no esté en uso por otro rol
            var existingRole = await _roleManager.FindByNameAsync(name);
            if (existingRole != null && existingRole.Id != id)
            {
                return Result.Failure<string>(new List<string> { "Ya existe un rol con este nombre" });
            }

            role.Name = name;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return Result.Failure<string>(result.Errors.Select(e => e.Description).ToList());
            }

            // Eliminar permisos existentes
            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == CustomClaimTypes.Permission))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Agregar nuevos permisos
            foreach (var permission in permissions)
            {
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            }

            return Result<string>.Success(role.Id);
        }

        public async Task<Result> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Result.Failure(new List<string> { "Rol no encontrado" });
            }

            // Verificar que no haya usuarios con este rol
            var users = await _userManager.GetUsersInRoleAsync(role.Name);
            if (users.Any())
            {
                return Result.Failure(new List<string> { "No se puede eliminar el rol porque hay usuarios asignados a él" });
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result<List<string>>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<List<string>>(new List<string> { "Usuario no encontrado" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    permissions.AddRange(claims
                        .Where(c => c.Type == CustomClaimTypes.Permission)
                        .Select(c => c.Value));
                }
            }

            return Result<List<string>>.Success(permissions.Distinct().ToList());
        }

        public async Task<Result> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return Result.Failure(new List<string> { $"El rol '{roleName}' no existe" });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return Result.Failure(new List<string> { $"El rol '{roleName}' no existe" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }
    }

    public static class CustomClaimTypes
    {
        public const string Permission = "permission";
    }
} 
