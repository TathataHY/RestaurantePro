using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.IntegrationTests;

public class PromocionesPageTests : TestContext
{
    [Fact]
    public void Promociones_Deberia_Listar_Items()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "http://localhost/api/public/promociones*")
            .Respond("application/json",
                "{ \"success\": true, \"data\": [{ \"id\": \"00000000-0000-0000-0000-000000000001\", \"codigo\": \"PROMO10\", \"nombre\": \"Promo 10%\", \"descripcion\": \"Descuento general\", \"tipo\": \"Porcentaje\", \"valorDescuento\": 10, \"montoMinimo\": 0, \"fechaInicio\": \"2024-01-01T00:00:00Z\", \"fechaFin\": \"2030-01-01T00:00:00Z\", \"estaVigente\": true }] }");

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<PromocionesApiService>();

        // Act
        var cut = RenderComponent<Pages.Promociones>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Promociones");
            cut.Markup.Should().Contain("Promo 10%");
            cut.Markup.Should().Contain("Código: PROMO10");
        });
    }
}


