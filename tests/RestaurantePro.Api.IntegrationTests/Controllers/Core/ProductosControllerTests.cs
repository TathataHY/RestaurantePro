using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de integración para el controlador de productos.
/// Ejemplos de tests HTTP completos con base de datos en memoria.
/// </summary>
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
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
        
        // Verificar que la respuesta tiene el formato esperado
        var apiResponse = await ExecuteAndDeserializeAsync<List<object>>(
            client => client.GetAsync("/api/core/productos"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().BeEmpty();
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
        
        var apiResponse = await ExecuteAndDeserializeAsync<List<object>>(
            client => client.GetAsync("/api/core/productos"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task PostProducto_ConDatosValidos_DebeCrearProducto()
    {
        // Arrange
        var nuevoProducto = new
        {
            Nombre = "Tacos",
            Descripcion = "Tacos mexicanos deliciosos",
            Precio = 80.00m,
            Categoria = "Comida Mexicana",
            Disponible = true
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/productos", nuevoProducto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verificar que el producto se creó en la base de datos
        var productosEnBD = await DbContext.Productos.ToListAsync();
        productosEnBD.Should().HaveCount(1);
        productosEnBD[0].Nombre.Should().Be("Tacos");
        productosEnBD[0].Precio.Should().Be(80.00m);
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
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar que el producto ya no existe en la base de datos
        var productoEnBD = await DbContext.Productos.FindAsync(producto.Id);
        productoEnBD.Should().BeNull();
    }
} 