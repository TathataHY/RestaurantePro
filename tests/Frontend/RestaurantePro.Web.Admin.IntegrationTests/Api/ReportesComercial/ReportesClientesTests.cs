using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
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
        await SeedClientesDePruebaAsync(20, 15); // 20 total, 15 activos

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(5);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConClientesNuevos_DeberiaCalcularCorrectamente()
    {
        // Arrange
        await SeedClientesNuevosAsync(10, DateTime.Now.AddDays(-7)); // 10 clientes nuevos en la última semana

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.ClientesNuevos.Should().Be(10);
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
        await SeedClientesNuevosAsync(20, DateTime.Now.AddDays(-30)); // 20 nuevos en el último mes

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(80); // Solo activos por defecto
        responseData.Data.ClientesActivos.Should().Be(80);
        responseData.Data.ClientesNuevos.Should().Be(20);
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(0);
        responseData.Data.ClientesActivos.Should().Be(0);
        responseData.Data.ClientesNuevos.Should().Be(0);
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var reporte = responseData.Data;
        
        reporte.TotalClientes.Should().BeGreaterOrEqualTo(0);
        reporte.ClientesActivos.Should().BeGreaterOrEqualTo(0);
        reporte.ClientesNuevos.Should().BeGreaterOrEqualTo(0);
        reporte.PorcentajeClientesActivos.Should().BeGreaterOrEqualTo(0);
        reporte.PorcentajeClientesNuevos.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmentosDiferentes_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedClientesConSegmentoAsync(5, "Premium");
        await SeedClientesConSegmentoAsync(10, "Regular");
        await SeedClientesConSegmentoAsync(3, "VIP");

        // Act - Filtrar por Premium
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?segmento=Premium");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().Be(15); // Solo activos por defecto
    }

    #endregion

    #region Métodos de Ayuda

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

    private async Task SeedClientesConSegmentoAsync(int cantidad, string segmento)
    {
        for (int i = 0; i < cantidad; i++)
        {
            await CrearClienteConSegmentoAsync(segmento);
        }
    }

    private async Task SeedClientesConFechasRegistroAsync(int cantidad, DateTime fechaDesde, DateTime fechaHasta)
    {
        for (int i = 0; i < cantidad; i++)
        {
            var fechaRegistro = fechaDesde.AddDays(Random.Shared.Next((fechaHasta - fechaDesde).Days));
            await CrearClienteConFechaRegistroAsync(fechaRegistro);
        }
    }

    private async Task SeedClientesConFiltrosCombinadosAsync(int cantidad, string segmento, DateTime fechaDesde, DateTime fechaHasta)
    {
        for (int i = 0; i < cantidad; i++)
        {
            var fechaRegistro = fechaDesde.AddDays(Random.Shared.Next((fechaHasta - fechaDesde).Days));
            await CrearClienteConFiltrosCombinadosAsync(segmento, fechaRegistro, true);
        }
    }

    private async Task SeedClientesNuevosAsync(int cantidad, DateTime fechaRegistro)
    {
        for (int i = 0; i < cantidad; i++)
        {
            await CrearClienteConFechaRegistroAsync(fechaRegistro);
        }
    }

    private async Task CrearClienteDePruebaAsync(bool activo)
    {
        // Aquí se crearían los clientes en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearClienteConSegmentoAsync(string segmento)
    {
        // Aquí se crearían los clientes con segmento específico en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearClienteConFechaRegistroAsync(DateTime fechaRegistro)
    {
        // Aquí se crearían los clientes con fecha de registro específica en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    private async Task CrearClienteConFiltrosCombinadosAsync(string segmento, DateTime fechaRegistro, bool activo)
    {
        // Aquí se crearían los clientes con filtros combinados en la base de datos de prueba
        // Por ahora solo simulamos la creación
        await Task.Delay(1); // Simular operación asíncrona
    }

    #endregion
}
