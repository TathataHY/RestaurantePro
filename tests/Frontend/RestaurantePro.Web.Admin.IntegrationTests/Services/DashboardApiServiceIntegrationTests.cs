using FluentAssertions;
using System.Net;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Pruebas de integración para el servicio DashboardApiService
/// </summary>
public class DashboardApiServiceIntegrationTests : BaseIntegrationTest
{
    public DashboardApiServiceIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerResumen_DeberiaRetornarResumenCompleto()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/dashboard/resumen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardResumenDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Metricas.Should().NotBeNull();
        resultado.Data.ProductosMasVendidos.Should().NotBeNull();
        resultado.Data.EstadoMesas.Should().NotBeNull();
        resultado.Data.ComandasPorEstado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerMetricas_DeberiaRetornarMetricasValidas()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/dashboard/metricas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardMetricasDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.VentasHoy.Should().BeGreaterOrEqualTo(0);
        resultado.Data.TotalMesas.Should().BeGreaterOrEqualTo(0);
        resultado.Data.ComandasActivas.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ObtenerProductosMasVendidos_ConCantidadValida_DeberiaRetornarProductos()
    {
        // Arrange
        var cantidad = 5;

        // Act
        var response = await Client.GetAsync($"/api/admin/dashboard/productos-mas-vendidos?cantidad={cantidad}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductoMasVendidoDto>>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Count.Should().BeLessOrEqualTo(cantidad);
    }

    [Fact]
    public async Task ObtenerVentasUltimos7Dias_DeberiaRetornarVentasPorDia()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/dashboard/ventas-ultimos-7-dias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<List<VentaPorPeriodoDto>>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Count.Should().BeLessOrEqualTo(7);
        
        // Verificar que las fechas están en orden
        if (resultado.Data.Count > 1)
        {
            for (int i = 0; i < resultado.Data.Count - 1; i++)
            {
                resultado.Data[i].Fecha.Should().BeOnOrBefore(resultado.Data[i + 1].Fecha);
            }
        }
    }

    [Fact]
    public async Task ObtenerEstadoMesas_DeberiaRetornarEstadoValido()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/dashboard/estado-mesas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<EstadoMesasDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Total.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Disponibles.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Ocupadas.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Reservadas.Should().BeGreaterOrEqualTo(0);
        resultado.Data.EnLimpieza.Should().BeGreaterOrEqualTo(0);
        
        // Verificar que la suma de estados es igual al total
        var sumaEstados = resultado.Data.Disponibles + resultado.Data.Ocupadas + 
                         resultado.Data.Reservadas + resultado.Data.EnLimpieza;
        sumaEstados.Should().Be(resultado.Data.Total);
    }

    [Fact]
    public async Task ObtenerComandasPorEstado_DeberiaRetornarComandasValidas()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/dashboard/comandas-por-estado");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<ComandasPorEstadoDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Total.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Pendientes.Should().BeGreaterOrEqualTo(0);
        resultado.Data.EnPreparacion.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Listas.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Completadas.Should().BeGreaterOrEqualTo(0);
        resultado.Data.Canceladas.Should().BeGreaterOrEqualTo(0);
        
        // Verificar que la suma de estados es igual al total
        var sumaEstados = resultado.Data.Pendientes + resultado.Data.EnPreparacion + 
                         resultado.Data.Listas + resultado.Data.Completadas + resultado.Data.Canceladas;
        sumaEstados.Should().Be(resultado.Data.Total);
    }

    [Fact]
    public async Task ObtenerIngresosPorHora_DeberiaRetornarIngresosValidos()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/dashboard/ingresos-por-hora");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<List<IngresosPorHoraDto>>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        
        // Verificar que las horas están en el rango correcto (12-22)
        foreach (var ingreso in resultado.Data!)
        {
            ingreso.Hora.Should().BeInRange(12, 22);
            ingreso.Monto.Should().BeGreaterOrEqualTo(0);
            ingreso.CantidadComandas.Should().BeGreaterOrEqualTo(0);
        }
    }
}
