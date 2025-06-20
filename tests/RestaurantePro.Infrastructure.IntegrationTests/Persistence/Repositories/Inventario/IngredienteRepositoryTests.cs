using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Inventario
{
    public class IngredienteRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IIngredienteRepository _repository = null!;
        private Guid _tomateId;
        private Guid _aguacateId;
        private Guid _cebollaId;
        private Guid _quesoId;
        private Guid _polloId;
        private Guid _calabazaId;
        private Guid _limonId;
        private Guid _proveedorId;

        public IngredienteRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IIngredienteRepository>();
            await SeedIngredientesAsync();
        }

        private async Task SeedIngredientesAsync()
        {
            // Proveedor
            var proveedor = Proveedor.Crear(
                "Lacteos S.A. de C.V.",
                "Juan Perez",
                "contacto@lacteos.com",
                "555-123-4567",
                "Calle Falsa 123",
                "Santiago",
                "7640000",
                "Chile",
                "LACSA990101XXX",
                "Cuenta 12345",
                30
            );
            _proveedorId = proveedor.Id;
            await DbContext.AddAsync(proveedor);

            // Ingredientes
            var tomate = Ingrediente.Crear("Tomate", "TOM-001", "Tomate Saladet", UnidadMedida.Kilogramo, 5, 20);
            _tomateId = tomate.Id;

            var aguacate = Ingrediente.Crear("Aguacate", "AGU-001", "Aguacate Hass", UnidadMedida.Kilogramo, 10, 8);
            _aguacateId = aguacate.Id;

            var cebolla = Ingrediente.Crear("Cebolla Morada", "CEB-002", "Cebolla morada para tacos", UnidadMedida.Kilogramo, 2, 15);
            cebolla.Desactivar();
            _cebollaId = cebolla.Id;

            var queso = Ingrediente.Crear("Queso Oaxaca", "QSO-001", "Queso fresco de Oaxaca", UnidadMedida.Kilogramo, 4, 10);
            queso.AsociarProveedorPrincipal(_proveedorId);
            _quesoId = queso.Id;

            var pollo = Ingrediente.Crear("Pollo", "POL-001", "Pechuga de pollo", UnidadMedida.Kilogramo, 10, 50);
            pollo.ActualizarBloqueoControlCalidad(true, "Revisión de lote");
            _polloId = pollo.Id;

            var calabaza = Ingrediente.Crear("Calabaza", "CAL-001", "Calabaza de Castilla", UnidadMedida.Piezas, 3, 10, RotacionIngrediente.Baja, TemporadaIngrediente.Invierno);
            _calabazaId = calabaza.Id;
            
            var limon = Ingrediente.Crear("Limón", "LIM-001", "Limón con semilla", UnidadMedida.Kilogramo, 5, 30, RotacionIngrediente.Alta);
            _limonId = limon.Id;

            await DbContext.AddRangeAsync(tomate, aguacate, cebolla, queso, pollo, calabaza, limon);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarIngredienteCorrecto()
        {
            // Act
            var ingrediente = await _repository.ObtenerPorIdAsync(_tomateId);

            // Assert
            ingrediente.Should().NotBeNull();
            ingrediente.Id.Should().Be(_tomateId);
            ingrediente.Nombre.Should().Be("Tomate");
        }

        [Fact]
        public async Task ObtenerPorNombreAsync_DebeRetornarIngredientesCoincidentes()
        {
            // Act
            var ingredientes = await _repository.ObtenerPorNombreAsync("Aguacat");

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Nombre.Should().Be("Aguacate");
        }

        [Fact]
        public async Task ObtenerConStockBajoAsync_DebeRetornarIngredientesCorrectos()
        {
            // Act
            var ingredientes = await _repository.ObtenerConStockBajoAsync();

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Id.Should().Be(_aguacateId);
        }
        
        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_DebeRetornarIngredientesActivos()
        {
            // Act
            var ingredientes = await _repository.ObtenerPorEstadoActivoAsync(true);

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().HaveCount(6); // Todos menos la cebolla
            ingredientes.Should().NotContain(i => i.Id == _cebollaId);
        }
        
        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_DebeRetornarIngredientesInactivos()
        {
            // Act
            var ingredientes = await _repository.ObtenerPorEstadoActivoAsync(false);

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Id.Should().Be(_cebollaId);
        }
        
        [Fact]
        public async Task ObtenerBloqueadosPorCalidadAsync_DebeRetornarSoloBloqueados()
        {
            // Act
            var ingredientes = await _repository.ObtenerBloqueadosPorCalidadAsync();

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Id.Should().Be(_polloId);
        }

        [Fact]
        public async Task ObtenerPorProveedorAsync_DebeRetornarIngredientesDelProveedor()
        {
            // Act
            var ingredientes = await _repository.ObtenerPorProveedorAsync(_proveedorId);

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Id.Should().Be(_quesoId);
        }

        [Fact]
        public async Task ObtenerPorRotacionAsync_DebeRetornarIngredientesCorrectos()
        {
            // Act
            var ingredientes = await _repository.ObtenerPorRotacionAsync(RotacionIngrediente.Alta);

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Id.Should().Be(_limonId);
        }

        [Fact]
        public async Task ObtenerPorTemporadaAsync_DebeRetornarIngredientesDeTemporada()
        {
            // Act
            var ingredientes = await _repository.ObtenerPorTemporadaAsync(TemporadaIngrediente.Invierno);

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().ContainSingle();
            ingredientes.First().Id.Should().Be(_calabazaId);
        }
    }
} 