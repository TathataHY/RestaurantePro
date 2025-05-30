namespace RestaurantePro.Application.UnitTests.Core.Productos.Commands;

public class EliminarProductoHandlerTests
{
    private readonly Mock<IProductoRepository> _mockRepository;
    private readonly Mock<ILogger<EliminarProductoHandler>> _mockLogger;
    private readonly Mock<INotificationManager> _mockNotificationManager;
    private readonly Mock<ILogger<ProductoBuilder>> _mockBuilderLogger;
    private readonly EliminarProductoHandler _handler;

    public EliminarProductoHandlerTests()
    {
        _mockRepository = new Mock<IProductoRepository>();
        _mockLogger = new Mock<ILogger<EliminarProductoHandler>>();
        _mockNotificationManager = new Mock<INotificationManager>();
        _mockBuilderLogger = new Mock<ILogger<ProductoBuilder>>();
        _handler = new EliminarProductoHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ProductoExistente_DeberiaEliminarExitosamente()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var command = new EliminarProductoCommand(productoId);

        var producto = CrearProductoTest("Producto Test", "Descripción test", 100.00m);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verificar que el producto fue desactivado
        producto.EstaActivo.Should().BeFalse();

        // Verificar que se llamó al repositorio para actualizar
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductoNoExistente_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var command = new EliminarProductoCommand(productoId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain($"Producto con ID {productoId} no encontrado");

        // Verificar que NO se llamó al repositorio para actualizar
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductoYaDesactivado_DeberiaRetornarExito()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var command = new EliminarProductoCommand(productoId);

        var producto = CrearProductoTest("Producto Test", "Descripción test", 100.00m);

        // Desactivar el producto antes de la prueba
        producto.Desactivar();

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verificar que el producto sigue desactivado
        producto.EstaActivo.Should().BeFalse();

        // Verificar que NO se llamó al repositorio para actualizar (ya estaba desactivado)
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var command = new EliminarProductoCommand(productoId);
        var excepcionMensaje = "Error de base de datos";

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(excepcionMensaje));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al eliminar el producto");
        result.Error.Should().Contain(excepcionMensaje);
    }

    [Fact]
    public async Task Handle_ExcepcionAlActualizar_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var command = new EliminarProductoCommand(productoId);
        var excepcionMensaje = "Error al actualizar";

        var producto = CrearProductoTest("Producto Test", "Descripción test", 100.00m);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        _mockRepository.Setup(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(excepcionMensaje));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al eliminar el producto");
        result.Error.Should().Contain(excepcionMensaje);
    }

    private Producto CrearProductoTest(string nombre, string descripcion, decimal precio)
    {
        // Setup mocks básicos
        var mockNotificationManager = new Mock<INotificationManager>();
        var mockBuilderLogger = new Mock<ILogger<ProductoBuilder>>();
        
        mockNotificationManager.Setup(x => x.HasErrors).Returns(false);

        var builder = new ProductoBuilder(mockNotificationManager.Object, mockBuilderLogger.Object);
        
        var resultado = builder
            .ConNombre(nombre)
            .ConDescripcion(descripcion)
            .ConPrecio(precio)
            .EnCategoria("Categoría Test")
            .Construir();

        return resultado.Value;
    }
} 