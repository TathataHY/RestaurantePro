using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

/// <summary>
/// Tests de integración para el flujo completo de gestión de inventario inteligente
/// Este flujo valida la detección automática de stock bajo, creación de órdenes de compra y recepción
/// </summary>
public class FlujoGestionInventarioInteligenteTests : ApiIntegrationTestBase
{
    public FlujoGestionInventarioInteligenteTests() : base(new TestWebApplicationFactory())
    {
    }

    [Fact]
    public async Task FlujoCompletoInventarioInteligente_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Preparar el escenario completo
        Logger.LogInformation("🚀 Iniciando FLUJO COMPLETO de Gestión de Inventario Inteligente");
        
        // Limpiar datos de prueba
        await LimpiarDatosPrueba();
        
        // 1. Crear entidades base necesarias
        var administrador = await CrearUsuarioPrueba("admin.inventario", "Admin Inventario", "admin.inventario@test.com", RolUsuario.Administrador);
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente1 = await CrearIngredientePrueba("Tomate", "TOM-001", stockInicial: 5, stockMinimo: 10);
        var ingrediente2 = await CrearIngredientePrueba("Queso", "QUE-001", stockInicial: 3, stockMinimo: 8);
        var ingrediente3 = await CrearIngredientePrueba("Harina", "HAR-001", stockInicial: 15, stockMinimo: 5);
        
        Logger.LogInformation("✅ Entidades base creadas - Admin: {AdminId}, Proveedor: {ProveedorId}", 
            administrador.Id, proveedor.Id);

        // 2. PASO 1: Verificar stock bajo automáticamente
        Logger.LogInformation("📊 PASO 1: Verificando stock bajo");
        var responseStockBajo = await HttpClient.GetAsync("/api/inventario/ingredientes/stock-bajo");
        responseStockBajo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var stockBajoResponse = await responseStockBajo.Content.ReadFromJsonAsync<ApiResponse<List<IngredienteDto>>>();
        stockBajoResponse.Should().NotBeNull();
        stockBajoResponse!.Success.Should().BeTrue();
        stockBajoResponse.Data.Should().NotBeNull();
        
        // Verificar que los ingredientes con stock bajo están en la lista
        var ingredientesStockBajo = stockBajoResponse.Data;
        ingredientesStockBajo.Should().Contain(i => i.Nombre == "Tomate" && i.StockActual < i.StockMinimo);
        ingredientesStockBajo.Should().Contain(i => i.Nombre == "Queso" && i.StockActual < i.StockMinimo);
        ingredientesStockBajo.Should().NotContain(i => i.Nombre == "Harina"); // Harina tiene stock suficiente
        
        Logger.LogInformation("✅ Stock bajo detectado - {Cantidad} ingredientes con stock bajo", ingredientesStockBajo.Count);

        // 3. PASO 2: Crear orden de compra automática
        Logger.LogInformation("📋 PASO 2: Creando orden de compra automática");
        var ordenCompraRequest = new
        {
            ProveedorId = proveedor.Id,
            FechaOrden = DateTime.Now,
            FechaEntregaEsperada = DateTime.Now.AddDays(2),
            Observaciones = "Orden automática por stock bajo",
            Items = new[]
            {
                new
                {
                    IngredienteId = ingrediente1.Id,
                    CantidadSolicitada = 20,
                    PrecioUnitario = 2.50m,
                    Observaciones = "Reposición de stock"
                },
                new
                {
                    IngredienteId = ingrediente2.Id,
                    CantidadSolicitada = 15,
                    PrecioUnitario = 8.00m,
                    Observaciones = "Reposición de stock"
                }
            }
        };
        
        var responseOrdenCompra = await HttpClient.PostAsJsonAsync("/api/inventario/ordenes-compra", ordenCompraRequest);
        responseOrdenCompra.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var ordenCompraResponse = await responseOrdenCompra.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        ordenCompraResponse.Should().NotBeNull();
        ordenCompraResponse!.Success.Should().BeTrue();
        var ordenCompra = ordenCompraResponse.Data;
        
        Logger.LogInformation("✅ Orden de compra creada - ID: {OrdenId}, Estado: {Estado}", 
            ordenCompra.Id, ordenCompra.Estado);

        // 4. PASO 3: Aprobar orden de compra
        Logger.LogInformation("✅ PASO 3: Aprobando orden de compra");
        var responseAprobar = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/aprobar", null);
        responseAprobar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la orden cambió de estado
        var ordenAprobada = await DbContext.OrdenesCompra.FindAsync(ordenCompra.Id);
        ordenAprobada!.Estado.Should().Be(EstadoOrdenCompra.Confirmada);
        
        Logger.LogInformation("✅ Orden de compra aprobada");

