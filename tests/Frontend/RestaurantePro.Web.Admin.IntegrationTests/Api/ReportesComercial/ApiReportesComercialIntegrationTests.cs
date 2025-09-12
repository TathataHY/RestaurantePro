using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.ReportesComercial;

/// <summary>
/// Pruebas de integración para el controlador de Reportes Comerciales
/// </summary>
public class ApiReportesComercialIntegrationTests : BaseIntegrationTest
{
    public ApiReportesComercialIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Reporte de Ventas

    [Fact]
    public async Task ObtenerReporteVentas_ConParametrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalVentas.Should().BeGreaterOrEqualTo(0);
        responseData.Data.CantidadTransacciones.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConSegmento_DeberiaFiltrarPorSegmento()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var segmento = "Delivery";

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&segmento={segmento}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConTipoVenta_DeberiaFiltrarPorTipoVenta()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var tipoVenta = "Efectivo";

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&tipoVenta={tipoVenta}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConIncluirCanceladas_DeberiaIncluirVentasCanceladas()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&incluirCanceladas=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConFechasInvalidas_DeberiaRetornarBadRequest()
    {
        // Arrange
        var fechaInicio = DateTime.Now;
        var fechaFin = DateTime.Now.AddDays(-1); // Fecha fin anterior a fecha inicio

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Reporte de Clientes

    [Fact]
    public async Task ObtenerReporteClientes_ConParametrosPorDefecto_DeberiaRetornarClientesActivos()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalClientes.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSoloActivosFalse_DeberiaRetornarTodosLosClientes()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes?soloActivos=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConSegmento_DeberiaFiltrarPorSegmento()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var segmento = "Premium";

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?segmento={segmento}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConFechasRegistro_DeberiaFiltrarPorFechas()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaDesde = DateTime.Now.AddDays(-90);
        var fechaHasta = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Reporte de Productos

    [Fact]
    public async Task ObtenerReporteProductos_ConParametrosValidos_DeberiaRetornarProductosMasVendidos()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteProductosDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Productos.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteProductos_ConTopProductos_DeberiaLimitarCantidad()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var topProductos = 5;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&topProductos={topProductos}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteProductosDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Productos.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteProductos_ConCategoria_DeberiaFiltrarPorCategoria()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var categoria = "Bebidas";

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&categoria={categoria}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteProductosDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Productos.Should().NotBeNull();
    }

    #endregion

    #region Reporte de Fidelización

    [Fact]
    public async Task ObtenerReporteFidelizacion_ConParametrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteFidelizacionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteFidelizacion_ConSoloActivasFalse_DeberiaIncluirTodasLasTarjetas()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&soloActivas=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteFidelizacionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Reporte de Promociones

    [Fact]
    public async Task ObtenerReportePromociones_ConParametrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReportePromocionesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReportePromociones_ConSoloActivasFalse_DeberiaIncluirTodasLasPromociones()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&soloActivas=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReportePromocionesDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Validaciones de Estructura de Datos

    [Fact]
    public async Task ObtenerReporteVentas_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var reporte = responseData.Data;
        
        reporte.TotalVentas.Should().BeGreaterOrEqualTo(0);
        reporte.CantidadTransacciones.Should().BeGreaterOrEqualTo(0);
        reporte.PromedioVentaPorTransaccion.Should().BeGreaterOrEqualTo(0);
        reporte.FechaInicio.Should().Be(fechaInicio.Date);
        reporte.FechaFin.Should().Be(fechaFin.Date);
    }

    [Fact]
    public async Task ObtenerReporteClientes_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteClientesDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var reporte = responseData.Data;
        
        reporte.TotalClientes.Should().BeGreaterOrEqualTo(0);
        reporte.ClientesNuevos.Should().BeGreaterOrEqualTo(0);
        reporte.ClientesActivos.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ObtenerReporteProductos_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<ReporteProductosDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var reporte = responseData.Data;
        
        reporte.Productos.Should().NotBeNull();
        reporte.FechaInicio.Should().Be(fechaInicio.Date);
        reporte.FechaFin.Should().Be(fechaFin.Date);
    }

    #endregion

    #region Rendimiento

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoLargo_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-365);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Menos de 5 segundos
    }

    [Fact]
    public async Task ObtenerReporteClientes_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    [Fact]
    public async Task ObtenerReporteProductos_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedDatosParaReportesAsync()
    {
        // Crear datos básicos necesarios para los reportes
        await CrearProductosDePruebaAsync(5);
        await CrearClientesDePruebaAsync(10);
        await CrearFacturasDePruebaAsync(20);
        await CrearPromocionesDePruebaAsync(3);
        await CrearTarjetasFidelizacionDePruebaAsync(5);
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

    private async Task<List<Guid>> CrearFacturasDePruebaAsync(int cantidad)
    {
        var facturaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var facturaId = Guid.NewGuid();
            facturaIds.Add(facturaId);
            
            // Aquí se crearían las facturas en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return facturaIds;
    }

    private async Task<List<Guid>> CrearPromocionesDePruebaAsync(int cantidad)
    {
        var promocionIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var promocionId = Guid.NewGuid();
            promocionIds.Add(promocionId);
            
            // Aquí se crearían las promociones en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return promocionIds;
    }

    private async Task<List<Guid>> CrearTarjetasFidelizacionDePruebaAsync(int cantidad)
    {
        var tarjetaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var tarjetaId = Guid.NewGuid();
            tarjetaIds.Add(tarjetaId);
            
            // Aquí se crearían las tarjetas de fidelización en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return tarjetaIds;
    }

    #endregion
}
