using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts
{
    public class CoreDbContextTests : IntegrationTestBase
    {
        [Fact]
        public async Task CoreDbContext_DebeGuardarUsuarioCorrectamente()
        {
            // Arrange
            var usuario = Usuario.Crear(
                "testuser", 
                "Test User", 
                "test@test.com", 
                RolUsuario.Mesero);
            
            // Act
            DbContext.Usuarios.Add(usuario);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var usuarioGuardado = await DbContext.Usuarios.FindAsync(usuario.Id);
            usuarioGuardado.Should().NotBeNull();
            usuarioGuardado.NombreUsuario.Should().Be("testuser");
            usuarioGuardado.Email.Should().Be("test@test.com");
        }
    }
} 