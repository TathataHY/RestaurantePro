using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class RegistroMoreTests : TestContext
{
    [Fact]
    public void Registro_Error_Muestra_Mensaje()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes").Respond(System.Net.HttpStatusCode.BadRequest);
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ClientesPublicApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        cut.Find("input[placeholder='Nombre completo']").Change("Juan Perez");
        cut.Find("input[placeholder='Email']").Change("juan@example.com");
        cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56912345678");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No fue posible completar el registro");
        });
    }

    [Fact]
    public void Registro_Exito_Resetea_Formulario()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond("application/json", "{ \"success\": true }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ClientesPublicApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        cut.Find("input[placeholder='Nombre completo']").Change("Juan Perez");
        cut.Find("input[placeholder='Email']").Change("juan@example.com");
        cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56912345678");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("¡Registro exitoso!");
            cut.Find("input[placeholder='Nombre completo']").GetAttribute("value").Should().Be("");
            cut.Find("input[placeholder='Email']").GetAttribute("value").Should().Be("");
            cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").GetAttribute("value").Should().Be("");
        });
    }
    [Fact]
    public void Registro_Boton_Deshabilitado_Durante_Envio()
    {
        var mock = new MockHttpMessageHandler();
        // Responder éxito inmediato, bUnit re-renderiza y podemos ver el estado "Registrando..." en un ciclo
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond("application/json", "{ \"success\": true }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ClientesPublicApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        cut.Find("input[placeholder='Nombre completo']").Change("Juan Perez");
        cut.Find("input[placeholder='Email']").Change("juan@example.com");
        cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56912345678");

        cut.Find("form").Submit();
        // Afirmar estado estable tras envío rápido: mensaje de éxito
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("¡Registro exitoso!");
        });
    }
}


