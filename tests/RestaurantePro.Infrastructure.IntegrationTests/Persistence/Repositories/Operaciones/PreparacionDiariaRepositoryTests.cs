using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Operaciones
{
    public class PreparacionDiariaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IPreparacionRepository _repository = null!;
        private IDateTimeService _dateTimeService = null!;
        private Guid _productoId;
        private Guid _chefId;
        private Guid _preparacionDisponibleId;
        private Guid _preparacionPorVencerId;
        private Guid _preparacionAgotadaId;

        public PreparacionDiariaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IPreparacionRepository>();
            _dateTimeService = ServiceProvider.GetRequiredService<IDateTimeService>();
            await SeedPreparacionesAsync();
        }

        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task SeedPreparacionesAsync()
        {
            // Crear Producto
            var precioProducto = new PrecioProducto(15.50m);
            var producto = Producto.Crear("Empanadas de Pino", "Empanadas chilenas rellenas de pino tradicional", precioProducto, Guid.NewGuid(), "Comida Chilena");
            _productoId = producto.Id;

            // Crear Chef (Usuario)
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var chef = Usuario.Crear($"chef.test.{uniqueId}", "Chef Test", $"chef.test.{uniqueId}@restaurante.com", RolUsuario.Cocinero);
            _chefId = chef.Id;

            await DbContext.AddAsync(producto);
            await DbContext.AddAsync(chef);

            // Crear Preparaciones
            var prepDisponible = PreparacionDiaria.Crear(_productoId, 20, _chefId, _dateTimeService.UtcNow.AddDays(1), "Lote de la mañana", _dateTimeService.UtcNow);
            prepDisponible.MarcarComoDisponible();
            _preparacionDisponibleId = prepDisponible.Id;

            var prepPorVencer = PreparacionDiaria.Crear(_productoId, 10, _chefId, _dateTimeService.UtcNow.AddHours(1), "Lote de la tarde, ¡casi se vence!", _dateTimeService.UtcNow);
            prepPorVencer.MarcarComoDisponible();
            prepPorVencer.MarcarComoPorVencer();
            _preparacionPorVencerId = prepPorVencer.Id;
            
            var prepAgotada = PreparacionDiaria.Crear(_productoId, 5, _chefId, _dateTimeService.UtcNow.AddDays(2), "Lote para evento especial", _dateTimeService.UtcNow);
            prepAgotada.MarcarComoDisponible();
            prepAgotada.ConsumirCantidad(5); // Agotarla
            _preparacionAgotadaId = prepAgotada.Id;

            var prepOtroDia = PreparacionDiaria.Crear(_productoId, 15, _chefId, _dateTimeService.UtcNow.AddDays(3), "Preparacion para mañana", _dateTimeService.UtcNow.AddDays(1));
            // No la marcamos como disponible para que no aparezca en las consultas de disponibles

            await DbContext.AddRangeAsync(prepDisponible, prepPorVencer, prepAgotada, prepOtroDia);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarPreparacionCorrecta()
        {
            // Act
            var preparacion = await _repository.ObtenerPorIdAsync(_preparacionDisponibleId);

            // Assert
            preparacion.Should().NotBeNull();
            preparacion.Id.Should().Be(_preparacionDisponibleId);
            preparacion.ProductoId.Should().Be(_productoId);
        }

        [Fact]
        public async Task ObtenerPreparacionesDelDiaAsync_DebeRetornarSoloLasDeHoy()
        {
            // Act
            var preparaciones = await _repository.ObtenerPreparacionesDelDiaAsync();

            // Assert
            preparaciones.Should().NotBeNull();
            preparaciones.Should().HaveCount(3); // Disponible, Por Vencer, Agotada
            preparaciones.Should().OnlyContain(p => p.FechaPreparacion.Date == _dateTimeService.UtcNow.Date);
        }

        [Fact]
        public async Task ObtenerPorChefAsync_DebeRetornarTodasLasPreparacionesDelChef()
        {
            // Act
            var preparaciones = await _repository.ObtenerPorChefAsync(_chefId);

            // Assert
            preparaciones.Should().NotBeNull();
            preparaciones.Should().HaveCount(4);
            preparaciones.Should().OnlyContain(p => p.ChefId == _chefId);
        }

        [Fact]
        public async Task ObtenerPreparacionesDisponiblesPorProductoAsync_DebeRetornarSoloDisponiblesYPorVencer()
        {
            // Act
            var preparaciones = await _repository.ObtenerPreparacionesDisponiblesPorProductoAsync(_productoId);

            // Assert
            preparaciones.Should().NotBeNull();
            preparaciones.Should().HaveCount(2); // Disponible y Por Vencer
            preparaciones.Should().OnlyContain(p => p.Estado == EstadoPreparacion.Disponible || p.Estado == EstadoPreparacion.PorVencer);
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_DebeRetornarPreparacionesCorrectas()
        {
            // Act
            var preparacionesAgotadas = await _repository.ObtenerPorEstadoAsync(EstadoPreparacion.Agotada);

            // Assert
            preparacionesAgotadas.Should().NotBeNull();
            preparacionesAgotadas.Should().ContainSingle();
            preparacionesAgotadas.First().Id.Should().Be(_preparacionAgotadaId);
        }
        
        [Fact]
        public async Task ObtenerPorVencerAsync_DebeRetornarPreparacionesProximasAVencer()
        {
            // Arrange
            // El seed ya tiene una preparación que vence en 1 hora.
            // El método por defecto busca con 2 horas de anticipación.

            // Act
            var preparacionesPorVencer = await _repository.ObtenerPorVencerAsync();

            // Assert
            preparacionesPorVencer.Should().NotBeNull();
            preparacionesPorVencer.Should().ContainSingle();
            preparacionesPorVencer.First().Id.Should().Be(_preparacionPorVencerId);
        }

        // --- TESTS AQUÍ ---

    }
} 