using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Repositories
{
    public class ClienteRepositoryTests
    {
        private readonly Mock<IClienteRepository> _mockRepository;
        private readonly List<Cliente> _clientes;

        public ClienteRepositoryTests()
        {
            _mockRepository = new Mock<IClienteRepository>();

            // Crear datos de prueba
            _clientes = new List<Cliente>
            {
                Cliente.Crear(ClienteNombre.Crear("Juan", "Pérez"), "juan.perez@example.com", "612345678"),
                Cliente.Crear(ClienteNombre.Crear("María", "López"), "maria.lopez@example.com", "623456789"),
                Cliente.Crear(ClienteNombre.Crear("Carlos", "Rodríguez"), "carlos.rodriguez@example.com", "634567890")
            };

            // Desactivar uno de los clientes para pruebas
            _clientes[2].Desactivar();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarCliente()
        {
            // Arrange
            var clienteId = _clientes[0].Id;
            var clienteEsperado = _clientes[0];

            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clienteEsperado);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(clienteId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(clienteEsperado);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_EmailExistente_DebeRetornarCliente()
        {
            // Arrange
            var email = "maria.lopez@example.com";
            var clienteEsperado = _clientes[1];

            _mockRepository.Setup(repo => repo.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clienteEsperado);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEmailAsync(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(clienteEsperado);
            resultado.Email.Should().Be(email);
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarTodosLosClientes()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_clientes);

            // Act
            var resultado = await _mockRepository.Object.ObtenerTodosAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3);
            resultado.Should().BeEquivalentTo(_clientes);
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_ClientesActivos_DebeRetornarSoloActivos()
        {
            // Arrange
            var clientesActivos = _clientes.Where(c => c.EstaActivo).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientesActivos);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoAsync(true);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Tenemos 2 clientes activos
            resultado.All(c => c.EstaActivo).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_ClientesInactivos_DebeRetornarSoloInactivos()
        {
            // Arrange
            var clientesInactivos = _clientes.Where(c => !c.EstaActivo).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientesInactivos);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoAsync(false);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Tenemos 1 cliente inactivo
            resultado.All(c => !c.EstaActivo).Should().BeTrue();
        }

        [Fact]
        public async Task AgregarAsync_ClienteValido_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevoCliente = Cliente.Crear(
                ClienteNombre.Crear("Pedro", "Gómez"),
                "pedro.gomez@example.com",
                "645678901");

            _mockRepository.Setup(repo => repo.AgregarAsync(nuevoCliente, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.AgregarAsync(nuevoCliente);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevoCliente, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
            resultadoGuardado.Should().Be(1);
        }

        [Fact]
        public async Task ActualizarAsync_ClienteExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var cliente = _clientes[0];
            cliente.ActualizarInformacionContacto("juan.nuevo@example.com", "687654321");

            _mockRepository.Setup(repo => repo.ActualizarAsync(cliente, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.ActualizarAsync(cliente);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(cliente, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
            resultadoGuardado.Should().Be(1);
        }
    }
}
