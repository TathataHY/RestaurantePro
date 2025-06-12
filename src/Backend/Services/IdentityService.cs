using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
        private readonly IAuthorizationService _authorizationService;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
            IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
            _authorizationService = authorizationService;
        }

        public async Task<(bool Success, string UserId, string Message)> CreateUserAsync(
            string nombre, string apellido, string email, string password, string rol)
        {
            // Verificar si el rol existe
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                await _roleManager.CreateAsync(new IdentityRole(rol));
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Rol = rol,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, rol);
                return (true, user.Id, "Usuario creado correctamente");
            }

            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<(bool Success, string UserId, string Message)> ValidateUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return (false, null, "Usuario no encontrado");
            }

            if (!user.Activo)
            {
                return (false, null, "Usuario inactivo");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (result.Succeeded)
            {
                // Actualizar último acceso
                user.UltimoAcceso = DateTime.Now;
                await _userManager.UpdateAsync(user);

                return (true, user.Id, "Autenticación exitosa");
            }

            return (false, null, "Credenciales incorrectas");
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<string> GetUserNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName;
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> AuthorizeAsync(string userId, string policyName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
            var result = await _authorizationService.AuthorizeAsync(principal, policyName);

            return result.Succeeded;
        }
    }
} 