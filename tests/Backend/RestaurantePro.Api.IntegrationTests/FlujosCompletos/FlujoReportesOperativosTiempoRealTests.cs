using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantePro.Api.Common;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;
using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteVentas;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

/// <summary>
/// Tests de integración para el flujo de reportes operativos en tiempo real
/// Valida la funcionalidad completa de analytics y reportes del sistema
/// </summary>
[Collection("ApiTestCollection")]
public class FlujoReportesOperativosTiempoRealTests : ApiIntegrationTestBase
{
    public FlujoReportesOperativosTiempoRealTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task FlujoCompletoReportesOperativos_DebeFuncionarCorrectamente()
    {
        // Arrange - Configurar datos de prueba
        var usuario = await CrearUsuarioPrueba();
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto1 = await CrearProductoPrueba("Hamburguesa", 15.99m);
        var producto2 = await CrearProductoPrueba("Pizza", 22.50m);
        var producto3 = await CrearProductoPrueba("Ensalada", 12.00m);

        // Crear comandas y facturas para generar datos de reportes
        var comanda1 = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto1.Id, producto2.Id });
        var comanda2 = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto2.Id, producto3.Id });
        var comanda3 = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto1.Id, producto3.Id });

        // Crear preparaciones para medir tiempos
        var preparacion1 = await CrearPreparacionPrueba(comanda1.Id, producto1.Id);
        var preparacion2 = await CrearPreparacionPrueba(comanda2.Id, producto2.Id);
        var preparacion3 = await CrearPreparacionPrueba(comanda3.Id, producto3.Id);

        // Simular completar preparaciones con diferentes tiempos
        await CompletarPreparacion(preparacion1.Id, TimeSpan.FromMinutes(8));
        await CompletarPreparacion(preparacion2.Id, TimeSpan.FromMinutes(12));
        await CompletarPreparacion(preparacion3.Id, TimeSpan.FromMinutes(6));

        // Act & Assert - 1. Reporte de ventas diarias
        var responseVentasDiarias = await HttpClient.GetAsync("/api/operaciones/reportes/ventas-diarias");
        responseVentasDiarias.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseVentasDiarias = await responseVentasDiarias.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseVentasDiarias.Should().NotBeNull();
        apiResponseVentasDiarias!.Success.Should().BeTrue();

        // Act & Assert - 2. Reporte de ocupación de mesas
        var fechaInicio = DateTime.Today;
        var fechaFin = DateTime.Today.AddDays(1);
        var responseOcupacion = await HttpClient.GetAsync($"/api/operaciones/reportes/ocupacion-mesas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        responseOcupacion.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseOcupacion = await responseOcupacion.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseOcupacion.Should().NotBeNull();
        apiResponseOcupacion!.Success.Should().BeTrue();

        // Act & Assert - 3. Productos más populares
        var responseProductosPopulares = await HttpClient.GetAsync($"/api/operaciones/reportes/productos-populares?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        responseProductosPopulares.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseProductosPopulares = await responseProductosPopulares.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseProductosPopulares.Should().NotBeNull();
        apiResponseProductosPopulares!.Success.Should().BeTrue();

        // Act & Assert - 4. Desempeño de empleados
        var responseDesempeno = await HttpClient.GetAsync($"/api/operaciones/reportes/desempeno-empleados?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        responseDesempeno.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseDesempeno = await responseDesempeno.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseDesempeno.Should().NotBeNull();
        apiResponseDesempeno!.Success.Should().BeTrue();

        // Act & Assert - 5. Tiempos de preparación
        var responseTiempos = await HttpClient.GetAsync($"/api/operaciones/reportes/tiempos-preparacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        responseTiempos.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseTiempos = await responseTiempos.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseTiempos.Should().NotBeNull();
        apiResponseTiempos!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ReporteVentasDiarias_DebeRetornarMetricasCorrectas()
    {
        // Arrange
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba("Producto Test", 25.00m);
        
        // Crear múltiples comandas para el día
        for (int i = 0; i < 3; i++)
        {
            var comanda = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto.Id });
            await CrearFacturaPrueba(comanda.Id, 25.00m);
        }

        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/reportes/ventas-diarias");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ReporteOcupacionMesas_DebeMostrarEstadoActual()
    {
        // Arrange
        var cliente = await CrearClientePrueba();
        var mesa1 = await CrearMesaPrueba();
        var mesa2 = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba("Producto Test", 20.00m);

        // Ocupar una mesa
        await CrearComandaConProductos(cliente.Id, mesa1.Id, new[] { producto.Id });

        // Act
        var fechaInicio = DateTime.Today;
        var fechaFin = DateTime.Today.AddDays(1);
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/ocupacion-mesas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ReporteProductosPopulares_DebeOrdenarPorVentas()
    {
        // Arrange
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto1 = await CrearProductoPrueba("Producto Popular", 15.00m);
        var producto2 = await CrearProductoPrueba("Producto Menos Popular", 20.00m);

        // Crear más ventas del producto popular
        for (int i = 0; i < 3; i++)
        {
            var comanda = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto1.Id });
            await CrearFacturaPrueba(comanda.Id, 15.00m);
        }

        // Una venta del producto menos popular
        var comanda2 = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto2.Id });
        await CrearFacturaPrueba(comanda2.Id, 20.00m);

        // Act
        var fechaInicio = DateTime.Today;
        var fechaFin = DateTime.Today.AddDays(1);
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/productos-populares?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ReporteTiemposPreparacion_DebeCalcularPromedios()
    {
        // Arrange
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba("Producto Test", 18.00m);
        
        // Crear preparaciones con diferentes tiempos
        var comanda1 = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto.Id });
        var preparacion1 = await CrearPreparacionPrueba(comanda1.Id, producto.Id);
        await CompletarPreparacion(preparacion1.Id, TimeSpan.FromMinutes(10));

        var comanda2 = await CrearComandaConProductos(cliente.Id, mesa.Id, new[] { producto.Id });
        var preparacion2 = await CrearPreparacionPrueba(comanda2.Id, producto.Id);
        await CompletarPreparacion(preparacion2.Id, TimeSpan.FromMinutes(15));

        // Act
        var fechaInicio = DateTime.Today;
        var fechaFin = DateTime.Today.AddDays(1);
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/tiempos-preparacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    #region Métodos Auxiliares

    private async Task<Comanda> CrearComandaConProductos(Guid clienteId, Guid mesaId, Guid[] productoIds)
    {
        // Crear y persistir un mesero real antes de crear la comanda
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        
        var comanda = Comanda.Crear(
            mesero.Id,
            clienteId,
            mesaId,
            "Comanda de prueba para reportes"
        );

        // Primero guardar la comanda
        DbContext.Comandas.Add(comanda);
        await DbContext.SaveChangesAsync();

        // Luego agregar los items
        foreach (var productoId in productoIds)
        {
            var producto = await DbContext.Productos.FindAsync(productoId);
            if (producto != null)
            {
                comanda.AgregarItem(productoId, producto.Nombre!, 1, producto.Precio!.Valor);
            }
        }

        // Guardar los cambios de los items
        await DbContext.SaveChangesAsync();
        return comanda;
    }

    private async Task<PreparacionDiaria> CrearPreparacionPrueba(Guid comandaId, Guid productoId)
    {
        var chefId = Guid.NewGuid(); // Chef de prueba
        var preparacion = PreparacionDiaria.Crear(
            productoId,
            10, // Cantidad
            chefId,
            DateTime.Now.AddHours(2), // Fecha vencimiento
            "Preparación de prueba para reportes"
        );

        DbContext.Preparaciones.Add(preparacion);
        await DbContext.SaveChangesAsync();
        return preparacion;
    }

    private async Task CompletarPreparacion(Guid preparacionId, TimeSpan tiempoPreparacion)
    {
        var preparacion = await DbContext.Preparaciones.FindAsync(preparacionId);
        if (preparacion != null)
        {
            preparacion.MarcarComoDisponible();
            // Consumir una cantidad para simular uso
            preparacion.ConsumirCantidad(1);
            await DbContext.SaveChangesAsync();
        }
    }

    private async Task<Factura> CrearFacturaPrueba(Guid comandaId, decimal montoTotal)
    {
        var comanda = await DbContext.Comandas
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == comandaId);

        if (comanda == null) throw new InvalidOperationException("Comanda no encontrada");

        var factura = Factura.Crear(
            $"F-{Guid.NewGuid():N}",
            TipoFactura.Normal,
            "Cliente de prueba",
            comanda.ClienteId,
            observaciones: "Factura de prueba para reportes"
        );

        // Agregar detalles de factura basados en los items de la comanda
        foreach (var item in comanda.Items)
        {
            factura.AgregarDetalle(
                item.ProductoId,
                $"Producto {item.ProductoId}", // Descripción del producto
                item.Cantidad,
                item.PrecioUnitario,
                0.12m, // 12% IVA
                0 // Sin descuento
            );
        }

        DbContext.Facturas.Add(factura);
        await DbContext.SaveChangesAsync();
        return factura;
    }

    #endregion
} 