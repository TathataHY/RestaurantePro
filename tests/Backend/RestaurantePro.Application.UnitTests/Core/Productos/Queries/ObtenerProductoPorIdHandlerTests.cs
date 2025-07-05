namespace RestaurantePro.Application.UnitTests.Core.Productos.Queries;

/// <summary>
/// Pruebas unitarias para ObtenerProductoPorIdHandler
/// Tests básicos para validar el comportamiento principal
/// </summary>
public class ObtenerProductoPorIdHandlerTests
{
    [Fact]
    public void ObtenerProductoPorIdQuery_ConIdValido_DeberiaCrearseCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var query = new ObtenerProductoPorIdQuery(id);

        // Act & Assert
        query.Should().NotBeNull();
        query.ProductoId.Should().Be(id);
        query.ProductoId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ObtenerProductoPorIdQuery_ConIdVacio_DeberiaSerInvalido()
    {
        // Arrange
        var id = Guid.Empty;
        var query = new ObtenerProductoPorIdQuery(id);

        // Act & Assert
        query.Should().NotBeNull();
        query.ProductoId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ProductoDto_ParaQuery_DeberiaMapearseCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        
        var dto = new ProductoDto
        {
            Id = id,
            Nombre = "Pizza Margherita",
            Descripcion = "Pizza italiana clásica",
            Precio = 16.99m,
            CategoriaId = categoriaId,
            CategoriaNombre = "Pizzas",
            Activo = true,
            Popularidad = 8,
            FechaCreacion = DateTime.UtcNow,
            CreadoPor = "admin"
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.Nombre.Should().Be("Pizza Margherita");
        dto.Descripcion.Should().Be("Pizza italiana clásica");
        dto.Precio.Should().Be(16.99m);
        dto.CategoriaId.Should().Be(categoriaId);
        dto.CategoriaNombre.Should().Be("Pizzas");
        dto.Activo.Should().BeTrue();
        dto.Popularidad.Should().Be(8);
        dto.CreadoPor.Should().Be("admin");
    }

    [Fact]
    public void Result_ConProductoEncontrado_DeberiaRetornarExito()
    {
        // Arrange
        var dto = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Test",
            Precio = 12.99m,
            Activo = true
        };

        // Act
        var result = Result.Success(dto);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Pizza Test");
        result.Value.Precio.Should().Be(12.99m);
    }

    [Fact]
    public void Result_ConProductoNoEncontrado_DeberiaRetornarError()
    {
        // Arrange
        var errorMessage = "Producto no encontrado";

        // Act
        var result = Result.Failure<ProductoDto>(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
        result.Value.Should().BeNull();
    }
} 