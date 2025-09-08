using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class PromocionesMoreTests : TestContext
{
    [Fact]
    public void Promociones_Estado_Vacio()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Respond("application/json", "{ \"success\": true, \"data\": [] }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<PromocionesApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Promociones>();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No hay promociones vigentes.");
        });
    }

    [Fact]
    public void Promociones_Manejo_Error_Muestra_Vacio()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Throw(new HttpRequestException("network"));
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<PromocionesApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Promociones>();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No hay promociones vigentes.");
        });
    }

    [Fact]
    public void Promociones_Parametros_URL_Y_EmptyState_Consistente()
    {
        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.ToString());
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"success\": true, \"data\": [] }", System.Text.Encoding.UTF8, "application/json")
                });
            });
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<PromocionesApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Promociones>();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("No hay promociones vigentes."));

        // Verificar que la solicitud incluyó los parámetros por defecto
        var last = captured.Last().ToLowerInvariant();
        last.Should().Contain("solovigentes=true");
        last.Should().Contain("ordenarpor=fechacreacion");
        last.Should().Contain("direccion=desc");
    }

    [Fact]
    public void Promociones_Timeout_TaskCanceled_Retorna_Vacio()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Throw(new TaskCanceledException("timeout"));
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<PromocionesApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Promociones>();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No hay promociones vigentes.");
        });
    }
}


