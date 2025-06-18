using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Identity.Services
{
    public class IdentityServiceTests : IntegrationTestBase
    {
        private IIdentityService _identityService = null!;
        private UserManager<IdentityApplicationUser> _userManager = null!;
        private RoleManager<ApplicationRole> _roleManager = null!;

        public IdentityServiceTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _identityService = ServiceProvider.GetRequiredService<IIdentityService>();
            _userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
            _roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        }

        [Fact]
        public async Task RegisterAsync_DebeCrearUsuarioConPropiedadesPersonalizadasYRol()
        {
            // Arrange
            var roleName = "Tester";
            if (await _roleManager.FindByNameAsync(roleName) == null)
            {
                await _identityService.CreateRoleAsync(roleName, "Rol para pruebas", false);
            }

            var nombre = "Usuario";
            var apellidos = "De Prueba";
            var email = "test@example.com";
            var userName = "testuser";
            var password = "Password123!";

            // Act
            var result = await _identityService.RegisterAsync(nombre, apellidos, email, userName, password, roleName);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNullOrEmpty();

            var userId = result.Value;
            var user = await _userManager.FindByIdAsync(userId);

            user.Should().NotBeNull();
            user!.Nombre.Should().Be(nombre);
            user.Apellidos.Should().Be(apellidos);
            user.UserName.Should().Be(userName);
            user.Email.Should().Be(email);

            var roles = await _userManager.GetRolesAsync(user);
            roles.Should().Contain(roleName);
        }

        [Fact]
        public async Task RegisterAsync_NoDebeCrearUsuarioConEmailDuplicado()
        {
            // Arrange
            var roleName = "Admin";
            await _identityService.CreateRoleAsync(roleName, "Rol de Administrador", true);
            await _identityService.RegisterAsync("Test", "User1", "test.duplicado@test.com", "testuser1", "Password123!", roleName);

            // Act
            var resultado = await _identityService.RegisterAsync("Test", "User2", "test.duplicado@test.com", "testuser2", "Password123!", roleName);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().Contain(e => e.Contains("El email ya está en uso"));
        }

        [Fact]
        public async Task RegisterAsync_NoDebeCrearUsuarioConUsernameDuplicado()
        {
            // Arrange
            var roleName = "Admin";
            await _identityService.CreateRoleAsync(roleName, "Rol de Administrador", true);
            await _identityService.RegisterAsync("Test", "User1", "user1@test.com", "testuser_duplicado", "Password123!", roleName);
        
            // Act
            var resultado = await _identityService.RegisterAsync("Test", "User2", "user2@test.com", "testuser_duplicado", "Password123!", roleName);
        
            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().Contain(e => e.Contains("El nombre de usuario ya está en uso"));
        }

        [Fact]
        public async Task CreateRoleAsync_DebeCrearRolConPropiedadesPersonalizadas()
        {
            // Arrange
            var roleName = "Auditor";
            var roleDescription = "Puede revisar registros";

            // Act
            var resultado = await _identityService.CreateRoleAsync(roleName, roleDescription, true);
            var rol = await _roleManager.FindByNameAsync(roleName);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            rol.Should().NotBeNull();
            rol.Description.Should().Be(roleDescription);
            rol.IsSystemRole.Should().BeTrue();
        }

        [Theory]
        [InlineData("pass", "Passwords must be at least 8 characters.")] // Demasiado corta
        [InlineData("password", "Passwords must have at least one non alphanumeric character.")] // Sin caracter especial
        [InlineData("Password", "Passwords must have at least one non alphanumeric character.")] // Sin caracter especial
        [InlineData("PASSWORD123", "Passwords must have at least one lowercase ('a'-'z').")] // Sin minúscula
        [InlineData("password123", "Passwords must have at least one uppercase ('A'-'Z').")] // Sin mayúscula
        [InlineData("Password!", "Passwords must have at least one digit ('0'-'9').")] // Sin número
        public async Task RegisterAsync_NoDebeCrearUsuarioConPasswordInvalido(string password, string expectedError)
        {
            // Arrange
            var roleName = "Usuario Invalido";
            if (await _roleManager.FindByNameAsync(roleName) == null)
            {
                await _identityService.CreateRoleAsync(roleName, "Rol para pruebas de contraseñas invalidas", false);
            }

            // Act
            var result = await _identityService.RegisterAsync(
                "Test", 
                "Invalido", 
                $"user-{Guid.NewGuid()}@test.com", 
                $"testuser-{Guid.NewGuid()}", 
                password, 
                roleName);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.Contains(expectedError));
        }
    }
} 