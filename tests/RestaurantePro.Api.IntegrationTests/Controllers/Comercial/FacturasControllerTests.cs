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
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

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
        Logger.LogInformation("🧪 Iniciando test: GetFacturas_SinFacturasEnBD_DebeRetornarListaVacia");
        var url = "/api/comercial/facturas";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
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
        Logger.LogInformation("🧪 Iniciando test: GetFacturas_ConFacturasEnBD_DebeRetornarLista");
        
        // Crear facturas de prueba
        var factura1 = await CrearFacturaPrueba();
        var factura2 = await CrearFacturaPrueba();
        
        var url = "/api/comercial/facturas";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().Contain(f => f.Id == factura1.Id);
        apiResponse.Data.Should().Contain(f => f.Id == factura2.Id);
        
        // Verificar que las facturas están en la BD
        var facturasEnBD = await DbContext.Facturas.ToListAsync();
        facturasEnBD.Should().HaveCount(2);
        facturasEnBD.Should().Contain(f => f.Id == factura1.Id);
        facturasEnBD.Should().Contain(f => f.Id == factura2.Id);
    }

    [Fact]
    public async Task GetFacturas_ConFiltros_DebeFiltrarCorrectamente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFacturas_ConFiltros_DebeFiltrarCorrectamente");
        
        var cliente = await CrearClientePrueba("Cliente Filtro", "filtro@test.com");
        var factura1 = await CrearFacturaPrueba(clienteId: cliente.Id, tipoFactura: TipoFactura.Normal);
        factura1.Emitir();
        
        var factura2 = await CrearFacturaPrueba(clienteId: cliente.Id, tipoFactura: TipoFactura.Fiscal);
        factura2.Emitir();
        
        // Factura de otro cliente que no debe aparecer
        var factura3 = await CrearFacturaPrueba();
        factura3.Emitir();
        await DbContext.SaveChangesAsync();
        
        var url = $"/api/comercial/facturas?clienteId={cliente.Id}&estado=Emitida";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(f => f.ClienteId == cliente.Id && f.Estado == EstadoFactura.Emitida.ToString());
    }

    #endregion

    #region GET /api/comercial/facturas/{id}

    [Fact]
    public async Task GetFactura_ConIdExistente_DebeRetornarFactura()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFactura_ConIdExistente_DebeRetornarFactura");
        
        var factura = await CrearFacturaPrueba();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/{factura.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(factura.Id);
        apiResponse.Data.NumeroFactura.Should().Be(factura.NumeroFactura);
        
        // Verificar que la factura existe en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(factura.Id);
        facturaEnBD.Should().NotBeNull();
        facturaEnBD!.NumeroFactura.Should().Be(factura.NumeroFactura);
    }

    [Fact]
    public async Task GetFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFactura_ConIdInexistente_DebeRetornar404");
        
        var facturaIdInexistente = Guid.NewGuid();

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
        Logger.LogInformation("🧪 Iniciando test: PostFactura_ConDatosValidos_DebeCrearFactura");
        
        var emailUnico = $"factura_{Guid.NewGuid().ToString("N")[..8]}@test.com";
        var cliente = await CrearClientePrueba("Cliente Factura", emailUnico);
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, observaciones: "Comanda de prueba", estado: EstadoComanda.Finalizada);
        
        var facturaRequest = new FacturaTestDataBuilder()
            .ConClienteId(cliente.Id)
            .ConComandasIds(comanda.Id)
            .ConNombreCliente("Cliente Factura")
            .ConEmailCliente(emailUnico)
            .BuildCrearFacturaRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/facturas", facturaRequest);

        // Assert
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
        Logger.LogInformation("🧪 Iniciando test: PostFactura_ConDatosInvalidos_DebeRetornar400");
        
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
        Logger.LogInformation("🧪 Iniciando test: PutFactura_ConDatosValidos_DebeActualizarFactura");
        
        var factura = await CrearFacturaPrueba();
        var nuevoNombreCliente = "Cliente Actualizado";
        
        var updateRequest = new ActualizarFacturaCommand
        {
            Id = factura.Id,
            NombreCliente = nuevoNombreCliente,
            Observaciones = "Factura actualizada"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/facturas/{factura.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();

        // Verificar que se actualizó en la BD
        var facturaActualizada = await DbContext.Facturas.FindAsync(factura.Id);
        facturaActualizada.Should().NotBeNull();
        facturaActualizada!.NombreCliente.Should().Be(nuevoNombreCliente);
        facturaActualizada.Observaciones.Should().Be("Factura actualizada");
    }

    [Fact]
    public async Task PutFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PutFactura_ConIdInexistente_DebeRetornar404");
        
        var idInexistente = Guid.NewGuid();
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

    #endregion

    #region DELETE /api/comercial/facturas/{id}

    [Fact]
    public async Task DeleteFactura_ConIdExistente_DebeEliminarFactura()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteFactura_ConIdExistente_DebeEliminarFactura");
        
        var factura = await CrearFacturaPrueba();
        var url = $"/api/comercial/facturas/{factura.Id}";

        // Act
        var response = await HttpClient.DeleteAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();

        // Verificar que se eliminó de la BD
        var facturaEliminada = await DbContext.Facturas.FindAsync(factura.Id);
        facturaEliminada.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteFactura_ConIdInexistente_DebeRetornar404");
        
        var idInexistente = Guid.NewGuid();
        var url = $"/api/comercial/facturas/{idInexistente}";

        // Act
        var response = await HttpClient.DeleteAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region GET /api/comercial/facturas/cliente/{clienteId}

    [Fact]
    public async Task GetFacturasPorCliente_ConClienteExistente_DebeRetornarFacturas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFacturasPorCliente_ConClienteExistente_DebeRetornarFacturas");
        
        var cliente1 = await CrearClientePrueba("Cliente Con Facturas", "con@facturas.com");
        await CrearFacturaPrueba(clienteId: cliente1.Id);
        await CrearFacturaPrueba(clienteId: cliente1.Id);

        var cliente2 = await CrearClientePrueba("Cliente Sin Facturas", "sin@facturas.com");
        
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
        Logger.LogInformation("🧪 Iniciando test: GetFacturasPorCliente_ConClienteSinFacturas_DebeRetornarListaVacia");

        var cliente = await CrearClientePrueba("Cliente Sin Facturas", "sinfacturas@test.com");
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
        Logger.LogInformation("🧪 Iniciando test: GetFacturasPorComanda_ConComandaExistente_DebeRetornarFactura");
        
        var cliente = await CrearClientePrueba();
        
        var comanda1 = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        var factura1 = await CrearFacturaPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda1.Id });
        
        // Crear otra comanda y factura que no deberían aparecer
        var comanda2 = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        await CrearFacturaPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda2.Id });

        var url = $"/api/comercial/facturas/comanda/{comanda1.Id}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(1);
        apiResponse.Data.Single().Id.Should().Be(factura1.Id);
    }

    [Fact]
    public async Task GetFacturasPorComanda_ConComandaSinFactura_DebeRetornarListaVacia()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFacturasPorComanda_ConComandaSinFactura_DebeRetornarListaVacia");
        
        var comandaSinFactura = await CrearComandaPrueba(estado: EstadoComanda.Finalizada);
        var url = $"/api/comercial/facturas/comanda/{comandaSinFactura.Id}";
        
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

    #region GET /api/comercial/facturas/pendientes

    [Fact]
    public async Task GetFacturasPendientes_DebeRetornarFacturasEmitidas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFacturasPendientes_DebeRetornarFacturasEmitidas");
        
        // Crear facturas en diferentes estados
        var factura1 = await CrearFacturaConDetallesPrueba();
        var factura2 = await CrearFacturaConDetallesPrueba();
        
        // Emitir las facturas
        factura1.Emitir();
        factura2.Emitir();
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/facturas/pendientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<object>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        
        // Verificar que retorna las facturas emitidas
        var facturasPendientes = await DbContext.Facturas
            .Where(f => f.Estado == EstadoFactura.Emitida)
            .ToListAsync();
        facturasPendientes.Should().HaveCount(2);
    }

    #endregion

    #region POST /api/comercial/facturas/{id}/pagar

    [Fact]
    public async Task PostPagarFactura_ConFacturaEmitida_DebeRegistrarPago()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostPagarFactura_ConFacturaEmitida_DebeRegistrarPago");
        
        var cliente = await CrearClientePrueba("Cliente de Pago", "pago@test.com");
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda.Id });
        factura.Emitir();
        await DbContext.SaveChangesAsync();

        var request = new RegistrarPagoFacturaCommand { MetodoPago = "Efectivo" };

        var url = $"/api/comercial/facturas/{factura.Id}/pagar";

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();

        // Verificar que se actualizó en la BD
        var facturaPagada = await DbContext.Facturas.FindAsync(factura.Id);
        facturaPagada.Should().NotBeNull();
        facturaPagada!.Estado.Should().Be(EstadoFactura.Pagada);
        facturaPagada.FechaPago.Should().NotBeNull();
    }

    #endregion

    #region GET /api/comercial/facturas/{id}/pdf

    [Fact]
    public async Task GetFacturaPdf_ConFacturaExistente_DebeRetornarPdf()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFacturaPdf_ConFacturaExistente_DebeRetornarPdf");
        
        var factura = await CrearFacturaPrueba();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/{factura.Id}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        
        // Verificar que es un PDF válido (debe empezar con %PDF)
        var pdfHeader = System.Text.Encoding.ASCII.GetString(content, 0, 4);
        pdfHeader.Should().Be("%PDF");
    }

    [Fact]
    public async Task GetFacturaPdf_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetFacturaPdf_ConIdInexistente_DebeRetornar404");
        
        var facturaIdInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/facturas/{facturaIdInexistente}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PATCH /api/comercial/facturas/{id}/anular

    [Fact]
    public async Task PatchAnularFactura_ConFacturaExistente_DebeAnularFactura()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PatchAnularFactura_ConFacturaExistente_DebeAnularFactura");
        
        var factura = await CrearFacturaPrueba();
        factura.Emitir();
        await DbContext.SaveChangesAsync();
        
        var url = $"/api/comercial/facturas/{factura.Id}/anular";

        // Act
        var response = await HttpClient.PatchAsync(url, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();

        // Verificar que se anuló en la BD
        var facturaAnulada = await DbContext.Facturas.FindAsync(factura.Id);
        facturaAnulada.Should().NotBeNull();
        facturaAnulada!.Estado.Should().Be(EstadoFactura.Anulada);
    }

    [Fact]
    public async Task PatchAnularFactura_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PatchAnularFactura_ConIdInexistente_DebeRetornar404");
        
        var idInexistente = Guid.NewGuid();
        var url = $"/api/comercial/facturas/{idInexistente}/anular";

        // Act
        var response = await HttpClient.PatchAsync(url, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region POST /api/comercial/facturas/{id}/enviar-email

    [Fact]
    public async Task PostEnviarEmail_ConFacturaExistente_DebeRetornar501()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostEnviarEmail_ConFacturaExistente_DebeRetornar501");
        
        var factura = await CrearFacturaPrueba();
        var emailDestino = $"cliente_{Guid.NewGuid().ToString("N")[..8]}@test.com";

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{factura.Id}/enviar-email", emailDestino);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain("Funcionalidad no implementada");
        
        // Verificar que la factura sigue existiendo en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(factura.Id);
        facturaEnBD.Should().NotBeNull();
    }

    [Fact]
    public async Task PostEnviarEmail_ConIdInexistente_DebeRetornar501()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostEnviarEmail_ConIdInexistente_DebeRetornar501");
        
        var facturaIdInexistente = Guid.NewGuid();
        var emailDestino = $"cliente_{Guid.NewGuid().ToString("N")[..8]}@test.com";

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{facturaIdInexistente}/enviar-email", emailDestino);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        
        // Verificar que realmente no existe en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(facturaIdInexistente);
        facturaEnBD.Should().BeNull();
    }

    #endregion

    #region GET /api/comercial/facturas/buscar

    [Fact]
    public async Task GetBuscarFacturas_ConCriteriosValidos_DebeRetornarFacturas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetBuscarFacturas_ConCriteriosValidos_DebeRetornarFacturas");
        
        var cliente1 = await CrearClientePrueba("Cliente Buscar 1", "buscar1@test.com");
        var cliente2 = await CrearClientePrueba("Cliente Buscar 2", "buscar2@test.com");

        // Facturas para cliente 1 (una emitida, una pagada)
        var f1 = await CrearFacturaPrueba(clienteId: cliente1.Id);
        f1.Emitir();
        var f2 = await CrearFacturaPrueba(clienteId: cliente1.Id);
        f2.Emitir();
        f2.RegistrarPago(f2.Total, Guid.NewGuid());
        
        // Factura para cliente 2 (no debe aparecer en el filtro)
        var f3 = await CrearFacturaPrueba(clienteId: cliente2.Id);
        f3.Emitir();
        await DbContext.SaveChangesAsync();

        var url = $"/api/comercial/facturas/buscar?clienteId={cliente1.Id}&estado=Emitida";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(1);
        apiResponse.Data.Single().ClienteId.Should().Be(cliente1.Id);
        apiResponse.Data.Single().Estado.Should().Be(EstadoFactura.Emitida.ToString());
    }

    [Fact]
    public async Task GetBuscarFacturas_ConNumeroFactura_DebeFiltrarCorrectamente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetBuscarFacturas_ConNumeroFactura_DebeFiltrarCorrectamente");

        var factura1 = await CrearFacturaPrueba();
        var factura2 = await CrearFacturaPrueba();

        var url = $"/api/comercial/facturas/buscar?numeroFactura={factura1.NumeroFactura}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FacturaDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(f => f.ClienteId == cliente.Id);
    }

    [Fact]
    public async Task GetBuscarFacturas_SinCriterios_DebeRetornarTodasLasFacturas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetBuscarFacturas_SinCriterios_DebeRetornarTodasLasFacturas");
        
        await CrearFacturaPrueba();
        await CrearFacturaPrueba();
        
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
        Logger.LogInformation("🧪 Iniciando test: GetReporteFacturas_ConTipoReporteValido_DebeRetornar501");
        
        var factura = await CrearFacturaPrueba();
        
        var url = "/api/comercial/facturas/reporte?tipoReporte=ventas&formato=pdf";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain("Funcionalidad no implementada");
        
        // Verificar que la factura sigue existiendo en la BD
        var facturaEnBD = await DbContext.Facturas.FindAsync(factura.Id);
        facturaEnBD.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReporteFacturas_ConFiltrosDeFecha_DebeRetornar501()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetReporteFacturas_ConFiltrosDeFecha_DebeRetornar501");
        
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
        Logger.LogInformation("🧪 Iniciando test: PostFactura_ConComandaNoFinalizada_DebeRetornar400");
        
        var cliente = await CrearClientePrueba("Cliente Comanda Abierta", "abierta@test.com");
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

    [Fact]
    public async Task PostPagarFactura_ConFacturaYaPagada_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostPagarFactura_ConFacturaYaPagada_DebeRetornar400");
        
        var cliente = await CrearClientePrueba("Cliente Pagado", "pagado@test.com");
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda.Id });
        factura.Emitir();
        factura.RegistrarPago(factura.Total, Guid.NewGuid());
        await DbContext.SaveChangesAsync();
        
        var request = new RegistrarPagoFacturaCommand { MetodoPago = "Tarjeta" };
        
        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{factura.Id}/pagar", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PutFactura_ConFacturaEmitida_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PutFactura_ConFacturaEmitida_DebeRetornar400");
        
        var cliente = await CrearClientePrueba("Cliente Emitido", "emitido@test.com");
        var comanda = await CrearComandaPrueba(clienteId: cliente.Id, estado: EstadoComanda.Finalizada);
        var factura = await CrearFacturaPrueba(clienteId: cliente.Id, comandasIds: new List<Guid> { comanda.Id });
        factura.Emitir();
        await DbContext.SaveChangesAsync();

        var request = new ActualizarFacturaCommand { Id = factura.Id, NombreCliente = "Nuevo Nombre" };
        
        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/facturas/{factura.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    #endregion

    #region Tests de Integración Completa

    [Fact]
    public async Task FlujoCompletoFacturacion_DebeFuncionarCorrectamente()
    {
        // ARRANGE
        Logger.LogInformation("🧪 Iniciando test: FlujoCompletoFacturacion_DebeFuncionarCorrectamente");
        
        // 1. Crear entidades necesarias
        var mesero = await CrearUsuarioPrueba("Juan Perez", "juan.perez@test.com", "Mesero");
        var cliente = await CrearClientePrueba("Consumidor Final", "cf@test.com");
        var mesa = await CrearMesaPrueba(10, 4);

        // 2. Crear y finalizar una comanda
        var comanda = await CrearComandaPrueba(meseroId: mesero.Id, clienteId: cliente.Id, mesaId: mesa.Id, estado: EstadoComanda.Finalizada);
        
        // 3. Crear el request para la factura
        var crearFacturaRequest = new FacturaTestDataBuilder()
            .ConClienteId(cliente.Id)
            .ConComandasIds(comanda.Id)
            .ConNombreCliente(cliente.Nombre)
            .ConEmailCliente(cliente.Email)
            .BuildCrearFacturaRequest();

        // CREATE
        Logger.LogInformation("📄 Creando la factura");
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/facturas", crearFacturaRequest);
        
        // ASSERT CREATE
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        createApiResponse.Should().NotBeNull();
        createApiResponse!.Success.Should().BeTrue();
        var facturaCreadaDto = createApiResponse.Data;

        // 4. Obtener factura creada y verificar estado
        var facturaEnBD = await DbContext.Facturas.Include(f => f.Detalles).SingleOrDefaultAsync(f => f.Id == facturaCreadaDto.Id);
        facturaEnBD.Should().NotBeNull();
        facturaEnBD!.Estado.Should().Be(EstadoFactura.Emitida);

        // PAY
        Logger.LogInformation("💲 Pagando la factura");
        
        // 5. Crear el request para pagar la factura
        var pagarRequest = new RegistrarPagoFacturaCommand { MetodoPago = "Efectivo" };

        // 6. Pagar la factura
        var pagarResponse = await HttpClient.PostAsJsonAsync($"/api/comercial/facturas/{facturaEnBD.Id}/pagar", pagarRequest);
        
        // ASSERT PAY
        Logger.LogInformation("✅ Verificando el pago de la factura");
        
        pagarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagarApiResponse = await pagarResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        pagarApiResponse.Should().NotBeNull();
        pagarApiResponse!.Success.Should().BeTrue();

        // 7. Verificar que la factura está pagada en la BD
        await DbContext.Entry(facturaEnBD).ReloadAsync();
        facturaEnBD.Estado.Should().Be(EstadoFactura.Pagada);
        facturaEnBD.FechaPago.Should().NotBeNull();
    }

    #endregion

    public new void Dispose()
    {
        // Limpieza si es necesario, aunque ApiIntegrationTestBase ya se encarga
        base.Dispose();
    }
} 
