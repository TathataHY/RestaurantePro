using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Application.Common.Models;
using System.Text.Json;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.AprobarOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RechazarOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.ActualizarOrdenCompra;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración completos para OrdenesCompraController
/// Valida interacción real con BD y reglas de negocio específicas
/// </summary>
[Collection("Sequential")]
public class OrdenesCompraControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public OrdenesCompraControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetOrdenesCompra_SinOrdenesEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetOrdenesCompra_SinOrdenesEnBD_DebeRetornarListaVacia");
        await LimpiarTablaOrdenesCompra();
        var url = "/api/inventario/ordenes-compra";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<OrdenCompraDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().BeEmpty();
        
        // Verificar que realmente no hay órdenes en BD
        var ordenesEnBD = await DbContext.OrdenesCompra.ToListAsync();
        ordenesEnBD.Should().BeEmpty();
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetOrdenesCompra_SinOrdenesEnBD_DebeRetornarListaVacia");
    }

    [Fact]
    public async Task GetOrdenesCompra_ConOrdenesEnBD_DebeRetornarOrdenes()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetOrdenesCompra_ConOrdenesEnBD_DebeRetornarOrdenes");
        await LimpiarTablaOrdenesCompra();
        
        // Crear datos reales para el test
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        var usuario = await CrearUsuarioPrueba("usuario.test", "Usuario Test", "usuario@test.com", RolUsuario.Administrador);
        
        var orden1 = await CrearOrdenCompraPrueba(proveedor.Id, "Orden 1");
        var orden2 = await CrearOrdenCompraPrueba(proveedor.Id, "Orden 2");

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ordenes-compra");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<OrdenCompraDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(2);
        
        // Verificar que los datos coinciden con la BD
        var ordenesEnBD = await DbContext.OrdenesCompra.ToListAsync();
        ordenesEnBD.Should().HaveCount(2);
        ordenesEnBD.Should().Contain(o => o.Id == orden1.Id);
        ordenesEnBD.Should().Contain(o => o.Id == orden2.Id);
        
        // Verificar que los datos de la respuesta coinciden con la BD
        var orden1Response = apiResponse.Data.Items.FirstOrDefault(o => o.Id == orden1.Id);
        orden1Response.Should().NotBeNull();
        orden1Response!.ProveedorId.Should().Be(proveedor.Id);
        orden1Response.Observaciones.Should().Be("Orden 1");
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetOrdenesCompra_ConOrdenesEnBD_DebeRetornarOrdenes");
    }

    [Fact]
    public async Task GetOrdenCompraPorId_ConOrdenExistente_DebeRetornarOrden()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetOrdenCompraPorId_ConOrdenExistente_DebeRetornarOrden");
        await LimpiarTablaOrdenesCompra();
        
        // Crear datos reales para el test
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        var usuario = await CrearUsuarioPrueba("usuario.test", "Usuario Test", "usuario@test.com", RolUsuario.Administrador);
        var orden = await CrearOrdenCompraPrueba(proveedor.Id, "Orden específica");

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/ordenes-compra/{orden.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(orden.Id);
        apiResponse.Data.ProveedorId.Should().Be(proveedor.Id);
        apiResponse.Data.Observaciones.Should().Be("Orden específica");
        
        // Verificar que los datos coinciden con la BD
        var ordenEnBD = await DbContext.OrdenesCompra.FindAsync(orden.Id);
        ordenEnBD.Should().NotBeNull();
        ordenEnBD!.Id.Should().Be(apiResponse.Data.Id);
        ordenEnBD.ProveedorId.Should().Be(proveedor.Id);
        ordenEnBD.Observaciones.Should().Be("Orden específica");
        
        // Verificar RowVersion solo si el proveedor no es SQLite
        var providerName = DbContext.Database.ProviderName;
        if (!providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            ordenEnBD!.RowVersion.Should().NotBeNull();
        }
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetOrdenCompraPorId_ConOrdenExistente_DebeRetornarOrden");
    }

    [Fact]
    public async Task GetOrdenCompraPorId_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetOrdenCompraPorId_ConIdInexistente_DebeRetornar404");
        await LimpiarTablaOrdenesCompra();
        
        // Crear datos reales para establecer contexto
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        var usuario = await CrearUsuarioPrueba("usuario.test", "Usuario Test", "usuario@test.com", RolUsuario.Administrador);
        var orden = await CrearOrdenCompraPrueba(proveedor.Id, "Orden real");
        
        // Generar un ID que realmente no existe en la BD
        var idInexistente = Guid.NewGuid();
        
        // Verificar que el ID realmente no existe en la BD
        var ordenEnBD = await DbContext.OrdenesCompra.FindAsync(idInexistente);
        ordenEnBD.Should().BeNull();

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/ordenes-compra/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        
        // Verificar que la orden real sigue existiendo en la BD
        var ordenReal = await DbContext.OrdenesCompra.FindAsync(orden.Id);
        ordenReal.Should().NotBeNull();
        ordenReal!.Id.Should().Be(orden.Id);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetOrdenCompraPorId_ConIdInexistente_DebeRetornar404");
    }

    [Fact]
    public async Task PostOrdenCompra_ConDatosValidos_DebeCrearOrden()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostOrdenCompra_ConDatosValidos_DebeCrearOrden");
        await LimpiarTablaOrdenesCompra();
        
        // Crear dependencias reales
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        var usuario = await CrearUsuarioPrueba("usuario.test", "Usuario Test", "usuario@test.com", RolUsuario.Administrador);
        
        var fechaEntrega = DateTime.Today.AddDays(7); // Fecha futura válida
        var observaciones = "Orden de compra para evento especial";
        
        var request = new {
            ProveedorId = proveedor.Id,
            UsuarioId = usuario.Id,
            FechaEntregaEsperada = fechaEntrega,
            Observaciones = observaciones,
            Items = new[] {
                new {
                    IngredienteId = ingrediente.Id,
                    Cantidad = 50,
                    PrecioUnitario = 5.00m,
                    Observaciones = "Item de prueba"
                }
            }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/inventario/ordenes-compra", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().NotBeEmpty();
        apiResponse.Data.ProveedorId.Should().Be(proveedor.Id);
        apiResponse.Data.Observaciones.Should().Be(observaciones);
        apiResponse.Data.Estado.Should().Be(EstadoOrdenCompra.Pendiente);
        
        // Verificar que la orden se creó en la BD
        var ordenCreada = await DbContext.OrdenesCompra.FindAsync(apiResponse.Data.Id);
        ordenCreada.Should().NotBeNull();
        await DbContext.Entry(ordenCreada!).ReloadAsync();
        ordenCreada.Id.Should().Be(apiResponse.Data.Id);
        ordenCreada.ProveedorId.Should().Be(proveedor.Id);
        ordenCreada.Observaciones.Should().Be(observaciones);
        ordenCreada.Estado.Should().Be(EstadoOrdenCompra.Pendiente);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostOrdenCompra_ConDatosValidos_DebeCrearOrden");
    }

    [Fact]
    public async Task PutOrdenCompra_ConDatosValidos_DebeActualizarOrden()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PutOrdenCompra_ConDatosValidos_DebeActualizarOrden");
        await LimpiarTablaOrdenesCompra();
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var orden = await CrearOrdenCompraPrueba(proveedor.Id, "Orden original", usuario.Id, estado: "Pendiente");

        var ordenEnBD = await DbContext.OrdenesCompra.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orden.Id);
        ordenEnBD.Should().NotBeNull();
        
        // Verificar RowVersion solo si el proveedor no es SQLite
        var providerName = DbContext.Database.ProviderName;
        if (!providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            ordenEnBD!.RowVersion.Should().NotBeNull();
        }

        var nuevaFechaEntrega = DateTime.Now.AddDays(10); // Fecha futura válida
        var command = new ActualizarOrdenCompraCommand
        {
            Id = orden.Id,
            FechaEntregaEsperada = nuevaFechaEntrega,
            Observaciones = "Orden actualizada con nueva fecha y observaciones",
            Items = new List<OrdenCompraItemCommand>
            {
                new OrdenCompraItemCommand
                {
                    IngredienteId = orden.Items.First().IngredienteId,
                    Cantidad = 20,
                    PrecioUnitario = 50
                }
            },
            UsuarioId = usuario.Id
        };
        
        // Solo incluir RowVersion si no es null (para evitar problemas de concurrencia en SQLite)
        if (ordenEnBD.RowVersion != null)
        {
            command.RowVersion = ordenEnBD.RowVersion;
        }

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/inventario/ordenes-compra/{orden.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Observaciones.Should().Be("Orden actualizada con nueva fecha y observaciones");
        apiResponse.Data!.Items.Should().HaveCount(1);
        apiResponse.Data!.Items.First().Cantidad.Should().Be(20);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PutOrdenCompra_ConDatosValidos_DebeActualizarOrden");
    }

    [Fact]
    public async Task PostAprobarOrdenCompra_ConOrdenPendiente_DebeAprobarOrden()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostAprobarOrdenCompra_ConOrdenPendiente_DebeAprobarOrden");
        await LimpiarTablaOrdenesCompra();
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var orden = await CrearOrdenCompraSimple(proveedor.Id, "Orden para aprobar", usuario.Id, estado: "Pendiente");

        // Configurar autenticación con el usuario creado
        ConfigurarAutenticacionConUsuario(usuario.Id);

        // Assert intermedio: la orden existe en BD
        var ordenEnBD = await DbContext.OrdenesCompra
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orden.Id);
        
        Logger.LogInformation($"🔍 Orden en BD después de crear: {(ordenEnBD != null ? $"ID={ordenEnBD.Id}, Items={ordenEnBD.Items.Count}" : "NO ENCONTRADA")}");
        
        ordenEnBD.Should().NotBeNull();
        ordenEnBD!.Items.Should().HaveCountGreaterThan(0);

        // Act
        var response = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{orden.Id}/aprobar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(orden.Id);
        apiResponse.Data.Estado.Should().Be(EstadoOrdenCompra.Confirmada);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostAprobarOrdenCompra_ConOrdenPendiente_DebeAprobarOrden");
    }

    [Fact]
    public async Task PostAprobarOrdenCompra_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var command = new AprobarOrdenCompraCommand
        {
            Id = idInexistente, // ID inexistente
            UsuarioId = usuario.Id // Incluir UsuarioId
        };
        
        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{idInexistente}/aprobar", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostRechazarOrdenCompra_ConOrdenPendiente_DebeRechazarOrden()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostRechazarOrdenCompra_ConOrdenPendiente_DebeRechazarOrden");
        await LimpiarTablaOrdenesCompra();
        
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var orden = await CrearOrdenCompraPrueba(proveedor.Id, "Orden para rechazar", usuario.Id, DateTime.Today.AddDays(5));
        
        var command = new RechazarOrdenCompraCommand
        {
            Id = orden.Id,
            UsuarioId = usuario.Id, // Incluir UsuarioId
            MotivoRechazo = "No cumple requisitos"
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{orden.Id}/rechazar", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(orden.Id);
        apiResponse.Data.Estado.Should().Be(EstadoOrdenCompra.Cancelada);
        
        // Verificar que la orden se rechazó en la BD
        var ordenRechazada = await DbContext.OrdenesCompra.FindAsync(orden.Id);
        ordenRechazada.Should().NotBeNull();
        await DbContext.Entry(ordenRechazada!).ReloadAsync();
        ordenRechazada.Id.Should().Be(orden.Id);
        ordenRechazada.Estado.Should().Be(EstadoOrdenCompra.Cancelada);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostRechazarOrdenCompra_ConOrdenPendiente_DebeRechazarOrden");
    }

    [Fact]
    public async Task PostRechazarOrdenCompra_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var command = new RechazarOrdenCompraCommand
        {
            Id = idInexistente, // ID inexistente
            UsuarioId = usuario.Id, // Incluir UsuarioId
            MotivoRechazo = "Motivo de rechazo de prueba"
        };
        
        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{idInexistente}/rechazar", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostRecibirOrdenCompra_ConOrdenAprobada_DebeRecibirOrden()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: PostRecibirOrdenCompra_ConOrdenAprobada_DebeRecibirOrden");
        await LimpiarTablaOrdenesCompra();
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var orden = await CrearOrdenCompraPrueba(proveedor.Id, "Orden para recibir", usuario.Id, estado: "Enviada");

        // Configurar autenticación con el usuario creado
        ConfigurarAutenticacionConUsuario(usuario.Id);

        // Assert intermedio: la orden existe y está enviada
        var ordenEnBd = await DbContext.OrdenesCompra
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orden.Id);
        Logger.LogInformation($"🔍 Orden en BD: {(ordenEnBd != null ? $"ID={ordenEnBd.Id}, Estado={ordenEnBd.Estado}, Items={ordenEnBd.Items.Count}" : "NULL")}");
        ordenEnBd.Should().NotBeNull();
        ordenEnBd!.Estado.Should().Be(EstadoOrdenCompra.Enviada);
        ordenEnBd.Items.Should().NotBeNull();
        ordenEnBd.Items.Count.Should().BeGreaterThan(0);

        var command = new RecibirOrdenCompraCommand
        {
            Id = orden.Id,
            UsuarioId = usuario.Id,
            NotasRecepcion = "Recibida correctamente"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{orden.Id}/recibir", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Estado.ToString().Should().Be(EstadoOrdenCompra.Recibida.ToString());
        Logger.LogInformation("✅ Test COMPLETO finalizado: PostRecibirOrdenCompra_ConOrdenAprobada_DebeRecibirOrden");
    }

    [Fact]
    public async Task PostRecibirOrdenCompra_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        var command = new RecibirOrdenCompraCommand
        {
            Id = idInexistente, // ID inexistente
            UsuarioId = usuario.Id, // Incluir UsuarioId
            NotasRecepcion = "Observaciones de prueba"
        };
        
        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ordenes-compra/{idInexistente}/recibir", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrdenesCompraPendientes_ConOrdenesPendientes_DebeRetornarOrdenes()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetOrdenesCompraPendientes_ConOrdenesPendientes_DebeRetornarOrdenes");
        await LimpiarTablaOrdenesCompra();
        
        // Crear dependencias reales
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        
        // Crear órdenes en diferentes estados usando el helper
        var ordenPendiente1 = await CrearOrdenCompraPrueba(proveedor.Id, "Orden pendiente 1", estado: "Pendiente");
        var ordenPendiente2 = await CrearOrdenCompraPrueba(proveedor.Id, "Orden pendiente 2", estado: "Pendiente");
        var ordenEnviada = await CrearOrdenCompraPrueba(proveedor.Id, "Orden enviada", estado: "Enviada");

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ordenes-compra/pendientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrdenCompraDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().BeGreaterThanOrEqualTo(2);
        
        // Verificar que las órdenes pendientes están en la respuesta
        var idsEnRespuesta = apiResponse.Data.Select(o => o.Id).ToList();
        idsEnRespuesta.Should().Contain(ordenPendiente1.Id);
        idsEnRespuesta.Should().Contain(ordenPendiente2.Id);
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetOrdenesCompraPendientes_ConOrdenesPendientes_DebeRetornarOrdenes");
    }

    [Fact]
    public async Task GetOrdenesCompraPendientes_SinOrdenesPendientes_DebeRetornarListaVacia()
    {
        // Arrange
        await LimpiarTablaOrdenesCompra();
        
        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ordenes-compra/pendientes");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrdenCompraDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetOrdenesCompraPorProveedor_ConOrdenesDelProveedor_DebeRetornarOrdenes()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test COMPLETO: GetOrdenesCompraPorProveedor_ConOrdenesDelProveedor_DebeRetornarOrdenes");
        await LimpiarTablaOrdenesCompra();
        
        // Crear dependencias reales
        var proveedor1 = await CrearProveedorPrueba("Proveedor 1");
        var proveedor2 = await CrearProveedorPrueba("Proveedor 2");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        var usuario = await CrearUsuarioPrueba("Usuario Test");
        
        // Crear órdenes para diferentes proveedores
        var ordenProveedor1 = await CrearOrdenCompraPrueba(proveedor1.Id, "Orden del proveedor 1");
        var ordenProveedor2 = await CrearOrdenCompraPrueba(proveedor2.Id, "Orden del proveedor 2");

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/ordenes-compra/proveedor/{proveedor1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrdenCompraDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().BeGreaterThanOrEqualTo(1);
        
        // Verificar que solo las órdenes del proveedor 1 están en la respuesta
        var ordenesProveedor1 = apiResponse.Data.Where(o => o.ProveedorId == proveedor1.Id).ToList();
        ordenesProveedor1.Should().HaveCount(1);
        ordenesProveedor1.First().Id.Should().Be(ordenProveedor1.Id);
        
        // Verificar que no hay órdenes del proveedor 2
        var ordenesProveedor2 = apiResponse.Data.Where(o => o.ProveedorId == proveedor2.Id).ToList();
        ordenesProveedor2.Should().BeEmpty();
        
        Logger.LogInformation("✅ Test COMPLETO finalizado: GetOrdenesCompraPorProveedor_ConOrdenesDelProveedor_DebeRetornarOrdenes");
    }

    [Fact]
    public async Task ObtenerOrdenCompraPorId_ConEstadoModificado_DebeRetornarEstadoCorrecto()
    {
        // Arrange
        var admin = await CrearUsuarioPrueba("admin.test", "Admin Test", "admin@test.com", RolUsuario.Administrador);
        var proveedor = await CrearProveedorPrueba("Proveedor Test");
        var ingrediente = await CrearIngredientePrueba("Ingrediente Test");
        
        // Crear una orden de compra con datos válidos
        var crearOrdenRequest = new
        {
            ProveedorId = proveedor.Id,
            FechaEntregaEsperada = DateTime.Today.AddDays(7), // Fecha futura válida
            Observaciones = "Test de persistencia de estado",
            Items = new[]
            {
                new
                {
                    IngredienteId = ingrediente.Id,
                    Cantidad = 10,
                    PrecioUnitario = 5.50m,
                    Observaciones = "Item de prueba"
                }
            }
        };
        
        var crearResponse = await HttpClient.PostAsJsonAsync("/api/inventario/ordenes-compra", crearOrdenRequest);
        
        // 🔍 DIAGNÓSTICO: Capturar el error específico
        if (!crearResponse.IsSuccessStatusCode)
        {
            var errorContent = await crearResponse.Content.ReadAsStringAsync();
            Logger.LogError("🔧 [TEST] Error al crear orden: Status={Status}, Content={Content}", 
                crearResponse.StatusCode, errorContent);
            
            // Intentar deserializar como ApiResponse para obtener más detalles
            try
            {
                var errorResponse = await crearResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
                Logger.LogError("🔧 [TEST] Error detallado: {Error}", errorResponse?.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError("🔧 [TEST] Error al deserializar respuesta: {Error}", ex.Message);
            }
        }
        
        crearResponse.EnsureSuccessStatusCode();
        
        var ordenCreada = await crearResponse.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        var ordenId = ordenCreada!.Data!.Id;
        
        Logger.LogInformation("🔧 [TEST] Orden creada con ID: {OrdenId}, Estado inicial: {Estado}", 
            ordenId, ordenCreada.Data.Estado);
        
        // Act - Aprobar la orden (esto cambia el estado)
        var aprobarResponse = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{ordenId}/aprobar", null);
        aprobarResponse.EnsureSuccessStatusCode();
        
        var ordenAprobada = await aprobarResponse.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        Logger.LogInformation("🔧 [TEST] Orden aprobada - Estado en respuesta: {Estado}", 
            ordenAprobada!.Data!.Estado);
        
        // Act - Obtener la orden por ID (endpoint problemático)
        var obtenerResponse = await HttpClient.GetAsync($"/api/inventario/ordenes-compra/{ordenId}");
        obtenerResponse.EnsureSuccessStatusCode();
        
        var ordenObtenida = await obtenerResponse.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>();
        
        // Assert
        ordenObtenida.Should().NotBeNull();
        ordenObtenida!.Success.Should().BeTrue();
        ordenObtenida.Data.Should().NotBeNull();
        
        Logger.LogInformation("🔧 [TEST] Orden obtenida por ID - Estado final: {Estado} (Valor: {Valor})", 
            ordenObtenida.Data!.Estado, (int)ordenObtenida.Data.Estado);
        
        // ⚠️ ESTA ES LA VALIDACIÓN QUE FALLA EN EL FLUJO COMPLETO
        ordenObtenida.Data!.Estado.Should().Be(EstadoOrdenCompra.Confirmada);
    }

    #region Métodos Helper

    private async Task LimpiarTablaOrdenesCompra()
    {
        var ordenes = await DbContext.OrdenesCompra.ToListAsync();
        DbContext.OrdenesCompra.RemoveRange(ordenes);
        
        await DbContext.SaveChangesAsync();
    }

    // Mock simple para IDateTimeService
    private class FakeDateTimeService : IDateTimeService
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Today => DateTime.Today;
    }

    /// <summary>
    /// Crea una orden de compra simple sin recargar desde BD para evitar pérdida de items
    /// </summary>
    private async Task<OrdenCompra> CrearOrdenCompraSimple(Guid proveedorId, string observaciones = "", Guid? usuarioId = null, DateTime? fechaEntrega = null, string estado = "Pendiente")
    {
        var ingrediente = await CrearIngredientePrueba("Ingrediente para orden");
        var usuario = usuarioId.HasValue ? await DbContext.Usuarios.FindAsync(usuarioId.Value) : await CrearUsuarioPrueba("Usuario Orden");

        // Crear la orden inicial
        var orden = OrdenCompra.Crear(proveedorId, observaciones, DateTime.Now);
        
        // Establecer fecha de entrega futura válida ANTES de agregar items
        var fechaEntregaValida = fechaEntrega ?? DateTime.Now.AddDays(7);
        orden.EstablecerFechaEntrega(fechaEntregaValida);
        
        // Ahora agregar el item
        orden.AgregarItem(ingrediente.Id, ingrediente.Nombre, 10, ingrediente.UnidadMedida);

        // Guardar la orden inicial
        DbContext.OrdenesCompra.Add(orden);
        await DbContext.SaveChangesAsync();

        // Aplicar transiciones de estado según el estado deseado
        switch (estado.ToLower())
        {
            case "pendiente":
                // Ya está en estado pendiente, no hacer nada
                break;
                
            case "confirmada":
            case "aprobada":
                // Aprobar la orden
                orden.Aprobar(new Mock<IDateTimeService>().Object);
                await DbContext.SaveChangesAsync();
                break;
                
            case "enviada":
                // Primero aprobar, luego enviar
                orden.Aprobar(new Mock<IDateTimeService>().Object);
                await DbContext.SaveChangesAsync();
                orden.Enviar();
                await DbContext.SaveChangesAsync();
                break;
                
            case "recibida":
                // Aprobar -> Enviar -> Recibir
                orden.Aprobar(new Mock<IDateTimeService>().Object);
                await DbContext.SaveChangesAsync();
                orden.Enviar();
                await DbContext.SaveChangesAsync();
                orden.Recibir(DateTime.Now, "Recibida en test");
                await DbContext.SaveChangesAsync();
                break;
                
            default:
                throw new ArgumentException($"Estado no soportado: {estado}");
        }

        Logger.LogInformation($"✅ Orden creada con estado '{estado}'. Items: {orden.Items.Count}");
        return orden;
    }

    /// <summary>
    /// Crea una orden de compra realista con al menos un item y el estado deseado.
    /// Estados soportados: Pendiente (default), Aprobada, Enviada, Recibida
    /// </summary>
    private async Task<OrdenCompra> CrearOrdenCompraPrueba(Guid proveedorId, string observaciones = "", Guid? usuarioId = null, DateTime? fechaEntrega = null, string estado = "Pendiente")
    {
        var ingrediente = await CrearIngredientePrueba("Ingrediente para orden");
        var usuario = usuarioId.HasValue ? await DbContext.Usuarios.FindAsync(usuarioId.Value) : await CrearUsuarioPrueba("Usuario Orden");
        
        // Crear la orden inicial
        var orden = OrdenCompra.Crear(proveedorId, observaciones, DateTime.Now);
        var fechaEntregaValida = fechaEntrega ?? DateTime.Now.AddDays(7);
        orden.EstablecerFechaEntrega(fechaEntregaValida);
        orden.AgregarItem(ingrediente.Id, ingrediente.Nombre, 10, ingrediente.UnidadMedida);
        
        DbContext.OrdenesCompra.Add(orden);
        await DbContext.SaveChangesAsync();
        
        switch (estado.ToLower())
        {
            case "pendiente":
                break;
            case "confirmada":
            case "aprobada":
                orden.Aprobar(new Mock<IDateTimeService>().Object);
                await DbContext.SaveChangesAsync();
                break;
            case "enviada":
                // Primero aprobar, luego enviar
                orden.Aprobar(new Mock<IDateTimeService>().Object);
                await DbContext.SaveChangesAsync();
                orden.Enviar();
                await DbContext.SaveChangesAsync();
                break;
            case "recibida":
                // Aprobar -> Enviar -> Recibir
                orden.Aprobar(new Mock<IDateTimeService>().Object);
                await DbContext.SaveChangesAsync();
                orden.Enviar();
                await DbContext.SaveChangesAsync();
                orden.Recibir(DateTime.Now, "Recibida en test");
                await DbContext.SaveChangesAsync();
                break;
            default:
                throw new ArgumentException($"Estado no soportado: {estado}");
        }
        
        Logger.LogInformation($"✅ Orden creada con estado '{estado}'. Items: {orden.Items.Count}");
        return orden;
    }

    private async Task<Usuario> CrearUsuarioPrueba(string nombreCompleto = "Usuario Test")
    {
        var usuario = Usuario.Crear(
            $"{nombreCompleto.Replace(" ", "").ToLower()}_{Guid.NewGuid().ToString("N")[..8]}", // Nombre único
            nombreCompleto,
            $"{nombreCompleto.Replace(" ", "").ToLower()}_{Guid.NewGuid().ToString("N")[..8]}@test.com", // Email único
            RolUsuario.Administrador
        );

        DbContext.Usuarios.Add(usuario);
        await DbContext.SaveChangesAsync();
        return usuario;
    }

    private void ConfigurarAutenticacionConUsuario(Guid usuarioId)
    {
        // Implementa la lógica para configurar la autenticación con el usuario creado
        // Esto puede incluir la adición de un token de autenticación o la configuración de encabezados HTTP
    }

    #endregion

    public new void Dispose()
    {
        base.Dispose();
    }
} 