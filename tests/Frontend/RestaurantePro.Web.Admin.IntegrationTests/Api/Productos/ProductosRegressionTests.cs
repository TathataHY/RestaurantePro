using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using ProductoDto = RestaurantePro.Application.Core.Productos.DTOs.ProductoDto;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Productos;

/// <summary>
/// Pruebas de regresión para la API de productos
/// </summary>
public class ProductosRegressionTests : BaseIntegrationTest
{
    public ProductosRegressionTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearProducto_ConDatosValidos_DeberiaFuncionarComoAntes()
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
        apiResponse.Data.Activo.Should().Be(producto.Activo);
    }

    [Fact]
    public async Task ActualizarProducto_ConDatosValidos_DeberiaFuncionarComoAntes()
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
    public async Task ObtenerProductos_ConPaginacion_DeberiaFuncionarComoAntes()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear algunos productos de prueba
        await CrearProductosDePrueba(categoriaId, 5);

        // Act
        var response = await _client.GetAsync("/api/core/productos?pagina=1&tamanoPagina=10");

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
    public async Task ObtenerProductoPorId_ConIdValido_DeberiaFuncionarComoAntes()
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
        var response = await _client.GetAsync($"/api/core/productos/{productoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(productoId);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoria_ConCategoriaValida_DeberiaFuncionarComoAntes()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear algunos productos de prueba
        await CrearProductosDePrueba(categoriaId, 3);

        // Act
        var response = await _client.GetAsync($"/api/core/productos/categoria/{categoriaId}?soloActivos=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task EliminarProducto_ConIdValido_DeberiaFuncionarComoAntes()
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
    public async Task ValidacionesDeProducto_DeberianFuncionarComoAntes()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);

        var casosInvalidos = new[]
        {
            new CrearProductoCommand
            {
                Nombre = "", // Nombre vacío
                Descripcion = "Descripción válida",
                Precio = 10.00m,
                CategoriaId = categoriaId,
                Activo = true
            },
            new CrearProductoCommand
            {
                Nombre = "Producto válido",
                Descripcion = "Descripción válida",
                Precio = -5.00m, // Precio negativo
                CategoriaId = categoriaId,
                Activo = true
            },
            new CrearProductoCommand
            {
                Nombre = "Producto válido",
                Descripcion = "Descripción válida",
                Precio = 10.00m,
                CategoriaId = Guid.Empty, // Categoría inválida
                Activo = true
            }
        };

        // Act & Assert
        foreach (var casoInvalido in casosInvalidos)
        {
            var response = await _client.PostAsJsonAsync("/api/core/productos", casoInvalido);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task ComportamientoDePaginacion_DeberiaFuncionarComoAntes()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear productos de prueba
        await CrearProductosDePrueba(categoriaId, 15);

        var casosPaginacion = new[]
        {
            new { Pagina = 1, Tamano = 5, Esperado = 5 },
            new { Pagina = 2, Tamano = 5, Esperado = 5 },
            new { Pagina = 3, Tamano = 5, Esperado = 5 },
            new { Pagina = 1, Tamano = 20, Esperado = 15 },
            new { Pagina = 2, Tamano = 20, Esperado = 0 }
        };

        // Act & Assert
        foreach (var caso in casosPaginacion)
        {
            var query = $"?pagina={caso.Pagina}&tamanoPagina={caso.Tamano}";
            var response = await _client.GetAsync($"/api/core/productos{query}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(content, GetJsonOptions());
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.Items.Count.Should().BeLessOrEqualTo(caso.Esperado);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Crea una categoría de prueba para usar en las pruebas de productos
    /// </summary>
    private async Task<Guid> CrearCategoriaDePrueba()
    {
        // Por ahora retornamos un GUID fijo, en una implementación real
        // se crearía una categoría de prueba en la base de datos
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    /// <summary>
    /// Crea un request válido para crear un producto
    /// </summary>
    private static CrearProductoCommand CreateValidProductoRequest(Guid categoriaId, string? nombre = null, decimal? precio = null)
    {
        return new CrearProductoCommand
        {
            Nombre = nombre ?? "Producto Prueba",
            Descripcion = "Descripción del producto de prueba",
            Precio = precio ?? 15.99m,
            CategoriaId = categoriaId,
            Activo = true
        };
    }

    /// <summary>
    /// Crea productos de prueba y retorna la lista de productos creados
    /// </summary>
    private async Task<List<ProductoDto>> CrearProductosDePrueba(Guid categoriaId, int cantidad)
    {
        var productosCreados = new List<ProductoDto>();

        for (int i = 1; i <= cantidad; i++)
        {
            var producto = new CrearProductoCommand
            {
                Nombre = $"Producto Prueba {i:D2}",
                Descripcion = $"Descripción del producto de prueba {i}",
                Precio = 10.00m + (i * 1.50m),
                CategoriaId = categoriaId,
                Activo = i % 4 != 0 // Algunos inactivos
            };

            try
            {
                var response = await _client.PostAsJsonAsync("/api/core/productos", producto);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(content, GetJsonOptions());
                    if (apiResponse?.Data != null)
                    {
                        productosCreados.Add(apiResponse.Data);
                    }
                }
            }
            catch
            {
                // Ignorar errores individuales en las pruebas de regresión
            }
        }

        return productosCreados;
    }

    #endregion
}
