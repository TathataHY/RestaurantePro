using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Inventario
{
    public class OrdenCompraRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IOrdenCompraRepository _repository = null!;
        private Guid _proveedorId;
        private Guid _ingredienteId1;
        private Guid _ingredienteId2;
        private Guid _ordenPendienteId;
        private Guid _ordenEnviadaId;
        private Guid _ordenCanceladaId;

        public OrdenCompraRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IOrdenCompraRepository>();
            await SeedDataAsync();
        }

        private async Task SeedDataAsync()
        {
            // 1. Crear Proveedor
            var proveedor = Proveedor.Crear(
                "Proveedor de Prueba", "Contacto", "contacto@proveedor.com", "123456789",
                "Direccion 123", "Ciudad", "1234567", "Pais", "PROV010101XYZ", "Info Bancaria", 30);
            _proveedorId = proveedor.Id;

            // 2. Crear Ingredientes
            var ingrediente1 = Ingrediente.Crear("Tomate", "TOM-01", "Tomate Saladet", UnidadMedida.Kilogramo, 10, 50);
            _ingredienteId1 = ingrediente1.Id;
            var ingrediente2 = Ingrediente.Crear("Cebolla", "CEB-01", "Cebolla Blanca", UnidadMedida.Kilogramo, 5, 20);
            _ingredienteId2 = ingrediente2.Id;

            await DbContext.AddRangeAsync(proveedor, ingrediente1, ingrediente2);

            // 3. Crear Ordenes de Compra
            // Orden Pendiente con items
            var ordenPendiente = OrdenCompra.Crear(_proveedorId, "Orden pendiente de prueba", DateTime.Now);
            ordenPendiente.EstablecerFechaEntrega(DateTime.Now.AddDays(7));
            ordenPendiente.AgregarItem(_ingredienteId1, "Tomate", 10, UnidadMedida.Kilogramo);
            ordenPendiente.AgregarItem(_ingredienteId2, "Cebolla", 5, UnidadMedida.Kilogramo);
            _ordenPendienteId = ordenPendiente.Id;

            // Orden Enviada
            var ordenEnviada = OrdenCompra.Crear(_proveedorId, "Orden enviada de prueba", DateTime.Now.AddDays(-5));
            ordenEnviada.EstablecerFechaEntrega(DateTime.Now.AddDays(2));
            ordenEnviada.AgregarItem(_ingredienteId1, "Tomate", 20, UnidadMedida.Kilogramo);
            ordenEnviada.Enviar();
            _ordenEnviadaId = ordenEnviada.Id;

            // Orden Cancelada
            var ordenCancelada = OrdenCompra.Crear(_proveedorId, "Orden cancelada de prueba", DateTime.Now.AddDays(-10));
            ordenCancelada.EstablecerFechaEntrega(DateTime.Now.AddDays(-5));
            ordenCancelada.AgregarItem(_ingredienteId1, "Tomate", 5, UnidadMedida.Kilogramo);
            ordenCancelada.Cancelar("Motivo de prueba");
            _ordenCanceladaId = ordenCancelada.Id;

            await DbContext.AddRangeAsync(ordenPendiente, ordenEnviada, ordenCancelada);
            await DbContext.SaveChangesAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task ObtenerPorIdConItemsAsync_DebeRetornarOrdenConSusItems()
        {
            // Act
            var orden = await _repository.ObtenerPorIdConItemsAsync(_ordenPendienteId);

            // Assert
            orden.Should().NotBeNull();
            orden.Id.Should().Be(_ordenPendienteId);
            orden.Items.Should().NotBeEmpty();
            orden.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_DebeRetornarOrdenesCorrectas()
        {
            // Act
            var ordenesPendientes = await _repository.ObtenerPorEstadoAsync(EstadoOrdenCompra.Pendiente);
            var ordenesEnviadas = await _repository.ObtenerPorEstadoAsync(EstadoOrdenCompra.Enviada);
            var ordenesCanceladas = await _repository.ObtenerPorEstadoAsync(EstadoOrdenCompra.Cancelada);

            // Assert
            ordenesPendientes.Should().ContainSingle(o => o.Id == _ordenPendienteId);
            ordenesEnviadas.Should().ContainSingle(o => o.Id == _ordenEnviadaId);
            ordenesCanceladas.Should().ContainSingle(o => o.Id == _ordenCanceladaId);
        }

        [Fact]
        public async Task ObtenerPorProveedorAsync_DebeRetornarTodasLasOrdenesDelProveedor()
        {
            // Act
            var ordenes = await _repository.ObtenerPorProveedorAsync(_proveedorId);

            // Assert
            ordenes.Should().NotBeNull();
            ordenes.Should().HaveCount(3);
            ordenes.Should().OnlyContain(o => o.ProveedorId == _proveedorId);
        }
    }
} 