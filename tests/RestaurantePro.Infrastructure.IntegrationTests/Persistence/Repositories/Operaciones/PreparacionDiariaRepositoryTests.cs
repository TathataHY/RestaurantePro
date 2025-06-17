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

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Operaciones
{
    public class PreparacionDiariaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private readonly IPreparacionRepository _repository;
        private Guid _productoId;
        private Guid _chefId;
        private Guid _preparacionDisponibleId;
        private Guid _preparacionPorVencerId;
        private Guid _preparacionAgotadaId;

        public PreparacionDiariaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
            _repository = ServiceProvider.GetRequiredService<IPreparacionRepository>();
        }

        public override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }

        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task SeedPreparacionesAsync()
        {
            // Crear Producto
            var precioProducto = new PrecioProducto(15.50m);
            var producto = Producto.Crear("Tacos al Pastor", "Tacos de cerdo marinado", precioProducto, Guid.NewGuid(), "Comida Mexicana");
            _productoId = producto.Id;

            // Crear Chef (Usuario)
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var chef = Usuario.Crear($"chef.test.{uniqueId}", "Chef Test", $"chef.test.{uniqueId}@restaurante.com", RolUsuario.Cocinero);
            _chefId = chef.Id;

            await DbContext.AddAsync(producto);
            await DbContext.AddAsync(chef);

            // Crear Preparaciones
            var prepDisponible = PreparacionDiaria.Crear(_productoId, 20, _chefId, DateTime.Now.AddDays(1), "Lote de la mañana");
            prepDisponible.MarcarComoDisponible();
            _preparacionDisponibleId = prepDisponible.Id;

            var prepPorVencer = PreparacionDiaria.Crear(_productoId, 10, _chefId, DateTime.Now.AddHours(1), "Lote de la tarde, ¡casi se vence!");
            prepPorVencer.MarcarComoDisponible();
            prepPorVencer.MarcarComoPorVencer();
            _preparacionPorVencerId = prepPorVencer.Id;
            
            var prepAgotada = PreparacionDiaria.Crear(_productoId, 5, _chefId, DateTime.Now.AddDays(2), "Lote para evento especial");
            prepAgotada.MarcarComoDisponible();
            prepAgotada.ConsumirCantidad(5); // Agotarla
            _preparacionAgotadaId = prepAgotada.Id;

            var prepOtroDia = PreparacionDiaria.Crear(_productoId, 15, _chefId, DateTime.Now.AddDays(3), "Preparacion para mañana", DateTime.Now.AddDays(1));
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
            preparaciones.Should().OnlyContain(p => p.FechaPreparacion.Date == DateTime.Today);
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