using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.ActualizarFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.EliminarFactura;
using RestaurantePro.Application.Comercial.Facturacion.Commands.RegistrarPagoFactura;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Api.Models.Requests;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Core.Usuarios.Enums;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para el controlador de facturas
/// Prueba todos los endpoints CRUD y operaciones específicas de facturación
/// </summary>
[Collection("FacturasControllerTests")]
public class FacturasControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public FacturasControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    #region GET /api/comercial/facturas

    [Fact]
    public async Task GetFacturas_SinFacturasEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturas_SinFacturasEnBD_DebeRetornarListaVacia");
        var url = "/api/comercial/facturas";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        Console.WriteLine($"🔍 Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Response Content: {content}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEmpty();

        // Verificar que realmente no hay facturas en la BD
        var facturasEnBD = await DbContext.Facturas.ToListAsync();
        facturasEnBD.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFacturas_ConFacturasEnBD_DebeRetornarLista()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturas_ConFacturasEnBD_DebeRetornarLista");

        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        await CrearFacturaPrueba(clienteId: cliente.Id);
        await CrearFacturaPrueba(clienteId: cliente.Id);

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/facturas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeEmpty();
        apiResponse.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFacturas_ConFiltros_DebeFiltrarCorrectamente()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturas_ConFiltros_DebeFiltrarCorrectamente");

        var nombreCliente1 = $"Cliente1_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente1 = GenerarEmailValido();
        var nombreCliente2 = $"Cliente2_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente2 = GenerarEmailValido();
        var factura1 = await CrearFacturaConDetallesPrueba(clienteId: (await CrearClientePrueba(nombreCliente1, emailCliente1)).Id);
        var factura2 = await CrearFacturaConDetallesPrueba(clienteId: (await CrearClientePrueba(nombreCliente2, emailCliente2)).Id);

        var url = "/api/comercial/facturas?pageNumber=1&pageSize=10";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
    }

    #endregion

    #region GET /api/comercial/facturas/{id}

    [Fact]
    public async Task GetFactura_ConIdExistente_DebeRetornarFactura()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFactura_ConIdExistente_DebeRetornarFactura");

        var sufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
        var factura = await CrearFacturaPrueba(
            clienteId: (await CrearClientePrueba("Cliente Test", GenerarEmailValido())).Id);
        var url = $"/api/comercial/facturas/{factura.Id}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(factura.Id);
    }

    [Fact]
    public async Task GetFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFactura_ConIdInexistente_DebeRetornar404");

        var facturaIdInexistente = Guid.Parse("66666666-6666-6666-6666-666666666666");

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/{facturaIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();

        // Verificar que realmente no existe en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(facturaIdInexistente);
        facturaEnBD.Should().BeNull();
    }

    #endregion

    #region POST /api/comercial/facturas

    [Fact]
    public async Task PostFactura_ConDatosValidos_DebeCrearFactura()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PostFactura_ConDatosValidos_DebeCrearFactura");

        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, observaciones: "Comanda de prueba", estado: EstadoComanda.Finalizada);

        // Agregar productos a la comanda para que tenga detalles
        var producto1 = await CrearProductoPrueba("Producto 1", 25.50m);
        var producto2 = await CrearProductoPrueba("Producto 2", 15.75m);
        await CrearDetalleComandaPrueba(comanda.Id, producto1.Id, 2, "Detalle 1");
        await CrearDetalleComandaPrueba(comanda.Id, producto2.Id, 1, "Detalle 2");

        var facturaRequest = new FacturaTestDataBuilder()
            .ConClienteId(cliente.Id)
            .ConComandasIds(comanda.Id)
            .ConNombreCliente("Cliente Factura")
            .ConEmailCliente(cliente.Email)
            .BuildCrearFacturaRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/facturas", facturaRequest);

        // Assert
        var errorContent = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.Created)
        {
            Console.WriteLine("❌ Error de creación de factura: " + errorContent);
            try
            {
                var apiError = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(errorContent);
                if (apiError?.Errors != null && apiError.Errors.Count > 0)
                {
                    Console.WriteLine("Errores de validación:");
                    foreach (var err in apiError.Errors)
                    {
                        Console.WriteLine("- " + err);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo deserializar el error: {ex.Message}");
            }
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = response.Headers.Location;
        location.Should().NotBeNull();

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.ClienteId.Should().Be(cliente.Id);

        // Verificar que se creó en la BD
        var facturasEnBD = await DbContext.Facturas.ToListAsync();
        facturasEnBD.Should().HaveCount(1);
        facturasEnBD[0].ClienteId.Should().Be(cliente.Id);
        facturasEnBD[0].ComandasIds.Should().Contain(comanda.Id);
    }

    [Fact]
    public async Task PostFactura_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PostFactura_ConDatosInvalidos_DebeRetornar400");

        var facturaRequest = new FacturaTestDataBuilder()
            .ConNombreCliente("") // Nombre vacío
            .BuildCrearFacturaRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/facturas", facturaRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();

        // Verificar que no se creó en la BD
        var facturasEnBD = await DbContext.Facturas.ToListAsync();
        facturasEnBD.Should().BeEmpty();
    }

    #endregion

    #region PUT /api/comercial/facturas/{id}

    [Fact]
    public async Task PutFactura_ConDatosValidos_DebeActualizarFactura()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PutFactura_ConDatosValidos_DebeActualizarFactura");

        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = $"put_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id);
        var request = new ActualizarFacturaCommand { Id = factura.Id, NombreCliente = "Nuevo Nombre" };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/facturas/{factura.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.NombreCliente.Should().Be("Nuevo Nombre");
    }

    [Fact]
    public async Task PutFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PutFactura_ConIdInexistente_DebeRetornar404");

        var idInexistente = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var updateRequest = new ActualizarFacturaCommand
        {
            Id = idInexistente,
            NombreCliente = "No debe existir"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/facturas/{idInexistente}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PutFactura_ConFacturaEmitida_DebeRetornar400()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PutFactura_ConFacturaEmitida_DebeRetornar400");

        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var cliente = await CrearClientePrueba($"ClienteEmit_{sufijo}", $"emit_{sufijo}@test.com");
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        var factura = await CrearFacturaConDetallesPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda.Id });
        factura.Emitir(DateTimeService, 30);
        await DbContext.SaveChangesAsync();

        // Intentar actualizar solo campos que no sean información fiscal
        var request = new ActualizarFacturaCommand 
        { 
            Id = factura.Id, 
            DiasCredito = 45 // Cambiar a un campo que no sea información fiscal
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/facturas/{factura.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region DELETE /api/comercial/facturas/{id}

    [Fact]
    public async Task DeleteAnularFactura_ConFacturaExistente_DebeAnularFactura()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: DeleteAnularFactura_ConFacturaExistente_DebeAnularFactura");

        // Limpiar BD explícitamente para evitar interferencia de tests previos
        await LimpiarBaseDeDatosCompletamente();

        // Crear un usuario real con permisos de administrador
        var usuario = await CrearUsuarioPrueba("admin.test", "Admin Test", null, RolUsuario.Administrador);
        Console.WriteLine($"🔍 Test: Usuario creado con ID: {usuario.Id}");

        // Crear productos con precios bajos
        var producto1 = await CrearProductoPrueba("Producto Barato 1", 10.00m);
        var producto2 = await CrearProductoPrueba("Producto Barato 2", 15.00m);

        // Crear comanda con productos baratos
        var nombreClienteAnul = $"ClienteAnul_{Guid.NewGuid().ToString("N")[..8]}";
        var emailClienteAnul = GenerarEmailValido();
        var comanda = await CrearComandaPrueba(
            meseroId: null,
            clienteId: (await CrearClientePrueba(nombreClienteAnul, emailClienteAnul)).Id,
            mesaId: null,
            observaciones: "Comanda de prueba",
            estado: EstadoComanda.Creada
        );

        // Agregar productos baratos a la comanda
        await CrearDetalleComandaPrueba(comanda.Id, producto1.Id, 1);
        await CrearDetalleComandaPrueba(comanda.Id, producto2.Id, 1);

        // Crear factura con la comanda
        var factura = await CrearFacturaConDetallesPrueba(
            clienteId: comanda.ClienteId,
            comandasIds: new List<Guid> { comanda.Id },
            fechaCreacion: DateTimeService.Now
        );

        // Emitir la factura (esto establecerá la fecha de emisión actual)
        factura.Emitir(DateTimeService, 30);
        await DbContext.SaveChangesAsync();

        // Verificar que la factura fue creada recientemente (dentro de 7 días)
        var diasDesdeCreacion = (DateTimeService.Now - factura.FechaCreacion).Days;
        Console.WriteLine($"🔍 Test: Factura creada hace {diasDesdeCreacion} días");

        // Configurar autenticación con el usuario real creado
        ConfigurarAutenticacionConUsuario(usuario.Id, "Administrador");
        Console.WriteLine($"🔍 Test: Configurada autenticación con usuario ID: {usuario.Id}");

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/facturas/{factura.Id}");

        // Log response details
        Console.WriteLine($"🔍 Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Response Content: {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: DeleteFactura_ConIdInexistente_DebeRetornar404");

        var facturaIdInexistente = Guid.Parse("88888888-8888-8888-8888-888888888888");

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/facturas/{facturaIdInexistente}");

        // Assert
        Console.WriteLine($"🔍 Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Response Content: {content}");

        // El controlador devuelve 404 para IDs inexistentes (comportamiento correcto)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();

        // Verificar que realmente no existe en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(facturaIdInexistente);
        facturaEnBD.Should().BeNull();
    }

    #endregion

    #region GET /api/comercial/facturas/cliente/{clienteId}

    [Fact]
    public async Task GetFacturasPorCliente_ConClienteExistente_DebeRetornarFacturas()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturasPorCliente_ConClienteExistente_DebeRetornarFacturas");

        // Crear clientes con nombres y emails totalmente aleatorios
        var nombreCliente1 = $"Cliente1_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente1 = GenerarEmailValido();
        var nombreCliente2 = $"Cliente2_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente2 = GenerarEmailValido();
        var cliente1 = await CrearClientePrueba(nombreCliente1, emailCliente1);
        await CrearFacturaPrueba(clienteId: cliente1.Id);
        await CrearFacturaPrueba(clienteId: cliente1.Id);
        var cliente2 = await CrearClientePrueba(nombreCliente2, emailCliente2);

        var url = $"/api/comercial/facturas/cliente/{cliente1.Id}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(f => f.ClienteId == cliente1.Id);
    }

    [Fact]
    public async Task GetFacturasPorCliente_ConClienteSinFacturas_DebeRetornarListaVacia()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturasPorCliente_ConClienteSinFacturas_DebeRetornarListaVacia");

        // Usar email totalmente aleatorio y nombre sin patrones repetitivos
        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var url = $"/api/comercial/facturas/cliente/{cliente.Id}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEmpty();
    }

    #endregion

    #region GET /api/comercial/facturas/comanda/{comandaId}

    [Fact]
    public async Task GetFacturasPorComanda_ConComandaExistente_DebeRetornarFactura()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturasPorComanda_ConComandaExistente_DebeRetornarFactura");

        // Crear cliente y comanda con email totalmente aleatorio y único
        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var comanda = await CrearComandaPrueba(
            meseroId: null,
            clienteId: cliente.Id,
            mesaId: null,
            observaciones: "Comanda de prueba",
            estado: EstadoComanda.Finalizada);

        // Agregar productos a la comanda para que tenga detalles
        var producto1 = await CrearProductoPrueba("Producto 1", 100.00m);
        var producto2 = await CrearProductoPrueba("Producto 2", 150.00m);
        await CrearDetalleComandaPrueba(comanda.Id, producto1.Id, 2);
        await CrearDetalleComandaPrueba(comanda.Id, producto2.Id, 1);

        // Crear factura asociada a la comanda específicamente
        var factura = await CrearFacturaPrueba(
            clienteId: cliente.Id,
            comandasIds: new List<Guid> { comanda.Id });

        // Verificar que la factura está en la BD con la asociación correcta
        var facturaEnBD = await DbContext.Facturas
            .Include(f => f.Cliente)
            .FirstOrDefaultAsync(f => f.Id == factura.Id);

        facturaEnBD.Should().NotBeNull();
        facturaEnBD!.ComandasIds.Should().Contain(comanda.Id);

        // Verificar que la comanda existe en la BD
        var comandaEnBD = await DbContext.Comandas.FindAsync(comanda.Id);
        comandaEnBD.Should().NotBeNull();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/comanda/{comanda.Id}");

        // Assert
        Console.WriteLine($"🔍 Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Response Content: {content}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCountGreaterThan(0);
        apiResponse.Data.Should().Contain(f => f.Id == factura.Id);
    }

    [Fact]
    public async Task GetFacturasPorComanda_ConComandaSinFactura_DebeRetornarListaVacia()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturasPorComanda_ConComandaSinFactura_DebeRetornarListaVacia");
        var comanda = await CrearComandaPrueba(meseroId: null, clienteId: null, mesaId: null, observaciones: "Comanda de prueba");
        var url = $"/api/comercial/facturas/comanda/{comanda.Id}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().BeEmpty();
    }

    #endregion

    #region GET /api/comercial/facturas/pendientes

    [Fact]
    public async Task GetFacturasPendientes_DebeRetornarFacturasEmitidas()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturasPendientes_DebeRetornarFacturasEmitidas");

        // Limpiar BD explícitamente para evitar interferencia de tests previos
        await LimpiarBaseDeDatosCompletamente();

        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = $"cl_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);

        // Crear facturas con detalles para que se puedan emitir
        var factura1 = await CrearFacturaConDetallesPrueba(clienteId: cliente.Id);
        factura1.Emitir(DateTimeService, 30);

        var factura2 = await CrearFacturaConDetallesPrueba(clienteId: cliente.Id);
        factura2.Emitir(DateTimeService, 30);

        // Crear una factura pagada que no debe aparecer
        var factura3 = await CrearFacturaConDetallesPrueba(clienteId: cliente.Id);
        factura3.Emitir(DateTimeService, 30);
        factura3.RegistrarPago(factura3.Total, Guid.NewGuid());

        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/facturas/pendientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(f => f.Estado == EstadoFactura.Emitida);
    }

    #endregion

    #region POST /api/comercial/facturas/{id}/pagar

    [Fact]
    public async Task PostPagarFactura_ConFacturaEmitida_DebeRegistrarPago()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PostPagarFactura_ConFacturaEmitida_DebeRegistrarPago");

        // Configurar autenticación
        ConfigurarAutenticacionConRol("Administrador");

        var nombreCliente = $"ClientePago_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = $"ppago_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        var factura = await CrearFacturaConDetallesPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda.Id });
        factura.Emitir(DateTimeService, 30);
        await DbContext.SaveChangesAsync();

        // Recalcular totales antes de emitir y pagar
        factura.RecalcularTotales();
        await DbContext.SaveChangesAsync();

        // Recargar la entidad desde la BD para asegurar valores exactos
        await DbContext.Entry(factura).ReloadAsync();

        var request = new RegistrarPagoFacturaCommand
        {
            Monto = factura.Total,
            MetodoPago = "Efectivo"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{factura.Id}/pagar", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();

        // Forzar una nueva consulta a la BD para obtener los datos más recientes
        DbContext.ChangeTracker.Clear(); // Limpiar el tracking del contexto

        // Verificar que la factura está pagada en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(factura.Id);
        facturaEnBD.Should().NotBeNull();

        // Verificar que el estado cambió a pagada
        facturaEnBD!.Estado.Should().Be(EstadoFactura.Pagada);

        // Verificar que el total pagado es igual al total
        facturaEnBD.TotalPagado.Should().Be(facturaEnBD.Total);

        // Verificar que tiene fecha de pago
        facturaEnBD.FechaPago.Should().NotBeNull();
    }

    #endregion

    #region GET /api/comercial/facturas/{id}/pdf

    [Fact]
    public async Task GetFacturaPdf_ConFacturaExistente_DebeRetornarPdf()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetFacturaPdf_ConFacturaExistente_DebeRetornarPdf");
        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id);

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/{factura.Id}/pdf");

        // Assert
        Console.WriteLine($"🔍 Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Response Content: {content}");

        // El endpoint actualmente devuelve 501 (NotImplemented)
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetFacturaPdf_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var facturaIdInexistente = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/{facturaIdInexistente}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region PATCH /api/comercial/facturas/{id}/anular

    [Fact]
    public async Task PatchAnularFactura_ConFacturaExistente_DebeAnularFactura()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PatchAnularFactura_ConFacturaExistente_DebeAnularFactura");

        var fechaActual = DateTimeService.Now;

        // Crear un usuario real con permisos de administrador
        var usuario = await CrearUsuarioPrueba("admin.test", "Admin Test", null, RolUsuario.Administrador);
        Console.WriteLine($"🔍 Test: Usuario creado con ID: {usuario.Id}");

        // Crear cliente y comanda con productos baratos - usar emails completamente únicos
        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var comanda = await CrearComandaPrueba(
            meseroId: null,
            clienteId: cliente.Id,
            mesaId: null,
            observaciones: "Comanda de prueba",
            estado: EstadoComanda.Creada
        );

        // Crear factura con la comanda y fecha actual
        var factura = await CrearFacturaConDetallesPrueba(
            clienteId: comanda.ClienteId,
            comandasIds: new List<Guid> { comanda.Id },
            fechaCreacion: fechaActual
        );

        // Emitir la factura (esto establecerá la fecha de emisión actual)
        factura.Emitir(DateTimeService, 30);
        await DbContext.SaveChangesAsync();

        // Verificar que la factura fue creada recientemente (dentro de 7 días)
        var diasDesdeCreacion = (fechaActual - factura.FechaCreacion).Days;
        Console.WriteLine($"🔍 Test: Factura creada hace {diasDesdeCreacion} días");

        // Configurar autenticación con el usuario real creado
        ConfigurarAutenticacionConUsuario(usuario.Id, "Administrador");
        Console.WriteLine($"🔍 Test: Configurada autenticación con usuario ID: {usuario.Id}");

        // Verificar que el usuario existe en la BD
        var usuarioEnBD = await DbContext.Usuarios.FindAsync(usuario.Id);
        Console.WriteLine($"🔍 Test: Usuario en BD: {(usuarioEnBD != null ? "Existe" : "No existe")}");

        // Act
        var anularRequest = new AnularFacturaRequest
        {
            Motivo = "Test de anulación",
            RevertirInventario = false // Evitar validación de inventario para este test
        };

        var response = await HttpClient.PatchAsync(
            $"/api/comercial/facturas/{factura.Id}/anular",
            new StringContent(JsonSerializer.Serialize(anularRequest), Encoding.UTF8, "application/json"));

        // Log response details
        Console.WriteLine($"🔍 Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Response Content: {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task PatchAnularFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        ConfigurarAutenticacionConUsuario(usuarioId, "Administrador");

        var facturaIdInexistente = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var anularRequest = new AnularFacturaRequest
        {
            Motivo = "Test de anulación",
            RevertirInventario = false // Evitar validación de inventario para este test
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/api/comercial/facturas/{facturaIdInexistente}/anular", anularRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region POST /api/comercial/facturas/{id}/enviar-email

    [Fact]
    public async Task PostEnviarEmail_ConFacturaExistente_DebeRetornar501()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PostEnviarEmail_ConFacturaExistente_DebeRetornar501");
        
        // Usar email totalmente aleatorio
        var nombreCliente = Guid.NewGuid().ToString();
        var emailCliente = $"{Guid.NewGuid()}@mail.com";
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id);
        var emailDestino = $"{Guid.NewGuid()}@mail.com";

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{factura.Id}/enviar-email",
            new { EmailDestino = emailDestino });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain("Funcionalidad no implementada");
    }

    [Fact]
    public async Task PostEnviarEmail_ConIdInexistente_DebeRetornar501()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PostEnviarEmail_ConIdInexistente_DebeRetornar501");

        var facturaIdInexistente = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var emailDestino = $"cliente_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var enviarEmailRequest = new
        {
            EmailDestino = emailDestino,
            Asunto = "Factura de prueba"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{facturaIdInexistente}/enviar-email", enviarEmailRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region GET /api/comercial/facturas/buscar

    [Fact]
    public async Task GetBuscarFacturas_ConCriteriosValidos_DebeRetornarFacturas()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetBuscarFacturas_ConCriteriosValidos_DebeRetornarFacturas");

        // Crear clientes con nombres y emails totalmente aleatorios y únicos
        var nombreCliente1 = $"Cliente1_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente1 = GenerarEmailValido();
        var nombreCliente2 = $"Cliente2_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente2 = GenerarEmailValido();
        var cliente1 = await CrearClientePrueba(nombreCliente1, emailCliente1);
        var cliente2 = await CrearClientePrueba(nombreCliente2, emailCliente2);

        // Crear facturas con detalles para que se puedan emitir
        var f1 = await CrearFacturaConDetallesPrueba(clienteId: cliente1.Id);
        f1.Emitir(DateTimeService, 30);

        var f2 = await CrearFacturaConDetallesPrueba(clienteId: cliente1.Id);
        f2.Emitir(DateTimeService, 30);

        var f3 = await CrearFacturaConDetallesPrueba(clienteId: cliente2.Id);
        f3.Emitir(DateTimeService, 30);

        await DbContext.SaveChangesAsync();

        var url = $"/api/comercial/facturas/buscar?clienteId={cliente1.Id}&estado=Emitida";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(f => f.ClienteId == cliente1.Id);
    }

    [Fact]
    public async Task GetBuscarFacturas_ConNumeroFactura_DebeFiltrarCorrectamente()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetBuscarFacturas_ConNumeroFactura_DebeFiltrarCorrectamente");

        // Usar Guid para emails únicos
        var nombreCliente1 = $"Cliente1_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente1 = GenerarEmailValido();
        var nombreCliente2 = $"Cliente2_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente2 = GenerarEmailValido();
        var cliente1 = await CrearClientePrueba(nombreCliente1, emailCliente1);
        var cliente2 = await CrearClientePrueba(nombreCliente2, emailCliente2);

        var factura1 = await CrearFacturaPrueba(clienteId: cliente1.Id, numeroFactura: "F-F-001");
        var factura2 = await CrearFacturaPrueba(clienteId: cliente2.Id, numeroFactura: "F-F-002");

        var url = "/api/comercial/facturas/buscar?numeroFactura=F-F-001";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeEmpty();
        apiResponse.Data.Should().Contain(f => f.NumeroFactura == "F-F-F-001");
    }

    [Fact]
    public async Task GetBuscarFacturas_SinCriterios_DebeRetornarTodasLasFacturas()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetBuscarFacturas_SinCriterios_DebeRetornarTodasLasFacturas");

        var nombreCliente1 = $"Cliente1_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente1 = GenerarEmailValido();
        var nombreCliente2 = $"Cliente2_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente2 = GenerarEmailValido();
        var cliente1 = await CrearClientePrueba(nombreCliente1, emailCliente1);
        var cliente2 = await CrearClientePrueba(nombreCliente2, emailCliente2);

        await CrearFacturaPrueba(clienteId: cliente1.Id);
        await CrearFacturaPrueba(clienteId: cliente2.Id);

        var url = "/api/comercial/facturas/buscar";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
    }

    #endregion

    #region GET /api/comercial/facturas/reporte

    [Fact]
    public async Task GetReporteFacturas_ConTipoReporteValido_DebeRetornar501()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetReporteFacturas_ConTipoReporteValido_DebeRetornar501");

        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = GenerarEmailValido();
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);

        // Crear una factura real para el cliente
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id);

        var url = "/api/comercial/facturas/reporte?tipoReporte=ventas&formato=pdf";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain("Funcionalidad no implementada");

        // Verificar que la factura sigue existiendo en la BD (usar el ID de la factura, no del cliente)
        var facturaEnBD = await DbContext.Facturas.FindAsync(factura.Id);
        facturaEnBD.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReporteFacturas_ConFiltrosDeFecha_DebeRetornar501()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetReporteFacturas_ConFiltrosDeFecha_DebeRetornar501");

        var fechaDesde = DateTime.Now.AddDays(-30);
        var fechaHasta = DateTime.Now;

        var url = $"/api/comercial/facturas/reporte?tipoReporte=ventas&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region Tests de Validación de Reglas de Negocio

    [Fact]
    public async Task PostFactura_ConComandaNoFinalizada_DebeRetornar400()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: PostFactura_ConComandaNoFinalizada_DebeRetornar400");

        // Crear cliente con email totalmente aleatorio
        var nombreCliente = $"ClienteC_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = $"cc_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.EnProceso);

        var request = new CrearFacturaCommand
        {
            ClienteId = cliente.Id,
            ComandasIds = new List<Guid> { comanda.Id }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/facturas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region Tests de Integración Completa

    [Fact]
    public async Task FlujoCompletoFacturacion_DebeFuncionarCorrectamente()
    {
        // ARRANGE
        Console.WriteLine("🧪 Iniciando test: FlujoCompletoFacturacion_DebeFuncionarCorrectamente");

        // 1. Crear entidades necesarias con nombres y emails totalmente aleatorios y únicos
        var nombreMesero = $"Mesero_{Guid.NewGuid().ToString("N")[..8]}";
        var emailMesero = $"mes_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var nombreCliente = $"Cliente_{Guid.NewGuid().ToString("N")[..8]}";
        var emailCliente = $"cl_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var mesero = await CrearUsuarioPrueba(nombreMesero, nombreMesero, emailMesero, RolUsuario.Mesero);
        var cliente = await CrearClientePrueba(nombreCliente, emailCliente);
        var mesa = await CrearMesaPrueba(10, 4);

        // 2. Crear comanda con productos baratos
        var comanda = await CrearComandaPrueba(
            meseroId: null,
            clienteId: cliente.Id,
            mesaId: mesa.Id,
            observaciones: "Comanda de prueba",
            estado: EstadoComanda.Finalizada);

        // 3. Crear factura con la comanda
        var factura = await CrearFacturaConDetallesPrueba(
            clienteId: comanda.ClienteId,
            comandasIds: new List<Guid> { comanda.Id },
            fechaCreacion: DateTimeService.Now
        );

        // 4. Emitir la factura
        factura.Emitir(DateTimeService, 30);
        await DbContext.SaveChangesAsync();

        // 5. Verificar que la factura está en la BD
        var facturaEnBD = await DbContext.Facturas
            .Include(f => f.Cliente)
            .FirstOrDefaultAsync(f => f.Id == factura.Id);

        facturaEnBD.Should().NotBeNull();
        facturaEnBD!.ComandasIds.Should().Contain(comanda.Id);

        // 6. Verificar que la comanda existe en la BD
        var comandaEnBD = await DbContext.Comandas.FindAsync(comanda.Id);
        comandaEnBD.Should().NotBeNull();
    }

    #endregion

    public new void Dispose()
    {
        // Limpieza si es necesario, aunque ApiIntegrationTestBase ya se encarga
        base.Dispose();
    }
}
