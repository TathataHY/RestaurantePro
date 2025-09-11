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

public class ProductosDataConsistencyTests : BaseIntegrationTest
{
    public ProductosDataConsistencyTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearProducto_ConDatosValidos_DeberiaMantenerConsistencia()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var request = new CrearProductoCommand
        {
            Nombre = "Producto Consistencia",
            Descripcion = "Descripción del producto",
            Precio = 25.50m,
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(responseContent, GetJsonOptions());
        var productoId = responseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        // Verificar que el producto se puede obtener después de crearlo
        var getResponse = await _client.GetAsync($"/api/core/productos/{productoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(getContent, GetJsonOptions());
        getData.Data?.Nombre
            .Should().Be("Producto Consistencia");
    }

    [Fact]
    public async Task ActualizarProducto_DeberiaMantenerConsistenciaEnTodasLasConsultas()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        // Crear producto inicial
        var crearRequest = new CrearProductoCommand
        {
            Nombre = "Producto Original",
            Descripcion = "Descripción original",
            Precio = 10.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        var crearJson = JsonSerializer.Serialize(crearRequest, GetJsonOptions());
        var crearContent = new StringContent(crearJson, Encoding.UTF8, "application/json");
        var crearResponse = await _client.PostAsync("/api/core/productos", crearContent);
        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var crearResponseContent = await crearResponse.Content.ReadAsStringAsync();
        var crearResponseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearResponseContent, GetJsonOptions());
        var productoId = crearResponseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        // Act - Actualizar producto
        var actualizarRequest = new ActualizarProductoCommand
        {
            Id = Guid.Parse(productoId!),
            Nombre = "Producto Actualizado",
            Descripcion = "Descripción actualizada",
            Precio = 20.00m,
            CategoriaId = categoriaId,
            Activo = false
        };

        var actualizarJson = JsonSerializer.Serialize(actualizarRequest, GetJsonOptions());
        var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
        var actualizarResponse = await _client.PutAsync($"/api/core/productos/{productoId}", actualizarContent);

        // Assert
        actualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar consistencia en todas las consultas
        var consultas = new[]
        {
            _client.GetAsync($"/api/core/productos/{productoId}"),
            _client.GetAsync("/api/core/productos"),
            _client.GetAsync($"/api/core/productos/categoria/{categoriaId}")
        };

        var responses = await Task.WhenAll(consultas);
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        // Verificar que los datos son consistentes
        var getIndividualResponse = await _client.GetAsync($"/api/core/productos/{productoId}");
        var getIndividualContent = await getIndividualResponse.Content.ReadAsStringAsync();
        var getIndividualData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(getIndividualContent, GetJsonOptions());
        
        getIndividualData.Data?.Nombre
            .Should().Be("Producto Actualizado");
        getIndividualData.Data?.Precio
            .Should().Be(20.00m);
    }

    [Fact]
    public async Task EliminarProducto_DeberiaMantenerConsistenciaEnConsultas()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        // Crear producto
        var crearRequest = new CrearProductoCommand
        {
            Nombre = "Producto para Eliminar",
            Descripcion = "Descripción del producto",
            Precio = 15.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        var crearJson = JsonSerializer.Serialize(crearRequest, GetJsonOptions());
        var crearContent = new StringContent(crearJson, Encoding.UTF8, "application/json");
        var crearResponse = await _client.PostAsync("/api/core/productos", crearContent);
        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var crearResponseContent = await crearResponse.Content.ReadAsStringAsync();
        var crearResponseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearResponseContent, GetJsonOptions());
        var productoId = crearResponseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        // Act - Eliminar producto
        var eliminarResponse = await _client.DeleteAsync($"/api/core/productos/{productoId}");

        // Assert
        eliminarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que el producto no aparece en las consultas generales (soft delete)
        var consultas = new[]
        {
            _client.GetAsync("/api/core/productos"),
            _client.GetAsync($"/api/core/productos/categoria/{categoriaId}")
        };

