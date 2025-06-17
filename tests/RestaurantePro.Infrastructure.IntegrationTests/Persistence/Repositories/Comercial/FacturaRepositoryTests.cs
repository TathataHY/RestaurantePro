using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class FacturaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IFacturaRepository _repository;
        private IUnitOfWork _unitOfWork;
        private Guid _clienteId1;
        private Guid _comandaId1;

        public FacturaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }
        public async Task InitializeAsync()
        {
            _repository = ServiceProvider.GetRequiredService<IFacturaRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();

            await ResetDatabaseAsync();
            await SeedFacturasAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedFacturasAsync()
        {
            _clienteId1 = Guid.NewGuid();
            _comandaId1 = Guid.NewGuid();
            var comandas1 = new List<Guid> { _comandaId1 };

            var factura1 = Factura.Crear(
                "F00001",
                TipoFactura.Venta,
                "Cliente de Prueba",
                _clienteId1,
                "123456789",
                comandasIds: comandas1
            );
            factura1.AgregarDetalle(Guid.NewGuid(), "Producto 1", 1, 100, 0.16m);
            factura1.Emitir();
            factura1.RegistrarPago(50, Guid.NewGuid());


            var factura2 = Factura.Crear(
                "F00002",
                TipoFactura.Venta,
                "Cliente de Prueba",
                _clienteId1,
                "123456789"
            );
            factura2.AgregarDetalle(Guid.NewGuid(), "Producto 2", 2, 50, 0.16m);
            factura2.Emitir();
            factura2.RegistrarPago(factura2.Total, Guid.NewGuid());


            await _repository.AgregarAsync(factura1);
            await _repository.AgregarAsync(factura2);
            await _unitOfWork.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorNumeroAsync_DebeRetornarFactura_CuandoExiste()
        {
            // Act
            var factura = await _repository.ObtenerPorNumeroAsync("F00001");

            // Assert
            factura.Should().NotBeNull();
            factura.NumeroFactura.Should().Be("F00001");
            factura.Detalles.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ObtenerPorComandaAsync_DebeRetornarFacturasAsociadas()
        {
            // Act
            var facturas = await _repository.ObtenerPorComandaAsync(_comandaId1);

            // Assert
            facturas.Should().NotBeNull();
            facturas.Should().ContainSingle(f => f.NumeroFactura == "F00001");
        }

        [Fact]
        public async Task ObtenerPorClienteAsync_DebeRetornarFacturasDelCliente()
        {
            // Act
            var facturas = await _repository.ObtenerPorClienteAsync(_clienteId1);

            // Assert
            facturas.Should().NotBeNull();
            facturas.Should().HaveCount(2);
        }
    }
} 