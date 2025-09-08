using FluentAssertions;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;
using System.Text.Json;

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

    [Fact]
    public async Task ClientesPublicApiService_Serializa_Body_Con_Fechas_Y_Campos()
    {
        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond(async req =>
            {
                var json = await req.Content!.ReadAsStringAsync();
                captured.Add(json);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"success\": true }", System.Text.Encoding.UTF8, "application/json")
                };
            });

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ClientesPublicApiService(http);
        var fecha = new DateTime(1990, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var ok = await svc.RegistrarAsync(new PublicClienteRegisterRequest
        {
            Nombre = "Juan Perez",
            Email = "juan@example.com",
            Telefono = "+56912345678",
            FechaNacimiento = fecha
        });

        ok.Should().BeTrue();
        captured.Should().HaveCount(1);

        using var doc = JsonDocument.Parse(captured[0]);
        var root = doc.RootElement;

        JsonElement GetProp(string pascal, string camel)
        {
            if (root.TryGetProperty(pascal, out var v1)) return v1;
            if (root.TryGetProperty(camel, out var v2)) return v2;
            throw new KeyNotFoundException($"Propiedad no encontrada: {pascal}/{camel}");
        }

        GetProp("Nombre", "nombre").GetString().Should().Be("Juan Perez");
        GetProp("Email", "email").GetString().Should().Be("juan@example.com");
        GetProp("Telefono", "telefono").GetString().Should().Be("+56912345678");
        GetProp("FechaNacimiento", "fechaNacimiento").GetDateTime().Should().Be(fecha);
    }

    [Fact]
    public async Task ContactApiService_ContentType_Es_ApplicationJson()
    {
        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/contact/messages")
            .Respond(async req =>
            {
                var ct = req.Content!.Headers.ContentType!.MediaType;
                captured.Add(ct ?? string.Empty);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"success\": true, \"data\": { } }", System.Text.Encoding.UTF8, "application/json")
                };
            });

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ContactApiService(http);
        var ok = await svc.EnviarAsync(new CreateContactMessageRequest { Nombre = "Ana", Email = "a@a.com", Mensaje = "Hola" });
        ok.Should().BeTrue();
        captured.Should().Contain(ct => ct == "application/json");
    }

    [Fact]
    public async Task ClientesPublicApiService_ContentType_Es_ApplicationJson()
    {
        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond(async req =>
            {
                var ct = req.Content!.Headers.ContentType!.MediaType;
                captured.Add(ct);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"success\": true }", System.Text.Encoding.UTF8, "application/json")
                };
            });

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ClientesPublicApiService(http);
        var ok = await svc.RegistrarAsync(new PublicClienteRegisterRequest { Nombre = "A", Email = "a@a.com", Telefono = "+56912345678", FechaNacimiento = DateTime.UtcNow.AddYears(-20) });
        ok.Should().BeTrue();
        captured.Should().Contain("application/json");
    }

    [Fact]
    public async Task MenuApiService_ObtenerCategorias_Defaults_Querystring()
    {
        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/core/categorias*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.ToString());
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }), System.Text.Encoding.UTF8, "application/json")
                });
            });

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new MenuApiService(http);
        var cats = await svc.ObtenerCategoriasAsync();
        cats.Should().NotBeNull();
        captured.Last().Should().Contain("soloActivas=True");
        captured.Last().Should().Contain("ocultarVacias=True");
    }

    [Fact]
    public async Task PromocionesApiService_Encoding_OrdenarPor_Y_Direccion()
    {
        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/promociones*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.OriginalString);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"success\": true, \"data\": [] }", System.Text.Encoding.UTF8, "application/json")
                });
            });

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new PromocionesApiService(http);

        var ordenarPor = "Fecha Creación"; // contiene espacio y carácter especial
        var direccion = "desc especial";   // contiene espacio
        var _ = await svc.ObtenerAsync(true, ordenarPor, direccion);

        var url = captured.Last();
        url.Should().Contain("soloVigentes=True");
        url.Should().Contain("ordenarPor=");
        url.Should().Contain("direccion=");
        url.Should().Contain("ordenarPor=Fecha%20Creaci%C3%B3n");
        url.Should().Contain("direccion=desc%20especial");
    }
}


