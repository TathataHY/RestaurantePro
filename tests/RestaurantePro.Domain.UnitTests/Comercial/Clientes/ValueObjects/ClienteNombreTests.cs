
namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.ValueObjects
{
    public class ClienteNombreTests
    {
        [Fact]
        public void Crear_ConDatosValidos_DebeCrearClienteNombre()
        {
            // Arrange
            var nombre = "Juan";
            var apellido = "Pérez";

            // Act
            var clienteNombre = ClienteNombre.Crear(nombre, apellido);

            // Assert
            clienteNombre.Should().NotBeNull();
            clienteNombre.Nombre.Should().Be(nombre);
            clienteNombre.Apellido.Should().Be(apellido);
            clienteNombre.NombreCompleto.Should().Be("Juan Pérez");
        }

        [Fact]
        public void Crear_ConNombreVacio_DebeLanzarArgumentException()
        {
            // Arrange
            var nombre = string.Empty;
            var apellido = "Pérez";

            // Act
            Action action = () => ClienteNombre.Crear(nombre, apellido);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*nombre*");
        }

        [Fact]
        public void Crear_ConApellidoVacio_DebeLanzarArgumentException()
        {
            // Arrange
            var nombre = "Juan";
            var apellido = string.Empty;

            // Act
            Action action = () => ClienteNombre.Crear(nombre, apellido);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*apellido*");
        }

        [Fact]
        public void Crear_ConNombreDemasiadoLargo_DebeLanzarArgumentException()
        {
            // Arrange
            var nombre = new string('A', 51);  // 51 caracteres
            var apellido = "Pérez";

            // Act
            Action action = () => ClienteNombre.Crear(nombre, apellido);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*nombre*50*");
        }

        [Fact]
        public void Crear_ConApellidoDemasiadoLargo_DebeLanzarArgumentException()
        {
            // Arrange
            var nombre = "Juan";
            var apellido = new string('A', 51);  // 51 caracteres

            // Act
            Action action = () => ClienteNombre.Crear(nombre, apellido);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*apellido*50*");
        }

        [Fact]
        public void Equals_MismosValores_DebeSerIguales()
        {
            // Arrange
            var nombre1 = ClienteNombre.Crear("Juan", "Pérez");
            var nombre2 = ClienteNombre.Crear("Juan", "Pérez");

            // Act & Assert
            nombre1.Should().Be(nombre2);
            (nombre1 == nombre2).Should().BeTrue();
            (nombre1 != nombre2).Should().BeFalse();
        }

        [Fact]
        public void Equals_DiferentesValores_DebeSerDiferentes()
        {
            // Arrange
            var nombre1 = ClienteNombre.Crear("Juan", "Pérez");
            var nombre2 = ClienteNombre.Crear("Pedro", "Gómez");

            // Act & Assert
            nombre1.Should().NotBe(nombre2);
            (nombre1 == nombre2).Should().BeFalse();
            (nombre1 != nombre2).Should().BeTrue();
        }
    }
}

