using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class ClienteRepositoryTests : IntegrationTestBase
    {
        private readonly ClienteRepository _repository;
        private readonly Mock<ILogger<ClienteRepository>> _loggerMock;

        // IDs para usar en las pruebas
        private Guid _clienteId1;
        private Guid _clienteId2;
        private Guid _clienteId3;
        private Guid _tarjetaId1;

        public ClienteRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<ClienteRepository>>();
            _repository = new ClienteRepository(DbContext, _loggerMock.Object);
        }

        protected override void SeedDatabase()
        {
            base.SeedDatabase();

            var cliente1 = Cliente.Crear(
                ClienteNombre.Crear("Juan", "Pérez García"),
                "juan.perez@test.com",
                "+521234567890",
                new DateTime(1990, 1, 15)
            );
            cliente1.RegistrarVisita();
            cliente1.RegistrarVisita();
            cliente1.AgregarPuntos(150);
            cliente1.ActualizarSegmento(SegmentoCliente.Premium);
            _clienteId1 = cliente1.Id;

            var tarjeta1 = TarjetaFidelizacion.Crear(cliente1.Id, "T-001");
            tarjeta1.Activar();
            cliente1.AsociarTarjetaFidelizacion(tarjeta1.Id);
            _tarjetaId1 = tarjeta1.Id;

            var cliente2 = Cliente.Crear(
                ClienteNombre.Crear("Ana", "López Martínez"),
                "ana.lopez@test.com",
                "+529876543210",
                new DateTime(1985, 5, 20)
            );
            cliente2.RegistrarVisita();
            cliente2.AgregarPuntos(50);
            cliente2.ActualizarSegmento(SegmentoCliente.Regular);
            _clienteId2 = cliente2.Id;

            var cliente3 = Cliente.Crear(
                ClienteNombre.Crear("Carlos", "García Sánchez"),
                "carlos.garcia@test.com",
                "+525555555555",
                new DateTime(2000, 10, 1)
            );
            cliente3.Desactivar(); // Cliente inactivo
            _clienteId3 = cliente3.Id;
            
            DbContext.Set<Cliente>().AddRange(cliente1, cliente2, cliente3);
            DbContext.Set<TarjetaFidelizacion>().Add(tarjeta1);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarCliente_CuandoExiste()
        {
            var cliente = await _repository.ObtenerPorIdAsync(_clienteId1);
            cliente.Should().NotBeNull();
            cliente!.Id.Should().Be(_clienteId1);
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_DebeRetornarCliente_CuandoExiste()
        {
            var cliente = await _repository.ObtenerPorEmailAsync("ana.lopez@test.com");
            cliente.Should().NotBeNull();
            cliente!.Id.Should().Be(_clienteId2);
        }

        [Fact]
        public async Task ObtenerPorNombreAsync_DebeRetornarClientesCoincidentes()
        {
            var clientes = await _repository.ObtenerPorNombreAsync("García");
            clientes.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerPorSegmentoAsync_DebeRetornarClientesDelSegmento()
        {
            var clientes = await _repository.ObtenerPorSegmentoAsync(SegmentoCliente.Premium);
            clientes.Should().ContainSingle();
            clientes.First().Id.Should().Be(_clienteId1);
        }

        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_DebeRetornarClientesActivos()
        {
            var clientes = await _repository.ObtenerPorEstadoActivoAsync(true);
            clientes.Should().HaveCount(2);
        }
        
        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_DebeRetornarClientesInactivos()
        {
            var clientes = await _repository.ObtenerPorEstadoActivoAsync(false);
            clientes.Should().ContainSingle();
            clientes.First().Id.Should().Be(_clienteId3);
        }

        [Fact]
        public async Task ObtenerConTarjetaFidelizacionAsync_DebeRetornarClientesConTarjeta()
        {
            var clientes = await _repository.ObtenerConTarjetaFidelizacionAsync();
            clientes.Should().ContainSingle();
            clientes.First().Id.Should().Be(_clienteId1);
        }

        [Fact]
        public async Task ObtenerClientesMasFrecuentesAsync_DebeRetornarClientesOrdenadosPorVisitas()
        {
            var clientes = await _repository.ObtenerClientesMasFrecuentesAsync(1);
            clientes.Should().ContainSingle();
            clientes.First().Id.Should().Be(_clienteId1);
        }

        [Fact]
        public async Task ObtenerPorPuntosMinimosAsync_DebeRetornarClientesConPuntosSuficientes()
        {
            var clientes = await _repository.ObtenerPorPuntosMinimosAsync(100);
            clientes.Should().ContainSingle();
            clientes.First().Id.Should().Be(_clienteId1);
        }
        
        [Fact]
        public async Task ObtenerPaginadoAsync_DebeRetornarResultadosPaginados()
        {
            var (clientes, total) = await _repository.ObtenerPaginadoAsync(0, 1);
            total.Should().Be(3);
            clientes.Should().HaveCount(1);
        }

        [Fact]
        public async Task VerificarExistenciaAsync_DebeRetornarTrue_SiClienteExiste()
        {
            var existe = await _repository.VerificarExistenciaAsync(_clienteId1);
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerClientesPorIdsAsync_DebeRetornarClientesCorrectos()
        {
            var ids = new List<Guid> { _clienteId1, _clienteId3 };
            var clientes = await _repository.ObtenerClientesPorIdsAsync(ids);
            clientes.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerClientesActivosConVisitasAsync_DebeRetornarClienteConVisitasSuficientes()
        {
            var clientes = await _repository.ObtenerClientesActivosConVisitasAsync(2, 365);
            clientes.Should().ContainSingle();
            clientes.First().Id.Should().Be(_clienteId1);
        }

        [Fact]
        public async Task GuardarAsync_DebeAgregarNuevoCliente()
        {
            var nuevoCliente = Cliente.Crear(
                ClienteNombre.Crear("Nuevo", "Cliente Test"),
                "nuevo.cliente@test.com",
                "+521122334455",
                new DateTime(1995, 3, 3)
            );
            
            await _repository.GuardarAsync(nuevoCliente);
            
            var clienteGuardado = await _repository.ObtenerPorIdAsync(nuevoCliente.Id);
            clienteGuardado.Should().NotBeNull();
        }

        [Fact]
        public async Task GuardarAsync_DebeActualizarClienteExistente()
        {
            var cliente = await _repository.ObtenerPorIdAsync(_clienteId1);
            cliente!.AgregarPuntos(100);

            await _repository.GuardarAsync(cliente);

            var clienteActualizado = await _repository.ObtenerPorIdAsync(_clienteId1);
            clienteActualizado!.PuntosAcumulados.Should().Be(250);
        }
    }
} 