        // 5. PASO 4: Recibir mercancía
        Logger.LogInformation("📦 PASO 4: Recibiendo mercancía");
        var recibirRequest = new
        {
            FechaRecepcion = DateTime.Now,
            Observaciones = "Recepción de mercancía",
            Items = new[]
            {
                new
                {
                    IngredienteId = ingrediente1.Id,
                    CantidadRecibida = 18, // Un poco menos de lo solicitado
                    PrecioUnitarioReal = 2.45m,
                    Calidad = "Excelente",
                    Observaciones = "Mercancía recibida en buen estado"
                },
                new
                {
                    IngredienteId = ingrediente2.Id,
                    CantidadRecibida = 15,
                    PrecioUnitarioReal = 8.00m,
                    Calidad = "Buena",
                    Observaciones = "Mercancía recibida"
                }
            }
        };
        
        var responseRecibir = await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/recibir", recibirRequest);
        responseRecibir.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Mercancía recibida");

        // 6. PASO 5: Verificar actualización automática de stock
        Logger.LogInformation("📊 PASO 5: Verificando actualización de stock");
        
        // Verificar que el stock se actualizó automáticamente
        var ingrediente1Actualizado = await DbContext.Ingredientes.FindAsync(ingrediente1.Id);
        ingrediente1Actualizado!.Stock.Should().Be(23); // 5 inicial + 18 recibido
        
        var ingrediente2Actualizado = await DbContext.Ingredientes.FindAsync(ingrediente2.Id);
        ingrediente2Actualizado!.Stock.Should().Be(18); // 3 inicial + 15 recibido
        
        Logger.LogInformation("✅ Stock actualizado - Tomate: {StockTomate}, Queso: {StockQueso}", 
            ingrediente1Actualizado.Stock, ingrediente2Actualizado.Stock);

        // 7. PASO 6: Verificar que ya no aparecen en stock bajo
        Logger.LogInformation("🔍 PASO 6: Verificando que ya no están en stock bajo");
        var responseStockBajoActualizado = await HttpClient.GetAsync("/api/inventario/ingredientes/stock-bajo");
        responseStockBajoActualizado.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var stockBajoActualizadoResponse = await responseStockBajoActualizado.Content.ReadFromJsonAsync<ApiResponse<List<IngredienteDto>>>();
        stockBajoActualizadoResponse.Should().NotBeNull();
        stockBajoActualizadoResponse!.Success.Should().BeTrue();
        
        // Verificar que los ingredientes ya no están en stock bajo
        var ingredientesStockBajoActualizado = stockBajoActualizadoResponse.Data;
        ingredientesStockBajoActualizado.Should().NotContain(i => i.Nombre == "Tomate");
        ingredientesStockBajoActualizado.Should().NotContain(i => i.Nombre == "Queso");
        
        Logger.LogInformation("✅ Ingredientes ya no están en stock bajo");

        // 8. PASO 7: Obtener alertas automáticas
        Logger.LogInformation("🚨 PASO 7: Obteniendo alertas automáticas");
        var responseAlertas = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        responseAlertas.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var alertasResponse = await responseAlertas.Content.ReadFromJsonAsync<ApiResponse<object>>();
        alertasResponse.Should().NotBeNull();
        alertasResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Alertas obtenidas");

        // 🎯 ASSERT - Validaciones finales del flujo completo
        Logger.LogInformation("🔍 Validando resultados del flujo completo");
        
        // Verificar que la orden de compra está en estado recibida
        var ordenFinal = await DbContext.OrdenesCompra.FindAsync(ordenCompra.Id);
        ordenFinal!.Estado.Should().Be(EstadoOrdenCompra.Recibida);
        
        // Verificar que los stocks están por encima del mínimo
        ingrediente1Actualizado.Stock.Should().BeGreaterThan(ingrediente1Actualizado.StockMinimo);
        ingrediente2Actualizado.Stock.Should().BeGreaterThan(ingrediente2Actualizado.StockMinimo);
        
