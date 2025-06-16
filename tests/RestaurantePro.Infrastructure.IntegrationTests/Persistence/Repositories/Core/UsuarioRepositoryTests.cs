using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class UsuarioRepositoryTests : IntegrationTestBase
    {
        private readonly UsuarioRepository _repository;
        private readonly Mock<ILogger<UsuarioRepository>> _loggerMock;

        public UsuarioRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<UsuarioRepository>>();
            _repository = new UsuarioRepository(DbContext, _loggerMock.Object);
        }

        protected override void SeedDatabase()
        {
            base.SeedDatabase();

            var usuario1 = Usuario.Crear("testuser1", "John Doe", "test1@test.com", RolUsuario.Cajero);
            usuario1.AsociarIdentity("identity-123");
            usuario1.ConfirmarCuenta();
            usuario1.Activar();

            var usuario2 = Usuario.Crear("testuser2", "Jane Smith", "test2@test.com", RolUsuario.Cajero);
            usuario2.AsignarRol(RolUsuario.Mesero);
            usuario2.AsociarIdentity("identity-456");
            usuario2.ConfirmarCuenta();
            usuario2.Activar();

            var usuario3 = Usuario.Crear("testuser3", "Admin User", "test3@test.com", RolUsuario.Administrador);
            usuario3.Desactivar(); // Dejar un usuario inactivo

            var usuarios = new List<Usuario> { usuario1, usuario2, usuario3 };

            DbContext.Set<Usuario>().AddRange(usuarios);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarUsuarioCorrecto()
        {
            // Arrange
            var usuarioExistente = await DbContext.Set<Usuario>().FirstAsync();

            // Act
            var usuario = await _repository.ObtenerPorIdAsync(usuarioExistente.Id);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.Id.Should().Be(usuarioExistente.Id);
            usuario.NombreUsuario.Should().Be(usuarioExistente.NombreUsuario);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarNullSiNoExiste()
        {
            // Act
            var usuario = await _repository.ObtenerPorIdAsync(Guid.NewGuid());

            // Assert
            usuario.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorNombreUsuarioAsync_DebeRetornarUsuarioCorrecto()
        {
            // Arrange
            var nombreUsuario = "testuser2";

            // Act
            var usuario = await _repository.ObtenerPorNombreUsuarioAsync(nombreUsuario);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.NombreUsuario.Should().Be(nombreUsuario);
        }

        [Fact]
        public async Task ObtenerPorIdentityIdAsync_DebeRetornarUsuarioCorrecto()
        {
            // Arrange
            var identityId = "identity-123";

            // Act
            var usuario = await _repository.ObtenerPorIdentityIdAsync(identityId);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.IdentityId.Should().Be(identityId);
            usuario.NombreUsuario.Should().Be("testuser1");
        }

        [Fact]
        public async Task ObtenerPorIdentityIdAsync_DebeRetornarNullSiNoExiste()
        {
            // Act
            var usuario = await _repository.ObtenerPorIdentityIdAsync("non-existent-identity-id");

            // Assert
            usuario.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_DebeRetornarUsuarioCorrecto()
        {
            // Arrange
            var email = "test1@test.com";

            // Act
            var usuario = await _repository.ObtenerPorEmailAsync(email);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.Email.Should().Be(email);
        }

        [Fact]
        public async Task ObtenerTodosAsync_ConSoloActivosTrue_DebeRetornarSoloUsuariosActivos()
        {
            // Act
            var usuarios = await _repository.ObtenerTodosAsync(true);

            // Assert
            usuarios.Should().HaveCount(2);
            usuarios.Should().OnlyContain(u => u.Estado == EstadoUsuario.Activo);
        }

        [Fact]
        public async Task ObtenerTodosAsync_ConSoloActivosFalse_DebeRetornarTodosLosUsuarios()
        {
            // Act
            var usuarios = await _repository.ObtenerTodosAsync(false);

            // Assert
            usuarios.Should().HaveCount(3);
        }

        [Fact]
        public async Task ObtenerPorRolAsync_DebeRetornarUsuariosConEseRol()
        {
            // Act
            var usuarios = await _repository.ObtenerPorRolAsync(RolUsuario.Mesero);

            // Assert
            usuarios.Should().ContainSingle();
            usuarios.First().NombreUsuario.Should().Be("testuser2");
        }

        [Fact]
        public async Task ExisteNombreUsuarioAsync_DebeRetornarTrueSiExiste()
        {
            // Act
            var existe = await _repository.ExisteNombreUsuarioAsync("testuser1");

            // Assert
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteEmailAsync_DebeRetornarTrueSiExiste()
        {
            // Act
            var existe = await _repository.ExisteEmailAsync("test2@test.com");

            // Assert
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task AgregarAsync_DebeGuardarNuevoUsuario()
        {
            // Arrange
            var nuevoUsuario = Usuario.Crear("newuser", "New User", "new@test.com", RolUsuario.Cocinero);

            // Act
            await _repository.AgregarAsync(nuevoUsuario);
            var usuarioGuardado = await DbContext.Set<Usuario>().FirstOrDefaultAsync(u => u.NombreUsuario == "newuser");

            // Assert
            usuarioGuardado.Should().NotBeNull();
            usuarioGuardado!.Email.Should().Be("new@test.com");
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarUsuarioExistente()
        {
            // Arrange
            var usuarioAActualizar = await DbContext.Set<Usuario>().FirstAsync(u => u.NombreUsuario == "testuser1");
            usuarioAActualizar.Actualizar("John Updated", "john.updated@test.com");

            // Act
            await _repository.ActualizarAsync(usuarioAActualizar);
            var usuarioActualizado = await DbContext.Set<Usuario>().AsNoTracking().FirstAsync(u => u.Id == usuarioAActualizar.Id);

            // Assert
            usuarioActualizado.Should().NotBeNull();
            usuarioActualizado!.NombreCompleto.Should().Be("John Updated");
            usuarioActualizado.Email.Should().Be("john.updated@test.com");
        }
    }
} 