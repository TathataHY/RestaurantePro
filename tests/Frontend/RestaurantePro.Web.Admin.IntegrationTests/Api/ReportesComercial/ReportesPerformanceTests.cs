using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.ReportesComercial;

/// <summary>
/// Pruebas de rendimiento para reportes comerciales
/// </summary>
public class ReportesPerformanceTests : BaseIntegrationTest
{
    public ReportesPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Rendimiento - Reporte de Ventas

    [Fact]
    public async Task ObtenerReporteVentas_Con1000Ventas_DeberiaResponderEnMenosDe3Segundos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(1000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000);
    }

    [Fact]
    public async Task ObtenerReporteVentas_Con10000Ventas_DeberiaResponderEnMenosDe5Segundos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10000);
        var fechaInicio = DateTime.Now.AddDays(-90);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConFiltrosComplejos_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(5000);
        var fechaInicio = DateTime.Now.AddDays(-60);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&segmento=Delivery&tipoVenta=Efectivo&incluirCanceladas=true");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoAnual_DeberiaResponderEnMenosDe8Segundos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(50000);
        var fechaInicio = DateTime.Now.AddDays(-365);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(8000);
    }

    #endregion

    #region Tests de Rendimiento - Reporte de Clientes

    [Fact]
    public async Task ObtenerReporteClientes_Con5000Clientes_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        await SeedClientesDePruebaAsync(5000, 4000);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task ObtenerReporteClientes_Con10000Clientes_DeberiaResponderEnMenosDe3Segundos()
    {
        // Arrange
        await SeedClientesDePruebaAsync(10000, 8000);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConFiltrosComplejos_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        await SeedClientesDePruebaAsync(8000, 6000);
        var fechaDesde = DateTime.Now.AddDays(-180);
        var fechaHasta = DateTime.Now.AddDays(-90);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?segmento=Premium&fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}&soloActivos=false");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    #endregion

    #region Tests de Rendimiento - Reporte de Productos

    [Fact]
    public async Task ObtenerReporteProductos_Con1000Productos_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        await SeedProductosDePruebaAsync(1000);
        await SeedVentasDePruebaAsync(5000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task ObtenerReporteProductos_ConTopProductosLimitado_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        await SeedProductosDePruebaAsync(500);
        await SeedVentasDePruebaAsync(2000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&topProductos=10");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task ObtenerReporteProductos_ConFiltroCategoria_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        await SeedProductosDePruebaAsync(200);
        await SeedVentasDePruebaAsync(1000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&categoria=Bebidas");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Tests de Rendimiento - Reporte de Fidelización

    [Fact]
    public async Task ObtenerReporteFidelizacion_Con5000Tarjetas_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        await SeedTarjetasFidelizacionDePruebaAsync(5000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task ObtenerReporteFidelizacion_ConFiltroSoloActivas_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        await SeedTarjetasFidelizacionDePruebaAsync(3000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&soloActivas=true");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Tests de Rendimiento - Reporte de Promociones

    [Fact]
    public async Task ObtenerReportePromociones_Con1000Promociones_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        await SeedPromocionesDePruebaAsync(1000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task ObtenerReportePromociones_ConFiltroSoloActivas_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        await SeedPromocionesDePruebaAsync(500);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&soloActivas=true");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Tests de Carga Concurrente

    [Fact]
    public async Task ObtenerReporteVentas_Con10RequestsConcurrentes_DeberiaResponderCorrectamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(1000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}"));
        }
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Todos en menos de 5 segundos
    }

    [Fact]
    public async Task ObtenerReporteClientes_Con5RequestsConcurrentes_DeberiaResponderCorrectamente()
    {
        // Arrange
        await SeedClientesDePruebaAsync(2000, 1500);
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.GetAsync("/api/comercial/reportes/clientes"));
        }
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Todos en menos de 3 segundos
    }

    [Fact]
    public async Task ObtenerReporteProductos_Con8RequestsConcurrentes_DeberiaResponderCorrectamente()
    {
        // Arrange
        await SeedProductosDePruebaAsync(500);
        await SeedVentasDePruebaAsync(2000);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 8; i++)
        {
            tasks.Add(_client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}"));
        }
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(4000); // Todos en menos de 4 segundos
    }

    #endregion

    #region Tests de Memoria

    [Fact]
    public async Task ObtenerReporteVentas_ConDatosGrandes_NoDeberiaConsumirMuchaMemoria()
    {
        // Arrange
        await SeedVentasDePruebaAsync(50000);
        var fechaInicio = DateTime.Now.AddDays(-365);
        var fechaFin = DateTime.Now;

        // Act
        var memoryBefore = GC.GetTotalMemory(true);
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        var memoryAfter = GC.GetTotalMemory(false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var memoryUsed = memoryAfter - memoryBefore;
        memoryUsed.Should().BeLessThan(50 * 1024 * 1024); // Menos de 50MB
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConDatosGrandes_NoDeberiaConsumirMuchaMemoria()
    {
        // Arrange
        await SeedClientesDePruebaAsync(20000, 15000);

        // Act
        var memoryBefore = GC.GetTotalMemory(true);
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");
        var memoryAfter = GC.GetTotalMemory(false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var memoryUsed = memoryAfter - memoryBefore;
        memoryUsed.Should().BeLessThan(30 * 1024 * 1024); // Menos de 30MB
    }

    #endregion

    #region Tests de Escalabilidad

    [Theory]
    [InlineData(100, 1000)]    // 100 ventas, 1 segundo
    [InlineData(1000, 3000)]   // 1000 ventas, 3 segundos
    [InlineData(5000, 5000)]   // 5000 ventas, 5 segundos
    public async Task ObtenerReporteVentas_ConDiferentesVolumenes_DeberiaEscalarCorrectamente(int cantidadVentas, int tiempoMaximoMs)
    {
        // Arrange
        await SeedVentasDePruebaAsync(cantidadVentas);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(tiempoMaximoMs);
    }

    [Theory]
    [InlineData(500, 1000)]    // 500 clientes, 1 segundo
    [InlineData(2000, 2000)]   // 2000 clientes, 2 segundos
    [InlineData(5000, 3000)]   // 5000 clientes, 3 segundos
    public async Task ObtenerReporteClientes_ConDiferentesVolumenes_DeberiaEscalarCorrectamente(int cantidadClientes, int tiempoMaximoMs)
    {
        // Arrange
        await SeedClientesDePruebaAsync(cantidadClientes, (int)(cantidadClientes * 0.8));

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(tiempoMaximoMs);
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedVentasDePruebaAsync(int cantidad)
    {
        // Crear productos y clientes primero
        await CrearProductosDePruebaAsync(Math.Max(1, cantidad / 100));
        await CrearClientesDePruebaAsync(Math.Max(1, cantidad / 50));
        
        // Crear facturas de prueba
        for (int i = 0; i < cantidad; i++)
        {
            await CrearFacturaDePruebaAsync();
        }
    }

    private async Task SeedClientesDePruebaAsync(int total, int activos)
    {
        // Crear clientes activos
        for (int i = 0; i < activos; i++)
        {
            await CrearClienteDePruebaAsync(true);
        }
        
        // Crear clientes inactivos
        for (int i = 0; i < total - activos; i++)
        {
            await CrearClienteDePruebaAsync(false);
        }
    }

    private async Task SeedProductosDePruebaAsync(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            await CrearProductoDePruebaAsync();
        }
    }

    private async Task SeedTarjetasFidelizacionDePruebaAsync(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            await CrearTarjetaFidelizacionDePruebaAsync();
        }
    }

    private async Task SeedPromocionesDePruebaAsync(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            await CrearPromocionDePruebaAsync();
        }
    }

    private async Task CrearFacturaDePruebaAsync()
    {
        // Aquí se crearían las facturas en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearClienteDePruebaAsync(bool activo)
    {
        // Aquí se crearían los clientes en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearProductoDePruebaAsync()
    {
        // Aquí se crearían los productos en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearTarjetaFidelizacionDePruebaAsync()
    {
        // Aquí se crearían las tarjetas de fidelización en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearPromocionDePruebaAsync()
    {
        // Aquí se crearían las promociones en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task<List<Guid>> CrearClientesDePruebaAsync(int cantidad)
    {
        var clienteIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var clienteId = Guid.NewGuid();
            clienteIds.Add(clienteId);
            
            // Aquí se crearían los clientes en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return clienteIds;
    }

    #endregion
}
