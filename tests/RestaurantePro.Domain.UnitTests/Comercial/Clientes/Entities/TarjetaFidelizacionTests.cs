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
        public void Crear_DatosValidos_DebeCrearTarjeta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var codigo = "FIDELCARD-001";

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
            tarjeta.FechaExpiracion.Should().BeCloseTo(DateTime.Now.AddYears(1), TimeSpan.FromMinutes(1));
            tarjeta.FechaActivacion.Should().BeNull();
            tarjeta.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Crear_CodigoVacio_DebeLanzarArgumentException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var codigo = string.Empty;

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
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-002");

            // Act
            tarjeta.Activar();

            // Assert
            tarjeta.Estado.Should().Be(EstadoTarjeta.Activa);
            tarjeta.FechaActivacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void Activar_TarjetaCancelada_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-003");
            tarjeta.Cancelar("Motivo de prueba");

            // Act
            Action action = () => tarjeta.Activar();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se puede activar*");
        }

        [Fact]
        public void AgregarPuntos_TarjetaActiva_DebeAgregarPuntosYActualizarTotales()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-004");
            tarjeta.Activar();
            var puntos = 100;

            // Act
            var historial = tarjeta.AgregarPuntos(puntos, "Prueba de puntos");

            // Assert
            tarjeta.PuntosAcumulados.Should().Be(puntos);
            tarjeta.PuntosDisponibles.Should().Be(puntos);
            historial.Should().NotBeNull();
            historial.Puntos.Should().Be(puntos);
            historial.TipoOperacion.Should().Be(TipoOperacionPuntos.Agregados);
        }

        [Fact]
        public void AgregarPuntos_TarjetaNoActiva_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-005");
            // Tarjeta solo emitida, no activada

            // Act
            Action action = () => tarjeta.AgregarPuntos(100, "Prueba");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Solo se pueden agregar puntos a tarjetas activas*");
        }

        [Fact]
        public void CanjearPuntos_PuntosDisponiblesSuficientes_DebeCanjearPuntos()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-006");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(200, "Puntos iniciales");
            var puntosCanje = 50;
            var conceptoCanje = "Descuento en restaurante";

            // Act
            var historial = tarjeta.CanjearPuntos(puntosCanje, conceptoCanje);

            // Assert
            tarjeta.PuntosAcumulados.Should().Be(200); // No cambia
            tarjeta.PuntosDisponibles.Should().Be(150); // 200 - 50
            historial.Should().NotBeNull();
            historial.Puntos.Should().Be(puntosCanje);
            historial.TipoOperacion.Should().Be(TipoOperacionPuntos.Canjeados);
            historial.Concepto.Should().Be(conceptoCanje);
        }

        [Fact]
        public void CanjearPuntos_PuntosInsuficientes_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-007");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(50, "Puntos iniciales");

            // Act - Intentar canjear más puntos de los disponibles
            Action action = () => tarjeta.CanjearPuntos(100, "Descuento imposible");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Puntos insuficientes*");
        }

        [Fact]
        public void AgregarPuntosPorCompra_MontoValido_DebeCalcularCorrectamentePuntos()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-008");
            tarjeta.Activar();
            var montoCompra = 1500m;
            var factorConversion = 10; // $10 = 1 punto
            var puntosEsperados = 150;

            // Act
            var historial = tarjeta.AgregarPuntosPorCompra(montoCompra, factorConversion, "Compra en restaurante");

            // Assert
            tarjeta.PuntosAcumulados.Should().Be(puntosEsperados);
            tarjeta.PuntosDisponibles.Should().Be(puntosEsperados);
            historial.Should().NotBeNull();
            historial.Puntos.Should().Be(puntosEsperados);
            historial.MontoCompra.Should().Be(montoCompra);
            historial.FactorConversion.Should().Be(factorConversion);
        }

        [Fact]
        public void ActualizarNivelSegunPuntos_DebeActualizarAutomaticamente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-009");
            tarjeta.Activar();

            // Act & Assert - Nivel Plata (>=1000 puntos)
            tarjeta.AgregarPuntos(1000, "Puntos para nivel Plata");
            tarjeta.NivelFidelizacion.Should().Be(NivelFidelizacion.Plata);

            // Act & Assert - Nivel Oro (>=5000 puntos)
            tarjeta.AgregarPuntos(4000, "Puntos para nivel Oro");
            tarjeta.NivelFidelizacion.Should().Be(NivelFidelizacion.Oro);

            // Act & Assert - Nivel Platino (>=10000 puntos)
            tarjeta.AgregarPuntos(5000, "Puntos para nivel Platino");
            tarjeta.NivelFidelizacion.Should().Be(NivelFidelizacion.Platino);
        }

        [Fact]
        public void ExpirarPuntos_PuntosDisponibles_DebeReducirPuntosDisponibles()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELCARD-010");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(500, "Puntos iniciales");
            var puntosAExpirar = 200;

            // Act
            var historial = tarjeta.ExpirarPuntos(puntosAExpirar, "Expiración anual");

            // Assert
            tarjeta.PuntosAcumulados.Should().Be(500); // No cambia
            tarjeta.PuntosDisponibles.Should().Be(300); // 500 - 200
            historial.Should().NotBeNull();
            historial.Puntos.Should().Be(puntosAExpirar);
            historial.TipoOperacion.Should().Be(TipoOperacionPuntos.Vencidos);
        }
    }
}
