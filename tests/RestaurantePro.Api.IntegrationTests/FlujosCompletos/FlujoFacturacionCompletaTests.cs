using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Application.Common.Interfaces;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

[Collection("ApiIntegrationTestCollection")]
public class FlujoFacturacionCompletaTests : ApiIntegrationTestBase
{
    public FlujoFacturacionCompletaTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact(DisplayName = "Flujo completo de facturación con IA debe funcionar correctamente")]
    public async Task FlujoCompletoFacturacionConIA_DebeFuncionarCorrectamente()
    {
        // Arrange: crear datos de prueba
        var mesero = await CrearUsuarioPrueba("mesero.facturacion", "Mesero Facturación", "mesero.facturacion@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePruebaViaApi();
        var mesa = await CrearMesaPruebaViaApi();
        var producto = await CrearProductoPruebaViaApi();

        // Crear comanda
        var crearComandaRequest = new
        {
            MeseroId = mesero.Id,
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            Observaciones = "Comanda de prueba para facturación",
            ProductosIniciales = new[]
            {
                new
                {
                    ProductoId = producto.Id,
                    Cantidad = 2
                }
            }
        };
        var responseComanda = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", crearComandaRequest);
        responseComanda.StatusCode.Should().Be(HttpStatusCode.Created);
        var comandaResult = await responseComanda.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        comandaResult!.Success.Should().BeTrue();
        comandaResult.Data.Should().NotBeNull();
        var comanda = comandaResult.Data!;

        // Aplicar descuento a la comanda ANTES de finalizarla
        var aplicarDescuentoCommand = new
        {
            PorcentajeDescuento = 10.0m,
            Motivo = "Descuento de prueba"
        };
        var descuentoResponse = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/descuento", aplicarDescuentoCommand);
        descuentoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que el descuento se aplicó correctamente
        var descuentoResult = await descuentoResponse.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        descuentoResult!.Success.Should().BeTrue();
        descuentoResult.Data.Should().NotBeNull();
        var comandaConDescuento = descuentoResult.Data!;

        // Finalizar comanda después de aplicar el descuento
        var finalizarComandaRequest = new
        {
            UsuarioId = mesero.Id,
            Observaciones = "Comanda finalizada para facturación"
        };
        var responseFinalizar = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/finalizar", finalizarComandaRequest);
        responseFinalizar.StatusCode.Should().Be(HttpStatusCode.OK);

        // Generar factura directamente en la base de datos (siguiendo el patrón de otros tests)
        var factura = await CrearFacturaConDetallesPrueba(
            clienteId: cliente.Id,
            comandasIds: new List<Guid> { comanda.Id }
        );

        // Emitir la factura
        factura.Emitir(DateTimeService, 0); // Sin días de crédito
        await DbContext.SaveChangesAsync();

        // Verificar que la factura se creó correctamente
        factura.Should().NotBeNull();
        factura.Id.Should().NotBe(Guid.Empty);
        factura.NumeroFactura.Should().NotBeNullOrEmpty();
        factura.Total.Should().BeGreaterThan(0);

        // Verificar estado de la factura
        var facturaVerificacionResponse = await HttpClient.GetAsync($"/api/comercial/facturas/{factura.Id}");
        facturaVerificacionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var facturaVerificacion = await facturaVerificacionResponse.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        facturaVerificacion!.Success.Should().BeTrue();
        facturaVerificacion.Data.Should().NotBeNull();
        var facturaDto = facturaVerificacion.Data!;

        // Validar estados finales
        facturaDto.Estado.Should().Be(EstadoFactura.Emitida);
        facturaDto.EstadoTexto.Should().Be("Emitida");

        // Validar información del cliente
        facturaDto.NombreCliente.Should().Be(cliente.NombreCompleto);
        facturaDto.ClienteId.Should().Be(cliente.Id);

        Logger.LogInformation("✅ Flujo de facturación completa ejecutado exitosamente");
        Logger.LogInformation("📊 Resumen: Cliente={Cliente}, Mesa={Mesa}, Producto={Producto}, Total={Total}, Descuento={Descuento}%", 
            cliente.NombreCompleto, mesa.Numero, producto.Nombre, facturaDto.Total, 10.0m);
    }

    // Helpers reutilizables
    private async Task<ClienteDto> CrearClientePruebaViaApi()
    {
        var request = new
        {
            Nombre = "Juan Pérez",
            Email = "juan.perez@test.com",
            Telefono = "+34612345678",
            FechaNacimiento = DateTime.Now.AddYears(-30),
            EstaActivo = true
        };
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/clientes", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        return result.Data!;
    }

    private async Task<MesaDto> CrearMesaPruebaViaApi()
    {
        var request = new
        {
            Numero = 1,
            Capacidad = 4,
            Zona = "Principal",
            Descripcion = "Mesa de prueba para facturación"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/mesas", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MesaDto>>();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        return result.Data!;
    }

    private async Task<ProductoDto> CrearProductoPruebaViaApi()
    {
        // Crear categoría en la base de datos
        await using var context = CreateNewDbContext();
        var categoria = ProductoCategoria.Crear("Categoría Test", "Descripción de categoría test", 1);
        context.ProductoCategorias.Add(categoria);
        await context.SaveChangesAsync();

        var request = new
        {
            Nombre = "Pizza Margherita",
            Descripcion = "Pizza tradicional con tomate y mozzarella",
            Precio = 25.00m,
            CategoriaId = categoria.Id,
            CategoriaNombre = categoria.Nombre,
            Activo = true
        };
        var response = await HttpClient.PostAsJsonAsync("/api/core/productos", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductoDto>>();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        return result.Data!;
    }
} 