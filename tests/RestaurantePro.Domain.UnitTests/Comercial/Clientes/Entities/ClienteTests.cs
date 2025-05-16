namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Entities
{
    public class ClienteTests
    {
        [Fact]
        public void Crear_ConDatosValidos_DebeCrearCliente()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var email = "juan.perez@example.com";
            var telefono = "+34612345678";

            // Act
            var cliente = Cliente.Crear(nombre, email, telefono);

            // Assert
            cliente.Should().NotBeNull();
            cliente.Nombre.Should().Be(nombre);
            cliente.Email.Should().Be(email);
            cliente.Telefono.Should().Be(telefono);
            cliente.EstaActivo.Should().BeTrue();
            cliente.PuntosAcumulados.Should().Be(0);
            cliente.Id.Should().NotBe(Guid.Empty);
            cliente.DomainEvents.Should().ContainSingle(e => e is ClienteCreadoEvent);
        }

        [Fact]
        public void AgregarPuntos_ClienteActivo_DebeAgregarPuntos()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            var puntosAAgregar = 50;

            // Act
            cliente.AgregarPuntos(puntosAAgregar);

            // Assert
            cliente.PuntosAcumulados.Should().Be(puntosAAgregar);
            cliente.DomainEvents.Should().ContainSingle(e => e is PuntosAgregadosEvent);
            var evento = cliente.DomainEvents.OfType<PuntosAgregadosEvent>().First();
            evento.PuntosAgregados.Should().Be(puntosAAgregar);
            evento.PuntosTotales.Should().Be(puntosAAgregar);
        }

        [Fact]
        public void AgregarPuntos_ClienteInactivo_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();

            // Act
            Action action = () => cliente.AgregarPuntos(50);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*inactivo*");
        }

        [Fact]
        public void AgregarPuntos_CantidadNegativa_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");

            // Act
            Action action = () => cliente.AgregarPuntos(-10);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*mayor a cero*");
        }

        [Fact]
        public void Desactivar_ClienteActivo_DebeDesactivarCliente()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");

            // Act
            cliente.Desactivar();

            // Assert
            cliente.EstaActivo.Should().BeFalse();
            cliente.DomainEvents.Should().ContainSingle(e => e is ClienteDesactivadoEvent);
        }

        [Fact]
        public void Desactivar_ClienteYaInactivo_NoDebeGenerarEvento()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();
            cliente.ClearDomainEvents();

            // Act
            cliente.Desactivar();

            // Assert
            cliente.EstaActivo.Should().BeFalse();
            cliente.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Reactivar_ClienteInactivo_DebeReactivarCliente()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();
            cliente.ClearDomainEvents();

            // Act
            cliente.Reactivar();

            // Assert
            cliente.EstaActivo.Should().BeTrue();
            cliente.DomainEvents.Should().ContainSingle(e => e is ClienteReactivadoEvent);
        }

        [Fact]
        public void ActualizarInformacionContacto_NuevosValues_DebeActualizarInformacion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            var nuevoEmail = "nuevo.juan@example.com";
            var nuevoTelefono = "687654321";

            // Act
            cliente.ActualizarInformacionContacto(nuevoEmail, nuevoTelefono);

            // Assert
            cliente.Email.Should().Be(nuevoEmail);
            cliente.Telefono.Should().Be(nuevoTelefono);
            cliente.DomainEvents.Should().ContainSingle(e => e is InformacionContactoActualizadaEvent);
        }

        [Fact]
        public void ActualizarInformacionContacto_MismosValues_NoDebeGenerarEvento()
        {
            // Arrange
            var email = "juan@example.com";
            var telefono = "612345678";
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, email, telefono);
            cliente.ClearDomainEvents();

            // Act
            cliente.ActualizarInformacionContacto(email, telefono);

            // Assert
            cliente.Email.Should().Be(email);
            cliente.Telefono.Should().Be(telefono);
            cliente.DomainEvents.Should().BeEmpty();
        }
    }
}
