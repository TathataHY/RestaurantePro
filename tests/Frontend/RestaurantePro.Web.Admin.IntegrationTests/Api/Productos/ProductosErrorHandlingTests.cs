using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Productos;

public class ProductosErrorHandlingTests : BaseIntegrationTest
{
    public ProductosErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearProducto_ConDatosInvalidos_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var request = new CrearProductoCommand
        {
            Nombre = "", // Nombre vacío
            Descripcion = "Descripción válida",
            Precio = -10.00m, // Precio negativo
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConCategoriaInexistente_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaInexistente = Guid.NewGuid();
        var request = new CrearProductoCommand
        {
            Nombre = "Producto Válido",
            Descripcion = "Descripción válida",
            Precio = 10.00m,
            CategoriaId = categoriaInexistente,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerProducto_ConIdInexistente_DeberiaRetornarError404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/productos/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ActualizarProducto_ConIdInexistente_DeberiaRetornarError404()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var idInexistente = Guid.NewGuid();

        var request = new ActualizarProductoCommand
        {
            Id = idInexistente,
            Nombre = "Producto Actualizado",
            Descripcion = "Descripción actualizada",
            Precio = 20.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PutAsync($"/api/core/productos/{idInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EliminarProducto_ConIdInexistente_DeberiaRetornarError404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/core/productos/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConJsonInvalido_DeberiaRetornarError400()
    {
        // Arrange
        var jsonInvalido = "{ \"nombre\": \"Producto\", \"precio\": \"invalid\" }";

        // Act
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarProducto_ConDatosInvalidos_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear un producto primero
        var productoId = await CrearUnProductoDePruebaAsync(categoriaId);

        var request = new ActualizarProductoCommand
        {
            Id = productoId,
            Nombre = "", // Nombre vacío
            Descripcion = "Descripción válida",
            Precio = -5.00m, // Precio negativo
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PutAsync($"/api/core/productos/{productoId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerProductos_ConParametrosInvalidos_DeberiaRetornarError400()
    {
        // Act - Página negativa
        var response1 = await _client.GetAsync("/api/core/productos?page=-1");
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Tamaño de página inválido
        var response2 = await _client.GetAsync("/api/core/productos?pageSize=0");
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Tamaño de página muy grande
        var response3 = await _client.GetAsync("/api/core/productos?pageSize=1000");
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoria_ConCategoriaInexistente_DeberiaRetornarError404()
    {
        // Arrange
        var categoriaInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/productos/categoria/{categoriaInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConNombreMuyLargo_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var request = new CrearProductoCommand
        {
            Nombre = new string('A', 1000), // Nombre muy largo
            Descripcion = "Descripción válida",
            Precio = 10.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConPrecioMuyAlto_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var request = new CrearProductoCommand
        {
            Nombre = "Producto Precio Alto",
            Descripcion = "Descripción válida",
            Precio = 999999999.99m, // Precio muy alto
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConDescripcionMuyLarga_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var request = new CrearProductoCommand
        {
            Nombre = "Producto Descripción Larga",
            Descripcion = new string('D', 10000), // Descripción muy larga
            Precio = 10.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ActualizarProducto_ConCategoriaInexistente_DeberiaRetornarError400()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var categoriaInexistente = Guid.NewGuid();
        
        // Crear un producto primero
        var productoId = await CrearUnProductoDePruebaAsync(categoriaId);

        var request = new ActualizarProductoCommand
        {
            Id = productoId,
            Nombre = "Producto Válido",
            Descripcion = "Descripción válida",
            Precio = 20.00m,
            CategoriaId = categoriaInexistente, // Categoría inexistente
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PutAsync($"/api/core/productos/{productoId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConDatosNulos_DeberiaRetornarError400()
    {
        // Arrange
        var request = new CrearProductoCommand
        {
            Nombre = null, // Nombre nulo
            Descripcion = null, // Descripción nula
            Precio = 0, // Precio cero
            CategoriaId = Guid.Empty, // ID vacío
            Activo = true
        };

        // Act
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerProductos_ConFiltroMuyLargo_DeberiaRetornarError400()
    {
        // Act
        var filtroMuyLargo = new string('F', 1000);
        var response = await _client.GetAsync($"/api/core/productos?filtro={filtroMuyLargo}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var request = new CrearProductoCommand
        {
            Nombre = "Producto con ñ, á, é, í, ó, ú y símbolos: @#$%",
            Descripcion = "Descripción con caracteres especiales: ñáéíóú @#$%^&*()",
            Precio = 15.50m,
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        responseData.Success.Should().BeTrue();
    }

    /// <summary>
    /// Helper para crear un producto de prueba y devolver su ID
    /// </summary>
    private async Task<Guid> CrearUnProductoDePruebaAsync(Guid categoriaId)
    {
        var request = new CrearProductoCommand
        {
            Nombre = "Producto Prueba",
            Descripcion = "Descripción de prueba",
            Precio = 10.00m,
            CategoriaId = categoriaId,
            Activo = true
        };

        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/core/productos", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<CrearProductoCommand>>(responseContent, GetJsonOptions());
        
        // Extraer el ID del Location header o de la respuesta
        var locationHeader = response.Headers.Location?.ToString();
        if (!string.IsNullOrEmpty(locationHeader))
        {
            var idString = locationHeader.Split('/').Last();
            return Guid.Parse(idString);
        }
        
        // Si no hay Location header, usar un ID generado
        return Guid.NewGuid();
    }
}
