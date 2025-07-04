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
                Cliente.Crear(
                    Guid.NewGuid(), 
                    ClienteNombre.Crear("Juan", "Pérez"), 
                    Email.Create("juan.perez@example.com"), 
                    PhoneNumber.Create("612345678"),
                    DateTime.Now.AddYears(-30)),
                Cliente.Crear(
                    Guid.NewGuid(), 
                    ClienteNombre.Crear("María", "López"), 
                    Email.Create("maria.lopez@example.com"), 
                    PhoneNumber.Create("623456789"),
                    DateTime.Now.AddYears(-28)),
                Cliente.Crear(
                    Guid.NewGuid(), 
                    ClienteNombre.Crear("Carlos", "Rodríguez"), 
                    Email.Create("carlos.rodriguez@example.com"), 
                    PhoneNumber.Create("634567890"),
                    DateTime.Now.AddYears(-35))
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

            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(clienteId, CancellationToken.None))
                .ReturnsAsync(clienteEsperado);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(clienteId, CancellationToken.None);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(clienteEsperado);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(clienteId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ObtenerPorEmailAsync_EmailExistente_DebeRetornarCliente()
        {
            // Arrange
            var email = "maria.lopez@example.com";
            var clienteEsperado = _clientes[1];

            _mockRepository.Setup(repo => repo.ObtenerPorEmailAsync(email, CancellationToken.None))
                .ReturnsAsync(clienteEsperado);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEmailAsync(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(clienteEsperado);
            resultado.Email.Value.Should().Be(email);
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarTodosLosClientes()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerTodosAsync(CancellationToken.None))
                .ReturnsAsync(_clientes);

            // Act
            var resultado = await _mockRepository.Object.ObtenerTodosAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3);
            resultado.Should().BeEquivalentTo(_clientes);
        }

        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_ClientesActivos_DebeRetornarSoloActivos()
        {
            // Arrange
            var clientesActivos = _clientes.Where(c => c.EstaActivo).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoActivoAsync(true, CancellationToken.None))
                .ReturnsAsync(clientesActivos);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoActivoAsync(true);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Tenemos 2 clientes activos
            resultado.All(c => c.EstaActivo).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorEstadoActivoAsync_ClientesInactivos_DebeRetornarSoloInactivos()
        {
            // Arrange
            var clientesInactivos = _clientes.Where(c => !c.EstaActivo).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoActivoAsync(false, CancellationToken.None))
                .ReturnsAsync(clientesInactivos);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoActivoAsync(false);

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
                Guid.NewGuid(),
                ClienteNombre.Crear("Pedro", "Gómez"),
                Email.Create("pedro.gomez@example.com"),
                PhoneNumber.Create("645678901"),
                DateTime.Now.AddYears(-25));

            _mockRepository.Setup(repo => repo.AgregarAsync(nuevoCliente, CancellationToken.None))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(CancellationToken.None))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.AgregarAsync(nuevoCliente);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevoCliente, CancellationToken.None), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(CancellationToken.None), Times.Once);
            resultadoGuardado.Should().Be(1);
        }

        [Fact]
        public async Task ActualizarAsync_ClienteExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var cliente = _clientes[0];
            cliente.ActualizarInformacionContacto(
                Email.Create("juan.nuevo@example.com"), 
                PhoneNumber.Create("687654321"));

            _mockRepository.Setup(repo => repo.ActualizarAsync(cliente, CancellationToken.None))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(CancellationToken.None))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.ActualizarAsync(cliente);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(cliente, CancellationToken.None), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(CancellationToken.None), Times.Once);
            resultadoGuardado.Should().Be(1);
        }
    }
}

