using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using RestaurantePro.Domain.Proveedores.Enums;
using System.Linq;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Proveedores
{
    public class ProveedorRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IProveedorRepository _repository = null!;
        private Guid _proveedorId1;
        private Guid _proveedorId2;
        private string _proveedorRut1 = "12345678-9";

        public ProveedorRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IProveedorRepository>();
            await SeedDataAsync();
        }

        private async Task SeedDataAsync()
        {
            var proveedor1 = Proveedor.Crear(
                "Proveedor Test 1", "Contacto 1", "test1@proveedor.com", "111111111",
                "Direccion 1", "Ciudad 1", "1111111", "Pais 1", _proveedorRut1, "Info Bancaria 1", 30);
            _proveedorId1 = proveedor1.Id;

            var proveedor2 = Proveedor.Crear(
                "Proveedor Test 2", "Contacto 2", "test2@proveedor.com", "222222222",
                "Direccion 2", "Ciudad 2", "2222222", "Pais 2", "P2P2P2P2P2P2", "Info Bancaria 2", 60);
            proveedor2.AgregarCategoria(CategoriaProveedor.AlimentosBasicos);
            _proveedorId2 = proveedor2.Id;

            var proveedorInactivo = Proveedor.Crear(
                "Proveedor Inactivo", "Contacto 3", "test3@proveedor.com", "333333333",
                "Direccion 3", "Ciudad 1", "3333333", "Pais 1", "P3P3P3P3P3P3", "Info Bancaria 3", 15);
            proveedorInactivo.Desactivar("Ya no se usa");


            await DbContext.AddRangeAsync(proveedor1, proveedor2, proveedorInactivo);
            await DbContext.SaveChangesAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarProveedor_CuandoExiste()
        {
            // Act
            var proveedor = await _repository.ObtenerPorIdAsync(_proveedorId1);

            // Assert
            proveedor.Should().NotBeNull();
            proveedor.Id.Should().Be(_proveedorId1);
            proveedor.Nombre.Should().Be("Proveedor Test 1");
        }

        [Fact]
        public async Task ObtenerPorNombreAsync_DebeRetornarProveedor_CuandoExiste()
        {
            // Act
            var proveedor = await _repository.ObtenerPorNombreAsync("Proveedor Test 2");

            // Assert
            proveedor.Should().NotBeNull();
            proveedor.Id.Should().Be(_proveedorId2);
        }

        [Fact]
        public async Task ObtenerPorRUTAsync_DebeRetornarProveedor_CuandoExiste()
        {
            // Act
            var proveedores = await _repository.ObtenerPorRUTAsync(_proveedorRut1);

            // Assert
            proveedores.Should().NotBeNull();
            proveedores.RFC.Should().Be(_proveedorRut1);
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarTodosLosProveedores()
        {
            // Act
            var proveedores = await _repository.ObtenerTodosAsync();

            // Assert
            // 3 seeded + others that might be in the db from other tests
            proveedores.Count().Should().BeGreaterThanOrEqualTo(3);
        }
        
        [Fact]
        public async Task ObtenerActivosAsync_DebeRetornarSoloProveedoresActivos()
        {
            // Act
            var proveedores = await _repository.ObtenerActivosAsync();

            // Assert
            proveedores.Should().NotBeNull();
            proveedores.Should().OnlyContain(p => p.Activo);
            proveedores.Should().HaveCount(2); // Based on seed data
        }

        [Theory]
        [InlineData("Proveedor Test", 2)]
        [InlineData("Ciudad 1", 2)] // one active, one inactive
        [InlineData("test1@proveedor.com", 1)]
        public async Task BuscarAsync_DebeRetornarProveedoresCoincidentes(string termino, int expectedCount)
        {
            // Act
            var proveedores = await _repository.BuscarAsync(termino);
            
            // Assert
            proveedores.Should().NotBeNull();
            proveedores.Should().HaveCount(expectedCount);
        }

        [Fact]
        public async Task ObtenerPaginadoAsync_DebeRetornarResultadosPaginados()
        {
            // Act
            var (items, total) = await _repository.ObtenerPaginadoAsync(0, 2);

            // Assert
            items.Should().NotBeNull();
            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        [Fact]
        public async Task ObtenerPorCategoriaAsync_DebeRetornarProveedoresCorrectos()
        {
            // Act
            var proveedores = await _repository.ObtenerPorCategoriaAsync(CategoriaProveedor.AlimentosBasicos);

            // Assert
            proveedores.Should().ContainSingle();
            proveedores.First().Id.Should().Be(_proveedorId2);
        }
    }
} 