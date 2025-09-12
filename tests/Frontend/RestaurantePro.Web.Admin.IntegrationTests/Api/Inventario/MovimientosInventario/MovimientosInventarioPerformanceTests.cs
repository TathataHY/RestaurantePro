using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.MovimientosInventario;

/// <summary>
/// Tests de rendimiento para el módulo de Movimientos de Inventario
/// Valida tiempos de respuesta, carga y escalabilidad del sistema
/// </summary>
[Collection("IntegrationTests")]
public class MovimientosInventarioPerformanceTests : BaseIntegrationTest
{
    public MovimientosInventarioPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Tiempo de Respuesta

    [Fact]
    public async Task ObtenerMovimientos_DeberiaResponderEnMenosDe500ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos?pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "La consulta de movimientos debería responder en menos de 500ms");
    }

    [Fact]
    public async Task ObtenerMovimientoPorId_DeberiaResponderEnMenosDe300ms()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300, "La consulta de movimiento por ID debería responder en menos de 300ms");
    }

    [Fact]
    public async Task ObtenerMovimientosPorIngrediente_DeberiaResponderEnMenosDe400ms()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/ingrediente/{ingredienteId}?fechaDesde=2024-01-01&fechaHasta=2024-12-31");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(400, "La consulta de movimientos por ingrediente debería responder en menos de 400ms");
    }

    [Fact]
    public async Task ObtenerMovimientosPorTipo_DeberiaResponderEnMenosDe400ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos/tipo/Ingreso?pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(400, "La consulta de movimientos por tipo debería responder en menos de 400ms");
    }

    [Fact]
    public async Task GenerarReporte_DeberiaResponderEnMenosDe1000ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos/reporte?fechaInicio=2024-01-01&fechaFin=2024-12-31");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La generación de reporte debería completarse en menos de 1000ms");
    }

    #endregion

    #region Tests de Carga Concurrente

    [Fact]
    public async Task ObtenerMovimientos_ConCargaConcurrente_DeberiaMantenerRendimiento()
    {
        // Arrange
        const int numeroRequests = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < numeroRequests; i++)
        {
            tasks.Add(Client.GetAsync("/api/inventario/movimientos?pageNumber=1&pageSize=10"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "10 requests concurrentes deberían completarse en menos de 2 segundos");
    }

    [Fact]
    public async Task RegistrarMovimientos_ConCargaConcurrente_DeberiaMantenerRendimiento()
    {
        // Arrange
        const int numeroRequests = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < numeroRequests; i++)
        {
            var request = new RegistrarMovimientoRequest
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = 25,
                TipoMovimiento = TipoMovimientoInventario.Ingreso,
                Motivo = $"Test concurrente {i}",
                UsuarioId = Guid.NewGuid()
            };
            
            tasks.Add(Client.PostAsJsonAsync("/api/inventario/movimientos", request));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "5 registros concurrentes deberían completarse en menos de 3 segundos");
    }

    #endregion

    #region Tests de Escalabilidad con Paginación

    [Theory]
    [InlineData(1, 10)]    // Página pequeña
    [InlineData(1, 50)]    // Página mediana
    [InlineData(1, 100)]   // Página grande
    public async Task ObtenerMovimientos_ConDiferentesTamanosPagina_DeberiaEscalarCorrectamente(int pageNumber, int pageSize)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos?pageNumber={pageNumber}&pageSize={pageSize}");

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
    public async Task ObtenerMovimientos_ConFiltrosComplejos_DeberiaResponderEnTiempoRazonable()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos?pageNumber=1&pageSize=20&fechaInicio=2024-01-01&fechaFin=2024-12-31&tipoMovimiento=Ingreso&ingredienteId={ingredienteId}&usuarioId={usuarioId}&filtroMotivo=compra&ordenarPor=Fecha&direccionOrdenamiento=desc");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La consulta con filtros complejos debería responder en menos de 1000ms");
    }

    [Fact]
    public async Task GenerarReporte_ConParametrosComplejos_DeberiaResponderEnTiempoRazonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos/reporte?fechaInicio=2024-01-01&fechaFin=2024-12-31&incluirDetalle=true&agruparPorTipo=true&agruparPorIngrediente=true");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1500, "El reporte con parámetros complejos debería generarse en menos de 1500ms");
    }

    #endregion

    #region Tests de Memoria y Recursos

    [Fact]
    public async Task ObtenerMovimientos_ConPaginacionGrande_NoDeberiaConsumirExcesivaMemoria()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos?pageNumber=1&pageSize=100");

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
    public async Task ObtenerMovimientos_ConTimeoutLargo_DeberiaCompletarse()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos?pageNumber=1&pageSize=50", cts.Token);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "La operación debería completarse dentro del timeout de 30 segundos");
    }

    #endregion

    #region Tests de Rendimiento de Búsqueda

    [Theory]
    [InlineData("compra")]           // Búsqueda simple
    [InlineData("ingrediente")]      // Búsqueda común
    [InlineData("xyz123")]           // Búsqueda sin resultados
    [InlineData("a")]                // Búsqueda muy corta
    public async Task ObtenerMovimientos_ConFiltroMotivo_DeberiaResponderRapidamente(string filtroMotivo)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos?filtroMotivo={filtroMotivo}&pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(600, 
            $"La búsqueda con filtro '{filtroMotivo}' debería responder en menos de 600ms");
    }

    #endregion

    #region Tests de Rendimiento de Operaciones de Escritura

    [Fact]
    public async Task RegistrarMovimiento_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = TipoMovimientoInventario.Ingreso,
            Motivo = "Test performance",
            UsuarioId = Guid.NewGuid()
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800, "El registro de movimiento debería completarse en menos de 800ms");
    }

    [Fact]
    public async Task ActualizarMovimiento_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = 75,
            Motivo = "Actualización de prueba"
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(600, "La actualización de movimiento debería completarse en menos de 600ms");
    }

    [Fact]
    public async Task EliminarMovimiento_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.DeleteAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "La eliminación de movimiento debería completarse en menos de 500ms");
    }

    #endregion

    #region Tests de Rendimiento con Filtros Complejos

    [Fact]
    public async Task ObtenerMovimientos_ConFiltrosComplejos_DeberiaMantenerRendimiento()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos?pageNumber=1&pageSize=20&fechaInicio=2024-01-01&fechaFin=2024-12-31&tipoMovimiento=Ingreso&ordenarPor=Fecha&direccionOrdenamiento=desc");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800, "La consulta con filtros complejos debería responder en menos de 800ms");
    }

    #endregion

    #region Tests de Benchmarking

    [Fact]
    public async Task Benchmark_ObtenerMovimientos_MultiplesEjecuciones()
    {
        // Arrange
        const int numeroEjecuciones = 5;
        var tiempos = new List<long>();

        // Act
        for (int i = 0; i < numeroEjecuciones; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await Client.GetAsync("/api/inventario/movimientos?pageNumber=1&pageSize=10");
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

    #region Tests de Rendimiento de Consultas Específicas

    [Fact]
    public async Task ObtenerMovimientosPorIngrediente_ConFiltros_DeberiaResponderRapidamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/ingrediente/{ingredienteId}?fechaDesde=2024-01-01&fechaHasta=2024-12-31&limite=50");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(600, "La consulta de movimientos por ingrediente debería responder en menos de 600ms");
    }

    [Fact]
    public async Task ObtenerMovimientosPorTipo_ConFiltros_DeberiaResponderRapidamente()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/movimientos/tipo/Egreso?pageNumber=1&pageSize=20&fechaInicio=2024-01-01&fechaFin=2024-12-31");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(600, "La consulta de movimientos por tipo debería responder en menos de 600ms");
    }

    #endregion

    #region Tests de Rendimiento de Ordenamiento

    [Theory]
    [InlineData("Fecha", "desc")]
    [InlineData("Fecha", "asc")]
    [InlineData("Cantidad", "desc")]
    [InlineData("Cantidad", "asc")]
    public async Task ObtenerMovimientos_ConOrdenamiento_DeberiaResponderRapidamente(string ordenarPor, string direccion)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos?ordenarPor={ordenarPor}&direccionOrdenamiento={direccion}&pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(600, 
            $"La consulta con ordenamiento {ordenarPor} {direccion} debería responder en menos de 600ms");
    }

    #endregion
}

