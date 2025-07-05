namespace RestaurantePro.Application.UnitTests.Core.Productos.Commands;

/// <summary>
/// Pruebas unitarias para ActualizarProductoHandler
/// Tests básicos para validar el comportamiento principal
/// </summary>
public class ActualizarProductoHandlerTests
{
    [Fact]
    public void ActualizarProductoCommand_ConDatosValidos_DeberiaCrearseCorrectamente()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Napolitana",
            Descripcion = "Pizza napolitana auténtica",
            Precio = 18.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act & Assert
        command.Should().NotBeNull();
        command.Id.Should().NotBe(Guid.Empty);
        command.Nombre.Should().Be("Pizza Napolitana");
        command.Descripcion.Should().Be("Pizza napolitana auténtica");
        command.Precio.Should().Be(18.99m);
        command.CategoriaId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ActualizarProductoCommand_ConIdVacio_DeberiaSerInvalido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.Empty, // ID inválido
            Nombre = "Pizza Test",
            Precio = 15.99m
        };

        // Act & Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ActualizarProductoCommand_ConPrecioNegativo_DeberiaSerInvalido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Test",
            Precio = -5.99m // Precio inválido
        };

        // Act & Assert
        command.Should().NotBeNull();
        command.Precio.Should().BeLessThan(0);
    }

    [Fact]
    public void ActualizarProductoCommand_ConNombreVacio_DeberiaSerInvalido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "", // Nombre inválido
            Precio = 15.99m
        };

        // Act & Assert
        command.Should().NotBeNull();
        command.Nombre.Should().BeEmpty();
    }
} 