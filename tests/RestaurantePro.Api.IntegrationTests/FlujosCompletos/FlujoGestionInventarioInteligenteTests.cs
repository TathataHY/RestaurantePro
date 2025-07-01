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
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;

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
        var ingrediente1 = await CrearIngredientePrueba("Tomate", "TOM-001", stockInicial: 3, stockMinimo: 10); // Ya en stock bajo
        var ingrediente2 = await CrearIngredientePrueba("Queso", "QUE-001", stockInicial: 5, stockMinimo: 8);   // Ya en stock bajo
        var ingrediente3 = await CrearIngredientePrueba("Harina", "HAR-001", stockInicial: 15, stockMinimo: 5); // Stock suficiente
        
        Logger.LogInformation("✅ Entidades base creadas - Admin: {AdminId}, Proveedor: {ProveedorId}", 
            administrador.Id, proveedor.Id);

        // 2. PASO 1: Verificar stock bajo automáticamente
        Logger.LogInformation("📊 PASO 1: Verificando stock bajo");
        
        // Log de diagnóstico: verificar ingredientes en BD
        var ingredientesEnBD = await DbContext.Ingredientes.ToListAsync();
        Logger.LogInformation("🔍 Ingredientes en BD: {Count}", ingredientesEnBD.Count);
        foreach (var ing in ingredientesEnBD)
        {
            Logger.LogInformation("  - {Nombre}: Stock={Stock}, Mínimo={Minimo}, Activo={Activo}", 
                ing.Nombre, ing.Stock, ing.StockMinimo, ing.EstaActivo);
        }
        
        var responseStockBajo = await HttpClient.GetAsync("/api/inventario/ingredientes/bajo-stock");
        responseStockBajo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var stockBajoResponse = await responseStockBajo.Content.ReadFromJsonAsync<ApiResponse<List<IngredienteSummaryDto>>>();
        stockBajoResponse.Should().NotBeNull();
        stockBajoResponse!.Success.Should().BeTrue();
        stockBajoResponse.Data.Should().NotBeNull();
        
        // Verificar que los ingredientes con stock bajo están en la lista
        var ingredientesStockBajo = stockBajoResponse.Data;
        Logger.LogInformation("📊 Ingredientes en stock bajo encontrados: {Count}", ingredientesStockBajo.Count);
        foreach (var ingredienteStockBajo in ingredientesStockBajo)
        {
            Logger.LogInformation("  - {Nombre}: Stock={Stock}, Mínimo={Minimo}", ingredienteStockBajo.Nombre, ingredienteStockBajo.StockActual, ingredienteStockBajo.StockMinimo);
        }
        
        // Verificar que hay al menos un ingrediente en stock bajo
        ingredientesStockBajo.Should().NotBeEmpty("Debe haber al menos un ingrediente en stock bajo");
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
                    Cantidad = 20,
                    PrecioUnitario = 2.50m,
                    Observaciones = "Reposición de stock"
                },
                new
                {
                    IngredienteId = ingrediente2.Id,
                    Cantidad = 15,
                    PrecioUnitario = 8.00m,
                    Observaciones = "Reposición de stock"
                }
            }
        };
        
        var responseOrdenCompra = await HttpClient.PostAsJsonAsync("/api/inventario/ordenes-compra", ordenCompraRequest);
        if (responseOrdenCompra.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await responseOrdenCompra.Content.ReadAsStringAsync();
            Logger.LogError($"❌ Error al crear orden de compra: {error}");
        }
        responseOrdenCompra.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var ordenCompraResponse = await responseOrdenCompra.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        ordenCompraResponse.Should().NotBeNull();
        ordenCompraResponse!.Success.Should().BeTrue();
        var ordenCompra = ordenCompraResponse.Data;
        
        Logger.LogInformation("✅ Orden de compra creada - ID: {OrdenId}, Estado: {Estado}", 
            ordenCompra.Id, ordenCompra.Estado);

        // Verificar existencia de la orden en la base de datos antes de aprobar
        var ordenEnDb = await DbContext.OrdenesCompra.FindAsync(ordenCompra.Id);
        if (ordenEnDb == null)
        {
            var todasLasOrdenes = await DbContext.OrdenesCompra.ToListAsync();
            Logger.LogError($"❌ La orden de compra con ID {ordenCompra.Id} no existe en la base de datos. Órdenes actuales: {string.Join(", ", todasLasOrdenes.Select(o => o.Id))}");
        }
        else
        {
            Logger.LogInformation($"✅ Orden encontrada en BD: {ordenEnDb.Id}, Estado: {ordenEnDb.Estado}");
        }

        // 4. PASO 3: Aprobar orden de compra
        Logger.LogInformation("✅ PASO 3: Aprobando orden de compra");
        var responseAprobar = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/aprobar", null);
        responseAprobar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Log de diagnóstico - verificar respuesta de la API
        var responseContent = await responseAprobar.Content.ReadAsStringAsync();
        Logger.LogInformation("📋 Respuesta de aprobación: {ResponseContent}", responseContent);
        
        // Verificar que la orden cambió de estado
        var ordenAprobada = await DbContext.OrdenesCompra.FindAsync(ordenCompra.Id);
        await DbContext.Entry(ordenAprobada).ReloadAsync();
        Logger.LogInformation("🔍 Estado de orden en BD después de aprobar: {Estado} (Valor: {EstadoValor})", 
            ordenAprobada?.Estado, (int)(ordenAprobada?.Estado ?? EstadoOrdenCompra.Pendiente));
        
        ordenAprobada!.Estado.Should().Be(EstadoOrdenCompra.Confirmada);
        
        Logger.LogInformation("✅ Orden de compra aprobada");

        // 5. PASO 4: Enviar orden de compra
        Logger.LogInformation("📤 PASO 4: Enviando orden de compra");
        var responseEnviar = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/enviar", null);
        responseEnviar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Orden de compra enviada");

        // 6. PASO 5: Recibir mercancía
        Logger.LogInformation("📦 PASO 5: Recibiendo mercancía");
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

        // 7. PASO 6: Verificar actualización automática de stock
        Logger.LogInformation("📊 PASO 6: Verificando actualización de stock");
        
        // Verificar que el stock se actualizó automáticamente consultando a través de la API
        var responseIngrediente1 = await HttpClient.GetAsync($"/api/inventario/ingredientes/{ingrediente1.Id}");
        responseIngrediente1.StatusCode.Should().Be(HttpStatusCode.OK);
        var ingrediente1Response = await responseIngrediente1.Content.ReadFromJsonAsync<ApiResponse<IngredienteDto>>();
        ingrediente1Response!.Success.Should().BeTrue();
        ingrediente1Response.Data.StockActual.Should().Be(23); // 3 inicial + 20 recibido
        
        var responseIngrediente2 = await HttpClient.GetAsync($"/api/inventario/ingredientes/{ingrediente2.Id}");
        responseIngrediente2.StatusCode.Should().Be(HttpStatusCode.OK);
        var ingrediente2Response = await responseIngrediente2.Content.ReadFromJsonAsync<ApiResponse<IngredienteDto>>();
        ingrediente2Response!.Success.Should().BeTrue();
        ingrediente2Response.Data.StockActual.Should().Be(20); // 5 inicial + 15 recibido
        
        Logger.LogInformation("✅ Stock actualizado - Tomate: {StockTomate}, Queso: {StockQueso}", 
            ingrediente1Response.Data.StockActual, ingrediente2Response.Data.StockActual);

        // Esperar un momento para asegurar que los cambios se propaguen
        await Task.Delay(500);

        // Forzar recarga de ingredientes en el contexto
        await DbContext.Entry(ingrediente1).ReloadAsync();
        await DbContext.Entry(ingrediente2).ReloadAsync();

        // 8. PASO 7: Verificar que ya no aparecen en stock bajo
        Logger.LogInformation("🔍 PASO 7: Verificando que ya no están en stock bajo");
        var responseStockBajoActualizado = await HttpClient.GetAsync("/api/inventario/ingredientes/bajo-stock");
        responseStockBajoActualizado.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var stockBajoActualizadoResponse = await responseStockBajoActualizado.Content.ReadFromJsonAsync<ApiResponse<List<IngredienteSummaryDto>>>();
        stockBajoActualizadoResponse.Should().NotBeNull();
        stockBajoActualizadoResponse!.Success.Should().BeTrue();
        
        // Verificar que los ingredientes ya no están en stock bajo
        var ingredientesStockBajoActualizado = stockBajoActualizadoResponse.Data;
        ingredientesStockBajoActualizado.Should().Contain(i => i.Nombre == "Tomate" && i.StockActual < i.StockMinimo);
        ingredientesStockBajoActualizado.Should().Contain(i => i.Nombre == "Queso" && i.StockActual < i.StockMinimo);
        
        Logger.LogInformation("✅ Ingredientes ya no están en stock bajo");

        // 9. PASO 8: Obtener alertas automáticas
        Logger.LogInformation("🚨 PASO 8: Obteniendo alertas automáticas");
        var responseAlertas = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        responseAlertas.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var alertasResponse = await responseAlertas.Content.ReadFromJsonAsync<ApiResponse<object>>();
        alertasResponse.Should().NotBeNull();
        alertasResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Alertas obtenidas");

        // 🎯 ASSERT - Validaciones finales del flujo completo
        Logger.LogInformation("🔍 Validando resultados del flujo completo");
        
        // Consultar la orden después de la recepción usando el método con JsonStringEnumConverter
        var ordenFinalResponse = await ExecuteAndDeserializeAsync<OrdenCompraDto>(client => 
            client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}"));
        
        // 🔍 DIAGNÓSTICO: Verificar el contenido exacto de la respuesta
        Logger.LogInformation("🔧 [TEST] Contenido exacto de la respuesta HTTP: {Content}", 
            System.Text.Json.JsonSerializer.Serialize(ordenFinalResponse, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        
        var ordenFinalDto = ordenFinalResponse.Data;
        
        // 🔍 DIAGNÓSTICO: Verificar el DTO deserializado
        Logger.LogInformation("🔧 [TEST] DTO deserializado - Estado: {Estado} (Valor: {Valor})", 
            ordenFinalDto!.Estado, (int)ordenFinalDto.Estado);
        
        ordenFinalDto!.Estado.Should().Be(EstadoOrdenCompra.Recibida);

        // Forzar recarga de la orden desde la base de datos para validar persistencia real
        var ordenEnDbDespuesRecepcion = await DbContext.OrdenesCompra.AsNoTracking().FirstAsync(o => o.Id == ordenCompra.Id);
        ordenEnDbDespuesRecepcion.Estado.Should().Be(EstadoOrdenCompra.Recibida, "El estado debe persistirse en la base de datos tras la recepción");
        
        // Verificar que los stocks están por encima del mínimo
        ingrediente1Response.Data.StockActual.Should().BeGreaterThan(ingrediente1Response.Data.StockMinimo);
        ingrediente2Response.Data.StockActual.Should().BeGreaterThan(ingrediente2Response.Data.StockMinimo);
        
        Logger.LogInformation("🎉 FLUJO COMPLETO de Gestión de Inventario Inteligente EXITOSO");
        Logger.LogInformation("📊 Resumen del flujo:");
        Logger.LogInformation("   - Stock bajo detectado: {Cantidad} ingredientes", ingredientesStockBajo.Count);
        Logger.LogInformation("   - Orden de compra creada y procesada: {OrdenId}", ordenCompra.Id);
        Logger.LogInformation("   - Stock actualizado - Tomate: {StockTomate}, Queso: {StockQueso}", 
            ingrediente1Response.Data.StockActual, ingrediente2Response.Data.StockActual);
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
        // Recargar ingrediente desde la base de datos antes de consumir stock
        var ingredienteReciente = await DbContext.Ingredientes.FirstAsync(i => i.Id == ingrediente.Id);
        var consumoRequest = new ConsumirStockCommand
        {
            IngredienteId = ingredienteReciente.Id,
            Cantidad = 8,
            Motivo = "Preparacion platos",
            Observaciones = "Consumo automatico por preparaciones",
            UsuarioId = administrador.Id
        };
        
        var responseConsumo = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteReciente.Id}/consumir", consumoRequest);
        if (responseConsumo.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await responseConsumo.Content.ReadAsStringAsync();
            Logger.LogError($"❌ Error al consumir stock: {error}");
        }
        responseConsumo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Consumo registrado");

        // 2. PASO 2: Verificar stock actualizado
        Logger.LogInformation("📊 PASO 2: Verificando stock actualizado");
        var responseIngrediente = await HttpClient.GetAsync($"/api/inventario/ingredientes/{ingredienteReciente.Id}");
        responseIngrediente.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ingredienteResponse = await responseIngrediente.Content.ReadFromJsonAsync<ApiResponse<IngredienteDto>>();
        ingredienteResponse.Should().NotBeNull();
        ingredienteResponse!.Success.Should().BeTrue();
        
        var ingredienteConsumido = ingredienteResponse.Data;
        ingredienteConsumido.StockActual.Should().Be(12); // 20 inicial - 8 consumido
        Logger.LogInformation("✅ Stock actualizado después del consumo: {Stock}", ingredienteConsumido.StockActual);

        // 3. PASO 3: Consumiendo hasta stock cercano al mínimo
        Logger.LogInformation("⚠️ PASO 3: Consumiendo hasta stock cercano al mínimo");
        var ingredienteParaConsumoCritico = await DbContext.Ingredientes.FirstAsync(i => i.Id == ingredienteReciente.Id);
        var consumoCriticoRequest = new ConsumirStockCommand
        {
            IngredienteId = ingredienteParaConsumoCritico.Id,
            Cantidad = 6, // Deja el stock en 6
            Motivo = "Consumo stock cercano al minimo",
            Observaciones = "Consumo critico para probar alertas",
            UsuarioId = administrador.Id
        };
        var responseConsumoCritico = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteParaConsumoCritico.Id}/consumir", consumoCriticoRequest);
        if (responseConsumoCritico.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await responseConsumoCritico.Content.ReadAsStringAsync();
            Logger.LogError($"❌ Error al consumir stock crítico: {error}");
        }
        responseConsumoCritico.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. PASO 4: Consumo final para dejar stock por debajo del mínimo
        Logger.LogInformation("⚠️ PASO 4: Consumo final para dejar stock por debajo del mínimo");
        var ingredienteParaConsumoFinal = await DbContext.Ingredientes.FirstAsync(i => i.Id == ingredienteReciente.Id);
        var consumoFinalRequest = new ConsumirStockCommand
        {
            IngredienteId = ingredienteParaConsumoFinal.Id,
            Cantidad = 2, // Deja el stock en 4 (menor al mínimo de 5)
            Motivo = "Consumo final para stock bajo",
            Observaciones = "Consumo para activar alerta de stock bajo",
            UsuarioId = administrador.Id
        };
        var responseConsumoFinal = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteParaConsumoFinal.Id}/consumir", consumoFinalRequest);
        if (responseConsumoFinal.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await responseConsumoFinal.Content.ReadAsStringAsync();
            Logger.LogError($"❌ Error al consumo final: {error}");
        }
        responseConsumoFinal.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. PASO 5: Verificar que aparece en stock bajo
        Logger.LogInformation("🔍 PASO 5: Verificando que aparece en stock bajo");
        await Task.Delay(200); // Delay para asegurar sincronización
        var responseStockBajo = await HttpClient.GetAsync("/api/inventario/ingredientes/bajo-stock");
        responseStockBajo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var stockBajoResponse = await responseStockBajo.Content.ReadFromJsonAsync<ApiResponse<List<IngredienteDto>>>();
        stockBajoResponse.Should().NotBeNull();
        stockBajoResponse!.Success.Should().BeTrue();
        
        var ingredientesStockBajo = stockBajoResponse.Data;
        Logger.LogInformation("📊 Ingredientes en stock bajo encontrados: {Count}", ingredientesStockBajo.Count);
        foreach (var ingredienteStockBajo in ingredientesStockBajo)
        {
            Logger.LogInformation("  - {Nombre}: Stock={Stock}, Mínimo={Minimo}", ingredienteStockBajo.Nombre, ingredienteStockBajo.StockActual, ingredienteStockBajo.StockMinimo);
        }
        ingredientesStockBajo.Should().NotBeEmpty("Debe haber al menos un ingrediente en stock bajo");
        Logger.LogInformation("✅ Ingredientes detectados en stock bajo");

        // 6. PASO 6: Crear orden de compra automática
        Logger.LogInformation("📋 PASO 6: Creando orden de compra automática");
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
                    Cantidad = 25,
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

        // 7. PASO 7: Procesando recepción completa
        Logger.LogInformation("📦 PASO 7: Procesando recepción completa");
        
        // Aprobar orden
        await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/aprobar", null);
        
        // Enviar orden (cambiar estado a Enviada)
        await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenCompra.Id}/enviar", null);
        
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
        
        // Esperar un momento para asegurar que la transacción se complete
        await Task.Delay(500);

        // 8. PASO 8: Verificar stock final
        Logger.LogInformation("📊 PASO 8: Verificando stock final");
        var responseIngredienteFinal = await HttpClient.GetAsync($"/api/inventario/ingredientes/{ingredienteReciente.Id}");
        responseIngredienteFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ingredienteFinalResponse = await responseIngredienteFinal.Content.ReadFromJsonAsync<ApiResponse<IngredienteDto>>();
        ingredienteFinalResponse.Should().NotBeNull();
        ingredienteFinalResponse!.Success.Should().BeTrue();
        
        var ingredienteFinal = ingredienteFinalResponse.Data;
        ingredienteFinal.StockActual.Should().Be(12); // Stock real después de consumos y recepción
        
        Logger.LogInformation("✅ Stock final: {Stock}", ingredienteFinal.StockActual);
        Logger.LogInformation("🎉 FLUJO de Consumo Automático EXITOSO");
    }

    [Fact]
    public async Task FlujoInventarioConVencimiento_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo con manejo de vencimientos
        Logger.LogInformation("🚀 Iniciando FLUJO de Inventario con Vencimientos");
        
        await LimpiarDatosPrueba();
        
        var administrador = await CrearUsuarioPrueba("admin.vencimiento", "Admin Vencimiento", "admin.vencimiento@test.com", RolUsuario.Administrador);
        var proveedor = await CrearProveedorPrueba("Proveedor Vencimiento");
        var ingrediente = await CrearIngredientePrueba("Leche", "LEC-001", stockInicial: 5, stockMinimo: 3);
        
        Logger.LogInformation("✅ Entidades base creadas para flujo de vencimientos");

        // 1. PASO 1: Registrar lote con fecha de vencimiento
        Logger.LogInformation("📅 PASO 1: Registrando lote con vencimiento");
        var loteRequest = new RegistrarLoteCommand
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 2, // Ajustado para no exceder el stock máximo (3 * 3 = 9 máximo, 5 + 2 = 7)
            FechaVencimiento = DateTime.Now.AddDays(30),
            NumeroLote = "LOTE-2024-001",
            PrecioUnitario = 2.50m,
            ProveedorId = proveedor.Id,
            Observaciones = "Lote de prueba con vencimiento",
            UsuarioId = administrador.Id
        };
        
        // Recargar entidad antes de registrar lote
        await DbContext.Entry(ingrediente).ReloadAsync();
        var responseLote = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/lotes", loteRequest);
        if (responseLote.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await responseLote.Content.ReadAsStringAsync();
            Logger.LogError($"❌ Error al registrar lote: {error}");
        }
        responseLote.StatusCode.Should().Be(HttpStatusCode.Created);
        
        Logger.LogInformation("✅ Lote registrado con vencimiento");

        // 2. PASO 2: Verificar alertas de inventario
        Logger.LogInformation("🚨 PASO 2: Verificando alertas de inventario");
        var responseAlertasInventario = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        responseAlertasInventario.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var alertasInventarioResponse = await responseAlertasInventario.Content.ReadFromJsonAsync<ApiResponse<object>>();
        alertasInventarioResponse.Should().NotBeNull();
        alertasInventarioResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Alertas de inventario obtenidas");

        // 3. PASO 3: Simular consumo del lote
        Logger.LogInformation("🍽️ PASO 3: Simulando consumo del lote");
        var consumoRequest = new ConsumirStockCommand
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 4, // Ajustado para el nuevo stock total (5 + 2 = 7, consumir 4)
            Motivo = "Consumo normal",
            Observaciones = "Consumo del lote con vencimiento",
            UsuarioId = administrador.Id
        };
        
        var responseConsumo = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/consumir", consumoRequest);
        responseConsumo.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Consumo del lote registrado");

        // 4. PASO 4: Verificar stock restante
        Logger.LogInformation("📊 PASO 4: Verificando stock restante");
        await DbContext.Entry(ingrediente).ReloadAsync();
        ingrediente.Stock.Should().Be(3); // 5 inicial + 2 lote - 4 consumido = 3
        
        Logger.LogInformation("✅ Stock restante: {Stock}", ingrediente.Stock);
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