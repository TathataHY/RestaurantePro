using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.Ingredientes;

/// <summary>
/// Tests de rendimiento para el módulo de Ingredientes
/// Valida tiempos de respuesta, carga y escalabilidad del sistema
/// </summary>
[Collection("IntegrationTests")]
public class IngredientesPerformanceTests : BaseIntegrationTest
{
    public IngredientesPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Tiempo de Respuesta

    [Fact]
    public async Task ObtenerIngredientes_DeberiaResponderEnMenosDe500ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "La consulta de ingredientes debería responder en menos de 500ms");
    }

    [Fact]
    public async Task ObtenerIngredientesLista_DeberiaResponderEnMenosDe300ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/lista?soloActivos=true");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300, "La consulta de lista de ingredientes debería responder en menos de 300ms");
    }

    [Fact]
    public async Task ObtenerEstadisticas_DeberiaResponderEnMenosDe1000ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/estadisticas");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Las estadísticas deberían calcularse en menos de 1000ms");
    }

    [Fact]
    public async Task ObtenerIngredientesBajoStock_DeberiaResponderEnMenosDe400ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/bajo-stock?porcentajeMinimo=20");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(400, "La consulta de ingredientes bajo stock debería responder en menos de 400ms");
    }

    [Fact]
    public async Task BuscarIngredientes_DeberiaResponderEnMenosDe600ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/buscar?termino=test&soloDisponibles=true");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(600, "La búsqueda de ingredientes debería responder en menos de 600ms");
    }

    #endregion

    #region Tests de Carga Concurrente

    [Fact]
    public async Task ObtenerIngredientes_ConCargaConcurrente_DeberiaMantenerRendimiento()
    {
        // Arrange
        const int numeroRequests = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < numeroRequests; i++)
        {
            tasks.Add(_client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=10"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "10 requests concurrentes deberían completarse en menos de 2 segundos");
    }

    [Fact]
    public async Task CrearIngredientes_ConCargaConcurrente_DeberiaMantenerRendimiento()
    {
        // Arrange
        const int numeroRequests = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < numeroRequests; i++)
        {
            var command = new CrearIngredienteCommand
            {
                Nombre = $"Ingrediente Performance Test {i}",
                Rotacion = RotacionIngrediente.Media,
                UnidadMedida = UnidadMedida.Kilogramo,
                StockMinimo = 1,
                StockMaximo = 10,
                PrecioUnitario = 5.00m
            };
            
            tasks.Add(_client.PostAsJsonAsync("/api/inventario/ingredientes", command));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "5 creaciones concurrentes deberían completarse en menos de 3 segundos");
    }

    #endregion

    #region Tests de Escalabilidad con Paginación

    [Theory]
    [InlineData(1, 10)]    // Página pequeña
    [InlineData(1, 50)]    // Página mediana
    [InlineData(1, 100)]   // Página grande
    public async Task ObtenerIngredientes_ConDiferentesTamanosPagina_DeberiaEscalarCorrectamente(int pageNumber, int pageSize)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // El tiempo debería escalar de manera razonable con el tamaño de página
        var tiempoMaximo = pageSize switch
        {
            10 => 500,
            50 => 800,
            100 => 1200,
            _ => 1500
        };
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(tiempoMaximo, 
            $"La consulta con pageSize={pageSize} debería responder en menos de {tiempoMaximo}ms");
    }

    #endregion

    #region Tests de Rendimiento de Operaciones Complejas

    [Fact]
    public async Task GenerarReporteValoracion_DeberiaResponderEnTiempoRazonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/reporte/valoracion?fechaDesde=2024-01-01&fechaHasta=2024-12-31");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "El reporte de valoración debería generarse en menos de 2 segundos");
    }

    [Fact]
    public async Task ObtenerMovimientosDeIngrediente_ConFiltros_DeberiaResponderRapidamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}/movimientos?fechaDesde=2024-01-01&fechaHasta=2024-12-31&limite=50");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800, "La consulta de movimientos debería responder en menos de 800ms");
    }

    #endregion

    #region Tests de Memoria y Recursos

    [Fact]
    public async Task ObtenerIngredientes_ConPaginacionGrande_NoDeberiaConsumirExcesivaMemoria()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Forzar garbage collection y verificar que no hay memory leak
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        
        // El aumento de memoria debería ser razonable (menos de 10MB)
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024, "No debería haber un aumento excesivo de memoria");
    }

    #endregion

    #region Tests de Timeout y Resilencia

    [Fact]
    public async Task ObtenerIngredientes_ConTimeoutLargo_DeberiaCompletarse()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=50", cts.Token);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "La operación debería completarse dentro del timeout de 30 segundos");
    }

    #endregion

    #region Tests de Rendimiento de Búsqueda

    [Theory]
    [InlineData("tomate")]           // Búsqueda simple
    [InlineData("ingrediente")]      // Búsqueda común
    [InlineData("xyz123")]           // Búsqueda sin resultados
    [InlineData("a")]                // Búsqueda muy corta
    public async Task BuscarIngredientes_ConDiferentesTerminos_DeberiaResponderRapidamente(string termino)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/buscar?termino={termino}");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            $"La búsqueda con término '{termino}' debería responder en menos de 1000ms");
    }

    #endregion

    #region Tests de Rendimiento de Operaciones de Escritura

    [Fact]
    public async Task CrearIngrediente_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Ingrediente Performance Test",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            PrecioUnitario = 5.00m
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La creación de ingrediente debería completarse en menos de 1000ms");
    }

    [Fact]
    public async Task ConsumirStock_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ConsumirStockCommand
        {
            Cantidad = 5,
            Motivo = "Test performance",
            Observaciones = "Consumo de prueba"
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/consumir", command);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800, "El consumo de stock debería completarse en menos de 800ms");
    }

    [Fact]
    public async Task RegistrarLote_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarLoteCommand
        {
            NumeroLote = "LOTE-PERF-001",
            Cantidad = 25,
            FechaVencimiento = DateTime.Now.AddDays(30),
            PrecioUnitario = 12.50m,
            Observaciones = "Lote de prueba performance"
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/lotes", command);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "El registro de lote debería completarse en menos de 1000ms");
    }

    #endregion

    #region Tests de Rendimiento con Filtros Complejos

    [Fact]
    public async Task ObtenerIngredientes_ConFiltrosComplejos_DeberiaMantenerRendimiento()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=20&soloActivos=true&categoria=Alta&ordenarPor=nombre&direccion=asc");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800, "La consulta con filtros complejos debería responder en menos de 800ms");
    }

    #endregion

    #region Tests de Benchmarking

    [Fact]
    public async Task Benchmark_ObtenerIngredientes_MultiplesEjecuciones()
    {
        // Arrange
        const int numeroEjecuciones = 5;
        var tiempos = new List<long>();

        // Act
        for (int i = 0; i < numeroEjecuciones; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=10");
            stopwatch.Stop();
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            tiempos.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        var tiempoPromedio = tiempos.Average();
        var tiempoMaximo = tiempos.Max();
        var tiempoMinimo = tiempos.Min();

        tiempoPromedio.Should().BeLessThan(500, "El tiempo promedio debería ser menor a 500ms");
        tiempoMaximo.Should().BeLessThan(1000, "El tiempo máximo debería ser menor a 1000ms");
        tiempoMinimo.Should().BeGreaterThan(0, "El tiempo mínimo debería ser mayor a 0ms");

        // La variación no debería ser excesiva (coeficiente de variación < 50%)
        var desviacionEstandar = Math.Sqrt(tiempos.Select(t => Math.Pow(t - tiempoPromedio, 2)).Average());
        var coeficienteVariacion = desviacionEstandar / tiempoPromedio;
        coeficienteVariacion.Should().BeLessThan(0.5, "La variación en los tiempos no debería ser excesiva");
    }

    #endregion
}
