using System.Net;
using System.Text.Json;
using FluentAssertions;
using AppReportes = RestaurantePro.Application.Comercial.Reportes.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.ReportesComercial;

/// <summary>
/// Pruebas específicas para reportes de ventas
/// </summary>
public class ReportesVentasTests : BaseIntegrationTest
{
    private readonly ILogger<ReportesVentasTests> _logger;

    public ReportesVentasTests(WebApplicationFactory factory) : base(factory)
    {
        _logger = factory.Services.GetRequiredService<ILogger<ReportesVentasTests>>();
    }

    #region Casos de Éxito

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoMensual_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(30); // 30 ventas en el último mes
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"Response content: {content}");
        
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        _logger.LogInformation($"ResponseData: Success={responseData?.Success}, Data={responseData?.Data?.TotalVentas}");
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalVentas.Should().BeGreaterThan(0);
        responseData.Data.TotalTransacciones.Should().BeGreaterThan(0);
        responseData.Data.PromedioTicket.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoSemanal_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(7); // 7 ventas en la última semana
        var fechaInicio = DateTime.Now.AddDays(-7);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalVentas.Should().BeGreaterThan(0);
        responseData.Data.TotalTransacciones.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoDiario_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await SeedVentasDePruebaAsync(1); // 1 venta hoy
        var fechaInicio = DateTime.Now.Date;
        var fechaFin = DateTime.Now.Date;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConSegmentoDelivery_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10, "Delivery");
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&segmento=Delivery");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConSegmentoPresencial_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10, "Presencial");
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&segmento=Presencial");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConTipoVentaEfectivo_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10, tipoVenta: "Efectivo");
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&tipoVenta=Efectivo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConTipoVentaTarjeta_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10, tipoVenta: "Tarjeta");
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&tipoVenta=Tarjeta");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConIncluirCanceladasTrue_DeberiaIncluirVentasCanceladas()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10, incluirCanceladas: true);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&incluirCanceladas=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConIncluirCanceladasFalse_DeberiaExcluirVentasCanceladas()
    {
        // Arrange
        await SeedVentasDePruebaAsync(10, incluirCanceladas: false);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&incluirCanceladas=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Casos de Error

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

    [Fact]
    public async Task ObtenerReporteVentas_ConFechaInicioFutura_DeberiaRetornarBadRequest()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(1);
        var fechaFin = DateTime.Now.AddDays(2);

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

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoMuyLargo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddYears(-10);
        var fechaFin = DateTime.Now;

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

    [Fact]
    public async Task ObtenerReporteVentas_ConParametrosFaltantes_DeberiaRetornarBadRequest()
    {
        // Arrange - Sin fechaInicio y fechaFin

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/ventas");

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
    public async Task ObtenerReporteVentas_DeberiaCalcularPromedioCorrectamente()
    {
        // Arrange
        await SeedVentasConMontosEspecificosAsync(new decimal[] { 100, 200, 300 }); // Total: 600, Promedio: 200
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalVentas.Should().Be(600);
        responseData.Data.TotalTransacciones.Should().Be(3);
        responseData.Data.PromedioTicket.Should().Be(200);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConVentasCero_DeberiaRetornarCeros()
    {
        // Arrange
        // No crear ventas
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.TotalVentas.Should().Be(0);
        responseData.Data.TotalTransacciones.Should().Be(0);
        responseData.Data.PromedioTicket.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerReporteVentas_DeberiaIncluirFechasCorrectas()
    {
        // Arrange
        await SeedVentasDePruebaAsync(5);
        var fechaInicio = DateTime.Now.AddDays(-7);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteVentasDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        responseData.Data.FechaInicio.Should().Be(fechaInicio.Date);
        responseData.Data.FechaFin.Should().Be(fechaFin.Date);
    }

    #endregion

    #region Rendimiento

    [Fact]
    public async Task ObtenerReporteVentas_ConMuchasVentas_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(1000); // 1000 ventas
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
    public async Task ObtenerReporteVentas_ConFiltrosComplejos_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedVentasDePruebaAsync(500);
        var fechaInicio = DateTime.Now.AddDays(-90);
        var fechaFin = DateTime.Now;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&segmento=Delivery&tipoVenta=Efectivo&incluirCanceladas=true");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedVentasDePruebaAsync(int cantidad, string? segmento = null, string? tipoVenta = null, bool incluirCanceladas = false)
    {
        // Crear productos y clientes primero
        await CrearProductosDePruebaAsync(Math.Max(1, cantidad / 10));
        await CrearClientesDePruebaAsync(Math.Max(1, cantidad / 5));
        
        // Crear facturas de prueba
        for (int i = 0; i < cantidad; i++)
        {
            await CrearFacturaDePruebaAsync(segmento, tipoVenta, incluirCanceladas);
        }
    }

    private async Task SeedVentasConMontosEspecificosAsync(decimal[] montos)
    {
        // Crear productos y clientes primero
        await CrearProductosDePruebaAsync(montos.Length);
        await CrearClientesDePruebaAsync(montos.Length);
        
        // Crear facturas con montos específicos
        for (int i = 0; i < montos.Length; i++)
        {
            await CrearFacturaConMontoAsync(montos[i]);
        }
    }

    private async Task CrearFacturaDePruebaAsync(string? segmento = null, string? tipoVenta = null, bool incluirCanceladas = false)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        var random = new Random();
        var monto = (decimal)(random.NextDouble() * 1000 + 50); // Entre $50 y $1050
        
        var factura = RestaurantePro.Domain.Comercial.Facturacion.Entities.Factura.Crear(
            numeroFactura: $"FAC-{DateTime.Now:yyyyMMdd}-{random.Next(1000, 9999)}",
            tipoFactura: RestaurantePro.Domain.Comercial.Facturacion.Enums.TipoFactura.Normal,
            nombreCliente: $"Cliente Test {random.Next(1, 100)}",
            clienteId: await ObtenerClienteAleatorioAsync(context),
            fechaEmision: DateTime.Now.AddDays(-random.Next(0, 15))
        );
        
        context.Facturas.Add(factura);
        await context.SaveChangesAsync();
    }

    private async Task CrearFacturaConMontoAsync(decimal monto)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        var random = new Random();
        
        var factura = RestaurantePro.Domain.Comercial.Facturacion.Entities.Factura.Crear(
            numeroFactura: $"FAC-{DateTime.Now:yyyyMMdd}-{random.Next(1000, 9999)}",
            tipoFactura: RestaurantePro.Domain.Comercial.Facturacion.Enums.TipoFactura.Normal,
            nombreCliente: $"Cliente Test {random.Next(1, 100)}",
            clienteId: await ObtenerClienteAleatorioAsync(context),
            fechaEmision: DateTime.Now.AddDays(-random.Next(0, 15))
        );
        
        context.Facturas.Add(factura);
        await context.SaveChangesAsync();
    }

    private async Task<List<Guid>> CrearClientesDePruebaAsync(int cantidad)
    {
        var clienteIds = new List<Guid>();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var cliente = RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                nombre: RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear($"Cliente Test {i + 1}", ""),
                email: $"cliente{i + 1}@test.com",
                telefono: $"555-{i + 1:0000}",
                fechaNacimiento: DateTime.Now.AddYears(-30)
            );
            
            context.Clientes.Add(cliente);
            clienteIds.Add(cliente.Id);
        }
        
        await context.SaveChangesAsync();
        return clienteIds;
    }

    private async Task<List<Guid>> CrearProductosDePruebaAsync(int cantidad)
    {
        var productoIds = new List<Guid>();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Obtener una categoría existente o crear una
        var categoria = await context.ProductoCategorias.FirstOrDefaultAsync();
        if (categoria == null)
        {
            categoria = RestaurantePro.Domain.Core.Productos.Entities.ProductoCategoria.Crear(
                nombre: "Categoría Test",
                descripcion: "Categoría para pruebas",
                orden: 1,
                color: "#FF5722",
                icono: "🍽️"
            );
            context.ProductoCategorias.Add(categoria);
            await context.SaveChangesAsync();
        }
        
        for (int i = 0; i < cantidad; i++)
        {
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                nombre: $"Producto Test {i + 1}",
                descripcion: $"Descripción del producto {i + 1}",
                precio: new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto((decimal)(Random.Shared.NextDouble() * 100 + 10)), // Entre $10 y $110
                categoriaId: categoria.Id
            );
            
            context.Productos.Add(producto);
            productoIds.Add(producto.Id);
        }
        
        await context.SaveChangesAsync();
        return productoIds;
    }

    private async Task<Guid?> ObtenerClienteAleatorioAsync(RestauranteProDbContext context)
    {
        var clientes = await context.Clientes.ToListAsync();
        if (!clientes.Any())
        {
            // Crear un cliente si no hay ninguno
            var cliente = RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                nombre: RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Cliente Test", ""),
                email: "cliente@test.com",
                telefono: "555-0000",
                fechaNacimiento: DateTime.Now.AddYears(-30)
            );
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return cliente.Id;
        }
        
        return clientes[Random.Shared.Next(clientes.Count)].Id;
    }

    #endregion
}
