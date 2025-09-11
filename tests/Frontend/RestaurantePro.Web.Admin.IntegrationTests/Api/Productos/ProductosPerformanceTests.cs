using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
using RestaurantePro.Application.Core.Productos.Queries;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Productos;

public class ProductosPerformanceTests : BaseIntegrationTest
{
    public ProductosPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerProductos_ConMuchosDatos_DeberiaResponderRapidamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds); // Crear productos de prueba

        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/api/core/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalMilliseconds.Should().BeLessThan(1000, "La consulta debería completarse en menos de 1 segundo");
    }

    [Fact]
    public async Task ObtenerProductos_ConPaginacionGrande_DeberiaMantenerRendimiento()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);

        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/api/core/productos?pageSize=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalMilliseconds.Should().BeLessThan(500, "La paginación grande debería ser rápida");
    }

    [Fact]
    public async Task CrearProducto_MultipleProductos_DeberiaMantenerRendimiento()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Crear 10 productos concurrentemente
        for (int i = 0; i < 10; i++)
        {
            var request = new CrearProductoCommand
            {
                Nombre = $"Producto Performance {i}",
                Descripcion = $"Descripción del producto {i}",
                Precio = 10.50m + i,
                CategoriaId = categoriaId,
                Activo = true
            };

            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
            tasks.Add(_client.PostAsync("/api/core/productos", content));
        }

        var responses = await Task.WhenAll(tasks);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));
        elapsed.TotalSeconds.Should().BeLessThan(5, "Crear 10 productos debería completarse en menos de 5 segundos");
    }

    [Fact]
    public async Task ObtenerProductosPorCategoria_ConFiltro_DeberiaSerEficiente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync($"/api/core/productos/categoria/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalMilliseconds.Should().BeLessThan(300, "La consulta por categoría debería ser muy rápida");
    }

    [Fact]
    public async Task BuscarProductos_ConFiltroTexto_DeberiaSerRapida()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);

        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/api/core/productos?filtro=Producto");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalMilliseconds.Should().BeLessThan(200, "La búsqueda por texto debería ser muy rápida");
    }

    [Fact]
    public async Task ActualizarProducto_MultipleActualizaciones_DeberiaMantenerRendimiento()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        // Crear un producto inicial
        var crearRequest = new CrearProductoCommand
        {
            Nombre = "Producto para Actualizar",
            Descripcion = "Descripción inicial",
            Precio = 10.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        var crearJson = JsonSerializer.Serialize(crearRequest, GetJsonOptions());
        var crearContent = new StringContent(crearJson, Encoding.UTF8, "application/json");
        var crearResponse = await _client.PostAsync("/api/core/productos", crearContent);
        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await crearResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(responseContent, GetJsonOptions());
        var productoId = responseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Actualizar el mismo producto 5 veces concurrentemente
        for (int i = 0; i < 5; i++)
        {
            var actualizarRequest = new ActualizarProductoCommand
            {
                Id = Guid.Parse(productoId!),
                Nombre = $"Producto Actualizado {i}",
                Descripcion = $"Descripción actualizada {i}",
                Precio = 15.00m + i,
                CategoriaId = categoriaId,
                Activo = true
            };

            var actualizarJson = JsonSerializer.Serialize(actualizarRequest, GetJsonOptions());
            var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
            tasks.Add(_client.PutAsync($"/api/core/productos/{productoId}", actualizarContent));
        }

        var responses = await Task.WhenAll(tasks);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        elapsed.TotalSeconds.Should().BeLessThan(3, "Actualizar 5 veces debería completarse en menos de 3 segundos");
    }

    [Fact]
    public async Task ObtenerProductos_ConCache_DeberiaSerMasRapida()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);

        // Primera consulta (sin cache)
        var startTime1 = DateTime.UtcNow;
        var response1 = await _client.GetAsync("/api/core/productos");
        var elapsed1 = DateTime.UtcNow - startTime1;

        // Segunda consulta (con cache)
        var startTime2 = DateTime.UtcNow;
        var response2 = await _client.GetAsync("/api/core/productos");
        var elapsed2 = DateTime.UtcNow - startTime2;

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        elapsed2.TotalMilliseconds.Should().BeLessThan(elapsed1.TotalMilliseconds * 0.8, 
            "La segunda consulta debería ser al menos 20% más rápida debido al cache");
    }

    [Fact]
    public async Task CrearProducto_ConValidacionesComplejas_DeberiaMantenerRendimiento()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Crear productos con diferentes validaciones
        for (int i = 0; i < 15; i++)
        {
            var request = new CrearProductoCommand
            {
                Nombre = $"Producto Validación {i}",
                Descripcion = $"Descripción muy larga para validar el rendimiento del sistema de validación {i}",
                Precio = 100.00m + (i * 0.01m),
                CategoriaId = categoriaId,
                Activo = i % 2 == 0
            };

            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
            tasks.Add(_client.PostAsync("/api/core/productos", content));
        }

        var responses = await Task.WhenAll(tasks);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        successCount.Should().Be(15, "Todos los productos deberían crearse exitosamente");
        elapsed.TotalSeconds.Should().BeLessThan(4, "Las validaciones complejas deberían completarse en menos de 4 segundos");
    }

    [Fact]
    public async Task ObtenerProductos_ConOrdenamiento_DeberiaMantenerRendimiento()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);

        var startTime = DateTime.UtcNow;

        // Act - Consultar con diferentes ordenamientos
        var tasks = new[]
        {
            _client.GetAsync("/api/core/productos?ordenarPor=nombre"),
            _client.GetAsync("/api/core/productos?ordenarPor=precio"),
            _client.GetAsync("/api/core/productos?ordenarPor=fechaCreacion")
        };

        var responses = await Task.WhenAll(tasks);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        elapsed.TotalMilliseconds.Should().BeLessThan(600, "El ordenamiento debería completarse en menos de 600ms");
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltrosComplejos_DeberiaSerEficiente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);

        var startTime = DateTime.UtcNow;

        // Act - Consultar con múltiples filtros
        var response = await _client.GetAsync("/api/core/productos?filtro=Producto&precioMinimo=5&precioMaximo=50&activo=true&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalMilliseconds.Should().BeLessThan(400, "Los filtros complejos deberían procesarse rápidamente");
    }
}
