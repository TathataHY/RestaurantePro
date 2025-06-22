using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de integración para el controlador de productos.
/// Ejemplos de tests HTTP completos con base de datos en memoria.
/// </summary>
[Collection("Sequential")]
public class ProductosControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    
    public ProductosControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }
    
    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }

    [Fact]
    public async Task GetProductos_SinProductos_DebeRetornarListaVacia()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/core/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
        
        // Verificar que la respuesta tiene el formato esperado
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync("/api/core/productos"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductos_ConProductosEnBD_DebeRetornarProductos()
    {
        // Arrange
        var producto1 = await CrearProductoPrueba("Hamburguesa", 150.00m);
        var producto2 = await CrearProductoPrueba("Pizza", 200.00m);

        // Act
        var response = await HttpClient.GetAsync("/api/core/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync("/api/core/productos"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task PostProducto_ConDatosValidos_DebeCrearProducto()
    {
        // Arrange
        var categoriaId = Guid.NewGuid(); // Generar un Guid válido para la categoría
        var nuevoProducto = new
        {
            Nombre = "Empanadas",
            Descripcion = "Empanadas chilenas tradicionales",
            Precio = 80.00m,
            CategoriaId = categoriaId,  // ✅ Corregido: usar CategoriaId como Guid
            Activo = true               // ✅ Corregido: usar Activo en lugar de Disponible
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/productos", nuevoProducto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verificar que el producto se creó en la base de datos
        var productosEnBD = await DbContext.Productos.ToListAsync();
        productosEnBD.Should().HaveCount(1);
        productosEnBD[0].Nombre.Should().Be("Empanadas");
        productosEnBD[0].Precio.Valor.Should().Be(80.00m); // ✅ Corregir: comparar con .Valor
    }

    [Fact]
    public async Task GetProducto_ConIdExistente_DebeRetornarProducto()
    {
        // Arrange
        var producto = await CrearProductoPrueba("Ensalada", 120.00m);

        // Act
        var response = await HttpClient.GetAsync($"/api/core/productos/{producto.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync($"/api/core/productos/{producto.Id}"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
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
    }

    [Fact]
    public async Task DeleteProducto_ConIdExistente_DebeEliminarProducto()
    {
        // Arrange
        var producto = await CrearProductoPrueba("Producto a eliminar", 50.00m);

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/productos/{producto.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el producto fue desactivado (soft delete)
        DbContext.ChangeTracker.Clear(); 
        var productoEnBD = await DbContext.Productos.FirstOrDefaultAsync(p => p.Id == producto.Id);
        productoEnBD.Should().NotBeNull();
        productoEnBD.EstaActivo.Should().BeFalse();
    }
} 