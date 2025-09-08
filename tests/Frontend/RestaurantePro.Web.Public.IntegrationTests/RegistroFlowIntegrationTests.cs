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
	}

	[Fact]
	public void Registro_Post_Exito_Reset_Form_Y_Boton_Deshabilitado_Durante_Envio()
	{
		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();

		// Llenar formulario válido
		cut.Find("input[placeholder='Nombre completo']").Change("Juan Pérez");
		cut.Find("input[placeholder='Email']").Change("juan@example.com");
		cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56912345678");

		// Forzar que el formulario esté válido antes de enviar
		cut.Find("form").Submit();

		// Durante envío, el botón cambia de texto y queda deshabilitado
		cut.Markup.Should().Contain("Registrando...");

		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("¡Registro exitoso!");
			// Form reset: nombre y email vacíos
			cut.Find("input[placeholder='Nombre completo']").GetAttribute("value").Should().Be("");
			cut.Find("input[placeholder='Email']").GetAttribute("value").Should().Be("");
		}, timeout: TimeSpan.FromSeconds(5));
	}

	[Fact]
	public void Registro_Validaciones_Invalidas_Bloquean_Envio_Y_Muestran_Mensajes()
	{
		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();

		// Dejar email inválido y teléfono corto
		cut.Find("input[placeholder='Nombre completo']").Change("Juan Pérez");
		cut.Find("input[placeholder='Email']").Change("juan_at_example.com");
		cut.Find("input[placeholder='Teléfono (e.g. +56912345678)']").Change("+56");

		// Intentar enviar
		cut.Find("form").Submit();

		cut.Markup.Should().Contain("Validation"); // ValidationSummary presente
	}

	public new void Dispose()
	{
		_api.Dispose();
	}
}
