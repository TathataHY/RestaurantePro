using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para ReportesComercialController
/// </summary>
[Collection("Sequential")]
public class ReportesComercialControllerTests : ApiIntegrationTestBase
{
    public ReportesComercialControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConDatosExistentes_RetornaReporteCompleto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos de prueba con sufijos únicos
        var cliente = CrearClienteTest("ventas");
        var producto = CrearProductoTest("ventas");
        var factura = CrearFacturaTest(cliente.Id, producto.Id, "ventas");
        
        context.Clientes.Add(cliente);
        context.Productos.Add(producto);
        context.Facturas.Add(factura);
        await context.SaveChangesAsync();

        // Act - Usar fechas que incluyan los datos de prueba
        var fechaInicio = DateTime.Today.AddDays(-30);
        var fechaFin = DateTime.Today.AddDays(1);
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReporteVentasDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalVentas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConDatosExistentes_RetornaReporteCompleto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos de prueba con sufijos únicos
        var cliente1 = CrearClienteTest("clientes1");
        var cliente2 = CrearClienteTest("clientes2");
        var tarjeta = CrearTarjetaFidelizacionTest(cliente1.Id, "clientes");
        
        context.Clientes.AddRange(cliente1, cliente2);
        context.TarjetasFidelizacion.Add(tarjeta);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReporteClientesDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalClientes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteProductos_ConDatosExistentes_RetornaReporteCompleto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos de prueba con sufijos únicos
        var producto1 = CrearProductoTest("productos1");
        var producto2 = CrearProductoTest("productos2");
        
        context.Productos.AddRange(producto1, producto2);
        await context.SaveChangesAsync();

        // Act - Usar fechas que incluyan los datos de prueba
        var fechaInicio = DateTime.Today.AddDays(-30);
        var fechaFin = DateTime.Today.AddDays(1);
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReporteProductosDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalProductosVendidos.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteFidelizacion_ConDatosExistentes_RetornaReporteCompleto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos de prueba con sufijos únicos
        var cliente = CrearClienteTest("fidelizacion");
        var tarjeta = CrearTarjetaFidelizacionTest(cliente.Id, "fidelizacion");
        
        context.Clientes.Add(cliente);
        context.TarjetasFidelizacion.Add(tarjeta);
        await context.SaveChangesAsync();

        // Act - Usar fechas que incluyan los datos de prueba
        var fechaInicio = DateTime.Today.AddDays(-30);
        var fechaFin = DateTime.Today.AddDays(1);
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReporteFidelizacionDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalTarjetas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReportePromociones_ConDatosExistentes_RetornaReporteCompleto()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Definir fechas que coincidan con el filtro del handler
        var fechaInicio = DateTime.Today.AddDays(-30);
        var fechaFin = DateTime.Today.AddDays(1);
        
        // Crear promoción con fechas que coincidan exactamente con el filtro
        var promocion = CrearPromocionTest("promociones", fechaInicio, fechaFin);
        
        context.Promociones.Add(promocion);
        await context.SaveChangesAsync();

        // Act - Usar las mismas fechas que se usaron para crear la promoción
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReportePromocionesDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalPromociones.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteVentas_SinDatos_DeberiaRetornarReporteVacio()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={DateTime.Today.AddDays(-30):yyyy-MM-dd}&fechaFin={DateTime.Today.AddDays(-1):yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteClientes_SinDatos_DeberiaRetornarReporteVacio()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/clientes?soloActivos=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteProductos_SinDatos_DeberiaRetornarReporteVacio()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/productos?fechaInicio={DateTime.Today.AddDays(-30):yyyy-MM-dd}&fechaFin={DateTime.Today.AddDays(-1):yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteFidelizacion_SinDatos_DeberiaRetornarReporteVacio()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={DateTime.Today.AddDays(-30):yyyy-MM-dd}&fechaFin={DateTime.Today.AddDays(-1):yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReportePromociones_SinDatos_DeberiaRetornarReporteVacio()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={DateTime.Today.AddDays(-30):yyyy-MM-dd}&fechaFin={DateTime.Today.AddDays(-1):yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    #region Métodos Helper

    private static Cliente CrearClienteTest(string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        var nombre = ClienteNombre.Crear($"Juan{sufijoUnico}", $"Pérez{sufijoUnico}");
        var email = Email.Create($"juan.perez{sufijoUnico}@test.com");
        var telefono = PhoneNumber.Create("+1234567890");
        var fechaNacimiento = DateTime.Now.AddYears(-25);
        
        return Cliente.Crear(guid, nombre, email, telefono, fechaNacimiento);
    }

    private static Producto CrearProductoTest(string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        var precio = new PrecioProducto(25.99m);
        var categoriaId = Guid.NewGuid();
        
        return Producto.Crear($"Hamburguesa Clásica {sufijoUnico}", $"Deliciosa hamburguesa con carne y vegetales {sufijoUnico}", precio, categoriaId, "Platos Principales");
    }

    private static Factura CrearFacturaTest(Guid clienteId, Guid productoId, string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        
        var factura = Factura.Crear(
            $"FAC-{sufijoUnico}",
            TipoFactura.Normal,
            $"Cliente {sufijoUnico}",
            clienteId,
            null,
            null,
            new List<Guid> { Guid.NewGuid() },
            $"Factura de prueba {sufijoUnico}"
        );

        // Agregar detalle a la factura
        factura.AgregarDetalle(
            productoId,
            $"Producto {sufijoUnico}",
            2,
            25.99m,
            16.0m, // IVA
            0m     // Sin descuento
        );

        return factura;
    }

    private static TarjetaFidelizacion CrearTarjetaFidelizacionTest(Guid clienteId, string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        var tarjeta = TarjetaFidelizacion.Crear(clienteId, $"TF-{DateTime.Now:yyyyMMdd}-{sufijoUnico}");
        tarjeta.Activar();
        return tarjeta;
    }

    private static Promocion CrearPromocionTest(string? sufijo = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        
        var promocion = Promocion.Crear(
            $"PROMO-{sufijoUnico}",
            $"Descuento 20% {sufijoUnico}",
            $"Descuento del 20% en todos los productos {sufijoUnico}",
            TipoPromocion.PorcentajeTotal,
            20.0m,
            fechaInicio ?? DateTime.Now.AddDays(-7),
            fechaFin ?? DateTime.Now.AddDays(7),
            50.0m, // Monto mínimo
            0,     // Puntos requeridos
            100,   // Máximo usos
            false  // No acumulable
        );
        promocion.Activar();
        return promocion;
    }

    #endregion
} 