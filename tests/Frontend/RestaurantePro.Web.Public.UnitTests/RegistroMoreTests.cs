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

    [Fact]
    public void Registro_Boton_Deshabilitado_Formulario_Invalido_Y_Durante_Envio()
    {
        var mock = new MockHttpMessageHandler();
        // Simular un pequeño retraso para poder observar el estado de envío
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond(async () =>
            {
                await Task.Delay(150);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"success\": true }", System.Text.Encoding.UTF8, "application/json")
                };
            });
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ClientesPublicApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        // Formulario inválido al inicio
        var boton = cut.Find("button.btn.btn-primary");
        boton.HasAttribute("disabled").Should().BeTrue();

        // Completar campos válidos
        cut.Find("input[placeholder='Nombre completo']").Change("Juan Perez");
        cut.Find("input[placeholder='Email']").Change("juan@example.com");
        cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56912345678");

        cut.WaitForAssertion(() =>
        {
            cut.Find("button.btn.btn-primary").HasAttribute("disabled").Should().BeFalse();
        });

        // Enviar y validar deshabilitado durante envío
        cut.Find("form").Submit();
        cut.WaitForAssertion(() =>
        {
            cut.Find("button.btn.btn-primary").HasAttribute("disabled").Should().BeTrue();
        });
        // Estado final: éxito mostrado
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("¡Registro exitoso!"));
    }

    [Fact]
    public void Registro_Validaciones_Muestran_Mensajes()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond("application/json", "{ \"success\": true }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ClientesPublicApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        // Dejar email inválido y teléfono corto
        cut.Find("input[placeholder='Nombre completo']").Change("");
        cut.Find("input[placeholder='Email']").Change("correo-invalido");
        cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("123");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ToLowerInvariant().Should().Contain("validation");
        });
    }

    [Fact]
    public void Registro_Timeout_No_Resetea_Formulario_Y_Muestra_Error()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Throw(new TaskCanceledException("timeout"));
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
            cut.Find("input[placeholder='Nombre completo']").GetAttribute("value").Should().Be("Juan Perez");
        });
    }
}


