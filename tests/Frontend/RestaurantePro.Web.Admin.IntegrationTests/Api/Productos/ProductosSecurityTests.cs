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
/// Pruebas de seguridad para la API de productos
/// Estas pruebas validan la robustez del sistema contra ataques y entrada maliciosa
/// </summary>
public class ProductosSecurityTests : BaseIntegrationTest
{
    public ProductosSecurityTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Pruebas de Inyección SQL

    [Fact]
    public async Task ObtenerProductos_ConFiltroSQLInjection_DeberiaEscaparCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var sqlInjection = "'; DROP TABLE Productos; --";
        var query = $"?filtro={sqlInjection}";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // El sistema debería manejar la inyección SQL sin fallar
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CrearProducto_ConNombreSQLInjection_DeberiaEscaparCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "'; DROP TABLE Productos; --";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        // Debería fallar por validación, no por inyección SQL
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            // El nombre debería estar escapado correctamente
            apiResponse.Data!.Nombre.Should().Be("'; DROP TABLE Productos; --");
        }
    }

    [Fact]
    public async Task ObtenerProductos_ConOrderBySQLInjection_DeberiaEscaparCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var sqlInjection = "Nombre; DROP TABLE Productos; --";
        var query = $"?orderBy={sqlInjection}";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pruebas de XSS (Cross-Site Scripting)

    [Fact]
    public async Task CrearProducto_ConNombreXSS_DeberiaEscaparCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "<script>alert('XSS')</script>";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        // El script debería estar escapado o filtrado
        apiResponse.Data!.Nombre.Should().NotContain("<script>");
    }

    [Fact]
    public async Task CrearProducto_ConDescripcionXSS_DeberiaEscaparCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Descripcion = "<img src=x onerror=alert('XSS')>";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        // El script debería estar escapado o filtrado
        apiResponse.Data!.Descripcion.Should().NotContain("onerror=");
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltroXSS_DeberiaEscaparCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var xssPayload = "<script>alert('XSS')</script>";
        var query = $"?filtro={xssPayload}";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pruebas de Validación de Entrada Maliciosa

    [Fact]
    public async Task CrearProducto_ConDatosNull_DeberiaRetornarError()
    {
        // Arrange
        var request = new CrearProductoCommand
        {
            Nombre = null!,
            Descripcion = null!,
            Precio = 0,
            CategoriaId = Guid.Empty,
            Activo = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConJsonMalformado_DeberiaRetornarError()
    {
        // Arrange
        var jsonMalformado = "{ \"nombre\": \"Test\", \"precio\": \"invalid\" }";
        var content = new StringContent(jsonMalformado, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearProducto_ConTiposIncorrectos_DeberiaRetornarError()
    {
        // Arrange
        var requestIncorrecto = new
        {
            nombre = 123, // Debería ser string
            descripcion = true, // Debería ser string
            precio = "invalid", // Debería ser decimal
            categoriaId = "not-a-guid", // Debería ser Guid
            estaDisponible = "yes" // Debería ser bool
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", requestIncorrecto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Límites de Tamaño

    [Fact]
    public async Task CrearProducto_ConRequestMuyGrande_DeberiaRetornarError()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Descripcion = new string('A', 10000); // Request muy grande

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConQueryMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var queryMuyLarga = "?filtro=" + new string('A', 5000);

        // Act
        var response = await _client.GetAsync($"/api/core/productos{queryMuyLarga}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Caracteres Especiales

    [Fact]
    public async Task CrearProducto_ConCaracteresUnicode_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "Café ☕ con leche 🥛 - 中文测试 🎉";
        request.Descripcion = "Descripción con emojis: 🍕🍔🍟🥤 y caracteres especiales: áéíóú ñüç";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Nombre.Should().Be("Café ☕ con leche 🥛 - 中文测试 🎉");
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltroUnicode_DeberiaFuncionar()
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

    #endregion

    #region Pruebas de Encoding

    [Fact]
    public async Task CrearProducto_ConEncodingUTF8_DeberiaFuncionar()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var request = CreateValidProductoRequest(categoriaId);
        request.Nombre = "Café con Açúcar";
        request.Descripcion = "Descripción con acentos: áéíóú ñüç";

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Nombre.Should().Be("Café con Açúcar");
    }

    #endregion

    #region Pruebas de Headers Maliciosos

    [Fact]
    public async Task ObtenerProductos_ConHeadersMaliciosos_DeberiaIgnorar()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        _client.DefaultRequestHeaders.Add("X-Malicious-Header", "<script>alert('XSS')</script>");
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "'; DROP TABLE Productos; --");

        // Act
        var response = await _client.GetAsync("/api/core/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Limpiar headers para otras pruebas
        _client.DefaultRequestHeaders.Clear();
    }

    #endregion

    #region Pruebas de Rate Limiting (Simulación)

    [Fact]
    public async Task CrearProducto_MultiplesRequestsRapidos_DeberiaManejarCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear múltiples requests simultáneos
        for (int i = 0; i < 10; i++)
        {
            var request = CreateValidProductoRequest(categoriaId);
            request.Nombre = $"Producto de prueba {i}";
            
            tasks.Add(_client.PostAsJsonAsync("/api/core/productos", request));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        // Al menos algunos deberían ser exitosos
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        successCount.Should().BeGreaterThan(0);
        
        // No debería haber errores 500 (Internal Server Error)
        var error500Count = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        error500Count.Should().Be(0);
    }

    #endregion

    #region Pruebas de Validación de Parámetros

    [Fact]
    public async Task ObtenerProductos_ConParametrosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var query = "?pagina=abc&tamanoPagina=xyz&soloActivos=maybe";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductos_ConParametrosExtranos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var query = "?pagina=1&tamanoPagina=10&parametroExtra=valor&otroParametro=123";

        // Act
        var response = await _client.GetAsync($"/api/core/productos{query}");

        // Assert
        // Debería ignorar parámetros extra y funcionar normalmente
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

}
