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
}


