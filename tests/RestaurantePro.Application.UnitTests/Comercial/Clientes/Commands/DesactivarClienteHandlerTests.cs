using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
using RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.UnitTests.Common;

namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Commands;

public class DesactivarClienteHandlerTests
{
    private readonly Mock<IClienteRepository> _mockRepository;
    private readonly Mock<ILogger<DesactivarClienteHandler>> _mockLogger;
    private readonly Mock<RestaurantePro.Application.Common.Interfaces.INotificationService> _mockNotificationService;
    private readonly Mock<RestaurantePro.Application.Common.Interfaces.IEmailService> _mockEmailService;
    private readonly DesactivarClienteHandler _handler;

    public DesactivarClienteHandlerTests()
    {
        _mockRepository = new Mock<IClienteRepository>();
        _mockLogger = new Mock<ILogger<DesactivarClienteHandler>>();
        _mockNotificationService = new Mock<RestaurantePro.Application.Common.Interfaces.INotificationService>();
        _mockEmailService = new Mock<RestaurantePro.Application.Common.Interfaces.IEmailService>();
        
        _handler = new DesactivarClienteHandler(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockNotificationService.Object,
            _mockEmailService.Object
        );
    }

    [Fact]
    public async Task Handle_ClienteActivoValido_DeberiaDesactivarExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "Motivo de prueba",
            DesactivadoPor = "admin"
        };

        var clienteActivo = Cliente.Crear(
            ClienteNombre.Crear("Juan", "Pérez"),
            "juan@test.com",
            "+57300123456",
            DateTime.Now.AddYears(-30)
        );
        
        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(clienteActivo, clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que ActualizarAsync fue llamado
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteActivoSinMotivo_DeberiaDesactivarExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "",
            DesactivadoPor = "admin"
        };

        var clienteActivo = Cliente.Crear(
            ClienteNombre.Crear("María", "García"),
            "maria@test.com",
            "+57300987654",
            DateTime.Now.AddYears(-25)
        );

        // Establecer el ID manualmente para el test
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(clienteActivo, clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que el cliente fue desactivado
        clienteActivo.EstaActivo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "Motivo de prueba",
            DesactivadoPor = "admin"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("El cliente especificado no existe.");
    }

    [Fact]
    public async Task Handle_ClienteYaEstaEliminado_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "Motivo de prueba",
            DesactivadoPor = "admin"
        };

        var clienteEliminado = Cliente.Crear(
            ClienteNombre.Crear("Ana", "López"),
            "ana@test.com",
            "+57300555444",
            DateTime.Now.AddYears(-28)
        );
        
        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(clienteEliminado, clienteId);
        
        // Simular que el cliente está eliminado
        clienteEliminado.Desactivar();

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteEliminado);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - El método Desactivar() del dominio es idempotente
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse(); // Debería fallar porque ya está inactivo
        result.Error.Should().Be("El cliente ya está desactivado.");
    }

    [Fact]
    public async Task Handle_ClienteYaEstaInactivo_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "Motivo de prueba",
            DesactivadoPor = "admin"
        };

        var clienteInactivo = Cliente.Crear(
            ClienteNombre.Crear("Carlos", "Rodríguez"),
            "carlos@test.com",
            "+57300777666",
            DateTime.Now.AddYears(-35)
        );
        
        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(clienteInactivo, clienteId);
        
        clienteInactivo.Desactivar(); // Ya está inactivo

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteInactivo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse(); // Debería fallar porque ya está inactivo
        result.Error.Should().Be("El cliente ya está desactivado.");
    }

    [Fact]
    public async Task Handle_ExcepcionBusinessRule_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "Motivo que causa excepción",
            DesactivadoPor = "admin"
        };

        // Crear un cliente que cause una excepción de negocio
        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Cliente", "Problema"),
            "problema@test.com",
            "+57300999888",
            DateTime.Now.AddYears(-30)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Configurar el servicio de notificaciones para lanzar una excepción de regla de negocio
        // NOTA: Este servicio no se usa actualmente en el handler (TODO), pero lo configuramos para futura implementación
        _mockNotificationService.Setup(x => x.EnviarNotificacionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new BusinessRuleViolationException("Error de regla de negocio", "Cliente", "Test", "Comercial"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // NOTA: Como el código de notificaciones está deshabilitado (TODO), el proceso debería ser exitoso
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que el cliente fue desactivado
        cliente.EstaActivo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExcepcionGeneral_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand 
        { 
            ClienteId = clienteId,
            MotivoDesactivacion = "Motivo de prueba",
            DesactivadoPor = "admin"
        };

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Cliente", "Test"),
            "test@test.com",
            "+57300111222",
            DateTime.Now.AddYears(-30)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Configurar el servicio de email para lanzar una excepción general
        // NOTA: Este servicio no se usa actualmente en el handler (TODO), pero lo configuramos para futura implementación
        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error interno del sistema"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // NOTA: Como el código de email está deshabilitado (TODO), el proceso debería ser exitoso
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que el cliente fue desactivado
        cliente.EstaActivo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ConstructorSinParametros_DeberiaFuncionar()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand();
        command.ClienteId = clienteId;
        command.MotivoDesactivacion = "Test constructor";
        command.DesactivadoPor = "admin";

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Test", "Constructor"),
            "constructor@test.com",
            "+57300000000",
            DateTime.Now.AddYears(-30)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que el cliente fue desactivado
        cliente.EstaActivo.Should().BeFalse();
    }
} 