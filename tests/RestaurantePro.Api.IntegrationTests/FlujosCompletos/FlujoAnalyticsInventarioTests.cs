using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantePro.Api.Common;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

/// <summary>
/// Tests de integración para el flujo de Analytics de Inventario con IA
/// Valida la funcionalidad completa de análisis predictivo, recomendaciones y alertas automáticas
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
        // Arrange - Crear datos de prueba con diferentes rotaciones y stocks
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.EncargadoInventario);
        var ingredienteAltaRotacion = await CrearIngredienteAltaRotacion();
        var ingredienteBajaRotacion = await CrearIngredienteBajaRotacion();
        var ingredienteStockBajo = await CrearIngredienteConStockBajo();
        var ingredienteStockCritico = await CrearIngredienteConStockCritico();

        // Act & Assert - 1. Análisis de inventario con IA
        var responseAnalisis = await HttpClient.GetAsync($"/api/inventario/reportes/analisis?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}");
        responseAnalisis.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseAnalisis = await responseAnalisis.Content.ReadFromJsonAsync<ApiResponse<RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisInventarioDto>>();
        apiResponseAnalisis.Should().NotBeNull();
        apiResponseAnalisis!.Success.Should().BeTrue();
        apiResponseAnalisis.Data.Should().NotBeNull();
        apiResponseAnalisis.Data.ResumenExecutivo.TotalIngredientes.Should().BeGreaterThanOrEqualTo(4);

        // Act & Assert - 2. Recomendaciones inteligentes de compra
        var responseRecomendaciones = await HttpClient.GetAsync($"/api/inventario/reportes/recomendaciones-compra?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}");
        responseRecomendaciones.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseRecomendaciones = await responseRecomendaciones.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseRecomendaciones.Should().NotBeNull();
        apiResponseRecomendaciones!.Success.Should().BeTrue();
        apiResponseRecomendaciones.Data.Should().NotBeNull();

        // Act & Assert - 3. Valor total del inventario
        var responseValorTotal = await HttpClient.GetAsync($"/api/inventario/reportes/valor-total?fecha={DateTime.Today:yyyy-MM-dd}");
        responseValorTotal.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseValorTotal = await responseValorTotal.Content.ReadFromJsonAsync<ApiResponse<RestaurantePro.Application.Inventario.Reportes.DTOs.ValorTotalInventarioDto>>();
        apiResponseValorTotal.Should().NotBeNull();
        apiResponseValorTotal!.Success.Should().BeTrue();
        apiResponseValorTotal.Data.Should().NotBeNull();
        apiResponseValorTotal.Data.ValorTotal.Should().BeGreaterThan(0);

        // Act & Assert - 4. Alertas de stock bajo
        var responseAlertas = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        responseAlertas.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseAlertas = await responseAlertas.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseAlertas.Should().NotBeNull();
        apiResponseAlertas!.Success.Should().BeTrue();

        // Cleanup
        await LimpiarDatosPrueba();
    }

    [Fact]
    public async Task FlujoAnalyticsConPrediccionDemanda_DebeFuncionarCorrectamente()
    {
        // Arrange - Crear ingredientes con patrones de consumo específicos
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.EncargadoInventario);
        var ingredienteEstacional = await CrearIngredienteEstacional();
        var ingredienteConsumoVariable = await CrearIngredienteConsumoVariable();

        // Act & Assert - Análisis predictivo de demanda
        var responsePrediccion = await HttpClient.GetAsync($"/api/inventario/reportes/analisis?fechaInicio={DateTime.Today.AddDays(-30):yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}&incluirPrediccion=true");
        responsePrediccion.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponsePrediccion = await responsePrediccion.Content.ReadFromJsonAsync<ApiResponse<RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisInventarioDto>>();
        apiResponsePrediccion.Should().NotBeNull();
        apiResponsePrediccion!.Success.Should().BeTrue();
        apiResponsePrediccion.Data.Should().NotBeNull();

        // Cleanup
        await LimpiarDatosPrueba();
    }

    [Fact]
    public async Task FlujoAnalyticsConOptimizacionStock_DebeFuncionarCorrectamente()
    {
        // Arrange - Crear ingredientes con diferentes niveles de stock
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.EncargadoInventario);
        var ingredienteSobreStock = await CrearIngredienteConSobreStock();
        var ingredienteStockOptimo = await CrearIngredienteConStockOptimo();

        // Act & Assert - Recomendaciones de optimización
        var responseOptimizacion = await HttpClient.GetAsync($"/api/inventario/reportes/recomendaciones-compra?fechaInicio={DateTime.Today.AddDays(-7):yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}&incluirOptimizacion=true");
        responseOptimizacion.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseOptimizacion = await responseOptimizacion.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseOptimizacion.Should().NotBeNull();
        apiResponseOptimizacion!.Success.Should().BeTrue();
        apiResponseOptimizacion.Data.Should().NotBeNull();

        // Cleanup
        await LimpiarDatosPrueba();
    }

    [Fact]
    public async Task FlujoAnalyticsConAlertasInteligentes_DebeFuncionarCorrectamente()
    {
        // Arrange - Crear ingredientes que generen alertas específicas
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.EncargadoInventario);
        var ingredienteVencimientoProximo = await CrearIngredienteConVencimientoProximo();
        var ingredienteRotacionAnormal = await CrearIngredienteConRotacionAnormal();

        // Act & Assert - Alertas inteligentes
        var responseAlertasInteligentes = await HttpClient.GetAsync($"/api/inventario/reportes/alertas?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}&incluirAlertasInteligentes=true");
        responseAlertasInteligentes.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponseAlertasInteligentes = await responseAlertasInteligentes.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponseAlertasInteligentes.Should().NotBeNull();
        apiResponseAlertasInteligentes!.Success.Should().BeTrue();

        // Cleanup
        await LimpiarDatosPrueba();
    }

    #region Métodos auxiliares

    private async Task<Usuario> CrearUsuarioPrueba(RolUsuario rol = RolUsuario.EncargadoInventario)
    {
        var email = $"usuario{Guid.NewGuid():N}@test.com";
        var usuario = Usuario.Crear(
            "juanperez",
            "Juan Pérez",
            email,
            rol);

        DbContext.Usuarios.Add(usuario);
        await DbContext.SaveChangesAsync();
        return usuario;
    }

    private async Task<Ingrediente> CrearIngredienteAltaRotacion()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Tomate",
            "TOM-001",
            "Tomate fresco de alta rotación",
            UnidadMedida.Kilogramo,
            5, // Stock mínimo bajo
            50); // Stock actual alto

        // Configurar costo promedio para que el valor total sea mayor a 0
        ingrediente.ActualizarCostoPromedio(2.50m);

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteBajaRotacion()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Especias Exóticas",
            "ESP-001",
            "Especias de baja rotación",
            UnidadMedida.Gramo,
            100, // Stock mínimo alto
            200); // Stock actual alto

        // Configurar costo promedio para que el valor total sea mayor a 0
        ingrediente.ActualizarCostoPromedio(0.15m);

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConStockBajo()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Lechuga",
            "LEC-001",
            "Lechuga fresca con stock bajo",
            UnidadMedida.Kilogramo,
            5, // Stock mínimo
            3); // Stock actual bajo

        // Configurar costo promedio para que el valor total sea mayor a 0
        ingrediente.ActualizarCostoPromedio(1.80m);

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConStockCritico()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Pollo",
            "POL-001",
            "Pollo con stock crítico",
            UnidadMedida.Kilogramo,
            10, // Stock mínimo
            1); // Stock actual crítico

        // Configurar costo promedio para que el valor total sea mayor a 0
        ingrediente.ActualizarCostoPromedio(8.50m);

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteEstacional()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Fresas",
            "FRE-001",
            "Fresas estacionales",
            UnidadMedida.Kilogramo,
            2, // Stock mínimo
            15); // Stock actual

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConsumoVariable()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Queso Azul",
            "QUE-001",
            "Queso azul con consumo variable",
            UnidadMedida.Kilogramo,
            1, // Stock mínimo
            8); // Stock actual

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConSobreStock()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Harina",
            "HAR-001",
            "Harina con sobre stock",
            UnidadMedida.Kilogramo,
            20, // Stock mínimo
            200); // Stock actual muy alto

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConStockOptimo()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Aceite de Oliva",
            "ACE-001",
            "Aceite de oliva con stock óptimo",
            UnidadMedida.Litro,
            5, // Stock mínimo
            25); // Stock actual óptimo

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConVencimientoProximo()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Yogur",
            "YOG-001",
            "Yogur con vencimiento próximo",
            UnidadMedida.Litro,
            10, // Stock mínimo
            30); // Stock actual

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Ingrediente> CrearIngredienteConRotacionAnormal()
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            "Salsa Especial",
            "SAL-001",
            "Salsa con rotación anormal",
            UnidadMedida.Litro,
            2, // Stock mínimo
            50); // Stock actual alto

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    private async Task LimpiarDatosPrueba()
    {
        // Limpiar en orden para evitar problemas de FK
        DbContext.Ingredientes.RemoveRange(DbContext.Ingredientes);
        DbContext.Usuarios.RemoveRange(DbContext.Usuarios);
        
        await DbContext.SaveChangesAsync();
    }

    #endregion
} 