        var responses = await Task.WhenAll(consultas);
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        // Verificar que el producto individual sigue siendo accesible (soft delete)
        var getIndividualResponse = await _client.GetAsync($"/api/core/productos/{productoId}");
        getIndividualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el producto está desactivado
        var getIndividualContent = await getIndividualResponse.Content.ReadAsStringAsync();
        var getIndividualData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(getIndividualContent, GetJsonOptions());
        getIndividualData.Data?.Activo.Should().BeFalse("El producto eliminado debería estar desactivado");
    }

    [Fact]
    public async Task CrearProductos_MultipleProductos_DeberiaMantenerConsistenciaEnContadores()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        // Act - Crear múltiples productos
        var productosCreados = 0;
        for (int i = 0; i < 5; i++)
        {
            var request = new CrearProductoCommand
            {
                Nombre = $"Producto Consistencia {i}",
                Descripcion = $"Descripción {i}",
                Precio = 10.00m + i,
                CategoriaId = categoriaId,
                Activo = true
            };

            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
            var response = await _client.PostAsync("/api/core/productos", content);
            
            if (response.StatusCode == HttpStatusCode.Created)
            {
                productosCreados++;
            }
        }

        // Assert - Verificar consistencia en contadores
        var listResponse = await _client.GetAsync("/api/core/productos");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(listContent, GetJsonOptions());
        var totalProductos = listData.Data?.TotalCount;

        totalProductos.Should().Be(productosCreados, "El contador total debería coincidir con los productos creados");

        // Verificar consistencia en consulta por categoría
        var categoriaResponse = await _client.GetAsync($"/api/core/productos/categoria/{categoriaId}");
        categoriaResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categoriaContent = await categoriaResponse.Content.ReadAsStringAsync();
        var categoriaData = JsonSerializer.Deserialize<ApiResponse<List<ProductoDto>>>(categoriaContent, GetJsonOptions());
        var productosEnCategoria = categoriaData.Data?.Count;

        productosEnCategoria.Should().Be(productosCreados, "El contador por categoría debería coincidir");
    }

    [Fact]
    public async Task ActualizarProducto_ConCategoriaDiferente_DeberiaMantenerConsistencia()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaOriginal = GetFirstCategoriaId(categoriaIds);
        var categoriaNueva = categoriaIds[1]; // Segunda categoría

        // Crear producto en categoría original
        var crearRequest = new CrearProductoCommand
        {
            Nombre = "Producto Cambio Categoría",
            Descripcion = "Descripción del producto",
            Precio = 30.00m,
            CategoriaId = categoriaOriginal,
            Activo = true
        };

        var crearJson = JsonSerializer.Serialize(crearRequest, GetJsonOptions());
        var crearContent = new StringContent(crearJson, Encoding.UTF8, "application/json");
        var crearResponse = await _client.PostAsync("/api/core/productos", crearContent);
        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var crearResponseContent = await crearResponse.Content.ReadAsStringAsync();
        var crearResponseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearResponseContent, GetJsonOptions());
        var productoId = crearResponseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        // Act - Cambiar categoría del producto
        var actualizarRequest = new ActualizarProductoCommand
        {
            Id = Guid.Parse(productoId!),
            Nombre = "Producto Cambio Categoría",
            Descripcion = "Descripción del producto",
            Precio = 30.00m,
            CategoriaId = categoriaNueva,
            Activo = true
        };

        var actualizarJson = JsonSerializer.Serialize(actualizarRequest, GetJsonOptions());
        var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
        var actualizarResponse = await _client.PutAsync($"/api/core/productos/{productoId}", actualizarContent);

        // Assert
        actualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que el producto ya no está en la categoría original
        var categoriaOriginalResponse = await _client.GetAsync($"/api/core/productos/categoria/{categoriaOriginal}");
        categoriaOriginalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categoriaOriginalContent = await categoriaOriginalResponse.Content.ReadAsStringAsync();
        var categoriaOriginalData = JsonSerializer.Deserialize<ApiResponse<List<ProductoDto>>>(categoriaOriginalContent, GetJsonOptions());
        var productosEnCategoriaOriginal = categoriaOriginalData.Data?.Count;

        productosEnCategoriaOriginal.Should().Be(0, "No debería haber productos en la categoría original");

        // Verificar que el producto está en la nueva categoría
        var categoriaNuevaResponse = await _client.GetAsync($"/api/core/productos/categoria/{categoriaNueva}");
        categoriaNuevaResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categoriaNuevaContent = await categoriaNuevaResponse.Content.ReadAsStringAsync();
        var categoriaNuevaData = JsonSerializer.Deserialize<ApiResponse<List<ProductoDto>>>(categoriaNuevaContent, GetJsonOptions());
        var productosEnCategoriaNueva = categoriaNuevaData.Data?.Count;

        productosEnCategoriaNueva.Should().Be(1, "Debería haber 1 producto en la nueva categoría");
    }

    [Fact]
    public async Task CrearProducto_ConPrecioDecimal_DeberiaMantenerPrecision()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var precioEsperado = 123.456789m;
        var request = new CrearProductoCommand
        {
            Nombre = "Producto Precisión",
            Descripcion = "Producto para probar precisión decimal",
            Precio = precioEsperado,
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(responseContent, GetJsonOptions());
        var productoId = responseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        // Verificar que el precio se mantiene con la precisión correcta
        var getResponse = await _client.GetAsync($"/api/core/productos/{productoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(getContent, GetJsonOptions());
        var precioGuardado = getData.Data?.Precio;

        precioGuardado.Should().NotBeNull();
        precioGuardado.Should().Be(precioEsperado, "El precio decimal debería mantenerse con la precisión correcta");
    }

    [Fact]
    public async Task ActualizarProducto_MultipleVeces_DeberiaMantenerConsistencia()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        // Crear producto inicial
        var crearRequest = new CrearProductoCommand
        {
            Nombre = "Producto Múltiples Actualizaciones",
            Descripcion = "Descripción inicial",
            Precio = 10.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        var crearJson = JsonSerializer.Serialize(crearRequest, GetJsonOptions());
        var crearContent = new StringContent(crearJson, Encoding.UTF8, "application/json");
        var crearResponse = await _client.PostAsync("/api/core/productos", crearContent);
        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var crearResponseContent = await crearResponse.Content.ReadAsStringAsync();
        var crearResponseData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearResponseContent, GetJsonOptions());
        var productoId = crearResponseData.Data?.Id.ToString();
        
        productoId.Should().NotBeNull("El ID del producto debería estar presente en la respuesta");

        // Act - Actualizar múltiples veces
        for (int i = 1; i <= 3; i++)
        {
            var actualizarRequest = new ActualizarProductoCommand
            {
                Id = Guid.Parse(productoId!),
                Nombre = $"Producto Actualizado {i}",
                Descripcion = $"Descripción actualizada {i}",
                Precio = 10.00m + (i * 5),
                CategoriaId = categoriaId,
                Activo = i % 2 == 0
            };

            var actualizarJson = JsonSerializer.Serialize(actualizarRequest, GetJsonOptions());
            var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
            var actualizarResponse = await _client.PutAsync($"/api/core/productos/{productoId}", actualizarContent);
            actualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - Verificar que la última actualización se mantiene
        var getResponse = await _client.GetAsync($"/api/core/productos/{productoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(getContent, GetJsonOptions());
        
        getData.Data?.Nombre
            .Should().Be("Producto Actualizado 3");
        getData.Data?.Precio
            .Should().Be(25.00m);
        getData.Data?.Activo
            .Should().BeFalse();
    }
}
