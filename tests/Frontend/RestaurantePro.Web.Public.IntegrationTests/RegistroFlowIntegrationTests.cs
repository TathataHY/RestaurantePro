using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.IntegrationTests;

public class RegistroFlowIntegrationTests : TestContext, IDisposable
{
	private readonly ApiTestFactory _api;

	public RegistroFlowIntegrationTests()
	{
		_api = new ApiTestFactory();
		Services.AddScoped(sp => _api.Client);
		Services.AddScoped<ClientesPublicApiService>();
		JSInterop.Mode = JSRuntimeMode.Loose;
	}

	[Fact]
	public void Registro_Post_Exito_Reset_Form_Y_Boton_Deshabilitado_Durante_Envio()
	{
		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();

		// Llenar formulario válido (inputs en orden: nombre, email, teléfono, fecha)
		cut.FindAll("input")[0].Change("Juan Pérez");
		cut.FindAll("input")[1].Change("juan@example.com");
		cut.FindAll("input")[2].Change("+56912345678");

		// Enviar
		cut.Find("form").Submit();

		// Durante envío
		cut.Markup.Should().Contain("Registrando...");

		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("¡Registro exitoso!");
			cut.FindAll("input")[0].GetAttribute("value").Should().Be("");
			cut.FindAll("input")[1].GetAttribute("value").Should().Be("");
		}, timeout: TimeSpan.FromSeconds(5));
	}

	[Fact]
	public void Registro_Validaciones_Invalidas_Bloquean_Envio_Y_No_Muta_Estado()
	{
		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();

		cut.FindAll("input")[0].Change("Juan Pérez");
		cut.FindAll("input")[1].Change("juan_at_example.com"); // email inválido
		cut.FindAll("input")[2].Change("+56"); // teléfono corto

		cut.Find("form").Submit();

		// No cambia a estado de envío ni éxito
		cut.Markup.Should().Contain("Registrarme");
		cut.Markup.Should().NotContain("Registrando...");
		cut.Markup.Should().NotContain("¡Registro exitoso!");
	}

	public new void Dispose()
	{
		_api.Dispose();
	}
}
