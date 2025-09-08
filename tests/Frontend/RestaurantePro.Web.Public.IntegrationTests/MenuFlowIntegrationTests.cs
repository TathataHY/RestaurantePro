using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.IntegrationTests;

public class MenuFlowIntegrationTests : TestContext, IDisposable
{
	private readonly ApiTestFactory _api;

	public MenuFlowIntegrationTests()
	{
		_api = new ApiTestFactory();
		Services.AddScoped(sp => _api.Client);
		Services.AddScoped<MenuApiService>();
	}

	[Fact]
	public void Menu_FlujoBase_CategoriasYProductos()
	{
		// Arrange
		// No necesitamos mock, usamos API real en memoria

		// Act
		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Menú");
			cut.Markup.Should().Contain("Categorías");
			// Debe mostrar skeleton o productos tras primera carga
			cut.Markup.Should().MatchRegex("(Cargando|Productos)");
		}, timeout: TimeSpan.FromSeconds(5));
	}

	public new void Dispose()
	{
		_api.Dispose();
	}
}
