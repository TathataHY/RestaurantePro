using Microsoft.AspNetCore.Identity;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Identity.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<Result<List<RoleDto>>> GetRolesAsync()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
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

            return Result<List<RoleDto>>.Success(roleDtos);
        }

        public async Task<Result<RoleDto>> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Result<RoleDto>.Failure(new List<string> { "Rol no encontrado" });
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims
                .Where(c => c.Type == CustomClaimTypes.Permission)
                .Select(c => c.Value)
                .ToList();

            return Result<RoleDto>.Success(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = permissions
            });
        }

        public async Task<Result<string>> CreateRoleAsync(string name, List<string> permissions)
        {
            if (await _roleManager.RoleExistsAsync(name))
            {
                return Result<string>.Failure(new List<string> { "Ya existe un rol con este nombre" });
            }

            var role = new IdentityRole(name);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            // Asignar permisos
            if (permissions != null && permissions.Count > 0)
            {
                foreach (var permission in permissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
                }
            }

            return Result<string>.Success(role.Id);
        }

        public async Task<Result> UpdateRoleAsync(string id, string name, List<string> permissions)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Result.Failure(new List<string> { "Rol no encontrado" });
            }

            // Verificar que el nombre no esté en uso por otro rol
            var existingRole = await _roleManager.FindByNameAsync(name);
            if (existingRole != null && existingRole.Id != id)
            {
                return Result.Failure(new List<string> { "Ya existe un rol con este nombre" });
            }

            role.Name = name;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            // Actualizar permisos
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = claims.Where(c => c.Type == CustomClaimTypes.Permission).ToList();

            // Eliminar permisos actuales
            foreach (var claim in permissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Agregar nuevos permisos
            if (permissions != null && permissions.Count > 0)
            {
                foreach (var permission in permissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
                }
            }

            return Result.Success();
        }

        public async Task<Result> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Result.Failure(new List<string> { "Rol no encontrado" });
            }

            // Verificar si hay usuarios con este rol
            var users = await _userManager.GetUsersInRoleAsync(role.Name);
            if (users.Any())
            {
                return Result.Failure(new List<string> { "No se puede eliminar el rol porque tiene usuarios asignados" });
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result> AssignRoleToUserAsync(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return Result.Failure(new List<string> { "Rol no encontrado" });
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            return Result.Success();
        }

        public async Task<Result> RemoveRoleFromUserAsync(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(new List<string> { "Usuario no encontrado" });
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return Result.Failure(new List<string> { "Rol no encontrado" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

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
                return Result<List<string>>.Failure(new List<string> { "Usuario no encontrado" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var rolePermissions = claims
                        .Where(c => c.Type == CustomClaimTypes.Permission)
                        .Select(c => c.Value);

                    permissions.AddRange(rolePermissions);
                }
            }

            return Result<List<string>>.Success(permissions.Distinct().ToList());
        }
    }

    public static class CustomClaimTypes
    {
        public const string Permission = "permission";
    }
} 