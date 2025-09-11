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
/// Pruebas de casos límite (edge cases) para la API de productos
/// Estas pruebas validan el comportamiento del sistema en situaciones extremas
/// </summary>
public class ProductosEdgeCasesTests : BaseIntegrationTest
{
    public ProductosEdgeCasesTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Casos Límite - Nombres de Productos

    [Fact]
    public async Task CrearProducto_ConNombreMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var nombreMuyLargo = new string('A', 500); // 500 caracteres
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = nombreMuyLargo;

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConNombreVacio_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConNombreSoloEspacios_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "   ";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConNombreConCaracteresEspeciales_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "Café con Açúcar & Café (100%) - ¡Especial!";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Nombre.Should().Be("Café con Açúcar & Café (100%) - ¡Especial!");
    }

    #endregion

    #region Casos Límite - Precios

    [Fact]
    public async Task CrearProducto_ConPrecioCero_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Precio = 0;

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConPrecioNegativo_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Precio = -10.50m;

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConPrecioMuyAlto_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Precio = 999999.99m;

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Precio.Should().Be(999999.99m);
    }

    [Fact]
    public async Task CrearProducto_ConPrecioConMuchosDecimales_DeberiaRedondear()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Precio = 15.999999m; // Debería redondear a 16.00

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        // El precio debería estar redondeado a 2 decimales
        apiResponse.Data!.Precio.Should().Be(16.00m);
    }

    #endregion

    #region Casos Límite - Descripciones

    [Fact]
    public async Task CrearProducto_ConDescripcionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Descripcion = new string('A', 2000); // 2000 caracteres

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConDescripcionVacia_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Descripcion = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CrearProducto_ConDescripcionConEmojis_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Descripcion = "Delicioso café ☕ con leche 🥛 y azúcar 🍯 - ¡Perfecto para el desayuno! 🌅";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Descripcion.Should().Be("Delicioso café ☕ con leche 🥛 y azúcar 🍯 - ¡Perfecto para el desayuno! 🌅");
    }

    #endregion

    #region Casos Límite - Categorías

    [Fact]
    public async Task CrearProducto_ConCategoriaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var request = CreateValidProductoRequest(Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConCategoriaIdInvalido_DeberiaRetornarError()
    {
        // Arrange
        var request = CreateValidProductoRequest(Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Casos Límite - Paginación

    [Fact]
    public async Task ObtenerProductos_ConPaginaCero_DeberiaRetornarError()
    {
        // Arrange
        var query = "?pagina=0&tamanoPagina=10";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConPaginaNegativa_DeberiaRetornarError()
    {
        // Arrange
        var query = "?pagina=-1&tamanoPagina=10";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConTamanoPaginaCero_DeberiaRetornarError()
    {
        // Arrange
        var query = "?pagina=1&tamanoPagina=0";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConTamanoPaginaMuyGrande_DeberiaRetornarError()
    {
        // Arrange
        var query = "?pagina=1&tamanoPagina=1000";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConTamanoPaginaMaximoPermitido_DeberiaFuncionar()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?pagina=1&tamanoPagina=100"; // Máximo permitido

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Casos Límite - Filtros

    [Fact]
    public async Task ObtenerProductos_ConFiltroMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var filtroMuyLargo = new string('A', 500);
        var query = $"?filtro={filtroMuyLargo}";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltroConCaracteresEspeciales_DeberiaFuncionar()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?filtro=café&soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltroVacio_DeberiaFuncionar()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?filtro=&soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Casos Límite - Ordenamiento

    [Fact]
    public async Task ObtenerProductos_ConOrderByInvalido_DeberiaUsarDefault()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?orderBy=CampoInexistente&orderDirection=asc";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObtenerProductos_ConOrderDirectionInvalido_DeberiaUsarDefault()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?orderBy=Nombre&orderDirection=invalid";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Casos Límite - Estados

    [Fact]
    public async Task ObtenerProductos_ConSoloActivosTrue_DeberiaRetornarSoloActivos()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        await CrearProductosDePruebaAsync(categoriaIds);
        
        // Crear un producto y luego eliminarlo (desactivarlo)
        var productoRequest = CreateValidProductoRequest(categoriaId);
        var crearResponse = await _client.PostAsJsonAsync("/api/core/productos", productoRequest);
        var crearContent = await crearResponse.Content.ReadAsStringAsync();
        var crearApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearContent, GetJsonOptions());
        var productoId = crearApiResponse!.Data!.Id;
        
        // Eliminar el producto
        await _client.DeleteAsync($"/api/core/productos/{productoId}");
        
        var query = "?soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        // No debería incluir el producto eliminado
        apiResponse.Data!.Items.Should().NotContain(p => p.Id == productoId);
    }

    [Fact]
    public async Task ObtenerProductos_ConSoloActivosFalse_DeberiaRetornarTodos()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?soloActivos=false";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Casos Límite - Límites de Sistema

    [Fact]
    public async Task CrearProducto_ConDatosMinimosValidos_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = new CrearProductoCommand
        {
            Nombre = "A", // Mínimo posible
            Descripcion = "", // Vacío
            Precio = 0.01m, // Mínimo posible
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CrearProducto_ConDatosMaximosValidos_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = new CrearProductoCommand
        {
            Nombre = new string('A', 100), // Cerca del máximo
            Descripcion = new string('B', 1000), // Cerca del máximo
            Precio = 999999.99m, // Máximo razonable
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

}
