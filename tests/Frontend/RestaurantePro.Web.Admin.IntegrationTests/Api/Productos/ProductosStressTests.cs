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
/// Pruebas de estrés y carga para la API de productos
/// </summary>
public class ProductosStressTests : BaseIntegrationTest
{
    public ProductosStressTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearProductos_ConDatosVariados_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var productos = new List<CrearProductoCommand>();

        // Crear productos con diferentes características
        for (int i = 1; i <= 20; i++)
        {
            productos.Add(new CrearProductoCommand
            {
                Nombre = $"Producto {i:D2}",
                Descripcion = $"Descripción del producto {i}",
                Precio = 10.00m + (i * 2.50m),
                CategoriaId = categoriaId,
                Activo = i % 3 != 0 // Algunos inactivos
            });
        }

        var resultadosExitosos = 0;
        var resultadosFallidos = 0;

        // Act
        foreach (var producto in productos)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/api/core/productos", producto);
                
                if (response.IsSuccessStatusCode)
                {
                    resultadosExitosos++;
                }
                else
                {
                    resultadosFallidos++;
                }
            }
            catch
            {
                resultadosFallidos++;
            }
        }

        // Assert
        resultadosExitosos.Should().BeGreaterThan(0);
        (resultadosExitosos + resultadosFallidos).Should().Be(productos.Count);
    }

    [Fact]
    public async Task ObtenerProductos_ConPaginacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear algunos productos de prueba
        await CrearProductosDePrueba(categoriaId, 15);

        var casosPaginacion = new[]
        {
            new { Pagina = 1, Tamano = 1 },
            new { Pagina = 2, Tamano = 50 },
            new { Pagina = 1, Tamano = 100 },
            new { Pagina = 50, Tamano = 2 },
            new { Pagina = 10, Tamano = 10 }
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
        }
    }

    [Fact]
    public async Task ObtenerProductosPorCategoria_ConDatosVariados_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear productos de prueba
        await CrearProductosDePrueba(categoriaId, 10);

        var casosFiltro = new[]
        {
            new { SoloActivos = true, OrdenarPorPopularidad = false },
            new { SoloActivos = false, OrdenarPorPopularidad = false },
            new { SoloActivos = true, OrdenarPorPopularidad = true },
            new { SoloActivos = false, OrdenarPorPopularidad = true }
        };

        // Act & Assert
        foreach (var caso in casosFiltro)
        {
            var query = $"?soloActivos={caso.SoloActivos.ToString().ToLower()}&ordenarPorPopularidad={caso.OrdenarPorPopularidad.ToString().ToLower()}";
            var response = await _client.GetAsync($"/api/core/productos/categoria/{categoriaId}{query}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductoDto>>>(content, GetJsonOptions());
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ActualizarProductos_EnLote_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var productosCreados = await CrearProductosDePrueba(categoriaId, 5);
        
        var actualizacionesExitosas = 0;
        var actualizacionesFallidas = 0;

        // Act
        foreach (var producto in productosCreados)
        {
            try
            {
                var productoActualizado = new CrearProductoCommand
                {
                    Nombre = $"{producto.Nombre} - Actualizado",
                    Descripcion = $"{producto.Descripcion} - Modificado",
                    Precio = producto.Precio + 5.00m,
                    CategoriaId = producto.CategoriaId,
                    Activo = producto.Activo
                };

                var response = await _client.PutAsJsonAsync($"/api/core/productos/{producto.Id}", productoActualizado);
                
                if (response.IsSuccessStatusCode)
                {
                    actualizacionesExitosas++;
                }
                else
                {
                    actualizacionesFallidas++;
                }
            }
            catch
            {
                actualizacionesFallidas++;
            }
        }

        // Assert
        actualizacionesExitosas.Should().BeGreaterThan(0);
        (actualizacionesExitosas + actualizacionesFallidas).Should().Be(productosCreados.Count);
    }

    [Fact]
    public async Task EliminarProductos_EnLote_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var productosCreados = await CrearProductosDePrueba(categoriaId, 8);
        
        var eliminacionesExitosas = 0;
        var eliminacionesFallidas = 0;

        // Act
        foreach (var producto in productosCreados)
        {
            try
            {
                var response = await _client.DeleteAsync($"/api/core/productos/{producto.Id}");
                
                if (response.IsSuccessStatusCode)
                {
                    eliminacionesExitosas++;
                }
                else
                {
                    eliminacionesFallidas++;
                }
            }
            catch
            {
                eliminacionesFallidas++;
            }
        }

        // Assert
        eliminacionesExitosas.Should().BeGreaterThan(0);
        (eliminacionesExitosas + eliminacionesFallidas).Should().Be(productosCreados.Count);
    }

    [Fact]
    public async Task OperacionesConcurrentes_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        var tareas = new List<Task<HttpResponseMessage>>();

        // Act - Ejecutar operaciones concurrentes
        for (int i = 0; i < 10; i++)
        {
            var producto = new CrearProductoCommand
            {
                Nombre = $"Producto Concurrente {i}",
                Descripcion = $"Descripción del producto concurrente {i}",
                Precio = 20.00m + i,
                CategoriaId = categoriaId,
                Activo = true
            };

            tareas.Add(_client.PostAsJsonAsync("/api/core/productos", producto));
        }

        // Esperar a que todas las tareas terminen
        var respuestas = await Task.WhenAll(tareas);

        // Assert
        var exitosas = respuestas.Count(r => r.IsSuccessStatusCode);
        var fallidas = respuestas.Count(r => !r.IsSuccessStatusCode);

        exitosas.Should().BeGreaterThan(0);
        (exitosas + fallidas).Should().Be(10);
    }

    #region Helper Methods

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
                // Ignorar errores individuales en las pruebas de estrés
            }
        }

        return productosCreados;
    }

    #endregion
}
