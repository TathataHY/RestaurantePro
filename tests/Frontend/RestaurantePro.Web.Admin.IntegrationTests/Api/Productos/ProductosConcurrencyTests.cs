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
/// Pruebas de concurrencia para la API de productos
/// Estas pruebas validan el comportamiento del sistema bajo condiciones de concurrencia
/// </summary>
public class ProductosConcurrencyTests : BaseIntegrationTest
{
    public ProductosConcurrencyTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Pruebas de Creación Concurrente

    [Fact]
    public async Task CrearProducto_MultiplesUsuariosSimultaneos_DeberiaManejarCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        var nombresProductos = new List<string>();
        
        // Crear 20 requests simultáneos
        for (int i = 0; i < 20; i++)
        {
            var nombre = $"Producto Concurrente {i}";
            nombresProductos.Add(nombre);
            
            var request = CreateValidProductoRequest(categoriaId);
            request.Nombre = nombre;
            
            tasks.Add(_client.PostAsJsonAsync("/api/core/productos", request));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // Al menos algunos deberían ser exitosos
        successCount.Should().BeGreaterThan(0);
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
        
        // Verificar que los productos creados existen
        var getResponse = await _client.GetAsync("/api/core/productos");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await getResponse.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(content, GetJsonOptions());
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearProducto_MismoNombreConcurrente_DeberiaManejarConflictos()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var nombreDuplicado = "Producto Duplicado Concurrente";
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear 5 requests con el mismo nombre
        for (int i = 0; i < 5; i++)
        {
            var request = CreateValidProductoRequest(categoriaId);
            request.Nombre = nombreDuplicado;
            
            tasks.Add(_client.PostAsJsonAsync("/api/core/productos", request));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
        
        // Solo uno debería ser exitoso
        successCount.Should().Be(1);
        
        // Los demás deberían fallar por conflicto
        conflictCount.Should().Be(4);
    }

    #endregion

    #region Pruebas de Actualización Concurrente

    [Fact]
    public async Task ActualizarProducto_MultiplesActualizacionesSimultaneas_DeberiaManejarCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear un producto inicial
        var productoRequest = CreateValidProductoRequest(categoriaId);
        var crearResponse = await _client.PostAsJsonAsync("/api/core/productos", productoRequest);
        var crearContent = await crearResponse.Content.ReadAsStringAsync();
        var crearApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearContent, GetJsonOptions());
        var productoId = crearApiResponse!.Data!.Id;
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear 10 actualizaciones simultáneas
        for (int i = 0; i < 10; i++)
        {
            var updateRequest = new
            {
                nombre = $"Producto Actualizado {i}",
                descripcion = $"Descripción actualizada {i}",
                precio = 10.50m + i,
                categoriaId = categoriaId,
                estaDisponible = true
            };
            
            tasks.Add(_client.PutAsJsonAsync($"/api/core/productos/{productoId}", updateRequest));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // Al menos algunas actualizaciones deberían ser exitosas
        successCount.Should().BeGreaterThan(0);
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
        
        // Verificar que el producto fue actualizado
        var getResponse = await _client.GetAsync($"/api/core/productos/{productoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarProducto_ConEliminacionSimultanea_DeberiaManejarCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear un producto inicial
        var productoRequest = CreateValidProductoRequest(categoriaId);
        var crearResponse = await _client.PostAsJsonAsync("/api/core/productos", productoRequest);
        var crearContent = await crearResponse.Content.ReadAsStringAsync();
        var crearApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductoDto>>(crearContent, GetJsonOptions());
        var productoId = crearApiResponse!.Data!.Id;
        
        // Crear tareas concurrentes: actualización y eliminación
        var updateRequest = new
        {
            nombre = "Producto Actualizado",
            descripcion = "Descripción actualizada",
            precio = 15.99m,
            categoriaId = categoriaId,
            estaDisponible = true
        };
        
        var updateTask = _client.PutAsJsonAsync($"/api/core/productos/{productoId}", updateRequest);
        var deleteTask = _client.DeleteAsync($"/api/core/productos/{productoId}");

        // Act
        var responses = await Task.WhenAll(updateTask, deleteTask);

        // Assert
        // Al menos una operación debería ser exitosa
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successCount.Should().BeGreaterThan(0);
        
        // No debería haber errores 500
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        errorCount.Should().Be(0);
    }

    #endregion

    #region Pruebas de Lectura Concurrente

    [Fact]
    public async Task ObtenerProductos_MultiplesLecturasSimultaneas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        await CrearProductosDePruebaAsync(categoriaIds);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear 50 requests de lectura simultáneos
        for (int i = 0; i < 50; i++)
        {
            var query = $"?pagina=1&tamanoPagina=10&filtro=Producto";
            tasks.Add(_client.GetAsync($"/api/core/productos{query}"));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // Todas las lecturas deberían ser exitosas
        successCount.Should().Be(50);
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerProductos_ConEscrituraSimultanea_DeberiaManejarCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var readTasks = new List<Task<HttpResponseMessage>>();
        var writeTasks = new List<Task<HttpResponseMessage>>();
        
        // Crear tareas de lectura
        for (int i = 0; i < 20; i++)
        {
            readTasks.Add(_client.GetAsync("/api/core/productos"));
        }
        
        // Crear tareas de escritura
        for (int i = 0; i < 10; i++)
        {
            var request = CreateValidProductoRequest(categoriaId);
            request.Nombre = $"Producto Concurrente {i}";
            writeTasks.Add(_client.PostAsJsonAsync("/api/core/productos", request));
        }

        // Act
        var readResponses = await Task.WhenAll(readTasks);
        var writeResponses = await Task.WhenAll(writeTasks);

        // Assert
        var readSuccessCount = readResponses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var writeSuccessCount = writeResponses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var errorCount = readResponses.Concat(writeResponses).Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // Las lecturas deberían ser exitosas
        readSuccessCount.Should().Be(20);
        
        // Al menos algunas escrituras deberían ser exitosas
        writeSuccessCount.Should().BeGreaterThan(0);
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
    }

    #endregion

    #region Pruebas de Transacciones Concurrentes

    [Fact]
    public async Task OperacionesComplejas_Concurrentes_DeberiaMantenerConsistencia()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear operaciones mixtas concurrentes
        for (int i = 0; i < 10; i++)
        {
            // Crear producto
            var createRequest = CreateValidProductoRequest(categoriaId);
            createRequest.Nombre = $"Producto Complejo {i}";
            tasks.Add(_client.PostAsJsonAsync("/api/core/productos", createRequest));
            
            // Leer productos
            tasks.Add(_client.GetAsync("/api/core/productos"));
            
            // Leer por categoría
            tasks.Add(_client.GetAsync($"/api/core/productos/categoria/{categoriaId}"));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.Created);
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // La mayoría de operaciones deberían ser exitosas
        successCount.Should().BeGreaterThan((int)(responses.Length * 0.8));
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
    }

