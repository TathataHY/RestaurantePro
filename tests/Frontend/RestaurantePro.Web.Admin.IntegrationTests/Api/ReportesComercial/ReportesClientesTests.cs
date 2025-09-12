using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using AppReportes = RestaurantePro.Application.Comercial.Reportes.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.ReportesComercial;

/// <summary>
/// Pruebas específicas para reportes de clientes
/// </summary>
public class ReportesClientesTests : BaseIntegrationTest
{
    public ReportesClientesTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Casos de Éxito

    [Fact]
    public async Task ObtenerReporteClientes_ConParametrosPorDefecto_DeberiaRetornarSoloActivos()
    {
        // Arrange
        Console.WriteLine("=== Iniciando test ObtenerReporteClientes_ConParametrosPorDefecto_DeberiaRetornarSoloActivos ===");
        await SeedClientesDePruebaAsync(20, 15); // 20 total, 15 activos
        
        // Verificar que los clientes se crearon
        var clientesEnBD = await _context.Clientes.CountAsync();
        Console.WriteLine($"Clientes en BD después del seed: {clientesEnBD}");

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(15); // Solo activos
        responseData.Data.ClientesActivos.Should().Be(15);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSoloActivosFalse_DeberiaRetornarTodosLosClientes()
    {
        // Arrange
        await SeedClientesDePruebaAsync(20, 15); // 20 total, 15 activos

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?soloActivos=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(20); // Todos
        responseData.Data.ClientesActivos.Should().Be(15);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmentoPremium_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedClientesConSegmentoAsync(10, "Premium");

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?segmento=Premium");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(10);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmentoRegular_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedClientesConSegmentoAsync(15, "Regular");

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?segmento=Regular");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(15);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConFechasRegistro_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var fechaDesde = DateTime.Now.AddDays(-30);
        var fechaHasta = DateTime.Now.AddDays(-15);
        await SeedClientesConFechasRegistroAsync(8, fechaDesde, fechaHasta);

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(8);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConFiltrosCombinados_DeberiaAplicarTodosLosFiltros()
    {
        // Arrange
        var fechaDesde = DateTime.Now.AddDays(-60);
        var fechaHasta = DateTime.Now.AddDays(-30);
        await SeedClientesConFiltrosCombinadosAsync(5, "Premium", fechaDesde, fechaHasta);

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?segmento=Premium&fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}&soloActivos=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(5);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConClientesActivos_DeberiaCalcularCorrectamente()
    {
        // Arrange
        await SeedClientesActivosAsync(10); // 10 clientes activos

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.ClientesActivos.Should().Be(10);
    }

    #endregion

    #region Casos de Error

    [Fact]
    public async Task ObtenerReporteClientes_ConFechaRegistroDesdeFutura_DeberiaRetornarBadRequest()
    {
        // Arrange
        var fechaDesde = DateTime.Now.AddDays(1);
        var fechaHasta = DateTime.Now.AddDays(2);

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConFechasInvalidas_DeberiaRetornarBadRequest()
    {
        // Arrange
        var fechaDesde = DateTime.Now;
        var fechaHasta = DateTime.Now.AddDays(-1); // Fecha hasta anterior a fecha desde

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmentoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var segmentoInvalido = "SegmentoInexistente";

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?segmento={segmentoInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Validaciones de Datos

    [Fact]
    public async Task ObtenerReporteClientes_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await SeedClientesDePruebaAsync(100, 80); // 100 total, 80 activos
        await SeedClientesActivosAsync(20); // 20 clientes activos

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(80); // Solo activos por defecto
        responseData.Data.ClientesActivos.Should().Be(80);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConClientesCero_DeberiaRetornarCeros()
    {
        // Arrange
        // No crear clientes

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(0);
        responseData.Data.ClientesActivos.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerReporteClientes_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        await SeedClientesDePruebaAsync(10, 8);

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var reporte = responseData.Data;
        
        reporte.TotalClientes.Should().BeGreaterOrEqualTo(0);
        reporte.ClientesActivos.Should().BeGreaterOrEqualTo(0);
        reporte.PorcentajeActivos.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmentosDiferentes_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedClientesConSegmentoAsync(5, "Premium");
        await SeedClientesConSegmentoAsync(10, "Regular");
        await SeedClientesConSegmentoAsync(3, "Creciente");

        // Act - Filtrar por Premium
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?segmento=Premium");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(5);
    }

    #endregion

    #region Rendimiento

    [Fact]
    public async Task ObtenerReporteClientes_ConMuchosClientes_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedClientesDePruebaAsync(10000, 8000); // 10,000 total, 8,000 activos

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConFiltrosComplejos_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedClientesDePruebaAsync(5000, 4000);
        var fechaDesde = DateTime.Now.AddDays(-180);
        var fechaHasta = DateTime.Now.AddDays(-90);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?segmento=Premium&fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}&soloActivos=false");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConRangoLargo_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedClientesDePruebaAsync(2000, 1500);
        var fechaDesde = DateTime.Now.AddDays(-365);
        var fechaHasta = DateTime.Now;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    #endregion

    #region Casos Especiales

    [Fact]
    public async Task ObtenerReporteClientes_ConTodosLosClientesInactivos_DeberiaRetornarCeros()
    {
        // Arrange
        await SeedClientesDePruebaAsync(10, 0); // 10 total, 0 activos

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(0);
        responseData.Data.ClientesActivos.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConTodosLosClientesActivos_DeberiaRetornarTodos()
    {
        // Arrange
        await SeedClientesDePruebaAsync(15, 15); // 15 total, 15 activos

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(15);
        responseData.Data.ClientesActivos.Should().Be(15);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmentoVacio_DeberiaRetornarTodosLosClientes()
    {
        // Arrange
        await SeedClientesDePruebaAsync(20, 15);

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?segmento=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(15); // Solo activos por defecto
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedClientesDePruebaAsync(int total, int activos)
    {
        await CrearClientesDePruebaAsync(total, activos);
    }

    private async Task SeedClientesConSegmentoAsync(int cantidad, string segmento)
    {
        await CrearClientesConSegmentoAsync(cantidad, segmento);
    }

    private async Task SeedClientesConFechasRegistroAsync(int cantidad, DateTime fechaDesde, DateTime fechaHasta)
    {
        await CrearClientesConFechasRegistroAsync(cantidad, fechaDesde, fechaHasta);
    }

    private async Task SeedClientesConFiltrosCombinadosAsync(int cantidad, string segmento, DateTime fechaDesde, DateTime fechaHasta)
    {
        await CrearClientesConFiltrosCombinadosAsync(cantidad, segmento, fechaDesde, fechaHasta);
    }

    private async Task SeedClientesActivosAsync(int cantidad)
    {
        await CrearClientesActivosAsync(cantidad);
    }

    private async Task SeedClientesNuevosAsync(int cantidad, DateTime fechaRegistro)
    {
        await CrearClientesConFechasRegistroAsync(cantidad, fechaRegistro, fechaRegistro);
    }

    #endregion
}
