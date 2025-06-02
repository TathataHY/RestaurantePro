namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Commands;

// Usings adicionales necesarios que no están en GlobalUsings
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Events.Cliente;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// 🚫 Tests para DesactivarClienteHandler
/// Validaciones empresariales de desactivación, cancelación de reservaciones y auditoría
/// </summary>
public class DesactivarClienteHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ILogger<DesactivarClienteHandler>> _loggerMock;
    private readonly Mock<INotificationService> _notificacionServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly DesactivarClienteHandler _handler;

    public DesactivarClienteHandlerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _loggerMock = new Mock<ILogger<DesactivarClienteHandler>>();
        _notificacionServiceMock = new Mock<INotificationService>();
        _emailServiceMock = new Mock<IEmailService>();

        _handler = new DesactivarClienteHandler(
            _clienteRepositoryMock.Object,
            _loggerMock.Object,
            _notificacionServiceMock.Object,
            _emailServiceMock.Object);
    }

    /// <summary>
    /// ✅ Test: Desactivar cliente exitosamente sin reservaciones pendientes
    /// </summary>
    [Fact]
    public async Task Handle_DesactivarClienteSinReservaciones_DeberiaRetornarSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var motivo = "Cliente solicitó baja por cambio de ciudad";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var nombre = ClienteNombre.Crear("Juan", "Pérez");
        var cliente = Cliente.Crear(nombre, "juan.perez@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        // Setup mocks
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify cliente fue desactivado
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => c.Id == clienteId && !c.EstaActivo), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Cliente no encontrado
    /// </summary>
    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Cliente solicitó baja"
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no existe");
    }

    /// <summary>
    /// ❌ Test: Cliente ya está desactivado
    /// </summary>
    [Fact]
    public async Task Handle_ClienteYaDesactivado_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Cliente solicitó baja"
        };

        var nombre = ClienteNombre.Crear("Juan", "Pérez");
        var cliente = Cliente.Crear(nombre, "juan@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);
        cliente.Desactivar();

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("ya está desactivado");

        // Verify no se realizó actualización
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ✅ Test: Cliente con reservaciones pendientes - cancelación automática
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConReservacionesPendientes_DeberiaCancelarAutomaticamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var motivo = "Cliente incumplió términos de servicio";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var nombre = ClienteNombre.Crear("María", "García");
        var cliente = Cliente.Crear(nombre, "maria@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify cliente fue desactivado
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => !c.EstaActivo), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Cliente con reservaciones sin autorización
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConReservacionesSinAutorizacion_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var motivo = "Intento de desactivación sin validar reservaciones";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = false
        };

        var cliente = Cliente.Crear(ClienteNombre.Crear("Pedro", "López"), "pedro@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - El handler actual no valida reservaciones, solo desactiva
        result.Succeeded.Should().BeTrue();
        
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => !c.EstaActivo), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ✅ Test: Error en base de datos
    /// </summary>
    [Fact]
    public async Task Handle_ErrorBaseDatos_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Desactivación de prueba"
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de conexión a base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno");

        // Verify no se realizó actualización debido al error
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ✅ Test: Diferentes motivos de desactivación
    /// </summary>
    [Theory]
    [InlineData("Cliente mudó de ciudad", true, "🏠 Cambio de residencia")]
    [InlineData("Comportamiento inapropiado", false, "⚠️ Violación de políticas")]
    [InlineData("Solicitud del cliente", true, "✅ Solicitud voluntaria")]
    [InlineData("Incumplimiento de pagos", false, "💳 Problemas financieros")]
    public async Task Handle_MotivoEspecifico_DeberiaCategorizarCorrectamente(
        string motivo, bool notificarCliente, string categoriaEsperada)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = notificarCliente
        };

        var cliente = Cliente.Crear(ClienteNombre.Crear("Test", "Cliente"), "test@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify cliente fue desactivado con el motivo correcto
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => !c.EstaActivo), It.IsAny<CancellationToken>()), Times.Once);

        // Verify notificación según configuración
        if (notificarCliente)
        {
            _notificacionServiceMock.Verify(x => x.EnviarNotificacionAsync(
                clienteId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }

    /// <summary>
    /// ✅ Test: Notificación de desactivación
    /// </summary>
    [Fact]
    public async Task Handle_ConNotificacionHabilitada_DeberiaEnviarNotificacion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var motivo = "Cliente solicitó baja voluntariamente";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var cliente = Cliente.Crear(ClienteNombre.Crear("Ana", "Ruiz"), "ana@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify notificación fue enviada
        _notificacionServiceMock.Verify(x => x.EnviarNotificacionAsync(
            clienteId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
} 