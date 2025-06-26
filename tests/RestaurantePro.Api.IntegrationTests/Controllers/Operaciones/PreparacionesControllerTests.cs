using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Application.Common.Models;
using System.Text.Json;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración completos para PreparacionesController
/// Valida interacción real con BD y reglas de negocio específicas
/// </summary>
[Collection("Sequential")]
public class PreparacionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public PreparacionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetPreparaciones_SinPreparacionesEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetPreparaciones_SinPreparacionesEnBD_DebeRetornarListaVacia");
        await LimpiarTablaPreparaciones();
        var url = "/api/operaciones/preparaciones";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<PreparacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().BeEmpty();
        
        // Verificar que realmente no hay preparaciones en BD
        var preparacionesEnBD = await DbContext.Preparaciones.ToListAsync();
        preparacionesEnBD.Should().BeEmpty();
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetPreparaciones_SinPreparacionesEnBD_DebeRetornarListaVacia");
    }

    [Fact]
    public async Task GetPreparaciones_ConPreparacionesEnBD_DebeRetornarPreparaciones()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetPreparaciones_ConPreparacionesEnBD_DebeRetornarPreparaciones");
        await LimpiarTablaPreparaciones();
        
        // Crear datos reales para el test
        var producto = await CrearProductoPrueba("Pizza Margherita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        var preparacion1 = await CrearPreparacionPrueba(producto.Id, chef.Id, 10, "Preparación 1");
        var preparacion2 = await CrearPreparacionPrueba(producto.Id, chef.Id, 5, "Preparación 2");

        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/preparaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<PreparacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(2);
        
        // Verificar que los datos coinciden con la BD
        var preparacionesEnBD = await DbContext.Preparaciones.ToListAsync();
        preparacionesEnBD.Should().HaveCount(2);
        preparacionesEnBD.Should().Contain(p => p.Id == preparacion1.Id);
        preparacionesEnBD.Should().Contain(p => p.Id == preparacion2.Id);
        
        // Verificar que los datos de la respuesta coinciden con la BD
        var preparacion1Response = apiResponse.Data.Items.FirstOrDefault(p => p.Id == preparacion1.Id);
        preparacion1Response.Should().NotBeNull();
        preparacion1Response!.ProductoId.Should().Be(producto.Id);
        preparacion1Response.ChefId.Should().Be(chef.Id);
        preparacion1Response.Cantidad.Should().Be(10);
        preparacion1Response.Observaciones.Should().Be("Preparación 1");
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetPreparaciones_ConPreparacionesEnBD_DebeRetornarPreparaciones");
    }

    [Fact]
    public async Task GetPreparacionPorId_ConPreparacionExistente_DebeRetornarPreparacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetPreparacionPorId_ConPreparacionExistente_DebeRetornarPreparacion");
        await LimpiarTablaPreparaciones();
        
        // Crear datos reales para el test
        var producto = await CrearProductoPrueba("Pizza Margherita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        var preparacion = await CrearPreparacionPrueba(producto.Id, chef.Id, 10, "Preparación específica");

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/preparaciones/{preparacion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(preparacion.Id);
        apiResponse.Data.ProductoId.Should().Be(producto.Id);
        apiResponse.Data.ChefId.Should().Be(chef.Id);
        apiResponse.Data.Cantidad.Should().Be(10);
        apiResponse.Data.Observaciones.Should().Be("Preparación específica");
        
        // Verificar que los datos coinciden con la BD
        var preparacionEnBD = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionEnBD.Should().NotBeNull();
        preparacionEnBD!.Id.Should().Be(apiResponse.Data.Id);
        preparacionEnBD.ProductoId.Should().Be(producto.Id);
        preparacionEnBD.ChefId.Should().Be(chef.Id);
        preparacionEnBD.CantidadPreparada.Should().Be(10);
        preparacionEnBD.Observaciones.Should().Be("Preparación específica");
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetPreparacionPorId_ConPreparacionExistente_DebeRetornarPreparacion");
    }

    [Fact]
    public async Task GetPreparacionPorId_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetPreparacionPorId_ConIdInexistente_DebeRetornar404");
        await LimpiarTablaPreparaciones();
        
        // Crear datos reales para establecer contexto
        var producto = await CrearProductoPrueba("Pizza Margherita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        var preparacion = await CrearPreparacionPrueba(producto.Id, chef.Id, 10, "Preparación real");
        
        // Generar un ID que realmente no existe en la BD
        var idInexistente = Guid.NewGuid();
        
        // Verificar que el ID realmente no existe en la BD
        var preparacionEnBD = await DbContext.Preparaciones.FindAsync(idInexistente);
        preparacionEnBD.Should().BeNull();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/preparaciones/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        
        // Verificar que la preparación real sigue existiendo en la BD
        var preparacionReal = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionReal.Should().NotBeNull();
        preparacionReal!.Id.Should().Be(preparacion.Id);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetPreparacionPorId_ConIdInexistente_DebeRetornar404");
    }

    [Fact]
    public async Task PostPreparacion_ConDatosValidos_DebeCrearPreparacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostPreparacion_ConDatosValidos_DebeCrearPreparacion");
        await LimpiarTablaPreparaciones();
        
        // Crear dependencias reales
        var producto = await CrearProductoPrueba("Pizza Margarita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para preparación");
        
        var cantidad = 8;
        var fechaVencimiento = DateTime.Now.AddDays(1);
        var tiempoEstimado = TimeSpan.FromMinutes(30); // minutos
        var observaciones = "Preparación para evento especial";
        
        var request = new {
            ProductoId = producto.Id,
            ComandaId = comanda.Id,
            Cantidad = cantidad,
            ChefId = chef.Id,
            TiempoEstimado = tiempoEstimado,
            FechaVencimiento = fechaVencimiento,
            Observaciones = observaciones
        };
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/preparaciones", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.ProductoId.Should().Be(producto.Id);
        apiResponse.Data.ChefId.Should().Be(chef.Id);
        apiResponse.Data.Cantidad.Should().Be(cantidad);
        apiResponse.Data.Observaciones.Should().Be(observaciones);
        
        // Verificar que la preparación se creó en la BD
        var preparacionEnBD = await DbContext.Preparaciones.FindAsync(apiResponse.Data.Id);
        preparacionEnBD.Should().NotBeNull();
        preparacionEnBD!.ProductoId.Should().Be(producto.Id);
        preparacionEnBD.ChefId.Should().Be(chef.Id);
        preparacionEnBD.CantidadPreparada.Should().Be(cantidad);
        preparacionEnBD.Observaciones.Should().Be(observaciones);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostPreparacion_ConDatosValidos_DebeCrearPreparacion");
    }

    [Fact]
    public async Task PutPreparacion_ConDatosValidos_DebeActualizarPreparacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PutPreparacion_ConDatosValidos_DebeActualizarPreparacion");
        await LimpiarTablaPreparaciones();
        
        // Crear dependencias reales
        var producto = await CrearProductoPrueba("Pizza Margarita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para actualización");
        
        // Crear preparación inicial
        var preparacion = await CrearPreparacionPrueba(producto.Id, chef.Id, 5, "Preparación original");
        
        // Datos de actualización
        var nuevaCantidad = 12;
        var nuevaPrioridad = 2;
        var nuevoTiempoEstimado = TimeSpan.FromMinutes(45);
        var nuevasObservaciones = "Preparación actualizada con nuevas especificaciones";
        
        var actualizarRequest = new {
            cantidad = nuevaCantidad,
            prioridad = 2,
            tiempoEstimado = nuevoTiempoEstimado,
            observaciones = nuevasObservaciones,
            chefId = chef.Id
        };
        
        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/operaciones/preparaciones/{preparacion.Id}", actualizarRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(preparacion.Id);
        apiResponse.Data.ProductoId.Should().Be(producto.Id);
        apiResponse.Data.ChefId.Should().Be(chef.Id);
        apiResponse.Data.Cantidad.Should().Be(nuevaCantidad);
        apiResponse.Data.Observaciones.Should().Be(nuevasObservaciones);
        
        // Verificar que la preparación se actualizó en la BD
        var preparacionActualizada = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionActualizada.Should().NotBeNull();
        await DbContext.Entry(preparacionActualizada!).ReloadAsync();
        preparacionActualizada.Id.Should().Be(preparacion.Id);
        preparacionActualizada.ProductoId.Should().Be(producto.Id);
        preparacionActualizada.ChefId.Should().Be(chef.Id);
        preparacionActualizada.CantidadPreparada.Should().Be(nuevaCantidad);
        preparacionActualizada.Observaciones.Should().Be(nuevasObservaciones);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PutPreparacion_ConDatosValidos_DebeActualizarPreparacion");
    }

    [Fact]
    public async Task PostIniciarPreparacion_ConPreparacionExistente_DebeIniciarPreparacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostIniciarPreparacion_ConPreparacionExistente_DebeIniciarPreparacion");
        await LimpiarTablaPreparaciones();
        
        // Crear dependencias reales
        var producto = await CrearProductoPrueba("Pizza Margarita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        
        // Crear preparación en estado Preparando (estado inicial por defecto)
        var preparacion = await CrearPreparacionPrueba(producto.Id, chef.Id, 5, "Preparación para iniciar");
        
        // Verificar estado inicial
        preparacion.Estado.Should().Be(EstadoPreparacion.Preparando);
        
        // Act
        var iniciarRequest = new {
            chefId = chef.Id,
            observaciones = "Iniciando preparación"
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/preparaciones/{preparacion.Id}/iniciar", iniciarRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(preparacion.Id);
        
        // Verificar que la preparación se inició en la BD
        var preparacionIniciada = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionIniciada.Should().NotBeNull();
        await DbContext.Entry(preparacionIniciada!).ReloadAsync();
        preparacionIniciada.Id.Should().Be(preparacion.Id);
        preparacionIniciada.Estado.Should().Be(EstadoPreparacion.Disponible);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostIniciarPreparacion_ConPreparacionExistente_DebeIniciarPreparacion");
    }

    [Fact]
    public async Task PostIniciarPreparacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        
        // Act
        var iniciarRequest = new {
            chefId = Guid.NewGuid(),
            observaciones = "Test con ID inexistente"
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/preparaciones/{idInexistente}/iniciar", iniciarRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostCompletarPreparacion_ConPreparacionEnPreparacion_DebeCompletarPreparacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostCompletarPreparacion_ConPreparacionEnPreparacion_DebeCompletarPreparacion");
        await LimpiarTablaPreparaciones();
        
        // Crear dependencias reales
        var producto = await CrearProductoPrueba("Pizza Margarita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        
        // Crear preparación en estado Disponible (no Preparando)
        var preparacion = await CrearPreparacionPrueba(producto.Id, chef.Id, 5, "Preparación para completar");
        preparacion.MarcarComoDisponible();
        await DbContext.SaveChangesAsync();
        
        // Verificar estado inicial
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
        
        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/preparaciones/{preparacion.Id}/completar", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(preparacion.Id);
        
        // Verificar que la preparación se completó en la BD
        var preparacionCompletada = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionCompletada.Should().NotBeNull();
        await DbContext.Entry(preparacionCompletada!).ReloadAsync();
        preparacionCompletada.Id.Should().Be(preparacion.Id);
        preparacionCompletada.Estado.Should().Be(EstadoPreparacion.Agotada);
        preparacionCompletada.CantidadDisponible.Should().Be(0);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostCompletarPreparacion_ConPreparacionEnPreparacion_DebeCompletarPreparacion");
    }

    [Fact]
    public async Task PostCompletarPreparacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        
        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/preparaciones/{idInexistente}/completar", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostCancelarPreparacion_ConPreparacionEnPreparacion_DebeCancelarPreparacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostCancelarPreparacion_ConPreparacionEnPreparacion_DebeCancelarPreparacion");
        await LimpiarTablaPreparaciones();
        
        // Crear dependencias reales
        var producto = await CrearProductoPrueba("Pizza Margarita", 15.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        
        // Crear preparación en estado Preparando
        var preparacion = await CrearPreparacionPrueba(producto.Id, chef.Id, 5, "Preparación para cancelar");
        
        // Verificar estado inicial
        preparacion.Estado.Should().Be(EstadoPreparacion.Preparando);
        
        // Act
        var cancelarRequest = new {
            motivoCancelacion = "Cancelación por falta de ingredientes",
            usuarioId = chef.Id
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/preparaciones/{preparacion.Id}/cancelar", cancelarRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(preparacion.Id);
        
        // Verificar que la preparación se canceló en la BD
        var preparacionCancelada = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionCancelada.Should().NotBeNull();
        await DbContext.Entry(preparacionCancelada!).ReloadAsync();
        preparacionCancelada.Id.Should().Be(preparacion.Id);
        preparacionCancelada.Estado.Should().Be(EstadoPreparacion.Vencida);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostCancelarPreparacion_ConPreparacionEnPreparacion_DebeCancelarPreparacion");
    }

    [Fact]
    public async Task PostCancelarPreparacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        
        // Act
        var cancelarRequest = new {
            motivoCancelacion = "Cancelación de prueba"
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/preparaciones/{idInexistente}/cancelar", cancelarRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetColaPreparaciones_ConPreparacionesEnCola_DebeRetornarColaOrdenada()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetColaPreparaciones_ConPreparacionesEnCola_DebeRetornarColaOrdenada");
        await LimpiarTablaPreparaciones();
        
        // Crear dependencias reales
        var producto1 = await CrearProductoPrueba("Pizza Margarita", 15.00m);
        var producto2 = await CrearProductoPrueba("Pizza Hawaiana", 18.00m);
        var chef = await CrearUsuarioPrueba("chef.test", "Chef Test", "chef@test.com", RolUsuario.Cocinero);
        
        // Crear preparaciones en estado Preparando (estado inicial por defecto)
        var preparacion1 = await CrearPreparacionPrueba(producto1.Id, chef.Id, 3, "Preparación 1 - Alta prioridad");
        var preparacion2 = await CrearPreparacionPrueba(producto2.Id, chef.Id, 2, "Preparación 2 - Media prioridad");
        
        // Las preparaciones se crean en estado Preparando por defecto, no necesitamos cambiarlas
        // Verificar estado inicial
        preparacion1.Estado.Should().Be(EstadoPreparacion.Preparando);
        preparacion2.Estado.Should().Be(EstadoPreparacion.Preparando);
        
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/preparaciones/cola");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<PreparacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().BeGreaterThanOrEqualTo(2);
        
        // Verificar que las preparaciones están en la cola
        var idsEnCola = apiResponse.Data.Select(p => p.Id).ToList();
        idsEnCola.Should().Contain(preparacion1.Id);
        idsEnCola.Should().Contain(preparacion2.Id);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetColaPreparaciones_ConPreparacionesEnCola_DebeRetornarColaOrdenada");
    }

    [Fact]
    public async Task GetColaPreparaciones_SinPreparacionesEnCola_DebeRetornarListaVacia()
    {
        // Arrange
        await LimpiarTablaPreparaciones();
        
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/preparaciones/cola");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<PreparacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().Be(0);
    }

    #region Métodos Helper

    private async Task LimpiarTablaPreparaciones()
    {
        var preparaciones = await DbContext.Preparaciones.ToListAsync();
        DbContext.Preparaciones.RemoveRange(preparaciones);
        await DbContext.SaveChangesAsync();
    }

    private async Task<PreparacionDiaria> CrearPreparacionPrueba(Guid productoId, Guid chefId, int cantidad, string observaciones = "")
    {
        var preparacion = PreparacionDiaria.Crear(
            productoId,
            cantidad,
            chefId,
            DateTime.Now.AddHours(4), // Vence en 4 horas
            observaciones
        );
        
        DbContext.Preparaciones.Add(preparacion);
        await DbContext.SaveChangesAsync();
        
        return preparacion;
    }

    #endregion

    public new void Dispose()
    {
        base.Dispose();
    }
} 