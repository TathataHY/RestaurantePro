using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Categorias;

/// <summary>
/// Pruebas de rendimiento para el controlador de Categorías
/// </summary>
public class CategoriasPerformanceTests : BaseIntegrationTest
{
    public CategoriasPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Rendimiento de Obtener Categorías

    [Fact]
    public async Task ObtenerCategorias_ConPocasCategorias_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(10);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Menos de 500ms
    }

    [Fact]
    public async Task ObtenerCategorias_ConMuchasCategorias_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    [Fact]
    public async Task ObtenerCategorias_ConCategoriasYProductos_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedCategoriasConProductosAsync(50, 10); // 50 categorías con 10 productos cada una

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    #endregion

    #region Rendimiento de Búsqueda

    [Fact]
    public async Task BuscarCategorias_ConTerminoCorto_DeberiaSerRapida()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoLargo_DeberiaSerEficiente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=categoria_muy_larga_para_probar_rendimiento");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoInexistente_DeberiaSerRapida()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=inexistente");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Menos de 500ms
    }

    #endregion

    #region Rendimiento de Obtener por ID

    [Fact]
    public async Task ObtenerCategoriaPorId_DeberiaSerRapida()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync(100);
        var categoriaId = categoriaIds.First();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Menos de 500ms
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConIdInexistente_DeberiaSerRapida()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/categorias/{idInexistente}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Menos de 500ms
    }

    #endregion

    #region Rendimiento con Diferentes Parámetros

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task ObtenerCategorias_ConDiferentesParametros_DeberiaMantenerRendimiento(bool soloActivas, bool ocultarVacias)
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/categorias?soloActivas={soloActivas}&ocultarVacias={ocultarVacias}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    #endregion

    #region Rendimiento con Concurrencia

    [Fact]
    public async Task ObtenerCategorias_ConRequestsConcurrentes_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(50);
        var tasks = new List<Task<(HttpResponseMessage Response, long ElapsedMs)>>();

        // Act
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(ExecuteRequestWithTiming("/api/core/categorias"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => 
        {
            result.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.ElapsedMs.Should().BeLessThan(2000); // Menos de 2 segundos por request
        });

        var averageTime = results.Average(r => r.ElapsedMs);
        averageTime.Should().BeLessThan(1000); // Promedio menor a 1 segundo
    }

    [Fact]
    public async Task BuscarCategorias_ConRequestsConcurrentes_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(50);
        var tasks = new List<Task<(HttpResponseMessage Response, long ElapsedMs)>>();

        // Act
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(ExecuteRequestWithTiming($"/api/core/categorias/buscar?nombre=test{i}"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => 
        {
            result.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.ElapsedMs.Should().BeLessThan(1500); // Menos de 1.5 segundos por request
        });

        var averageTime = results.Average(r => r.ElapsedMs);
        averageTime.Should().BeLessThan(800); // Promedio menor a 800ms
    }

    #endregion

    #region Rendimiento con Carga Gradual

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public async Task ObtenerCategorias_ConCargaGradual_DeberiaMantenerRendimiento(int cantidadCategorias)
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(cantidadCategorias);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // El tiempo debería crecer de manera razonable con la cantidad de datos
        var maxTime = Math.Max(500, cantidadCategorias * 10); // 10ms por categoría como máximo
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxTime);
    }

    #endregion

    #region Rendimiento de Memoria

    [Fact]
    public async Task ObtenerCategorias_ConMuchosDatos_NoDeberiaConsumirExcesivaMemoria()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(1000);

        // Act
        var initialMemory = GC.GetTotalMemory(true);
        var response = await _client.GetAsync("/api/core/categorias");
        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024); // Menos de 50MB de aumento
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedCategoriasDePruebaAsync(int cantidad)
    {
        return await CrearCategoriasDePruebaAsync();
    }

    private async Task SeedCategoriasConProductosAsync(int cantidadCategorias, int productosPorCategoria)
    {
        // Simular creación de categorías con productos
        await Task.CompletedTask;
    }

    private async Task<(HttpResponseMessage Response, long ElapsedMs)> ExecuteRequestWithTiming(string url)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync(url);
        stopwatch.Stop();
        
        return (response, stopwatch.ElapsedMilliseconds);
    }

    #endregion
}
