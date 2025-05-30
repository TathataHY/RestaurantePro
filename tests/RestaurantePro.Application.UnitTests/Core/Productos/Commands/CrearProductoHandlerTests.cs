namespace RestaurantePro.Application.UnitTests.Core.Productos.Commands;

/// <summary>
/// Pruebas unitarias para CrearProductoHandler
/// Tests básicos para validar el comportamiento principal
/// </summary>
public class CrearProductoHandlerTests
{
    [Fact]
    public void CrearProductoCommand_ConDatosValidos_DeberiaCrearseCorrectamente()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Margherita",
            Descripcion = "Deliciosa pizza italiana",
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act & Assert
        command.Should().NotBeNull();
        command.Nombre.Should().Be("Pizza Margherita");
        command.Descripcion.Should().Be("Deliciosa pizza italiana");
        command.Precio.Should().Be(15.99m);
        command.CategoriaId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ProductoDto_ConDatosValidos_DeberiaCrearseCorrectamente()
    {
        // Arrange & Act
        var dto = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Test",
            Descripcion = "Pizza de prueba",
            Precio = 10.99m,
            CategoriaId = Guid.NewGuid(),
            CategoriaNombre = "Pizza",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        // Assert
        dto.Should().NotBeNull();
        dto.Nombre.Should().Be("Pizza Test");
        dto.Descripcion.Should().Be("Pizza de prueba");
        dto.Precio.Should().Be(10.99m);
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public void Result_Success_DeberiaRetornarExito()
    {
        // Arrange
        var dto = new ProductoDto { Nombre = "Test" };

        // Act
        var result = Result.Success(dto);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Test");
    }

    [Fact]
    public void Result_Failure_DeberiaRetornarError()
    {
        // Arrange
        var errorMessage = "Error de prueba";

        // Act
        var result = Result.Failure<ProductoDto>(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
    }
} 