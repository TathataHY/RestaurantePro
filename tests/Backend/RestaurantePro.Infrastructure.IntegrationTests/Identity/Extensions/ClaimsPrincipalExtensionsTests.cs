using System;
using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using RestaurantePro.Infrastructure.Identity.Extensions;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Identity.Extensions
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetUserId_DebeRetornarId_CuandoClaimExiste()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.GetUserId();

            // Assert
            result.Should().Be(userId);
        }

        [Fact]
        public void GetUserId_DebeRetornarNull_CuandoClaimNoExiste()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = principal.GetUserId();

            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public void GetUserId_DebeLanzarExcepcion_CuandoPrincipalEsNulo()
        {
            // Arrange
            ClaimsPrincipal principal = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => principal.GetUserId());
        }

        [Fact]
        public void GetFullName_DebeRetornarNombreCompleto_CuandoClaimsExisten()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("FirstName", "John"),
                new Claim("LastName", "Doe")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.GetFullName();

            // Assert
            result.Should().Be("John Doe");
        }

        [Fact]
        public void GetFullName_DebeRetornarSoloNombre_CuandoApellidoNoExiste()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("FirstName", "John") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.GetFullName();

            // Assert
            result.Should().Be("John");
        }
        
        [Fact]
        public void GetFullName_DebeRetornarIdentityName_CuandoClaimsNoExisten()
        {
            // Arrange
            var identity = new ClaimsIdentity(new List<Claim>(), "TestAuthType");
            identity.AddClaim(new Claim(identity.NameClaimType, "testuser"));
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.GetFullName();

            // Assert
            result.Should().Be("testuser");
        }

        [Fact]
        public void GetEmail_DebeRetornarEmail_CuandoClaimExiste()
        {
            // Arrange
            var email = "test@test.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.GetEmail();

            // Assert
            result.Should().Be(email);
        }

        [Fact]
        public void HasRole_DebeRetornarTrue_CuandoRolExiste()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Admin") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.HasRole("Admin");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetRoles_DebeRetornarTodosLosRoles()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var expectedRoles = new List<string> { "Admin", "User" };

            // Act
            var result = principal.GetRoles();

            // Assert
            result.Should().BeEquivalentTo(expectedRoles);
        }

        [Fact]
        public void HasPermission_DebeRetornarTrue_CuandoPermisoExiste()
        {
            // Arrange
            var claims = new List<Claim> { new Claim("Permission", "can_delete_users") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.HasPermission("can_delete_users");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetPermissions_DebeRetornarTodosLosPermisos()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("Permission", "can_read_data"),
                new Claim("Permission", "can_write_data")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var expectedPermissions = new List<string> { "can_read_data", "can_write_data" };

            // Act
            var result = principal.GetPermissions();

            // Assert
            result.Should().BeEquivalentTo(expectedPermissions);
        }
    }
} 