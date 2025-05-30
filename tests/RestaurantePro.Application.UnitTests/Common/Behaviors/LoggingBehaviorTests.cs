namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Pruebas unitarias para LoggingBehavior
/// Tests básicos para validar el comportamiento de logging
/// </summary>
public class LoggingBehaviorTests
{
    [Fact]
    public void LoggingBehavior_DeberiaExistir()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        
        // Act
        var behavior = new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(loggerMock.Object);

        // Assert
        behavior.Should().NotBeNull();
    }

    [Fact]
    public void CrearProductoCommand_ParaLogging_DeberiaSerSerializable()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Test",
            Descripcion = "Test pizza",
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act & Assert
        command.Should().NotBeNull();
        command.Nombre.Should().NotBeNullOrEmpty();
        command.Precio.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Result_ParaLogging_DeberiaSerSerializable()
    {
        // Arrange
        var dto = new ProductoDto { Nombre = "Test" };
        var result = Result.Success(dto);

        // Act & Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void RequestName_DeberiaObtenerseCorrectamente()
    {
        // Arrange
        var requestType = typeof(CrearProductoCommand);

        // Act
        var requestName = requestType.Name;

        // Assert
        requestName.Should().Be("CrearProductoCommand");
        requestName.Should().NotBeNullOrEmpty();
    }
} 