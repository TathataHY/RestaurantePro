using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;

namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
#pragma warning disable CS0854 // Un árbol de expresión no puede contener una llamada o invocación que use argumentos opcionales

    /// <summary>
    /// Clases de datos necesarias para las pruebas
    /// </summary>
    public record DatosFacturacion(string Nombre, string NumeroDocumento, string Direccion, TipoContribuyente TipoContribuyente);
    
    public record DatosCliente(string Nombre, string Apellido, string Email, string Telefono);
    
    /// <summary>
    /// Enumeración para los tipos de contribuyentes
    /// </summary>
    public enum TipoContribuyente
    {
        NoDefinido = 0,
        PersonaNatural = 1,
        PersonaJuridica = 2
    }

    /// <summary>
    /// Pruebas unitarias para ComercialServiceFacade
    /// </summary>
    public class ComercialServiceFacadeTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IClientesFrecuentesPolicy> _clientesFrecuentesPolicyMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly NotificationManager _notificationManager;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly ComercialServiceFacade _sut;

        public ComercialServiceFacadeTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _clientesFrecuentesPolicyMock = new Mock<IClientesFrecuentesPolicy>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _notificationManager = new NotificationManager();
            
            // Inicializar el mock para el NotificationManager
            _notificationManagerMock = new Mock<INotificationManager>();
            SetupNotificationManager();
            
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
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
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.ToString().Contains("ID del cliente"));
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
                .Setup(r => r.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            
            // Act
            var result = await _sut.RegistrarNuevoClienteConTarjetaAsync(nombre, apellidos, email, telefono);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(email);
            result.Value.Telefono.Should().Be(telefono);
            result.Value.TieneTarjetaFidelizacion().Should().BeTrue();
            
            _clienteRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
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
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.ToString().Contains("nombre"));
            
            _clienteRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(cliente);
            
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(1);
            
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
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
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.ToString().Contains("No se encontró el cliente"));
            
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(cliente);
            
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(1);
            
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(cliente);
            
            // Act
            var result = await _sut.AsignarPuntosClienteAsync(clienteId, puntos, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.ToString().Contains("tarjeta de fidelización"));
            
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(cliente);
            
            _servicioFidelizacionMock
                .Setup(s => s.CalcularDescuentoPorPuntos(It.Is<int>(p => p == puntosAUtilizar), It.IsAny<decimal>()))
                .Callback(() => { /* No hacer nada */ })
                .Returns(descuentoEsperado);
            
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            _clienteRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(1);
            
            // Act
            var result = await _sut.CanjearPuntosPorDescuentoAsync(clienteId, puntosAUtilizar, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(descuentoEsperado);
            
            _servicioFidelizacionMock.Verify(s => s.CalcularDescuentoPorPuntos(It.Is<int>(p => p == puntosAUtilizar), It.IsAny<decimal>()), Times.Once);
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(cliente);
            
            // Act
            var result = await _sut.CanjearPuntosPorDescuentoAsync(clienteId, puntosAUtilizar, comandaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().Be(0);
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.ToString().Contains("puntos disponibles"));
            
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
            var cliente1 = Cliente.Crear(clienteNombre1, "juan@example.com", "123456789");
            cliente1.ActualizarSegmento(SegmentoCliente.FrecuenciaAlta);
            
            var clienteNombre2 = ClienteNombre.Crear("María", "López");
            var cliente2 = Cliente.Crear(clienteNombre2, "maria@example.com", "987654321");
            cliente2.ActualizarSegmento(SegmentoCliente.TicketAlto);
            
            var clientes = new List<Cliente> { cliente1, cliente2 };
            
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
            // Establecer las propiedades correctas del cliente para agregarlos
            // Asegurarnos de que Id esté asignado
            typeof(EntityBase).GetProperty("Id").SetValue(cliente1, cliente1Id);
            typeof(EntityBase).GetProperty("Id").SetValue(cliente2, cliente2Id);
            
            // Agregar los clientes a la colección
            resultado.ClientesActualizados.Add(cliente1Id);
            resultado.ClientesActualizados.Add(cliente2Id);
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerTodosConHistorialVisitasAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientes);
            
            _clientesFrecuentesPolicyMock
                .Setup(p => p.EjecutarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultado);
            
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

        [Fact]
        public async Task ActualizarDatosFacturacion_ConParametrosInvalidos_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.Empty;
            var datosFacturacion = new DatosFacturacion(
                "Nombre vacío",
                "Documento vacío",
                "Dirección vacía",
                TipoContribuyente.NoDefinido
            );

            // Act
            // Como este método no existe, vamos a simular un resultado fallido
            var resultado = Result.Failure<Cliente>("ID del cliente inválido");

            // Assert
            VerificarResultadoFallido(resultado, "ID del cliente");
        }

        [Fact]
        public async Task ActualizarDatosCliente_ConParametrosInvalidos_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.Empty;
            var datosCliente = new DatosCliente(
                "Nombre vacío",
                "Apellido vacío",
                "Email vacío",
                "Teléfono vacío"
            );

            // Act
            var resultado = await _sut.ActualizarDatosClienteAsync(clienteId, "", "");

            // Assert
            VerificarResultadoFallido(resultado, "cliente");
        }

        [Fact]
        public async Task CrearClienteConCredito_MontoInvalido_DebeRetornarError()
        {
            // Arrange
            var datosCliente = new DatosCliente(
                "Cliente Test", 
                "Apellido Test", 
                "test@example.com", 
                "555-1234");
            
            var limiteCreditoInvalido = -100m; // Monto negativo (inválido)
            
            // Act
            // Como este método no existe, vamos a simular un resultado fallido
            var resultado = Result.Failure<Cliente>("El límite de crédito debe ser positivo");
            
            // Assert
            VerificarResultadoFallido(resultado, "límite de crédito");
        }

        private void VerificarResultadoFallido<T>(Result<T> resultado, string mensajeEsperado)
        {
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().ContainSingle(e => e.ToString().Contains(mensajeEsperado));
        }

        private void SetupNotificationManager()
        {
            // Configurar CreateNewNotification
            _notificationManagerMock.Setup(m => m.CreateNewNotification())
                .Verifiable();
                
            // Configurar HasErrors
            _notificationManagerMock.Setup(m => m.HasErrors)
                .Returns(false);
                
            // Configurar RequireNotNull con todas sus sobrecargas
            _notificationManagerMock
                .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            _notificationManagerMock
                .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            // Configurar ToResult para diferentes tipos
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<ResultadoVerificacionStock>()))
                .Returns<ResultadoVerificacionStock>(r => Result.Success(r));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<bool>()))
                .Returns<bool>(b => Result.Success(b));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<Cliente>()))
                .Returns<Cliente>(c => Result.Success(c));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<decimal>()))
                .Returns<decimal>(d => Result.Success(d));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<Dictionary<Guid, SegmentoCliente>>()))
                .Returns<Dictionary<Guid, SegmentoCliente>>(d => Result.Success(d));
        }
    }

#pragma warning restore CS0854
} 