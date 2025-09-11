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
/// Pruebas de monitoreo y métricas para la API de productos
/// </summary>
public class ProductosMonitoringTests : BaseIntegrationTest
{
    public ProductosMonitoringTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task TiempoDeRespuesta_ObtenerProductos_DeberiaSerAceptable()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        await CrearProductosDePrueba(categoriaId, 10);

        var inicio = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/api/core/productos?pagina=1&tamanoPagina=10");

        var fin = DateTime.UtcNow;
        var tiempoTranscurrido = fin - inicio;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tiempoTranscurrido.TotalMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    [Fact]
    public async Task TiempoDeRespuesta_CrearProducto_DeberiaSerAceptable()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var producto = CreateValidProductoRequest(categoriaId);

        var inicio = DateTime.UtcNow;

        // Act
        var response = await _client.PostAsJsonAsync("/api/core/productos", producto);

        var fin = DateTime.UtcNow;
        var tiempoTranscurrido = fin - inicio;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        tiempoTranscurrido.TotalMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    [Fact]
    public async Task TiempoDeRespuesta_ActualizarProducto_DeberiaSerAceptable()
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

        var productoActualizado = CreateValidProductoRequest(categoriaId, "Producto Actualizado", 25.99m);

        var inicio = DateTime.UtcNow;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/core/productos/{productoId}", productoActualizado);

        var fin = DateTime.UtcNow;
        var tiempoTranscurrido = fin - inicio;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tiempoTranscurrido.TotalMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    [Fact]
    public async Task TiempoDeRespuesta_EliminarProducto_DeberiaSerAceptable()
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

        var inicio = DateTime.UtcNow;

        // Act
        var response = await _client.DeleteAsync($"/api/core/productos/{productoId}");

        var fin = DateTime.UtcNow;
        var tiempoTranscurrido = fin - inicio;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tiempoTranscurrido.TotalMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    [Fact]
    public async Task ConsistenciaDeDatos_DespuesDeOperaciones_DeberiaMantenerse()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var productosCreados = new List<ProductoDto>();

        // Act - Crear varios productos
        for (int i = 1; i <= 5; i++)
        {
            var producto = CreateValidProductoRequest(categoriaId, $"Producto {i}", 10.00m + i);
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

        // Verificar que todos los productos se pueden obtener
        foreach (var producto in productosCreados)
        {
            var getResponse = await _client.GetAsync($"/api/core/productos/{producto.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Verificar que aparecen en la lista paginada
        var listResponse = await _client.GetAsync("/api/core/productos?pagina=1&tamanoPagina=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listApiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(listContent, GetJsonOptions());
        
        listApiResponse.Should().NotBeNull();
        listApiResponse!.Success.Should().BeTrue();
        listApiResponse.Data.Should().NotBeNull();
        listApiResponse.Data!.Items.Count.Should().BeGreaterOrEqualTo(productosCreados.Count);
    }

    [Fact]
    public async Task ManejoDeErrores_ConDatosInvalidos_DeberiaSerConsistente()
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
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeFalse();
            apiResponse.Errors.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task Rendimiento_ConCargaModerada_DeberiaMantenerse()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var tareas = new List<Task<HttpResponseMessage>>();

        // Act - Ejecutar múltiples operaciones concurrentes
        for (int i = 0; i < 20; i++)
        {
            var producto = CreateValidProductoRequest(categoriaId, $"Producto Carga {i}", 10.00m + i);
            tareas.Add(_client.PostAsJsonAsync("/api/core/productos", producto));
        }

        var inicio = DateTime.UtcNow;
        var respuestas = await Task.WhenAll(tareas);
        var fin = DateTime.UtcNow;

        var tiempoTotal = fin - inicio;

        // Assert
        tiempoTotal.TotalSeconds.Should().BeLessThan(10); // Menos de 10 segundos para 20 operaciones
        
        var exitosas = respuestas.Count(r => r.IsSuccessStatusCode);
        var fallidas = respuestas.Count(r => !r.IsSuccessStatusCode);
        
        exitosas.Should().BeGreaterThan(0);
        (exitosas + fallidas).Should().Be(20);
    }

    [Fact]
    public async Task Disponibilidad_EndpointsPrincipales_DeberiaSerAlta()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var endpoints = new[]
        {
            "/api/core/productos?pagina=1&tamanoPagina=10",
            $"/api/core/productos/categoria/{categoriaId}?soloActivos=true"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Escalabilidad_ConDatosCrecientes_DeberiaMantenerse()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear productos en lotes
        var lotes = new[] { 5, 10, 15, 20 };
        var tiemposRespuesta = new List<double>();

        foreach (var lote in lotes)
        {
            // Crear productos del lote
            await CrearProductosDePrueba(categoriaId, lote);

            // Medir tiempo de respuesta para obtener todos los productos
            var inicio = DateTime.UtcNow;
            var response = await _client.GetAsync("/api/core/productos?pagina=1&tamanoPagina=50");
            var fin = DateTime.UtcNow;

            var tiempo = (fin - inicio).TotalMilliseconds;
            tiemposRespuesta.Add(tiempo);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - El tiempo de respuesta no debería crecer exponencialmente
        for (int i = 1; i < tiemposRespuesta.Count; i++)
        {
            var crecimiento = tiemposRespuesta[i] / tiemposRespuesta[i - 1];
            crecimiento.Should().BeLessThan(2.5); // Ajustado para ser más realista con base de datos en memoria
        }
    }

    #region Helper Methods

    // Los métodos helper ahora están en la clase base BaseIntegrationTest

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
                // Ignorar errores individuales en las pruebas de monitoreo
            }
        }

        return productosCreados;
    }

    #endregion
}
