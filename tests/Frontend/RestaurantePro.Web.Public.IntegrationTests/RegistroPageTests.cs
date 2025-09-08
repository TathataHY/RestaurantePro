using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.IntegrationTests;

public class RegistroPageTests : TestContext
{
    [Fact]
    public void Registro_Deberia_Enviar_Formulario_Y_Mostrar_Exito()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "http://localhost/api/public/clientes")
            .Respond("application/json", "{ \"success\": true }");

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ClientesPublicApiService>();

        // Act
        var cut = RenderComponent<Pages.Registro>();

        cut.Find("input[placeholder='Nombre completo']").Change("Juan Pérez");
        cut.Find("input[placeholder='Email']").Change("juan@example.com");
        cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56912345678");
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("¡Registro exitoso!");
        });
    }
}


