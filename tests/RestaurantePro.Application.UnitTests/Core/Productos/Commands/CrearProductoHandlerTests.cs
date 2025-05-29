namespace RestaurantePro.Application.UnitTests.Core.Productos.Commands;

/// <summary>
/// Pruebas unitarias para CrearProductoHandler
/// Valida comportamiento de creación de productos con mocks
/// </summary>
public class CrearProductoHandlerTests
{
    private readonly Mock<IProductoRepository> _repositoryMock;
    private readonly Mock<IProductoBuilder> _builderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CrearProductoHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CrearProductoHandler _handler;

    public CrearProductoHandlerTests()
    {
        _repositoryMock = new Mock<IProductoRepository>();
        _builderMock = new Mock<IProductoBuilder>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CrearProductoHandler>>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _handler = new CrearProductoHandler(
            _repositoryMock.Object,
            _builderMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task Handle_ConComandoValido_DeberiaRetornarExito()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Margherita",
            Descripcion = "Deliciosa pizza italiana",
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        var producto = CrearProductoEjemplo();
        var productoDto = CrearProductoDtoEjemplo();
        var userId = "test-user-123";

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        
        _builderMock.Setup(x => x.ConNombre(command.Nombre)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.ConDescripcion(command.Descripcion)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.ConPrecio(command.Precio)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.EnCategoria(command.CategoriaId)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.Construir()).Returns(Result<Producto>.Success(producto));

        _repositoryMock.Setup(x => x.AgregarAsync(It.IsAny<Producto>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Returns(productoDto);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Nombre.Should().Be(command.Nombre);

        // Verify interactions
        _builderMock.Verify(x => x.ConNombre(command.Nombre), Times.Once);
        _builderMock.Verify(x => x.Construir(), Times.Once);
        _repositoryMock.Verify(x => x.AgregarAsync(It.IsAny<Producto>()), Times.Once);
        _mapperMock.Verify(x => x.Map<ProductoDto>(producto), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoBuilderFalla_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Producto Inválido",
            Precio = -10m, // Precio inválido
            CategoriaId = Guid.NewGuid()
        };

        var errorMessage = "El precio debe ser mayor a 0";

        _builderMock.Setup(x => x.ConNombre(command.Nombre)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.ConPrecio(command.Precio)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.Construir())
            .Returns(Result<Producto>.Failure(errorMessage));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.ErrorMessage.Should().Be(errorMessage);

        // Verify no repository calls were made
        _repositoryMock.Verify(x => x.AgregarAsync(It.IsAny<Producto>()), Times.Never);
        _mapperMock.Verify(x => x.Map<ProductoDto>(It.IsAny<Producto>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoRepositoryFalla_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Napolitana",
            Precio = 18.99m,
            CategoriaId = Guid.NewGuid()
        };

        var producto = CrearProductoEjemplo();

        _builderMock.Setup(x => x.ConNombre(command.Nombre)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.ConPrecio(command.Precio)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.Construir()).Returns(Result<Producto>.Success(producto));

        _repositoryMock.Setup(x => x.AgregarAsync(It.IsAny<Producto>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.ErrorMessage.Should().Be("Error interno del servidor al crear el producto");

        // Verify logger was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error inesperado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConUsuarioActual_DeberiaEstablecerAuditoria()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Pepperoni",
            Precio = 16.99m,
            CategoriaId = Guid.NewGuid()
        };

        var producto = CrearProductoEjemplo();
        var productoDto = CrearProductoDtoEjemplo();
        var userId = "admin-user-456";

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        _builderMock.Setup(x => x.ConNombre(It.IsAny<string>())).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.ConPrecio(It.IsAny<decimal>())).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.Construir()).Returns(Result<Producto>.Success(producto));
        
        _repositoryMock.Setup(x => x.AgregarAsync(It.IsAny<Producto>()))
            .Returns(Task.CompletedTask);
        
        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Returns(productoDto);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        // Verify que se estableció la auditoría
        _repositoryMock.Verify(x => x.AgregarAsync(It.Is<Producto>(p => 
            p.CreadoPor == userId)), Times.Once);
    }

    #region Helpers

    private static Producto CrearProductoEjemplo()
    {
        var builder = new ProductoBuilder();
        return builder
            .ConNombre("Pizza Test")
            .ConDescripcion("Pizza de prueba")
            .ConPrecio(10.99m)
            .EnCategoria(Guid.NewGuid())
            .Construir()
            .Value;
    }

    private static ProductoDto CrearProductoDtoEjemplo()
    {
        return new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Test",
            Descripcion = "Pizza de prueba",
            Precio = 10.99m,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            CreadoPor = "test-user"
        };
    }

    #endregion
} 