    [Fact]
    public async Task CrearProducto_ConLecturaSimultanea_DeberiaMantenerConsistencia()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        // Crear tareas concurrentes
        var createTask = Task.Run(async () =>
        {
            var request = CreateValidProductoRequest(categoriaId);
            request.Nombre = "Producto de Consistencia";
            return await _client.PostAsJsonAsync("/api/core/productos", request);
        });
        
        var readTask = Task.Run(async () =>
        {
            await Task.Delay(100); // Pequeño delay para simular concurrencia
            return await _client.GetAsync("/api/core/productos");
        });

        // Act
        var responses = await Task.WhenAll(createTask, readTask);

        // Assert
        var createResponse = responses[0];
        var readResponse = responses[1];
        
        // Ambas operaciones deberían ser exitosas
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar consistencia de datos
        var readContent = await readResponse.Content.ReadAsStringAsync();
        var readApiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ProductoDto>>>(readContent, GetJsonOptions());
        
        readApiResponse.Should().NotBeNull();
        readApiResponse!.Success.Should().BeTrue();
        readApiResponse.Data!.Items.Should().NotBeEmpty();
    }

    #endregion

    #region Pruebas de Stress Concurrente

    [Fact]
    public async Task StressTest_CreacionMasiva_DeberiaManejarCorrectamente()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear 100 productos simultáneamente
        for (int i = 0; i < 100; i++)
        {
            var request = CreateValidProductoRequest(categoriaId);
            request.Nombre = $"Producto Stress {i}";
            
            tasks.Add(_client.PostAsJsonAsync("/api/core/productos", request));
        }

        // Act
        var startTime = DateTime.UtcNow;
        var responses = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // Al menos algunos deberían ser exitosos
        successCount.Should().BeGreaterThan(0);
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
        
        // El tiempo total debería ser razonable (menos de 30 segundos)
        duration.TotalSeconds.Should().BeLessThan(30);
        
        // Verificar que los productos fueron creados
        var getResponse = await _client.GetAsync("/api/core/productos");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StressTest_OperacionesMixtas_DeberiaMantenerEstabilidad()
    {
        // Arrange
        await CrearCategoriasDePruebaAsync();
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        var categoriaId = GetFirstCategoriaId(categoriaIds);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Crear operaciones mixtas intensivas
        for (int i = 0; i < 50; i++)
        {
            // Crear producto
            var createRequest = CreateValidProductoRequest(categoriaId);
            createRequest.Nombre = $"Producto Mixto {i}";
            tasks.Add(_client.PostAsJsonAsync("/api/core/productos", createRequest));
            
            // Leer productos
            tasks.Add(_client.GetAsync("/api/core/productos"));
            
            // Leer por categoría
            tasks.Add(_client.GetAsync($"/api/core/productos/categoria/{categoriaId}"));
        }

        // Act
        var startTime = DateTime.UtcNow;
        var responses = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.Created);
        var errorCount = responses.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
        
        // La mayoría de operaciones deberían ser exitosas
        successCount.Should().BeGreaterThan((int)(responses.Length * 0.7));
        
        // No debería haber errores 500
        errorCount.Should().Be(0);
        
        // El tiempo total debería ser razonable
        duration.TotalSeconds.Should().BeLessThan(60);
    }

    #endregion

}
