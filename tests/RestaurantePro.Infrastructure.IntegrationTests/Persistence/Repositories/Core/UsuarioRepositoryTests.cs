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
using RestaurantePro.Domain.Core.Usuarios.Interfaces;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class UsuarioRepositoryTests : IntegrationTestBase
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioRepositoryTests()
        {
            _repository = new UsuarioRepository(DbContext, Mock.Of<ILogger<UsuarioRepository>>());
        }

        protected override async Task SeedDataAsync()
        {
            var usuarios = new List<Usuario>();

            // Usuario 1: Activo, Administrador, con IdentityID
            var admin = Usuario.Crear("admin", "Admin User", "admin@test.com", RolUsuario.Administrador);
            admin.ConfirmarCuenta();
            admin.AsociarIdentity("identity-admin-123");
            usuarios.Add(admin);

            // Usuario 2: Activo, Mesero
            var mesero = Usuario.Crear("mesero", "Mesero User", "mesero@test.com", RolUsuario.Mesero);
            mesero.ConfirmarCuenta();
            usuarios.Add(mesero);

            // Usuario 3: Inactivo
            var cocinero = Usuario.Crear("cocinero", "Cocinero User", "cocinero@test.com", RolUsuario.Cocinero);
            cocinero.ConfirmarCuenta(); // lo activamos
            cocinero.Desactivar();      // y luego lo desactivamos para que quede Inactivo
            usuarios.Add(cocinero);
            
            // Usuario 4: Pendiente de confirmacion (no activo)
            var pendiente = Usuario.Crear("pendiente", "Pendiente User", "pendiente@test.com", RolUsuario.Cajero);
            usuarios.Add(pendiente);

            await DbContext.Usuarios.AddRangeAsync(usuarios);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarUsuarioCorrecto_CuandoExiste()
        {
            // Arrange
            var usuarioExistente = await DbContext.Usuarios.FirstAsync(u => u.NombreUsuario == "admin");

            // Act
            var usuario = await _repository.ObtenerPorIdAsync(usuarioExistente.Id);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.Id.Should().Be(usuarioExistente.Id);
            usuario.NombreUsuario.Should().Be("admin");
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarNull_CuandoNoExiste()
        {
            // Act
            var usuario = await _repository.ObtenerPorIdAsync(Guid.NewGuid());

            // Assert
            usuario.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorNombreUsuarioAsync_DebeRetornarUsuarioCorrecto_CuandoExiste()
        {
            // Arrange
            var nombreUsuario = "mesero";

            // Act
            var usuario = await _repository.ObtenerPorNombreUsuarioAsync(nombreUsuario);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.NombreUsuario.Should().Be(nombreUsuario);
        }

        [Fact]
        public async Task ObtenerPorIdentityIdAsync_DebeRetornarUsuarioCorrecto_CuandoExiste()
        {
            // Arrange
            var identityId = "identity-admin-123";

            // Act
            var usuario = await _repository.ObtenerPorIdentityIdAsync(identityId);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.IdentityId.Should().Be(identityId);
            usuario.NombreUsuario.Should().Be("admin");
        }

        [Fact]
        public async Task ObtenerPorIdentityIdAsync_DebeRetornarNull_CuandoNoExiste()
        {
            // Act
            var usuario = await _repository.ObtenerPorIdentityIdAsync("non-existent-identity-id");

            // Assert
            usuario.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_DebeRetornarUsuarioCorrecto_CuandoExiste()
        {
            // Arrange
            var email = "admin@test.com";

            // Act
            var usuario = await _repository.ObtenerPorEmailAsync(email);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.Email.Should().Be(email);
        }
        
        [Fact]
        public async Task ObtenerPorEmailAsync_DebeSerCaseInsensitive()
        {
            // Arrange
            var email = "ADMIN@test.com";

            // Act
            var usuario = await _repository.ObtenerPorEmailAsync(email);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.NombreUsuario.Should().Be("admin");
        }


        [Fact]
        public async Task ObtenerTodosAsync_ConSoloActivosTrue_DebeRetornarSoloUsuariosActivos()
        {
            // Act
            var usuarios = await _repository.ObtenerTodosAsync(true);

            // Assert
            // admin y mesero están activos. cocinero está inactivo, pendiente está pendiente.
            usuarios.Should().HaveCount(2);
            usuarios.Should().OnlyContain(u => u.Estado == EstadoUsuario.Activo);
        }

        [Fact]
        public async Task ObtenerTodosAsync_ConSoloActivosFalse_DebeRetornarTodosLosUsuarios()
        {
            // Act
            var usuarios = await _repository.ObtenerTodosAsync(false);

            // Assert
            usuarios.Should().HaveCount(4);
        }

        [Fact]
        public async Task ObtenerPorRolAsync_DebeRetornarUsuariosConEseRol()
        {
            // Act
            var usuarios = await _repository.ObtenerPorRolAsync(RolUsuario.Mesero);

            // Assert
            usuarios.Should().ContainSingle();
            usuarios.First().NombreUsuario.Should().Be("mesero");
        }
        
        [Fact]
        public async Task ObtenerUsuariosActivos_DebeRetornarSoloUsuariosActivos()
        {
            // Act
            var usuarios = await _repository.ObtenerUsuariosActivos();
            
            // Assert
            usuarios.Should().HaveCount(2);
            usuarios.Should().AllBeEquivalentTo(new { Estado = EstadoUsuario.Activo });
        }


        [Fact]
        public async Task ExisteNombreUsuarioAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Act
            var existe = await _repository.ExisteNombreUsuarioAsync("admin");

            // Assert
            existe.Should().BeTrue();
        }
        
        [Fact]
        public async Task ExisteNombreUsuarioAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var existe = await _repository.ExisteNombreUsuarioAsync("usuario-inexistente");

            // Assert
            existe.Should().BeFalse();
        }

        [Fact]
        public async Task ExisteEmailAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Act
            var existe = await _repository.ExisteEmailAsync("mesero@test.com");

            // Assert
            existe.Should().BeTrue();
        }
        
        [Fact]
        public async Task ExisteEmailAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var existe = await _repository.ExisteEmailAsync("email-inexistente@test.com");

            // Assert
            existe.Should().BeFalse();
        }

        [Fact]
        public async Task AgregarAsync_DebeGuardarNuevoUsuarioEnLaBaseDeDatos()
        {
            // Arrange
            var nuevoUsuario = Usuario.Crear("newuser", "New User", "new@test.com", RolUsuario.Cocinero);

            // Act
            await _repository.AgregarAsync(nuevoUsuario);
            var usuarioGuardado = await DbContext.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == "newuser");

            // Assert
            usuarioGuardado.Should().NotBeNull();
            usuarioGuardado!.Id.Should().Be(nuevoUsuario.Id);
            usuarioGuardado.Email.Should().Be("new@test.com");
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarUsuarioExistenteEnLaBaseDeDatos()
        {
            // Arrange
            var usuarioAActualizar = await DbContext.Usuarios.FirstAsync(u => u.NombreUsuario == "mesero");
            var id = usuarioAActualizar.Id;
            usuarioAActualizar.Actualizar("Mesero Updated", "mesero.updated@test.com");

            // Act
            await _repository.ActualizarAsync(usuarioAActualizar);
            var usuarioActualizado = await DbContext.Usuarios.AsNoTracking().FirstAsync(u => u.Id == id);

            // Assert
            usuarioActualizado.Should().NotBeNull();
            usuarioActualizado.NombreCompleto.Should().Be("Mesero Updated");
            usuarioActualizado.Email.Should().Be("mesero.updated@test.com");
        }
        
        [Fact]
        public async Task BuscarPorEmailAsync_DebeRetornarUsuarioCorrecto()
        {
            // Act
            var usuario = await _repository.BuscarPorEmailAsync("admin@test.com");

            // Assert
            usuario.Should().NotBeNull();
            usuario!.NombreUsuario.Should().Be("admin");
        }
        
        [Fact]
        public async Task ExisteUsuarioConEmailAsync_DebeRetornarTrueSiExiste()
        {
            // Act
            var result = await _repository.ExisteUsuarioConEmailAsync("cocinero@test.com", CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
} 