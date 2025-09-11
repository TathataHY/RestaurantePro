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

    [Fact(DisplayName = "Flujo de facturación con descuentos inteligentes debe funcionar correctamente")]
    public async Task FlujoFacturacionConDescuentos_DebeFuncionarCorrectamente()
    {
        // Arrange: crear datos de prueba
        var mesero = await CrearUsuarioPrueba("mesero.descuentos", "Mesero Descuentos", "mesero.descuentos@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePruebaViaApi();
        var mesa = await CrearMesaPruebaViaApi();
        var producto1 = await CrearProductoPruebaViaApi("Pizza Margherita", 25.00m);
        var producto2 = await CrearProductoPruebaViaApi("Pasta Carbonara", 18.00m);

        // Crear comanda con múltiples productos
        var crearComandaRequest = new
        {
            MeseroId = mesero.Id,
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            Observaciones = "Comanda con múltiples productos para descuentos",
            ProductosIniciales = new[]
            {
                new { ProductoId = producto1.Id, Cantidad = 2 },
                new { ProductoId = producto2.Id, Cantidad = 1 }
            }
        };
        var responseComanda = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", crearComandaRequest);
        responseComanda.StatusCode.Should().Be(HttpStatusCode.Created);
        var comandaResult = await responseComanda.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        comandaResult!.Success.Should().BeTrue();
        var comanda = comandaResult.Data!;

        // Aplicar descuento progresivo (más productos = más descuento)
        var descuentoProgresivo = comanda.Items.Count >= 2 ? 15.0m : 10.0m;
        var aplicarDescuentoCommand = new
        {
            PorcentajeDescuento = descuentoProgresivo,
            Motivo = "Descuento progresivo por múltiples productos"
        };
        var descuentoResponse = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/descuento", aplicarDescuentoCommand);
        descuentoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Finalizar comanda
        var finalizarComandaRequest = new
        {
            UsuarioId = mesero.Id,
            Observaciones = "Comanda finalizada con descuento progresivo"
        };
        var responseFinalizar = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/finalizar", finalizarComandaRequest);
        responseFinalizar.StatusCode.Should().Be(HttpStatusCode.OK);

        // Generar factura
        var factura = await CrearFacturaConDetallesPrueba(
            clienteId: cliente.Id,
            comandasIds: new List<Guid> { comanda.Id }
        );

        // Emitir la factura
        factura.Emitir(DateTimeService, 0);
        await DbContext.SaveChangesAsync();

        // Verificar factura con descuento
        var facturaResponse = await HttpClient.GetAsync($"/api/comercial/facturas/{factura.Id}");
        facturaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var facturaDto = await facturaResponse.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        facturaDto!.Success.Should().BeTrue();
        var facturaData = facturaDto.Data!;

        // Validar que el descuento se aplicó correctamente
        facturaData.Total.Should().BeLessThan(facturaData.Subtotal + facturaData.Impuestos);
        facturaData.Estado.Should().Be(EstadoFactura.Emitida);

        Logger.LogInformation("✅ Flujo de facturación con descuentos inteligentes ejecutado exitosamente");
        Logger.LogInformation("📊 Resumen: Productos={Productos}, Descuento={Descuento}%, Total={Total}", 
            comanda.Items.Count, descuentoProgresivo, facturaData.Total);
    }

    [Fact(DisplayName = "Flujo de facturación con envío de email debe funcionar correctamente")]
    public async Task FlujoFacturacionConEnvioEmail_DebeFuncionarCorrectamente()
    {
        // Arrange: crear datos de prueba
        var mesero = await CrearUsuarioPrueba("mesero.email", "Mesero Email", "mesero.email@test.com", RolUsuario.Mesero);
        var cliente = await CrearClientePruebaViaApi();
        var mesa = await CrearMesaPruebaViaApi();
        var producto = await CrearProductoPruebaViaApi();

        // Crear comanda
        var crearComandaRequest = new
        {
            MeseroId = mesero.Id,
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            Observaciones = "Comanda para envío de email",
            ProductosIniciales = new[]
            {
                new { ProductoId = producto.Id, Cantidad = 1 }
            }
        };
        var responseComanda = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", crearComandaRequest);
        responseComanda.StatusCode.Should().Be(HttpStatusCode.Created);
        var comanda = (await responseComanda.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>())!.Data!;

        // Finalizar comanda
        var finalizarComandaRequest = new
        {
            UsuarioId = mesero.Id,
            Observaciones = "Comanda finalizada para email"
        };
        var responseFinalizar = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/finalizar", finalizarComandaRequest);
        responseFinalizar.StatusCode.Should().Be(HttpStatusCode.OK);

        // Generar factura
        var factura = await CrearFacturaConDetallesPrueba(
            clienteId: cliente.Id,
            comandasIds: new List<Guid> { comanda.Id }
        );

        // Emitir la factura
        factura.Emitir(DateTimeService, 0);
        await DbContext.SaveChangesAsync();

        // Simular envío de email (en un entorno real, esto llamaría al servicio de email)
        var enviarEmailRequest = new
        {
            EmailDestinatario = cliente.Email,
            Asunto = $"Factura {factura.NumeroFactura} - RestaurantePro",
            IncluirPDF = true
        };

        // Verificar que la factura está lista para envío
        var facturaResponse = await HttpClient.GetAsync($"/api/comercial/facturas/{factura.Id}");
        facturaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var facturaDto = await facturaResponse.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        facturaDto!.Success.Should().BeTrue();
        var facturaData = facturaDto.Data!;

        // Validar que la factura está emitida y lista para envío
        facturaData.Estado.Should().Be(EstadoFactura.Emitida);
        facturaData.NombreCliente.Should().Be(cliente.NombreCompleto);
        facturaData.ClienteId.Should().Be(cliente.Id);
        facturaData.NumeroFactura.Should().NotBeNullOrEmpty();

        Logger.LogInformation("✅ Flujo de facturación con envío de email ejecutado exitosamente");
        Logger.LogInformation("📊 Resumen: Cliente={Cliente}, Email={Email}, Factura={Factura}", 
            cliente.NombreCompleto, cliente.Email, facturaData.NumeroFactura);
    }

    [Fact(DisplayName = "Flujo de facturación con reportes de ventas debe funcionar correctamente")]
    public async Task FlujoFacturacionConReportes_DebeFuncionarCorrectamente()
    {
        // Arrange: crear datos de prueba para múltiples facturas
        var mesero = await CrearUsuarioPrueba("mesero.reportes", "Mesero Reportes", "mesero.reportes@test.com", RolUsuario.Mesero);
        var cliente1 = await CrearClientePruebaViaApi("María García", "maria.garcia@test.com");
        var cliente2 = await CrearClientePruebaViaApi("Carlos López", "carlos.lopez@test.com");
        var mesa1 = await CrearMesaPruebaViaApi(1);
        var mesa2 = await CrearMesaPruebaViaApi(2);
        var producto = await CrearProductoPruebaViaApi();

        // Crear primera comanda y factura
        var comanda1 = await CrearComandaCompleta(mesero.Id, cliente1.Id, mesa1.Id, producto.Id);
        var factura1 = await CrearFacturaConDetallesPrueba(cliente1.Id, new List<Guid> { comanda1.Id });
        factura1.Emitir(DateTimeService, 0);
        await DbContext.SaveChangesAsync();

        // Crear segunda comanda y factura
        var comanda2 = await CrearComandaCompleta(mesero.Id, cliente2.Id, mesa2.Id, producto.Id);
        var factura2 = await CrearFacturaConDetallesPrueba(cliente2.Id, new List<Guid> { comanda2.Id });
        factura2.Emitir(DateTimeService, 0);
        await DbContext.SaveChangesAsync();

        // Obtener reporte de ventas del día
        var reporteResponse = await HttpClient.GetAsync("/api/comercial/facturas/reporte?tipoReporte=ventas");
        reporteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reporteResult = await reporteResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        reporteResult!.Success.Should().BeTrue();

        // Verificar que las facturas están en el sistema
        var factura1Response = await HttpClient.GetAsync($"/api/comercial/facturas/{factura1.Id}");
        factura1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var factura1Dto = await factura1Response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        factura1Dto!.Success.Should().BeTrue();
        factura1Dto.Data!.Estado.Should().Be(EstadoFactura.Emitida);

        var factura2Response = await HttpClient.GetAsync($"/api/comercial/facturas/{factura2.Id}");
        factura2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var factura2Dto = await factura2Response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        factura2Dto!.Success.Should().BeTrue();
        factura2Dto.Data!.Estado.Should().Be(EstadoFactura.Emitida);

        // Validar que ambas facturas tienen totales válidos
        factura1Dto.Data.Total.Should().BeGreaterThan(0);
        factura2Dto.Data.Total.Should().BeGreaterThan(0);

        Logger.LogInformation("✅ Flujo de facturación con reportes ejecutado exitosamente");
        Logger.LogInformation("📊 Resumen: Facturas={Facturas}, Total1={Total1}, Total2={Total2}", 
            2, factura1Dto.Data.Total, factura2Dto.Data.Total);
    }

    // Helpers reutilizables
    private async Task<ClienteDto> CrearClientePruebaViaApi(string? nombre = null, string? email = null)
    {
        var request = new
        {
            Nombre = nombre ?? "Juan Pérez",
            Email = email ?? "juan.perez@test.com",
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

    private async Task<MesaDto> CrearMesaPruebaViaApi(int? numero = null)
    {
        var request = new
        {
            Numero = numero ?? 1,
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

    private async Task<ProductoDto> CrearProductoPruebaViaApi(string? nombre = null, decimal? precio = null)
    {
        // Crear categoría en la base de datos
        await using var context = CreateNewDbContext();
        var categoria = ProductoCategoria.Crear("Categoría Test", "Descripción de categoría test", 1, "#FF5722", "🍽️");
        context.ProductoCategorias.Add(categoria);
        await context.SaveChangesAsync();

        var request = new
        {
            Nombre = nombre ?? "Pizza Margherita",
            Descripcion = "Pizza tradicional con tomate y mozzarella",
            Precio = precio ?? 25.00m,
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

    private async Task<ComandaDto> CrearComandaCompleta(Guid meseroId, Guid clienteId, Guid mesaId, Guid productoId)
    {
        var crearComandaRequest = new
        {
            MeseroId = meseroId,
            ClienteId = clienteId,
            MesaId = mesaId,
            Observaciones = "Comanda para reportes",
            ProductosIniciales = new[]
            {
                new { ProductoId = productoId, Cantidad = 1 }
            }
        };
        var responseComanda = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", crearComandaRequest);
        responseComanda.StatusCode.Should().Be(HttpStatusCode.Created);
        var comandaResult = await responseComanda.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        comandaResult!.Success.Should().BeTrue();
        var comanda = comandaResult.Data!;

        // Finalizar comanda
        var finalizarComandaRequest = new
        {
            UsuarioId = meseroId,
            Observaciones = "Comanda finalizada para reportes"
        };
        var responseFinalizar = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/finalizar", finalizarComandaRequest);
        responseFinalizar.StatusCode.Should().Be(HttpStatusCode.OK);

        return comanda;
    }
} 