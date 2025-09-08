using FluentAssertions;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class ServicesTests
{
    [Fact]
    public async Task PromocionesApiService_Constructs_Url_And_Parses_List()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Respond("application/json",
                "{ \"success\": true, \"data\": [{ \"id\": \"00000000-0000-0000-0000-000000000001\", \"codigo\": \"PROMO10\", \"nombre\": \"Promo\", \"descripcion\": \"\", \"tipo\": \"P\", \"valorDescuento\": 10, \"montoMinimo\": 0, \"fechaInicio\": \"2024-01-01T00:00:00Z\", \"fechaFin\": \"2030-01-01T00:00:00Z\", \"estaVigente\": true }] }");
        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new PromocionesApiService(http);

        var list = await svc.ObtenerAsync(true, "FechaCreacion", "desc");

        list.Should().NotBeNull();
        list.Should().HaveCount(1);
        list[0].Codigo.Should().Be("PROMO10");
    }

    [Fact]
    public async Task PromocionesApiService_Null_Response_Returns_Empty_List()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Respond("application/json", "null");
        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new PromocionesApiService(http);

        var list = await svc.ObtenerAsync();
        list.Should().NotBeNull();
        list.Should().HaveCount(0);
    }

    [Fact]
    public async Task PromocionesApiService_Error_De_Red_Retorna_Vacio()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Throw(new HttpRequestException("Network error"));
        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new PromocionesApiService(http);

        var list = await svc.ObtenerAsync();
        list.Should().NotBeNull();
        list.Should().HaveCount(0);
    }

    [Fact]
    public async Task ClientesPublicApiService_Returns_True_On_Success()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond("application/json", "{ \"success\": true }");
        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ClientesPublicApiService(http);

        var ok = await svc.RegistrarAsync(new PublicClienteRegisterRequest { Nombre = "A", Email = "a@a.com", Telefono = "+569", FechaNacimiento = DateTime.UtcNow.AddYears(-20) });
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task ClientesPublicApiService_Returns_False_On_Error()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes").Respond(System.Net.HttpStatusCode.BadRequest);
        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ClientesPublicApiService(http);

        var ok = await svc.RegistrarAsync(new PublicClienteRegisterRequest { Nombre = "A", Email = "a@a.com", Telefono = "+569", FechaNacimiento = DateTime.UtcNow.AddYears(-20) });
        ok.Should().BeFalse();
    }
}


