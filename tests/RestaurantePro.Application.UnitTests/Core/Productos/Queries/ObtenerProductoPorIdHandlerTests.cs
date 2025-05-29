namespace RestaurantePro.Application.UnitTests.Core.Productos.Queries;

/// <summary>
/// Pruebas unitarias para ObtenerProductoPorIdHandler
/// Valida comportamiento de consulta por ID con casos exitosos y de error
/// </summary>
public class ObtenerProductoPorIdHandlerTests
{
    private readonly Mock<IProductoRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerProductoPorIdHandler>> _loggerMock;
    private readonly ObtenerProductoPorIdHandler _handler;

    public ObtenerProductoPorIdHandlerTests()
    {
        _repositoryMock = new Mock<IProductoRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerProductoPorIdHandler>>();

        _handler = new ObtenerProductoPorIdHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ConIdValido_DeberiaRetornarProducto()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);
        
        var producto = CrearProductoEjemplo();
        var productoDto = CrearProductoDtoEjemplo();

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync(producto);

        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Returns(productoDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Id.Should().Be(productoDto.Id);
        resultado.Value.Nombre.Should().Be(productoDto.Nombre);

        // Verify interactions
        _repositoryMock.Verify(x => x.ObtenerPorIdAsync(productoId), Times.Once);
        _mapperMock.Verify(x => x.Map<ProductoDto>(producto), Times.Once);
    }

    [Fact]
    public async Task Handle_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync((Producto)null);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("no fue encontrado");

        // Verify que no se intentó mapear
        _mapperMock.Verify(x => x.Map<ProductoDto>(It.IsAny<Producto>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoRepositoryFalla_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.ErrorMessage.Should().Be("Error interno del servidor al obtener el producto");

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
    public async Task Handle_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProductoPorIdQuery(Guid.Empty);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("ID válido");

        // Verify que no se hizo llamada al repository
        _repositoryMock.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoMapperFalla_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);
        var producto = CrearProductoEjemplo();

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync(producto);

        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Throws(new InvalidOperationException("Error de mapeo"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.ErrorMessage.Should().Be("Error interno del servidor al obtener el producto");
    }

    [Fact]
    public async Task Handle_ConProductoActivo_DeberiaRetornarProducto()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);
        
        var producto = CrearProductoEjemplo();
        var productoDto = CrearProductoDtoEjemplo();
        productoDto.Activo = true;

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync(producto);

        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Returns(productoDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConProductoInactivo_DeberiaRetornarProducto()
    {
        // Arrange - Los productos inactivos también se pueden consultar por ID
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);
        
        var producto = CrearProductoEjemplo();
        var productoDto = CrearProductoDtoEjemplo();
        productoDto.Activo = false;

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync(producto);

        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Returns(productoDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_VerificarLogDeInicio()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(productoId);
        var producto = CrearProductoEjemplo();
        var productoDto = CrearProductoDtoEjemplo();

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync(producto);
        _mapperMock.Setup(x => x.Map<ProductoDto>(producto))
            .Returns(productoDto);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Verificar que se logueó el inicio de la operación
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Obteniendo producto por ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
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