        Logger.LogInformation("🎉 FLUJO COMPLETO de Gestión de Inventario Inteligente EXITOSO");
        Logger.LogInformation("📊 Resumen del flujo:");
        Logger.LogInformation("   - Stock bajo detectado: {Cantidad} ingredientes", ingredientesStockBajo.Count);
        Logger.LogInformation("   - Orden de compra creada y procesada: {OrdenId}", ordenCompra.Id);
        Logger.LogInformation("   - Stock actualizado - Tomate: {StockTomate}, Queso: {StockQueso}", 
            ingrediente1Actualizado.Stock, ingrediente2Actualizado.Stock);
    }

    [Fact]
    public async Task FlujoInventarioConConsumoAutomatico_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo con consumo automático de inventario
        Logger.LogInformation("🚀 Iniciando FLUJO de Inventario con Consumo Automático");
        
        await LimpiarDatosPrueba();
        
        var administrador = await CrearUsuarioPrueba("admin.consumo", "Admin Consumo", "admin.consumo@test.com", RolUsuario.Administrador);
        var proveedor = await CrearProveedorPrueba("Proveedor Consumo");
        var ingrediente = await CrearIngredientePrueba("Carne", "CAR-001", stockInicial: 20, stockMinimo: 5);
        
        Logger.LogInformation("✅ Entidades base creadas para flujo de consumo");

        // 1. PASO 1: Simular consumo de inventario
        Logger.LogInformation("🍽️ PASO 1: Simulando consumo de inventario");
        var consumoRequest = new
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 8,
            Motivo = "Preparación de platos",
            Observaciones = "Consumo automático por preparaciones"
        };
        
        var responseConsumo = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/consumir", consumoRequest);
        responseConsumo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Consumo registrado");

        // 2. PASO 2: Verificar stock actualizado
        Logger.LogInformation("📊 PASO 2: Verificando stock actualizado");
        var ingredienteConsumido = await DbContext.Ingredientes.FindAsync(ingrediente.Id);
        ingredienteConsumido!.Stock.Should().Be(12); // 20 inicial - 8 consumido
        
        Logger.LogInformation("✅ Stock actualizado después del consumo: {Stock}", ingredienteConsumido.Stock);

        // 3. PASO 3: Consumir más hasta llegar al stock mínimo
        Logger.LogInformation("⚠️ PASO 3: Consumiendo hasta stock mínimo");
        var consumoCriticoRequest = new
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 7,
            Motivo = "Preparación adicional",
            Observaciones = "Consumo que llevará al stock mínimo"
        };
        
        var responseConsumoCritico = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/consumir", consumoCriticoRequest);
        responseConsumoCritico.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ingredienteCritico = await DbContext.Ingredientes.FindAsync(ingrediente.Id);
        ingredienteCritico!.Stock.Should().Be(5); // 12 - 7 = 5 (stock mínimo)
        
        Logger.LogInformation("✅ Stock llegó al mínimo: {Stock}", ingredienteCritico.Stock);

        // 4. PASO 4: Verificar que aparece en stock bajo
        Logger.LogInformation("🔍 PASO 4: Verificando que aparece en stock bajo");
        var responseStockBajo = await HttpClient.GetAsync("/api/inventario/ingredientes/stock-bajo");
        responseStockBajo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var stockBajoResponse = await responseStockBajo.Content.ReadFromJsonAsync<ApiResponse<List<IngredienteDto>>>();
        stockBajoResponse.Should().NotBeNull();
        stockBajoResponse!.Success.Should().BeTrue();
        
        var ingredientesStockBajo = stockBajoResponse.Data;
        ingredientesStockBajo.Should().Contain(i => i.Nombre == "Carne" && i.StockActual == i.StockMinimo);
        
        Logger.LogInformation("✅ Ingrediente detectado en stock bajo");

        // 5. PASO 5: Crear orden de compra automática
        Logger.LogInformation("📋 PASO 5: Creando orden de compra automática");
        var ordenCompraRequest = new
        {
            ProveedorId = proveedor.Id,
            FechaOrden = DateTime.Now,
            FechaEntregaEsperada = DateTime.Now.AddDays(1),
            Observaciones = "Orden automática por consumo",
            Items = new[]
            {
                new
                {
                    IngredienteId = ingrediente.Id,
                    CantidadSolicitada = 25,
                    PrecioUnitario = 15.00m,
                    Observaciones = "Reposición urgente"
                }
            }
        };
        
        var responseOrdenCompra = await HttpClient.PostAsJsonAsync("/api/inventario/ordenes-compra", ordenCompraRequest);
        responseOrdenCompra.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var ordenCompraResponse = await responseOrdenCompra.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        var ordenCompra = ordenCompraResponse!.Data;
        
        Logger.LogInformation("✅ Orden de compra creada por consumo: {OrdenId}", ordenCompra.Id);

        // 6. PASO 6: Procesar recepción completa
        Logger.LogInformation("📦 PASO 6: Procesando recepción completa");
        
        // Aprobar orden
        await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/aprobar", null);
        
        // Recibir mercancía
        var recibirRequest = new
        {
            FechaRecepcion = DateTime.Now,
            Observaciones = "Recepción completa",
            Items = new[]
            {
                new
                {
                    IngredienteId = ingrediente.Id,
                    CantidadRecibida = 25,
                    PrecioUnitarioReal = 15.00m,
                    Calidad = "Excelente",
                    Observaciones = "Mercancía recibida completa"
                }
            }
        };
        
        await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/recibir", recibirRequest);
        
        Logger.LogInformation("✅ Recepción procesada");

        // 7. PASO 7: Verificar stock final
        Logger.LogInformation("📊 PASO 7: Verificando stock final");
        var ingredienteFinal = await DbContext.Ingredientes.FindAsync(ingrediente.Id);
        ingredienteFinal!.Stock.Should().Be(30); // 5 + 25 recibido
        
        Logger.LogInformation("✅ Stock final: {Stock}", ingredienteFinal.Stock);
        Logger.LogInformation("🎉 FLUJO de Consumo Automático EXITOSO");
    }

    [Fact]
    public async Task FlujoInventarioConVencimiento_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo con manejo de vencimientos
        Logger.LogInformation("🚀 Iniciando FLUJO de Inventario con Vencimientos");
        
        await LimpiarDatosPrueba();
        
        var administrador = await CrearUsuarioPrueba("admin.vencimiento", "Admin Vencimiento", "admin.vencimiento@test.com", RolUsuario.Administrador);
        var ingrediente = await CrearIngredientePrueba("Leche", "LEC-001", stockInicial: 10, stockMinimo: 3);
        
        Logger.LogInformation("✅ Entidades base creadas para flujo de vencimientos");

        // 1. PASO 1: Registrar lote con fecha de vencimiento
        Logger.LogInformation("📅 PASO 1: Registrando lote con vencimiento");
        var loteRequest = new
        {
            IngredienteId = ingrediente.Id,
            NumeroLote = "LOTE-2024-001",
            Cantidad = 10,
            FechaVencimiento = DateTime.Now.AddDays(5),
            PrecioUnitario = 3.50m,
            Observaciones = "Lote con vencimiento próximo"
        };
        
        var responseLote = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/lotes", loteRequest);
        responseLote.StatusCode.Should().Be(HttpStatusCode.Created);
        
        Logger.LogInformation("✅ Lote registrado con vencimiento");

        // 2. PASO 2: Verificar alertas de vencimiento
        Logger.LogInformation("🚨 PASO 2: Verificando alertas de vencimiento");
        var responseAlertasVencimiento = await HttpClient.GetAsync("/api/inventario/reportes/vencimientos-proximos");
        responseAlertasVencimiento.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var alertasVencimientoResponse = await responseAlertasVencimiento.Content.ReadFromJsonAsync<ApiResponse<object>>();
        alertasVencimientoResponse.Should().NotBeNull();
        alertasVencimientoResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Alertas de vencimiento obtenidas");

        // 3. PASO 3: Simular consumo del lote
        Logger.LogInformation("🍽️ PASO 3: Simulando consumo del lote");
        var consumoRequest = new
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 7,
            LoteId = "LOTE-2024-001",
            Motivo = "Consumo normal",
            Observaciones = "Consumo del lote con vencimiento"
        };
        
        var responseConsumo = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/consumir", consumoRequest);
        responseConsumo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Consumo del lote registrado");

        // 4. PASO 4: Verificar stock restante
        Logger.LogInformation("📊 PASO 4: Verificando stock restante");
        var ingredienteConsumido = await DbContext.Ingredientes.FindAsync(ingrediente.Id);
        ingredienteConsumido!.Stock.Should().Be(3); // 10 inicial - 7 consumido
        
        Logger.LogInformation("✅ Stock restante: {Stock}", ingredienteConsumido.Stock);
        Logger.LogInformation("🎉 FLUJO de Vencimientos EXITOSO");
    }

    private async Task LimpiarDatosPrueba()
    {
        // Limpiar datos en orden para evitar problemas de FK
        await LimpiarTablaOrdenesCompra();
        await LimpiarTablaIngredientes();
        await LimpiarTablaProveedores();
        await LimpiarTablaUsuarios();
        
        Logger.LogInformation("🧹 Datos de prueba limpiados");
    }

    private async Task LimpiarTablaOrdenesCompra()
    {
        var ordenes = await DbContext.OrdenesCompra.ToListAsync();
        DbContext.OrdenesCompra.RemoveRange(ordenes);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaIngredientes()
    {
        var ingredientes = await DbContext.Ingredientes.ToListAsync();
        DbContext.Ingredientes.RemoveRange(ingredientes);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaProveedores()
    {
        var proveedores = await DbContext.Proveedores.ToListAsync();
        DbContext.Proveedores.RemoveRange(proveedores);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaUsuarios()
    {
        var usuarios = await DbContext.Usuarios.ToListAsync();
        DbContext.Usuarios.RemoveRange(usuarios);
        await DbContext.SaveChangesAsync();
    }
} 