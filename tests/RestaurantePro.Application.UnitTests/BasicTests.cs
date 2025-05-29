namespace RestaurantePro.Application.UnitTests;

/// <summary>
/// Tests básicos para verificar que la estructura funciona
/// </summary>
public class BasicTests
{
    [Fact]
    public void AutoMapper_CoreProfile_ShouldBeValid()
    {
        // Arrange
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CoreMappingProfile>();
        });

        // Act & Assert
        configuration.Invoking(config => config.AssertConfigurationIsValid())
            .Should().NotThrow("porque todos los mapeos deben estar correctamente configurados");
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<IMediator>().Should().NotBeNull("porque MediatR debe estar registrado");
        serviceProvider.GetService<IMapper>().Should().NotBeNull("porque AutoMapper debe estar registrado");
    }

    [Fact]
    public void ProductoDto_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var dto = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Test Producto",
            Descripcion = "Test Descripción",
            Precio = 10.99m,
            CategoriaNombre = "Test Categoría",
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            CreadoPor = "Test User"
        };

        // Assert
        dto.Id.Should().NotBeEmpty();
        dto.Nombre.Should().Be("Test Producto");
        dto.Precio.Should().Be(10.99m);
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public void CrearProductoCommand_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Margherita",
            Descripcion = "Deliciosa pizza italiana",
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Assert
        command.Nombre.Should().Be("Pizza Margherita");
        command.Precio.Should().Be(15.99m);
        command.CategoriaId.Should().NotBeEmpty();
    }

    [Fact]
    public void ObtenerProductoPorIdQuery_ShouldHaveProductoId()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var query = new ObtenerProductoPorIdQuery(productoId);

        // Assert
        query.ProductoId.Should().Be(productoId);
    }

    [Fact]
    public void PaginatedList_ShouldWorkCorrectly()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        var paginatedList = new PaginatedList<string>(items, 10, 1, 3);

        // Assert
        paginatedList.Items.Should().HaveCount(3);
        paginatedList.TotalCount.Should().Be(10);
        paginatedList.PageNumber.Should().Be(1);
        paginatedList.PageSize.Should().Be(3);
        paginatedList.HasNextPage.Should().BeTrue();
        paginatedList.HasPreviousPage.Should().BeFalse();
    }
} 