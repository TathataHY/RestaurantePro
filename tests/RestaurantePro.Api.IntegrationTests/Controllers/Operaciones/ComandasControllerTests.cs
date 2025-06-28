using Microsoft.Extensions.Logging;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Operaciones;
using System.Net;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasPaginadas;
using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;
using RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.EliminarComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CambiarEstadoComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProducto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.RemoverProducto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AplicarDescuento;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;
using RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;
using RestaurantePro.Application.Operaciones.Comandas.Commands.ProcesarPedidoCompleto;
using RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;
using RestaurantePro.Application.Operaciones.Commands.FinalizarServicioCompleto;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración para ComandasController
/// Valida todos los endpoints REST del controlador de comandas del restaurante
/// Tests COMPLETOS con interacción real de base de datos
/// </summary>
[Collection("Sequential")]
public class ComandasControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public ComandasControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetComandas_SinComandasEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetComandas_SinComandasEnBD_DebeRetornarListaVacia");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/comandas");

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ComandaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Items.Should().BeEmpty();
        apiResponse.Data.TotalCount.Should().Be(0);
        
        // Verificar que realmente no hay comandas en BD
        var comandasEnBD = await DbContext.Comandas.ToListAsync();
        comandasEnBD.Should().BeEmpty();
        
        Logger.LogInformation("✅ Test completado: GetComandas_SinComandasEnBD_DebeRetornarListaVacia");
    }

    [Fact]
    public async Task GetComandas_ConComandasEnBD_DebeRetornarComandas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetComandas_ConComandasEnBD_DebeRetornarComandas");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda1 = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda 1");
        var comanda2 = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda 2");

        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/comandas");

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ComandaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Items.Should().HaveCount(2);
        apiResponse.Data.TotalCount.Should().Be(2);
        
        // Verificar que los datos coinciden con la BD
        var comandasEnBD = await DbContext.Comandas.ToListAsync();
        comandasEnBD.Should().HaveCount(2);
        comandasEnBD.Should().Contain(c => c.Id == comanda1.Id);
        comandasEnBD.Should().Contain(c => c.Id == comanda2.Id);
        
        // Verificar que los datos de la respuesta coinciden con la BD
        var comanda1Response = apiResponse.Data.Items.FirstOrDefault(c => c.Id == comanda1.Id);
        comanda1Response.Should().NotBeNull();
        comanda1Response!.ClienteId.Should().Be(cliente.Id);
        comanda1Response.MesaId.Should().Be(mesa.Id);
        comanda1Response.UsuarioId.Should().Be(mesero.Id);
        
        Logger.LogInformation("✅ Test completado: GetComandas_ConComandasEnBD_DebeRetornarComandas");
    }

    [Fact]
    public async Task PostComanda_ConDatosValidos_DebeCrearComanda()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostComanda_ConDatosValidos_DebeCrearComanda");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var mesa = await CrearMesaPrueba(1, 4);
        var producto = await CrearProductoPrueba("Pizza Margherita", 15.00m);
        
        var comandaRequest = new ComandaTestDataBuilder()
            .ConMesero(mesero.Id)
            .ConCliente(cliente.Id)
            .ConMesa(mesa.Id)
            .ConObservaciones("Mesa cerca de la ventana")
            .ConProducto(producto.Id, 2, "Sin cebolla")
            .BuildCrearComandaRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequest);

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().NotBeEmpty();
        apiResponse.Data.ClienteId.Should().Be(cliente.Id);
        apiResponse.Data.MesaId.Should().Be(mesa.Id);
        apiResponse.Data.UsuarioId.Should().Be(mesero.Id);
        apiResponse.Data.Estado.Should().Be(EstadoComanda.Creada);
        apiResponse.Data.Observaciones.Should().Be("Mesa cerca de la ventana");
        
        // Verificar que se creó en la BD
        var comandasEnBD = await DbContext.Comandas.ToListAsync();
        comandasEnBD.Should().HaveCount(1);
        
        var comandaCreada = comandasEnBD[0];
        comandaCreada.Id.Should().Be(apiResponse.Data.Id);
        comandaCreada.MeseroId.Should().Be(mesero.Id);
        comandaCreada.ClienteId.Should().Be(cliente.Id);
        comandaCreada.MesaId.Should().Be(mesa.Id);
        comandaCreada.Estado.Should().Be(EstadoComanda.Creada);
        comandaCreada.Observaciones.Should().Be("Mesa cerca de la ventana");
        
        // Verificar que se crearon los detalles de la comanda
        var detallesEnBD = await DbContext.ItemsComanda.Where(d => d.ComandaId == comandaCreada.Id).ToListAsync();
        detallesEnBD.Should().HaveCount(1);
        detallesEnBD[0].ProductoId.Should().Be(producto.Id);
        detallesEnBD[0].Cantidad.Should().Be(2);
        detallesEnBD[0].Observaciones.Should().Be("Sin cebolla");
        
        Logger.LogInformation("✅ Test completado: PostComanda_ConDatosValidos_DebeCrearComanda");
    }

    [Fact]
    public async Task PostComanda_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostComanda_ConDatosInvalidos_DebeRetornar400");
        
        var comandaRequest = new ComandaTestDataBuilder()
            .ConCliente(Guid.Empty) // ID inválido
            .ConMesa(Guid.Empty)    // ID inválido
            .BuildCrearComandaRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequest);

        // Assert - Validación estricta para tests completos
        // Como estamos usando datos inválidos (Guid.Empty), debería devolver 400 BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Success.Should().BeFalse();
        errorResponse.Errors.Should().NotBeEmpty();
        // Verificar que contiene errores relacionados con datos inválidos
        errorResponse.Errors.Should().Contain(e => e.Contains("obligatorio") || e.Contains("inválido") || e.Contains("requerido"));
        
        // Verificar que no se creó nada en BD
        var comandasEnBD = await DbContext.Comandas.ToListAsync();
        comandasEnBD.Should().BeEmpty();
        
        Logger.LogInformation("✅ Test completado: PostComanda_ConDatosInvalidos_DebeRetornar400");
    }

    [Fact]
    public async Task GetComanda_ConIdExistente_DebeRetornarComanda()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetComanda_ConIdExistente_DebeRetornarComanda");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda específica");

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/comandas/{comanda.Id}");

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        apiResponse.Data.ClienteId.Should().Be(cliente.Id);
        apiResponse.Data.MesaId.Should().Be(mesa.Id);
        apiResponse.Data.UsuarioId.Should().Be(mesero.Id);
        apiResponse.Data.Estado.Should().Be(EstadoComanda.Creada);
        apiResponse.Data.Observaciones.Should().Be("Comanda específica");
        
        // Verificar que los datos coinciden con la BD
        var comandaEnBD = await DbContext.Comandas.FirstOrDefaultAsync(c => c.Id == comanda.Id);
        comandaEnBD.Should().NotBeNull();
        comandaEnBD!.Id.Should().Be(apiResponse.Data.Id);
        comandaEnBD.MeseroId.Should().Be(mesero.Id);
        comandaEnBD.ClienteId.Should().Be(cliente.Id);
        comandaEnBD.MesaId.Should().Be(mesa.Id);
        
        Logger.LogInformation("✅ Test completado: GetComanda_ConIdExistente_DebeRetornarComanda");
    }

    [Fact]
    public async Task GetComanda_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetComanda_ConIdInexistente_DebeRetornar404");
        
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/comandas/{idInexistente}");

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Success.Should().BeFalse();
        errorResponse.Errors.Should().NotBeEmpty();
        
        Logger.LogInformation("✅ Test completado: GetComanda_ConIdInexistente_DebeRetornar404");
    }

    [Fact]
    public async Task PutComanda_ConDatosValidos_DebeActualizarComanda()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PutComanda_ConDatosValidos_DebeActualizarComanda");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda original");
        
        var actualizarRequest = new ActualizarComandaCommand
        {
            Id = comanda.Id,
            Observaciones = "Comanda actualizada con nuevas observaciones"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}", actualizarRequest);

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        apiResponse.Data.Observaciones.Should().Be("Comanda actualizada con nuevas observaciones");
        
        // Verificar que se actualizó en la BD
        var comandaActualizada = await DbContext.Comandas.FirstOrDefaultAsync(c => c.Id == comanda.Id);
        comandaActualizada.Should().NotBeNull();
        await DbContext.Entry(comandaActualizada!).ReloadAsync();
        comandaActualizada!.Observaciones.Should().Be("Comanda actualizada con nuevas observaciones");
        
        Logger.LogInformation("✅ Test completado: PutComanda_ConDatosValidos_DebeActualizarComanda");
    }

    [Fact]
    public async Task CambiarEstadoComanda_ConEstadoValido_DebeActualizarEstado()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: CambiarEstadoComanda_ConEstadoValido_DebeActualizarEstado");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", $"mesero_{Guid.NewGuid().ToString("N")[..8]}@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba($"Cliente_{Guid.NewGuid().ToString("N")[..8]}", GenerarEmailValido());
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para cambiar estado");
        
        var cambiarEstadoRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "EnProceso"
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", cambiarEstadoRequest);

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        apiResponse.Data.Estado.Should().Be(EstadoComanda.EnProceso);
        
        // Verificar que se actualizó en la BD
        var comandaActualizada = await DbContext.Comandas.FirstOrDefaultAsync(c => c.Id == comanda.Id);
        comandaActualizada.Should().NotBeNull();
        await DbContext.Entry(comandaActualizada!).ReloadAsync();
        comandaActualizada!.Estado.Should().Be(EstadoComanda.EnProceso);
        
        Logger.LogInformation("✅ Test completado: CambiarEstadoComanda_ConEstadoValido_DebeActualizarEstado");
    }

    [Fact]
    public async Task AgregarProducto_ConProductoValido_DebeAgregarProducto()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: AgregarProducto_ConProductoValido_DebeAgregarProducto");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para agregar producto");
        var producto = await CrearProductoPrueba("Hamburguesa", 12.50m);
        
        var agregarProductoRequest = new AgregarProductoCommand
        {
            ComandaId = comanda.Id,
            ProductoId = producto.Id,
            Cantidad = 2,
            Observaciones = "Sin tomate"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/productos", agregarProductoRequest);

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        apiResponse.Data.Items.Should().HaveCount(1);
        apiResponse.Data.Items[0].ProductoId.Should().Be(producto.Id);
        apiResponse.Data.Items[0].Cantidad.Should().Be(2);
        apiResponse.Data.Items[0].Observaciones.Should().Be("Sin tomate");
        
        // Verificar que se agregó en la BD
        var detallesEnBD = await DbContext.ItemsComanda.Where(d => d.ComandaId == comanda.Id).ToListAsync();
        detallesEnBD.Should().HaveCount(1);
        detallesEnBD[0].ProductoId.Should().Be(producto.Id);
        detallesEnBD[0].Cantidad.Should().Be(2);
        detallesEnBD[0].Observaciones.Should().Be("Sin tomate");
        
        Logger.LogInformation("✅ Test completado: AgregarProducto_ConProductoValido_DebeAgregarProducto");
    }

    [Fact]
    public async Task RemoverProducto_ConDetalleExistente_DebeRemoverProducto()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: RemoverProducto_ConDetalleExistente_DebeRemoverProducto");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para remover producto");
        var producto = await CrearProductoPrueba("Pizza", 18.00m);
        
        // Agregar producto primero usando el endpoint de la API
        var agregarProductoRequest = new AgregarProductoCommand
        {
            ComandaId = comanda.Id,
            ProductoId = producto.Id,
            Cantidad = 1,
            Observaciones = "Extra queso"
        };
        var responseAgregar = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/productos", agregarProductoRequest);
        responseAgregar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Obtener el detalle creado desde la BD
        var detallesEnBD = await DbContext.ItemsComanda.Where(d => d.ComandaId == comanda.Id).ToListAsync();
        detallesEnBD.Should().HaveCount(1);
        var detalle = detallesEnBD.First();
        
        // Verificar que el detalle existe en la BD
        var detalleEnBD = await DbContext.ItemsComanda.FindAsync(detalle.Id);
        Logger.LogInformation("💾 Detalle en BD - Existe: {Existe}, ID: {DetalleId}", 
            detalleEnBD != null, detalleEnBD?.Id);
        
        // Verificar que la comanda tiene el item
        var comandaConItems = await DbContext.Comandas.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == comanda.Id);
        Logger.LogInformation("📋 Comanda con items - ItemsCount: {ItemsCount}", comandaConItems?.Items.Count ?? 0);
        
        // Act
        Logger.LogInformation("🚀 Llamando al endpoint DELETE: /api/operaciones/comandas/{ComandaId}/productos/{DetalleId}", comanda.Id, detalle.Id);
        var response = await HttpClient.DeleteAsync($"/api/operaciones/comandas/{comanda.Id}/productos/{detalle.Id}");

        // Assert - Validación estricta para tests completos
        Logger.LogInformation("📥 Response Status: {StatusCode}", response.StatusCode);
        
        // Log temporal para debug
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.LogError("❌ ERROR RESPONSE - Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            Console.WriteLine($"=== ERROR RESPONSE ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Content: {errorContent}");
            Console.WriteLine($"======================");
        }
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        apiResponse.Data.Items.Should().BeEmpty();
        
        // Verificar que se removió de la BD
        var detallesEnBDRemovidos = await DbContext.ItemsComanda.Where(d => d.ComandaId == comanda.Id).ToListAsync();
        detallesEnBDRemovidos.Should().BeEmpty();
        
        Logger.LogInformation("✅ Test completado: RemoverProducto_ConDetalleExistente_DebeRemoverProducto");
    }

    [Fact]
    public async Task AplicarDescuento_ConDescuentoValido_DebeAplicarDescuento()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: AplicarDescuento_ConDescuentoValido_DebeAplicarDescuento");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", $"mes_{Guid.NewGuid().ToString("N")[..8]}@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba($"Cliente_{Guid.NewGuid().ToString("N")[..8]}", GenerarEmailValido());
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para aplicar descuento");
        var producto = await CrearProductoPrueba("Pasta", 16.00m);
        
        // Agregar producto correctamente a la comanda usando el método del dominio
        comanda.AgregarItem(producto.Id, producto.Nombre, 2, producto.Precio!.Valor, "Al dente");
        await DbContext.SaveChangesAsync(); // Guardar los cambios para que se calcule el total
        
        var aplicarDescuentoRequest = new AplicarDescuentoCommand
        {
            ComandaId = comanda.Id,
            PorcentajeDescuento = 31.25m,
            Motivo = "Cliente frecuente"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/descuento", aplicarDescuentoRequest);

        // Assert - Validación estricta para tests completos
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation("🔍 Response Status: {StatusCode}", response.StatusCode);
        Logger.LogInformation("🔍 Response Content: {Content}", content);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError("❌ Test falló con status {StatusCode}. Contenido: {Content}", response.StatusCode, content);
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        apiResponse.Data.Descuentos.Should().BeGreaterThan(0);
        
        // Verificar que se aplicó el descuento en la BD
        var comandaActualizada = await DbContext.Comandas.FirstOrDefaultAsync(c => c.Id == comanda.Id);
        comandaActualizada.Should().NotBeNull();
        // Nota: La verificación del descuento en BD dependerá de cómo se implemente en la entidad
        
        Logger.LogInformation("✅ Test completado: AplicarDescuento_ConDescuentoValido_DebeAplicarDescuento");
    }

    [Fact]
    public async Task CerrarComanda_ConDatosValidos_DebeCerrarComanda()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: CerrarComanda_ConDatosValidos_DebeCerrarComanda");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", $"mesero_{Guid.NewGuid().ToString("N")[..8]}@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba($"Cliente_{Guid.NewGuid().ToString("N")[..8]}", GenerarEmailValido());
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para cerrar");
        var producto = await CrearProductoPrueba("Ensalada", 8.50m);
        
        // Agregar producto usando el endpoint de la API
        var agregarProductoRequest = new
        {
            ProductoId = producto.Id,
            Cantidad = 1,
            Observaciones = "Sin aderezo"
        };
        var responseAgregar = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/productos", agregarProductoRequest);
        responseAgregar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Cerrar la comanda
        var cerrarComandaRequest = new CerrarComandaCommand
        {
            ComandaId = comanda.Id,
            MetodoPago = "Efectivo",
            MontoPagado = 8.50m
        };

        // Act
        Logger.LogInformation("🚀 Llamando al endpoint de cierre: POST /api/operaciones/comandas/{ComandaId}/cerrar", comanda.Id);
        Logger.LogInformation("📋 Request data: {RequestData}", System.Text.Json.JsonSerializer.Serialize(cerrarComandaRequest));
        
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/cerrar", cerrarComandaRequest);

        // Assert - Validación estricta para tests completos
        Logger.LogInformation("📥 Response Status: {StatusCode}", response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation("📄 Response Content: {Content}", content);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError("❌ Test falló con status {StatusCode}. Contenido: {Content}", response.StatusCode, content);
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(comanda.Id);
        Logger.LogInformation("🎯 Estado en respuesta API: {Estado}", apiResponse.Data.Estado);
        apiResponse.Data.Estado.Should().Be(EstadoComanda.Finalizada);
        
        // Verificar que se cerró en la BD
        var comandaCerrada = await DbContext.Comandas.FirstOrDefaultAsync(c => c.Id == comanda.Id);
        comandaCerrada.Should().NotBeNull();
        await DbContext.Entry(comandaCerrada!).ReloadAsync();
        Logger.LogInformation("💾 Estado en BD (refrescado): {Estado}", comandaCerrada!.Estado);
        comandaCerrada!.Estado.Should().Be(EstadoComanda.Finalizada);
        
        Logger.LogInformation("✅ Test completado: CerrarComanda_ConDatosValidos_DebeCerrarComanda");
    }

    [Fact]
    public async Task EliminarComanda_ConIdExistente_DebeEliminarComanda()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: EliminarComanda_ConIdExistente_DebeEliminarComanda");
        
        // Limpiar BD antes del test
        await LimpiarTablaComandas();
        
        var mesero = await CrearUsuarioPrueba("mesero.test", "Mesero Test", $"mesero_{Guid.NewGuid().ToString("N")[..8]}@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePrueba($"Cliente_{Guid.NewGuid().ToString("N")[..8]}", $"cli_{Guid.NewGuid().ToString("N")[..8]}@test.com");
        var mesa = await CrearMesaPrueba(1, 4);
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, observaciones: "Comanda para eliminar");

        // Act
        var response = await HttpClient.DeleteAsync($"/api/operaciones/comandas/{comanda.Id}");

        // Assert - Validación estricta para tests completos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
        
        // Verificar que se eliminó de la BD (soft delete)
        var comandaEliminada = await DbContext.Comandas.FirstOrDefaultAsync(c => c.Id == comanda.Id);
        comandaEliminada.Should().NotBeNull();
        await DbContext.Entry(comandaEliminada!).ReloadAsync();
        comandaEliminada!.EstaEliminado.Should().BeTrue();
        
        Logger.LogInformation("✅ Test completado: EliminarComanda_ConIdExistente_DebeEliminarComanda");
    }

    private async Task LimpiarTablaComandas()
    {
        try
        {
            // Limpiar items de comanda primero (por FK)
            var itemsComanda = await DbContext.ItemsComanda.ToListAsync();
            DbContext.ItemsComanda.RemoveRange(itemsComanda);
            
            // Limpiar comandas
            var comandas = await DbContext.Comandas.ToListAsync();
            DbContext.Comandas.RemoveRange(comandas);
            
            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning("⚠️ Error al limpiar tabla comandas: {Message}", ex.Message);
        }
    }

    public new void Dispose()
    {
        base.Dispose();
    }
} 