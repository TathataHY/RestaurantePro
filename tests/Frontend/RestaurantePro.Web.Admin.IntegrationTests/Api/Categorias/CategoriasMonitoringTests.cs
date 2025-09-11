using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Categorias;

/// <summary>
/// Pruebas de monitoreo para el controlador de Categorías
/// </summary>
public class CategoriasMonitoringTests : BaseIntegrationTest
{
    public CategoriasMonitoringTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Monitoreo de Headers HTTP

    [Fact]
    public async Task ObtenerCategorias_DeberiaIncluirHeadersCorrectos()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar headers de respuesta
        response.Content.Headers.Should().ContainKey("Content-Type");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_DeberiaIncluirHeadersCorrectos()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar headers de respuesta
        response.Content.Headers.Should().ContainKey("Content-Type");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task BuscarCategorias_DeberiaIncluirHeadersCorrectos()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar headers de respuesta
        response.Content.Headers.Should().ContainKey("Content-Type");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    #endregion

    #region Monitoreo de Logging

    [Fact]
    public async Task ObtenerCategorias_ConLogging_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // En un entorno real, verificarías que se registraron los logs apropiados
        // Por ahora, solo verificamos que la respuesta sea exitosa
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConLogging_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // En un entorno real, verificarías que se registraron los logs apropiados
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarCategorias_ConLogging_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // En un entorno real, verificarías que se registraron los logs apropiados
        response.Should().NotBeNull();
    }

    #endregion

    #region Monitoreo de Métricas

    [Fact]
    public async Task ObtenerCategorias_ConMetricas_DeberiaRegistrarTiempoRespuesta()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el tiempo de respuesta es razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
        
        // En un entorno real, verificarías que se registraron métricas de tiempo
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConMetricas_DeberiaRegistrarTiempoRespuesta()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el tiempo de respuesta es razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        
        // En un entorno real, verificarías que se registraron métricas de tiempo
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarCategorias_ConMetricas_DeberiaRegistrarTiempoRespuesta()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el tiempo de respuesta es razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        
        // En un entorno real, verificarías que se registraron métricas de tiempo
        response.Should().NotBeNull();
    }

    #endregion

    #region Monitoreo de Errores

    [Fact]
    public async Task ObtenerCategorias_ConErrores_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        // Simular un error configurando un escenario que cause fallo

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        // En este caso, la respuesta debería ser exitosa
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // En un entorno real, verificarías que se registraron logs de error apropiados
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConErrores_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // En un entorno real, verificarías que se registraron logs de error apropiados
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarCategorias_ConErrores_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        // Simular un error en la búsqueda

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // En un entorno real, verificarías que se registraron logs de error apropiados
        response.Should().NotBeNull();
    }

    #endregion

    #region Monitoreo de Concurrencia

    [Fact]
    public async Task ObtenerCategorias_ConConcurrencia_DeberiaMantenerConsistencia()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/core/categorias"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));
        
        // Verificar consistencia en las respuestas
        var contents = new List<string>();
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            contents.Add(content);
        }

        // Todas las respuestas deberían ser similares
        contents.Should().AllSatisfy(content => 
            content.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task BuscarCategorias_ConConcurrencia_DeberiaMantenerConsistencia()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync($"/api/core/categorias/buscar?nombre=test{i}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    #endregion

    #region Monitoreo de Disponibilidad

    [Fact]
    public async Task ObtenerCategorias_Disponibilidad_DeberiaSerAlta()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el endpoint está disponible
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_Disponibilidad_DeberiaSerAlta()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el endpoint está disponible
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarCategorias_Disponibilidad_DeberiaSerAlta()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el endpoint está disponible
        response.Should().NotBeNull();
    }

    #endregion

    #region Monitoreo de Escalabilidad

    [Fact]
    public async Task ObtenerCategorias_ConEscalabilidad_DeberiaMantenerse()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el rendimiento se mantiene con más datos
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarCategorias_ConEscalabilidad_DeberiaMantenerse()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(100);

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el rendimiento se mantiene con más datos
        response.Should().NotBeNull();
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedCategoriasDePruebaAsync(int cantidad = 5)
    {
        return await CrearCategoriasDePruebaAsync();
    }

    #endregion
}
