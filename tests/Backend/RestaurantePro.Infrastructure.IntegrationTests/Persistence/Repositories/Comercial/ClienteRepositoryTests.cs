using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class ClienteRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IClienteRepository _repository;
        private Guid _cliente1Id, _cliente2Id, _cliente3Id;

        public ClienteRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IClienteRepository>();
            await SeedClientesAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedClientesAsync()
        {
            var clientes = new List<Cliente>();

            // Cliente 1: Frecuente, con puntos
            var cliente1 = Cliente.Crear(
                Guid.NewGuid(),
                ClienteNombre.Crear("Juan", "Perez"),
                Email.Create("juan.perez@test.com"),
                PhoneNumber.Create("111222333"),
                new DateTime(1990, 1, 1));
            cliente1.ActualizarSegmento(SegmentoCliente.FrecuenciaAlta);
            cliente1.AgregarPuntos(150);
            cliente1.RegistrarVisita();
            cliente1.RegistrarVisita();
            clientes.Add(cliente1);
            _cliente1Id = cliente1.Id;

            // Cliente 2: Nuevo, sin puntos
            var cliente2 = Cliente.Crear(
                Guid.NewGuid(),
                ClienteNombre.Crear("Maria", "Gomez"),
                Email.Create("maria.gomez@test.com"),
                PhoneNumber.Create("444555666"),
                new DateTime(1985, 5, 10));
            cliente2.ActualizarSegmento(SegmentoCliente.SinClasificar);
            clientes.Add(cliente2);
            _cliente2Id = cliente2.Id;

            // Cliente 3: VIP, muchos puntos y visitas
            var cliente3 = Cliente.Crear(
                Guid.NewGuid(),
                ClienteNombre.Crear("Carlos", "Lopez"),
                Email.Create("carlos.lopez@test.com"),
                PhoneNumber.Create("777888999"),
                new DateTime(1980, 10, 20));
            cliente3.ActualizarSegmento(SegmentoCliente.Premium);
            cliente3.AgregarPuntos(1000);
            for(int i=0; i<10; i++) cliente3.RegistrarVisita();
            clientes.Add(cliente3);
            _cliente3Id = cliente3.Id;
            
            // Cliente 4: Inactivo
            var cliente4 = Cliente.Crear(
                Guid.NewGuid(),
                ClienteNombre.Crear("Ana", "Martinez"),
                Email.Create("ana.martinez@test.com"),
                PhoneNumber.Create("123123123"),
                new DateTime(1995, 3, 15));
            cliente4.Desactivar();
            clientes.Add(cliente4);

            await DbContext.Clientes.AddRangeAsync(clientes);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarCliente_CuandoExiste()
        {
            var cliente = await _repository.ObtenerPorIdAsync(_cliente1Id);
            cliente.Should().NotBeNull();
            cliente!.Id.Should().Be(_cliente1Id);
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_DebeRetornarCliente_CuandoExiste()
        {
            var cliente = await _repository.ObtenerPorEmailAsync("maria.gomez@test.com");
            cliente.Should().NotBeNull();
            cliente!.Id.Should().Be(_cliente2Id);
        }
        
        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_DebeRetornarClientesActivos()
        {
            var clientes = await _repository.ObtenerPorEstadoActivoAsync(true);
            clientes.Should().HaveCount(3);
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_DebeRetornarCliente_CuandoEmailExiste()
        {
            // Arrange
            var emailExistente = "juan.perez@test.com";

            // Act
            var resultado = await _repository.ObtenerPorEmailAsync(emailExistente);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Email.Value.Should().Be(emailExistente);
        }

        [Fact]
        public async Task ObtenerPorSegmentoAsync_DebeRetornarClientesDelSegmentoEspecificado()
        {
            // Arrange
            var segmento = SegmentoCliente.Premium;

            // Act
            var resultado = await _repository.ObtenerPorSegmentoAsync(segmento);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.First().Segmento.Should().Be(segmento);
        }

        [Fact]
        public async Task ObtenerClientesMasFrecuentesAsync_DebeRetornarClientesOrdenadosPorVisitas()
        {
            // Arrange
            var cantidad = 2;

            // Act
            var resultado = await _repository.ObtenerClientesMasFrecuentesAsync(cantidad);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(cantidad);
            resultado.First().Nombre.NombreCompleto.Should().Be("Carlos Lopez");
            resultado.First().CantidadVisitas.Should().Be(10);
        }

        [Fact]
        public async Task ObtenerPorPuntosMinimosAsync_DebeRetornarClientesConPuntosSuficientes()
        {
            // Arrange
            var puntosMinimos = 500;

            // Act
            var resultado = await _repository.ObtenerPorPuntosMinimosAsync(puntosMinimos);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.First().PuntosAcumulados.Should().BeGreaterThanOrEqualTo(puntosMinimos);
            resultado.First().Nombre.NombreCompleto.Should().Be("Carlos Lopez");
        }
    }
} 