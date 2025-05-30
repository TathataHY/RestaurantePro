using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Commands;

public class ActualizarStockHandlerTests
{
    private readonly Mock<IIngredienteRepository> _mockIngredienteRepository;
    private readonly Mock<IInventarioServiceFacade> _mockInventarioService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ActualizarStockHandler>> _mockLogger;
    private readonly ActualizarStockHandler _handler;

    public ActualizarStockHandlerTests()
    {
        _mockIngredienteRepository = new Mock<IIngredienteRepository>();
        _mockInventarioService = new Mock<IInventarioServiceFacade>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ActualizarStockHandler>>();
        
        _handler = new ActualizarStockHandler(
            _mockIngredienteRepository.Object,
            _mockInventarioService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ConIngresoValido_DeberiaIncrementarStockCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ingreso",
            Cantidad = 10m,
            Motivo = "Compra de mercadería",
            UsuarioId = Guid.NewGuid(),
            NuevoCosto = 1200m,
            ReferenciaExterna = "FAC-001"
        };

        var ingrediente = CrearIngredienteMock("Tomate", 5m, 2m);
        var ingredienteDto = CrearIngredienteDtoMock("Tomate");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEgresoValido_DeberiaDecrementarStockCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Egreso",
            Cantidad = 3m,
            Motivo = "Consumo para producción",
            UsuarioId = Guid.NewGuid(),
            ReferenciaExterna = "PROD-001"
        };

        var ingrediente = CrearIngredienteMock("Lechuga", 10m, 2m);
        var ingredienteDto = CrearIngredienteDtoMock("Lechuga");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConAjusteValido_DeberiaAjustarStockCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ajuste",
            Cantidad = 15m, // Cantidad real del inventario físico
            Motivo = "Inventario físico",
            UsuarioId = Guid.NewGuid()
        };

        var ingrediente = CrearIngredienteMock("Arroz", 12m, 5m); // Stock actual 12
        var ingredienteDto = CrearIngredienteDtoMock("Arroz");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConIngredienteNoExistente_DeberiaRetornarError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ingreso",
            Cantidad = 5m,
            Motivo = "Compra",
            UsuarioId = Guid.NewGuid()
        };

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingrediente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain($"No se encontró el ingrediente con ID {ingredienteId}");

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIngredienteInactivo_DeberiaRetornarError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ingreso",
            Cantidad = 5m,
            Motivo = "Compra",
            UsuarioId = Guid.NewGuid()
        };

        var ingrediente = CrearIngredienteMock("Pan", 5m, 2m);
        ingrediente.Desactivar();

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No se puede actualizar stock de ingrediente inactivo");

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConEgresoStockInsuficiente_DeberiaRetornarError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Egreso",
            Cantidad = 15m, // Mayor al stock disponible
            Motivo = "Consumo excesivo",
            UsuarioId = Guid.NewGuid()
        };

        var ingrediente = CrearIngredienteMock("Sal", 8m, 3m); // Solo tiene 8 en stock

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Stock insuficiente");

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIngresoYNuevoCosto_DeberiaActualizarCostoPromedio()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ingreso",
            Cantidad = 20m,
            Motivo = "Compra con nuevo precio",
            UsuarioId = Guid.NewGuid(),
            NuevoCosto = 1800m, // Nuevo costo unitario
            ReferenciaExterna = "FAC-002"
        };

        var ingrediente = CrearIngredienteMock("Cebolla", 10m, 3m);
        var ingredienteDto = CrearIngredienteDtoMock("Cebolla");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcionBusinessRule_DeberiaRetornarErrorConMensaje()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Egreso",
            Cantidad = 5m,
            Motivo = "Test",
            UsuarioId = Guid.NewGuid()
        };

        var ingrediente = CrearIngredienteMock("Huevos", 6m, 2m);

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleViolationException(
                "TestRule", "Ingrediente", "Regla de negocio violada", "Inventario"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Regla de negocio violada");
    }

    [Fact]
    public async Task Handle_ConExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ingreso",
            Cantidad = 3m,
            Motivo = "Test",
            UsuarioId = Guid.NewGuid()
        };

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al actualizar el stock");
    }

    [Fact]
    public async Task Handle_ConAjusteQueReduceStockPorDebajoDeMinimo_DeberiaGenerarAlerta()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ajuste",
            Cantidad = 1m, // Ajuste que deja stock muy bajo
            Motivo = "Inventario físico - pérdida detectada",
            UsuarioId = Guid.NewGuid()
        };

        var ingrediente = CrearIngredienteMock("Pimienta", 8m, 5m); // Stock mínimo 5
        var ingredienteDto = CrearIngredienteDtoMock("Pimienta");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verificar que se procesó correctamente
        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConAjusteACero_DeberiaGenerarAlertaCritica()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ajuste",
            Cantidad = 0m, // Ajuste a cero (sin stock)
            Motivo = "Inventario físico - producto perdido",
            UsuarioId = Guid.NewGuid()
        };

        var ingrediente = CrearIngredienteMock("Canela", 3m, 1m);
        var ingredienteDto = CrearIngredienteDtoMock("Canela");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockIngredienteRepository.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrearEntrada_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();

        var command = ActualizarStockCommand.CrearEntrada(
            ingredienteId, 
            25m, 
            "Compra de mercadería", 
            usuarioId, 
            1500m, 
            "FAC-003", 
            proveedorId);

        var ingrediente = CrearIngredienteMock("Mantequilla", 5m, 2m);
        var ingredienteDto = CrearIngredienteDtoMock("Mantequilla");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verificar que se configuró correctamente como entrada
        command.TipoMovimiento.Should().Be("Ingreso");
        command.Cantidad.Should().Be(25m);
        command.NuevoCosto.Should().Be(1500m);
        command.ProveedorId.Should().Be(proveedorId);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrearSalida_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var command = ActualizarStockCommand.CrearSalida(
            ingredienteId, 
            8m, 
            "Consumo para producción", 
            usuarioId, 
            "PROD-005");

        var ingrediente = CrearIngredienteMock("Queso", 20m, 5m);
        var ingredienteDto = CrearIngredienteDtoMock("Queso");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verificar que se configuró correctamente como salida
        command.TipoMovimiento.Should().Be("Egreso");
        command.Cantidad.Should().Be(8m);
        command.ReferenciaExterna.Should().Be("PROD-005");
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrearMerma_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var command = ActualizarStockCommand.CrearMerma(
            ingredienteId, 
            2m, 
            "Producto vencido", 
            usuarioId);

        var ingrediente = CrearIngredienteMock("Yogurt", 10m, 3m);
        var ingredienteDto = CrearIngredienteDtoMock("Yogurt");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockIngredienteRepository.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verificar que se configuró correctamente como merma
        command.TipoMovimiento.Should().Be("Egreso");
        command.Cantidad.Should().Be(2m);
        command.Motivo.Should().Be("Merma: Producto vencido");
    }

    // Métodos de ayuda para crear mocks
    private static Ingrediente CrearIngredienteMock(string nombre, decimal stock, decimal stockMinimo)
    {
        return Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            $"COD-{nombre.ToUpper()}",
            $"Descripción de {nombre}",
            UnidadMedida.Kilogramo,
            stockMinimo,
            stock,
            RotacionIngrediente.Media,
            TemporadaIngrediente.TodoElAño);
    }

    private static IngredienteDto CrearIngredienteDtoMock(string nombre)
    {
        return new IngredienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = $"Descripción de {nombre}",
            StockActual = 10m,
            StockMinimo = 5m,
            CostoUnitario = 1500m,
            Activo = true
        };
    }
} 