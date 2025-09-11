using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Core.Productos;
using RestaurantePro.Domain.Core.Productos.ValueObjects;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de integración completos para el controlador de productos.
/// Valida interacción real con BD y reglas de negocio específicas.
/// </summary>
[Collection("Sequential")]
public class ProductosControllerTests : ApiIntegrationTestBase
{
    public ProductosControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProductos_SinProductos_DebeRetornarListaVacia()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/core/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductos_ConProductosEnBD_DebeRetornarProductos()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var producto1 = CrearProductoTest("hamburguesa");
        var producto2 = CrearProductoTest("pizza");
        
        context.Productos.Add(producto1);
        context.Productos.Add(producto2);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/core/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProducto_ConIdExistente_DebeRetornarProducto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        var producto = CrearProductoTest("ensalada");
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/core/productos/{producto.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProducto_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/core/productos/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetProductosPorCategoria_ConProductosEnCategoria_DebeRetornarProductos()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        var categoria = CrearCategoriaTest("cat1");
        context.ProductoCategorias.Add(categoria);
        await context.SaveChangesAsync();
        var producto1 = CrearProductoTest("producto1", categoria.Id);
        var producto2 = CrearProductoTest("producto2", categoria.Id);
        context.Productos.Add(producto1);
        context.Productos.Add(producto2);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/core/productos/categoria/{categoria.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task PostProducto_ConDatosValidos_DebeCrearProducto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        var categoria = CrearCategoriaTest("cat2");
        context.ProductoCategorias.Add(categoria);
        await context.SaveChangesAsync();
        var nuevoProducto = new
        {
            Nombre = "Empanadas Chilenas",
            Descripcion = "Empanadas chilenas tradicionales con carne",
            Precio = 80.00m,
            CategoriaId = categoria.Id,
            CategoriaNombre = categoria.Nombre
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/productos", nuevoProducto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Verificar que el producto se creó en la base de datos
        var productoEnBD = await context.Productos.FirstOrDefaultAsync(p => p.Nombre == "Empanadas Chilenas");
        productoEnBD.Should().NotBeNull();
        productoEnBD!.Precio.Valor.Should().Be(80.00m);
        productoEnBD.EstaActivo.Should().BeTrue();
    }

    [Fact]
    public async Task PostProducto_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        var productoInvalido = new
        {
            Nombre = "", // Nombre vacío
            Descripcion = "Descripción válida",
            Precio = -10.00m, // Precio negativo
            CategoriaId = Guid.NewGuid(),
            CategoriaNombre = "Platos Principales"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/productos", productoInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PutProducto_ConDatosValidos_DebeActualizarProducto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        var categoria = CrearCategoriaTest("cat3");
        context.ProductoCategorias.Add(categoria);
        await context.SaveChangesAsync();
        var producto = CrearProductoTest("producto-original", categoria.Id);
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var datosActualizados = new
        {
            Nombre = "Producto Actualizado",
            Descripcion = "Descripción actualizada",
            Precio = 150.00m,
            CategoriaId = categoria.Id,
            CategoriaNombre = categoria.Nombre
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/core/productos/{producto.Id}", datosActualizados);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Verificar que el producto se actualizó en la base de datos
        context.ChangeTracker.Clear();
        var productoActualizado = await context.Productos.FindAsync(producto.Id);
        productoActualizado.Should().NotBeNull();
        // Log temporal para depuración
        Console.WriteLine($"Nombre real tras update: '{productoActualizado!.Nombre}'");
        productoActualizado!.Nombre.Should().Be("Producto Actualizado");
        productoActualizado.Precio.Valor.Should().Be(150.00m);
    }

    [Fact]
    public async Task PutProducto_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var datosActualizados = new
        {
            Nombre = "Producto Inexistente",
            Descripcion = "Descripción",
            Precio = 100.00m,
            CategoriaId = Guid.NewGuid(),
            CategoriaNombre = "Categoría"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/core/productos/{idInexistente}", datosActualizados);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProducto_ConIdExistente_DebeEliminarProducto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        var producto = CrearProductoTest("producto-a-eliminar");
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/productos/{producto.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Verificar que el producto fue desactivado (soft delete)
        context.ChangeTracker.Clear();
        var productoEnBD = await context.Productos.FindAsync(producto.Id);
        productoEnBD.Should().NotBeNull();
        productoEnBD!.EstaActivo.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProducto_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/productos/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    #region Métodos Helper

    private static ProductoCategoria CrearCategoriaTest(string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        return ProductoCategoria.Crear($"Categoría {sufijoUnico}", $"Descripción {sufijoUnico}", 1, "#FF5722", "🍽️");
    }

    private static Producto CrearProductoTest(string? sufijo = null, Guid? categoriaId = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        var precio = new PrecioProducto(25.99m);
        var categoriaIdFinal = categoriaId ?? Guid.NewGuid();
        
        return Producto.Crear(
            sufijoUnico,
            $"Descripción del producto {sufijoUnico}",
            precio,
            categoriaIdFinal,
            $"Categoría {sufijoUnico}"
        );
    }

    #endregion
} 