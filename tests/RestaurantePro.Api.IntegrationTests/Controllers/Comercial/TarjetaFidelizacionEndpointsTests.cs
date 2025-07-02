using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

public class TarjetaFidelizacionEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TarjetaFidelizacionEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "AuthenticatedUser-Administrador");
    }

    [Fact(DisplayName = "Acumular puntos en tarjeta de fidelización recién creada debe funcionar (flujo mínimo aislado)")]
    public async Task AcumularPuntos_TarjetaRecienCreada_DebeFuncionar()
    {
        // 1. Crear cliente de prueba
        var clienteResponse = await _client.PostAsJsonAsync("/api/comercial/clientes", new { 
            nombre = "Test User", 
            email = "testuser@demo.com",
            telefono = "+1234567890",
            fechaNacimiento = DateTime.Now.AddYears(-25)
        });
        if (!clienteResponse.IsSuccessStatusCode)
        {
            var errorContent = await clienteResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error creando cliente - Status: {clienteResponse.StatusCode}\nContenido: {errorContent}");
        }
        var cliente = await clienteResponse.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        var clienteId = cliente!.Data!.Id;

        // 2. Crear tarjeta de fidelización
        var tarjetaResponse = await _client.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", new { clienteId, tipo = "Premium" });
        if (!tarjetaResponse.IsSuccessStatusCode)
        {
            var errorContent = await tarjetaResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error creando tarjeta - Status: {tarjetaResponse.StatusCode}\nContenido: {errorContent}");
        }
        var tarjeta = await tarjetaResponse.Content.ReadFromJsonAsync<ApiResponse<TarjetaFidelizacionDto>>();
        var tarjetaId = tarjeta!.Data!.Id;

        // 3. Acumular puntos
        var acumularResponse = await _client.PostAsJsonAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}/puntos", new { 
            Puntos = 50,
            Descripcion = "Puntos por compra de prueba",
            UsuarioId = Guid.Parse("12345678-1234-1234-1234-123456789012")
        });
        
        // 4. Verificar respuesta
        if (!acumularResponse.IsSuccessStatusCode)
        {
            var errorContent = await acumularResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error acumulando puntos - Status: {acumularResponse.StatusCode}\nContenido: {errorContent}");
        }
        acumularResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
} 