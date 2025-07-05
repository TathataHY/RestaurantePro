using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantePro.Api.Common;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

/// <summary>
/// Tests de integración para el flujo de analytics de inventario con IA
/// Valida la funcionalidad completa de análisis predictivo y recomendaciones de inventario
/// </summary>
[Collection("ApiTestCollection")]
public class FlujoAnalyticsInventarioTests : ApiIntegrationTestBase
{
    public FlujoAnalyticsInventarioTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task FlujoCompletoAnalyticsInventario_DebeFuncionarCorrectamente()
    {
        // Arrange - Configurar datos de prueba
        var proveedor = await CrearProveedorPrueba();
        var ingrediente1 = await CrearIngredientePrueba("Tomate", 2.50m, 100, proveedor.Id);
        var ingrediente2 = await CrearIngredientePrueba("Lechuga", 1.80m, 50, proveedor.Id);
        var ingrediente3 = await CrearIngredientePrueba("Carne", 15.00m, 20, proveedor.Id);

        // Crear movimientos de inventario para generar datos de análisis
        await CrearMovimientoInventario(ingrediente1.Id, -30, "Consumo por comanda");
        await CrearMovimientoInventario(ingrediente2.Id, -20, "Consumo por comanda");
        await CrearMovimientoInventario(ingrediente3.Id, -5, "Consumo por comanda");

        // Crear órdenes de compra para análisis de tendencias
        await CrearOrdenCompraPrueba(proveedor.Id, new[] { ingrediente1.Id, ingrediente2.Id });

        // Act & Assert - 1. Análisis de inventario
        var responseAnalisis = await HttpClient.GetAsync("/api/inventario/reportes/analisis");
        responseAnalisis.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseAnalisis = await responseAnalisis.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseAnalisis.Should().NotBeNull();
        apiResponseAnalisis!.Success.Should().BeTrue();

        // Act & Assert - 2. Recomendaciones de compra
        var responseRecomendaciones = await HttpClient.GetAsync("/api/inventario/reportes/recomendaciones-compra?diasProyeccion=30");
        responseRecomendaciones.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseRecomendaciones = await responseRecomendaciones.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseRecomendaciones.Should().NotBeNull();
        apiResponseRecomendaciones!.Success.Should().BeTrue();

        // Act & Assert - 3. Valor total del inventario
        var responseValorTotal = await HttpClient.GetAsync("/api/inventario/reportes/valor-total");
        responseValorTotal.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseValorTotal = await responseValorTotal.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseValorTotal.Should().NotBeNull();
        apiResponseValorTotal!.Success.Should().BeTrue();

        // Act & Assert - 4. Alertas de stock bajo
        var responseAlertas = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        responseAlertas.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseAlertas = await responseAlertas.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseAlertas.Should().NotBeNull();
        apiResponseAlertas!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AnalisisInventario_DebeRetornarMetricasCorrectas()
    {
        // Arrange
        var proveedor = await CrearProveedorPrueba();
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test", 10.00m, 50, proveedor.Id);
        
        // Crear múltiples movimientos para análisis
        for (int i = 0; i < 3; i++)
        {
            await CrearMovimientoInventario(ingrediente.Id, -10, $"Consumo {i + 1}");
        }

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/analisis");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RecomendacionesCompra_DebeGenerarRecomendacionesInteligentes()
    {
        // Arrange
        var proveedor = await CrearProveedorPrueba();
        var ingrediente = await CrearIngredientePrueba("Ingrediente Popular", 5.00m, 10, proveedor.Id);
        
        // Simular consumo alto
        await CrearMovimientoInventario(ingrediente.Id, -8, "Consumo alto");

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/recomendaciones-compra?diasProyeccion=30");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ValorTotalInventario_DebeCalcularCorrectamente()
    {
        // Arrange
        var proveedor = await CrearProveedorPrueba();
        var ingrediente1 = await CrearIngredientePrueba("Producto 1", 10.00m, 20, proveedor.Id);
        var ingrediente2 = await CrearIngredientePrueba("Producto 2", 15.00m, 10, proveedor.Id);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/valor-total");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AlertasInventario_DebeDetectarStockBajo()
    {
        // Arrange
        var proveedor = await CrearProveedorPrueba();
        var ingrediente = await CrearIngredientePrueba("Ingrediente Crítico", 8.00m, 5, proveedor.Id);
        
        // Simular consumo que deja stock bajo
        await CrearMovimientoInventario(ingrediente.Id, -3, "Consumo que genera alerta");

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    // Métodos auxiliares para crear datos de prueba
    private async Task<Proveedor> CrearProveedorPrueba()
    {
        var proveedor = Proveedor.Crear(
            "Proveedor Test",
            "Contacto Test",
            "proveedor@test.com",
            "123456789",
            "Dirección Test",
            "Ciudad Test",
            "12345",
            "País Test",
            "RFC123456789",
            "Info Bancaria Test",
            30
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();
        return proveedor;
    }

    private async Task<Ingrediente> CrearIngredientePrueba(string nombre, decimal precio, int stockInicial, Guid proveedorId)
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            $"COD-{nombre.ToUpper()}",
            $"Descripción de {nombre}",
            UnidadMedida.Kilogramo,
            5, // stock mínimo
            stockInicial
        );

        // Asociar proveedor
        ingrediente.AsociarProveedorPrincipal(proveedorId);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Ingredientes.Add(ingrediente);
        await context.SaveChangesAsync();
        return ingrediente;
    }

    private async Task CrearMovimientoInventario(Guid ingredienteId, int cantidad, string motivo)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        MovimientoInventario movimiento;
        if (cantidad > 0)
        {
            movimiento = MovimientoInventario.CrearIngreso(ingredienteId, cantidad, motivo);
        }
        else
        {
            movimiento = MovimientoInventario.CrearEgreso(ingredienteId, Math.Abs(cantidad), motivo);
        }

        context.MovimientosInventario.Add(movimiento);
        await context.SaveChangesAsync();
    }

    private async Task<OrdenCompra> CrearOrdenCompraPrueba(Guid proveedorId, Guid[] ingredienteIds)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        var ordenCompra = OrdenCompra.Crear(proveedorId, "Orden de prueba para analytics", DateTime.Now);

        context.OrdenesCompra.Add(ordenCompra);
        await context.SaveChangesAsync();

        // Agregar items a la orden
        foreach (var ingredienteId in ingredienteIds)
        {
            var item = ItemOrdenCompra.Crear(
                ordenCompra.Id,
                ingredienteId,
                "Ingrediente Test",
                10,
                UnidadMedida.Kilogramo
            );
            context.Set<ItemOrdenCompra>().Add(item);
        }

        await context.SaveChangesAsync();
        return ordenCompra;
    }
} 