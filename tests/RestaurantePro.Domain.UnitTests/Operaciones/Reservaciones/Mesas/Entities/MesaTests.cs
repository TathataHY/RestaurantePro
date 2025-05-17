using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Mesas.Entities
{
    public class MesaTests
    {
        [Fact]
        public void CrearMesa_ConParametrosValidos_DebeCrearMesaConEstadoDisponible()
        {
            // Arrange
            var numero = 10;
            var capacidad = 4;
            var ubicacion = "Terraza";

            // Act
            var mesa = Mesa.Crear(numero, capacidad, ubicacion);

            // Assert
            mesa.Should().NotBeNull();
            mesa.Id.Should().NotBe(Guid.Empty);
            mesa.Numero.Should().Be(numero);
            mesa.Capacidad.Should().Be(capacidad);
            mesa.Ubicacion.Should().Be(ubicacion);
            mesa.Estado.Should().Be(EstadoMesa.Disponible);

            // Verificamos que se generó el evento de dominio
            mesa.DomainEvents.Should().ContainSingle(e => e is MesaCreada);
            var evento = mesa.DomainEvents.OfType<MesaCreada>().First();
            evento.MesaId.Should().Be(mesa.Id);
            evento.Numero.Should().Be(numero);
            evento.Ubicacion.Should().Be(ubicacion);
        }

        [Fact]
        public void MarcarComoOcupada_CuandoMesaEstaDisponible_DebeCambiarEstadoAOcupada()
        {
            // Arrange
            var mesa = Mesa.Crear(1, 4, "Interior");

            // Act
            mesa.MarcarComoOcupada();

            // Assert
            mesa.Estado.Should().Be(EstadoMesa.Ocupada);

            // Verificar evento de dominio
            mesa.DomainEvents.Should().Contain(e => e is MesaOcupada);
            var evento = mesa.DomainEvents.OfType<MesaOcupada>().Last();
            evento.MesaId.Should().Be(mesa.Id);
        }

        [Fact]
        public void MarcarComoReservada_CuandoMesaEstaDisponible_DebeCambiarEstadoAReservada()
        {
            // Arrange
            var mesa = Mesa.Crear(1, 4, "Interior");

            // Act
            mesa.MarcarComoReservada();

            // Assert
            mesa.Estado.Should().Be(EstadoMesa.Reservada);

            // Verificar evento de dominio
            mesa.DomainEvents.Should().Contain(e => e is MesaReservada);
            var evento = mesa.DomainEvents.OfType<MesaReservada>().Last();
            evento.MesaId.Should().Be(mesa.Id);
        }

        [Fact]
        public void MarcarComoDisponible_CuandoMesaEstaOcupada_DebeCambiarEstadoADisponible()
        {
            // Arrange
            var mesa = Mesa.Crear(1, 4, "Interior");
            mesa.MarcarComoOcupada();

            // Act
            mesa.MarcarComoDisponible();

            // Assert
            mesa.Estado.Should().Be(EstadoMesa.Disponible);

            // Verificar evento de dominio
            mesa.DomainEvents.Should().Contain(e => e is MesaDisponible);
            var evento = mesa.DomainEvents.OfType<MesaDisponible>().Last();
            evento.MesaId.Should().Be(mesa.Id);
        }

        [Fact]
        public void MarcarComoOcupada_CuandoMesaNoEstaDisponible_DebeLanzarExcepcion()
        {
            // Arrange
            var mesa = Mesa.Crear(1, 4, "Interior");
            mesa.MarcarComoReservada(); // Primero la marcamos como reservada

            // Act & Assert
            Action action = () => mesa.MarcarComoOcupada(); // Intentamos marcarla como ocupada estando reservada
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*no puede marcarse como ocupada*");
        }

        [Fact]
        public void CrearMesa_ConNumeroNegativo_DebeLanzarExcepcion()
        {
            // Arrange
            var numero = -5;
            var capacidad = 4;
            var ubicacion = "Terraza";

            // Act & Assert
            Action action = () => Mesa.Crear(numero, capacidad, ubicacion);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*número de mesa no puede ser negativo*");
        }

        [Fact]
        public void CrearMesa_ConCapacidadCero_DebeLanzarExcepcion()
        {
            // Arrange
            var numero = 1;
            var capacidad = 0;
            var ubicacion = "Terraza";

            // Act & Assert
            Action action = () => Mesa.Crear(numero, capacidad, ubicacion);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*capacidad debe ser mayor que cero*");
        }
    }
}

