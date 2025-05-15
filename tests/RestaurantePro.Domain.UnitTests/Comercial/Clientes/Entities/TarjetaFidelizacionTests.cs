using System;
using FluentAssertions;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Events;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using Xunit;

namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Entities
{
    public class TarjetaFidelizacionTests
    {
        [Fact]
        public void Crear_ConDatosValidos_DebeCrearTarjetaFidelizacion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var codigo = "TF-2023-001";

            // Act
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, codigo);

            // Assert
            tarjeta.Should().NotBeNull();
            tarjeta.ClienteId.Should().Be(clienteId);
            tarjeta.Codigo.Should().Be(codigo);
            tarjeta.Estado.Should().Be(EstadoTarjeta.Emitida);
            tarjeta.NivelFidelizacion.Should().Be(NivelFidelizacion.Basico);
            tarjeta.PuntosAcumulados.Should().Be(0);
            tarjeta.PuntosDisponibles.Should().Be(0);
            tarjeta.FechaEmision.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
            tarjeta.FechaActivacion.Should().BeNull();
            tarjeta.FechaExpiracion.Should().NotBeNull();
            tarjeta.FechaExpiracion.Value.Should().BeCloseTo(DateTime.Now.AddYears(1), TimeSpan.FromMinutes(1));
            tarjeta.Id.Should().NotBe(Guid.Empty);
            tarjeta.DomainEvents.Should().ContainSingle(e => e is TarjetaFidelizacionCreadaEvent);
        }

        [Fact]
        public void Crear_CodigoNuloOVacio_DebeLanzarArgumentException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            string codigo = string.Empty;

            // Act
            Action action = () => TarjetaFidelizacion.Crear(clienteId, codigo);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*código*");
        }

        [Fact]
        public void Activar_TarjetaEmitida_DebeActivarTarjeta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.ClearDomainEvents();

            // Act
            tarjeta.Activar();

            // Assert
            tarjeta.Estado.Should().Be(EstadoTarjeta.Activa);
            tarjeta.FechaActivacion.Should().NotBeNull();
            tarjeta.FechaActivacion.Value.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
            tarjeta.DomainEvents.Should().ContainSingle(e => e is TarjetaFidelizacionActivadaEvent);
        }

        [Fact]
        public void Activar_TarjetaYaActiva_NoDebeGenerarEvento()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.ClearDomainEvents();

            // Act
            tarjeta.Activar();

            // Assert
            tarjeta.Estado.Should().Be(EstadoTarjeta.Activa);
            tarjeta.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Activar_TarjetaCancelada_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Cancelar("Prueba");

            // Act
            Action action = () => tarjeta.Activar();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*cancelada*");
        }

        [Fact]
        public void AgregarPuntos_TarjetaActiva_DebeAgregarPuntos()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.ClearDomainEvents();
            var puntosAAgregar = 100;

            // Act
            tarjeta.AgregarPuntos(puntosAAgregar);

            // Assert
            tarjeta.PuntosAcumulados.Should().Be(puntosAAgregar);
            tarjeta.PuntosDisponibles.Should().Be(puntosAAgregar);
            tarjeta.DomainEvents.Should().ContainSingle(e => e is PuntosAgregadosATarjetaEvent);
        }

        [Fact]
        public void AgregarPuntos_TarjetaNoActiva_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");

            // Act
            Action action = () => tarjeta.AgregarPuntos(100);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*activa*");
        }

        [Fact]
        public void CanjearPuntos_PuntosSuficientes_DebeRestarPuntosDisponibles()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(100);
            tarjeta.ClearDomainEvents();
            var puntosACanjear = 50;

            // Act
            tarjeta.CanjearPuntos(puntosACanjear, "Descuento en comida");

            // Assert
            tarjeta.PuntosAcumulados.Should().Be(100);
            tarjeta.PuntosDisponibles.Should().Be(50);
            tarjeta.DomainEvents.Should().ContainSingle(e => e is PuntosCanjeadosEvent);
        }

        [Fact]
        public void CanjearPuntos_PuntosInsuficientes_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(30);

            // Act
            Action action = () => tarjeta.CanjearPuntos(50, "Descuento en comida");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*puntos insuficientes*");
        }

        [Fact]
        public void Suspender_TarjetaActiva_DebeSuspenderTarjeta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.ClearDomainEvents();
            var motivo = "Cliente solicitó suspensión temporal";

            // Act
            tarjeta.Suspender(motivo);

            // Assert
            tarjeta.Estado.Should().Be(EstadoTarjeta.Suspendida);
            tarjeta.DomainEvents.Should().ContainSingle(e => e is TarjetaFidelizacionSuspendidaEvent);
        }

        [Fact]
        public void Cancelar_TarjetaActiva_DebeCancelarTarjeta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.ClearDomainEvents();
            var motivo = "Cliente ya no desea participar";

            // Act
            tarjeta.Cancelar(motivo);

            // Assert
            tarjeta.Estado.Should().Be(EstadoTarjeta.Cancelada);
            tarjeta.DomainEvents.Should().ContainSingle(e => e is TarjetaFidelizacionCanceladaEvent);
        }

        [Fact]
        public void ActualizarNivel_NivelSuperior_DebeActualizarNivel()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.Activar();
            tarjeta.ClearDomainEvents();
            var nuevoNivel = NivelFidelizacion.Oro;

            // Act
            tarjeta.ActualizarNivel(nuevoNivel);

            // Assert
            tarjeta.NivelFidelizacion.Should().Be(nuevoNivel);
            tarjeta.DomainEvents.Should().ContainSingle(e => e is NivelFidelizacionActualizadoEvent);
        }

        [Fact]
        public void ActualizarNivel_MismoNivel_NoDebeGenerarEvento()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2023-001");
            tarjeta.ClearDomainEvents();
            var mismoNivel = NivelFidelizacion.Basico;

            // Act
            tarjeta.ActualizarNivel(mismoNivel);

            // Assert
            tarjeta.NivelFidelizacion.Should().Be(mismoNivel);
            tarjeta.DomainEvents.Should().BeEmpty();
        }
    }
}
