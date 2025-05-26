namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    /// <summary>
    /// Pruebas unitarias para ComercialServiceFacade
    /// </summary>
    public class ComercialServiceFacadeTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IClientesFrecuentesPolicy> _clientesFrecuentesPolicyMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly NotificationManager _notificationManager;
        private readonly ComercialServiceFacade _sut;

        public ComercialServiceFacadeTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _clientesFrecuentesPolicyMock = new Mock<IClientesFrecuentesPolicy>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _notificationManager = new NotificationManager();
            
            _sut = new ComercialServiceFacade(
                _clienteRepositoryMock.Object,
                _clientesFrecuentesPolicyMock.Object,
                _servicioFidelizacionMock.Object,
                _notificationManager);
        }
        
        [Fact]
        public async Task ObtenerClientePorIdAsync_ClienteExiste_DebeRetornarCliente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan.perez@example.com", "123456789");
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Act
            var result = await _sut.ObtenerClientePorIdAsync(clienteId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().Be(cliente);
        }
        
        [Fact]
        public async Task ObtenerClientePorIdAsync_ClienteNoExiste_DebeRetornarNull()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente)null);
            
            // Act
            var result = await _sut.ObtenerClientePorIdAsync(clienteId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeNull();
        }
        
        [Fact]
        public async Task ObtenerClientePorIdAsync_IdInvalido_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.Empty;
            
            // Act
            var result = await _sut.ObtenerClientePorIdAsync(clienteId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("ID del cliente");
        }
        
        [Fact]
        public async Task RegistrarNuevoClienteConTarjetaAsync_DatosValidos_DebeRegistrarCliente()
        {
            // Arrange
            var nombre = "Juan";
            var apellidos = "Pérez";
            var email = "juan.perez@example.com";
            var telefono = "123456789";
            
            _clienteRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.RegistrarNuevoClienteConTarjetaAsync(nombre, apellidos, email, telefono);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(email);
            result.Value.Telefono.Should().Be(telefono);
            result.Value.TieneTarjetaFidelizacion().Should().BeTrue();
            
            _clienteRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Cliente>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarNuevoClienteConTarjetaAsync_DatosInvalidos_DebeRetornarError()
        {
            // Arrange
            var nombre = "";
            var apellidos = "Pérez";
            var email = "juan.perez@example.com";
            
            // Act
            var result = await _sut.RegistrarNuevoClienteConTarjetaAsync(nombre, apellidos, email);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("nombre");
            
            _clienteRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Cliente>()), Times.Never);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ActualizarDatosClienteAsync_ClienteExiste_DebeActualizarCliente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan.perez@example.com", "123456789");
            
            var nuevoNombre = "Carlos";
            var nuevosApellidos = "García";
            var nuevoEmail = "carlos.garcia@example.com";
            var nuevoTelefono = "987654321";
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.ActualizarDatosClienteAsync(
                clienteId, 
                nuevoNombre, 
                nuevosApellidos, 
                nuevoEmail, 
                nuevoTelefono);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(nuevoEmail);
            result.Value.Telefono.Should().Be(nuevoTelefono);
            
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarDatosClienteAsync_ClienteNoExiste_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente)null);
            
            // Act
            var result = await _sut.ActualizarDatosClienteAsync(
                clienteId, 
                "Carlos", 
                "García");
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("No se encontró el cliente");
            
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>()), Times.Never);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task AsignarPuntosClienteAsync_ClienteConTarjeta_DebeAsignarPuntos()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntos = 100;
            var comandaId = Guid.NewGuid();
            
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan.perez@example.com", "123456789");
            cliente.CrearTarjetaFidelizacion();
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.AsignarPuntosClienteAsync(clienteId, puntos, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarPuntosClienteAsync_ClienteSinTarjeta_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntos = 100;
            var comandaId = Guid.NewGuid();
            
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan.perez@example.com", "123456789");
            // No crear tarjeta de fidelización
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Act
            var result = await _sut.AsignarPuntosClienteAsync(clienteId, puntos, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("tarjeta de fidelización");
            
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>()), Times.Never);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task CanjearPuntosPorDescuentoAsync_PuntosDisponibles_DebeAplicarDescuento()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosAUtilizar = 500;
            var comandaId = Guid.NewGuid();
            var descuentoEsperado = 50.0m; // 500 puntos = $50 de descuento
            
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan.perez@example.com", "123456789");
            cliente.CrearTarjetaFidelizacion();
            cliente.AgregarPuntosFidelizacion(1000, "Compra anterior"); // 1000 puntos disponibles
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _servicioFidelizacionMock
                .Setup(s => s.CalcularDescuentoPorPuntos(puntosAUtilizar, It.IsAny<decimal>()))
                .Returns(descuentoEsperado);
            
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.CanjearPuntosPorDescuentoAsync(clienteId, puntosAUtilizar, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(descuentoEsperado);
            
            _servicioFidelizacionMock.Verify(s => s.CalcularDescuentoPorPuntos(puntosAUtilizar, It.IsAny<decimal>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CanjearPuntosPorDescuentoAsync_PuntosInsuficientes_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosAUtilizar = 1000;
            var comandaId = Guid.NewGuid();
            
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan.perez@example.com", "123456789");
            cliente.CrearTarjetaFidelizacion();
            cliente.AgregarPuntosFidelizacion(500, "Compra anterior"); // Solo 500 puntos disponibles
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Act
            var result = await _sut.CanjearPuntosPorDescuentoAsync(clienteId, puntosAUtilizar, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().Be(0);
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("puntos disponibles");
            
            _servicioFidelizacionMock.Verify(s => s.CalcularDescuentoPorPuntos(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>()), Times.Never);
            _clienteRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task EjecutarPoliticaClientesFrecuentesAsync_ClientesExistentes_DebeRetornarSegmentos()
        {
            // Arrange
            var cliente1Id = Guid.NewGuid();
            var cliente2Id = Guid.NewGuid();
            
            var clienteNombre1 = ClienteNombre.Crear("Juan", "Pérez");
            var cliente1 = Cliente.Crear(clienteNombre1, "juan@example.com");
            cliente1.ActualizarSegmento(SegmentoCliente.FrecuenciaAlta);
            
            var clienteNombre2 = ClienteNombre.Crear("María", "López");
            var cliente2 = Cliente.Crear(clienteNombre2, "maria@example.com");
            cliente2.ActualizarSegmento(SegmentoCliente.TicketAlto);
            
            var clientes = new List<Cliente> { cliente1, cliente2 };
            
            var resultadoPolicy = new ResultadoClientesFrecuentesPolicy
            {
                ClientesActualizados = new List<Guid> { cliente1Id, cliente2Id }
            };
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerTodosConHistorialVisitasAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientes);
            
            _clientesFrecuentesPolicyMock
                .Setup(p => p.EjecutarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoPolicy);
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(cliente1Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente1);
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(cliente2Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente2);
            
            // Act
            var result = await _sut.EjecutarPoliticaClientesFrecuentesAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().ContainKey(cliente1Id);
            result.Value.Should().ContainKey(cliente2Id);
            result.Value[cliente1Id].Should().Be(SegmentoCliente.FrecuenciaAlta);
            result.Value[cliente2Id].Should().Be(SegmentoCliente.TicketAlto);
            
            _clientesFrecuentesPolicyMock.Verify(p => p.EjecutarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 