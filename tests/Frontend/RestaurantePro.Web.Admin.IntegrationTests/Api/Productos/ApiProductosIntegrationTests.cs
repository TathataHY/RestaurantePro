using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Web.Admin.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using ProductoDto = RestaurantePro.Application.Core.Productos.DTOs.ProductoDto;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Productos;

/// <summary>
/// Pruebas de integración para la API de productos usando la API con base de datos en memoria
/// </summary>
public class ApiProductosIntegrationTests : BaseIntegrationTest
{
    public ApiProductosIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/core/productos

    [Fact]
    public async Task ObtenerProductos_ConParametrosValidos_DeberiaRetornarListaPaginada()
    {
        // Arrange
        var query = "?pagina=1&tamanoPagina=10&soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerProductos_ConPaginacionInvalida_DeberiaRetornarError()
    {
        // Arrange
        var query = "?pagina=0&tamanoPagina=0";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltros_DeberiaAplicarFiltrosCorrectamente()
    {
        // Arrange
        var query = "?pagina=1&tamanoPagina=5&soloActivos=true&categoriaId=00000000-0000-0000-0000-000000000001";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    #endregion

    #region GET /api/core/productos/{id}

    [Fact]
    public async Task ObtenerProductoPorId_ConIdValido_DeberiaRetornarProducto()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/productos/{productoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerProductoPorId_ConIdInvalido_DeberiaRetornarError()
    {
        // Arrange
        var productoId = "id-invalido";

        // Act
        var response = await _client.GetAsync($"/api/core/productos/{productoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/core/productos/categoria/{categoriaId}

    [Fact]
    public async Task ObtenerProductosPorCategoria_ConCategoriaValida_DeberiaRetornarLista()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var query = "?soloActivos=true&ordenarPorPopularidad=false";

        // Act
        var response = await _client.GetAsync($"/api/core/productos/categoria/{categoriaId}{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerProductosPorCategoria_ConCategoriaInvalida_DeberiaRetornarError()
    {
        // Arrange
        var categoriaId = "categoria-invalida";

        // Act
        var response = await _client.GetAsync($"/api/core/productos/categoria/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/core/productos

    [Fact]
    public async Task CrearProducto_ConDatosValidos_DeberiaCrearProducto()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var producto = CreateValidProductoRequest(categoriaId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", producto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Nombre.Should().Be(producto.Nombre);
        apiResponse.Data.Precio.Should().Be(producto.Precio);
        apiResponse.Data.CategoriaId.Should().Be(producto.CategoriaId);
    }

    [Fact]
    public async Task CrearProducto_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var producto = CreateInvalidProductoRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", producto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConCategoriaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var categoriaId = Guid.NewGuid(); // Categoría que no existe
        var producto = CreateValidProductoRequest(categoriaId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", producto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConNombreDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var producto1 = CreateValidProductoRequest(categoriaId, "Producto Duplicado");
        var producto2 = CreateValidProductoRequest(categoriaId, "Producto Duplicado");

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/core/productos", producto1);
        var response2 = await _client.PostAsJsonAsync("/api/core/productos", producto2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/core/productos/{id}

    [Fact]
    public async Task ActualizarProducto_ConDatosValidos_DeberiaActualizarProducto()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var producto = CreateValidProductoRequest(categoriaId);
        
        // Crear producto primero
        var createResponse = await _client.PostAsJsonAsync("/api/core/productos", producto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(createContent, GetJsonOptions());
        var productoId = createApiResponse!.Data!.Id;

        // Actualizar producto
        var productoActualizado = CreateValidProductoRequest(categoriaId, "Producto Actualizado", 25.99m);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/core/productos/{productoId}", productoActualizado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Nombre.Should().Be("Producto Actualizado");
        apiResponse.Data.Precio.Should().Be(25.99m);
    }

    [Fact]
    public async Task ActualizarProducto_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var productoId = Guid.NewGuid();
        var producto = CreateValidProductoRequest(categoriaId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/core/productos/{productoId}", producto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarProducto_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var producto = CreateValidProductoRequest(categoriaId);
        
        // Crear producto primero
        var createResponse = await _client.PostAsJsonAsync("/api/core/productos", producto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(createContent, GetJsonOptions());
        var productoId = createApiResponse!.Data!.Id;

        // Actualizar con datos inválidos
        var productoInvalido = CreateInvalidProductoRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/core/productos/{productoId}", productoInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/core/productos/{id}

    [Fact]
    public async Task EliminarProducto_ConIdValido_DeberiaEliminarProducto()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var producto = CreateValidProductoRequest(categoriaId);
        
        // Crear producto primero
        var createResponse = await _client.PostAsJsonAsync("/api/core/productos", producto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(createContent, GetJsonOptions());
        var productoId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/core/productos/{productoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarProducto_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/core/productos/{productoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    // Los métodos helper ahora están en la clase base BaseIntegrationTest

    #endregion
}
