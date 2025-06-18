using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class NotificacionRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private INotificacionRepository _repository;

        private Guid _usuarioId1;
        private Guid _usuarioId2;

        public NotificacionRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<INotificacionRepository>();
            await SeedNotificacionesAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedNotificacionesAsync()
        {
            var usuarioRepository = ServiceProvider.GetRequiredService<IUsuarioRepository>();

            var usuario1 = Usuario.Crear("user1", "Usuario Uno", "user1@test.com", RolUsuario.Mesero);
            var usuario2 = Usuario.Crear("user2", "Usuario Dos", "user2@test.com", RolUsuario.Mesero);
            
            _usuarioId1 = usuario1.Id;
            _usuarioId2 = usuario2.Id;

            await usuarioRepository.AgregarAsync(usuario1);
            await usuarioRepository.AgregarAsync(usuario2);
            await DbContext.SaveChangesAsync();

            var notificacionAntigua = Notificacion.Crear("Antigua", "Mensaje Antiguo", TipoNotificacion.Informativa, _usuarioId1, null, DateTime.UtcNow.AddDays(-10));

            var notificaciones = new List<Notificacion>
            {
                notificacionAntigua,
                Notificacion.Crear("Test 2", "Mensaje 2", TipoNotificacion.Alerta, _usuarioId1),
                Notificacion.Crear("Test 3", "Mensaje 3", TipoNotificacion.Error, _usuarioId2),
                Notificacion.Crear("Test 4", "Mensaje 4", TipoNotificacion.Informativa, _usuarioId1),
                Notificacion.Crear("Test 5", "Mensaje 5", TipoNotificacion.Alerta, _usuarioId2)
            };
            
            notificaciones[2].MarcarComoLeida();

            await DbContext.Notificaciones.AddRangeAsync(notificaciones);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task AgregarAsync_DebeGuardarNotificacion()
        {
            // Arrange
            var nuevaNotificacion = Notificacion.Crear("Nueva Alerta", "Mensaje de alerta", TipoNotificacion.Alerta, _usuarioId1);

            // Act
            await _repository.AgregarAsync(nuevaNotificacion);
            await DbContext.SaveChangesAsync();

            // Assert
            var notificacionGuardada = await _repository.ObtenerPorIdAsync(nuevaNotificacion.Id);
            notificacionGuardada.Should().NotBeNull();
            notificacionGuardada.Should().BeEquivalentTo(nuevaNotificacion);
        }

        [Fact]
        public async Task ObtenerPorDestinatarioAsync_DebeRetornarNotificacionesCorrectas()
        {
            // Arrange
            var destinatarioId = _usuarioId1;

            // Act
            var notificaciones = await _repository.ObtenerPorDestinatarioAsync(destinatarioId);

            // Assert
            notificaciones.Should().NotBeNull();
            notificaciones.Should().HaveCount(3);
            notificaciones.All(n => n.DestinatarioId == destinatarioId).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorTipoYDestinatarioAsync_DebeRetornarNotificacionesFiltradas()
        {
            // Arrange
            var destinatarioId = _usuarioId1;
            var tipo = TipoNotificacion.Informativa;

            // Act
            var notificaciones = await _repository.ObtenerPorTipoYDestinatarioAsync(tipo, destinatarioId);

            // Assert
            notificaciones.Should().NotBeNull();
            notificaciones.Should().HaveCount(2);
            notificaciones.All(n => n.Tipo == tipo).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerNoLeidasPorDestinatarioAsync_DebeRetornarSoloNoLeidas()
        {
            // Arrange
            var destinatarioId = _usuarioId1;

            // Act
            var notificaciones = await _repository.ObtenerNoLeidasPorDestinatarioAsync(destinatarioId);

            // Assert
            notificaciones.Should().NotBeNull();
            notificaciones.Should().HaveCount(3);
            notificaciones.All(n => !n.EstaLeida).Should().BeTrue();
        }

        [Fact]
        public async Task EliminarAnterioresAFechaAsync_DebeEliminarNotificacionesAntiguas()
        {
            // Arrange
            var fechaLimite = DateTime.UtcNow.AddDays(-5);

            // Act
            var cantidadEliminada = await _repository.EliminarAnterioresAFechaAsync(fechaLimite);

            // Assert
            cantidadEliminada.Should().Be(1);
            var notificacionesRestantes = await _repository.ObtenerTodosAsync();
            notificacionesRestantes.Should().HaveCount(4);
        }
    }
} 