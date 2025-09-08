using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class SecurityXssTests : TestContext
{
	[Fact]
	public void Reviews_Renderiza_Comentario_Escapado()
	{
		var mock = new MockHttpMessageHandler();
		mock.When("http://localhost/api/public/reviews")
			.Respond("application/json", "{ \"success\": true, \"data\": [{ \"nombre\": \"Ana\", \"comentario\": \"<script>alert('x')</script>\", \"valoracion\": 5, \"fecha\": \"2024-01-01T00:00:00Z\" }] }");
		Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
		Services.AddScoped<ReviewsApiService>();

		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reviews>();
		cut.WaitForAssertion(() =>
		{
			// Debe mostrarse escapado en el HTML y no como HTML crudo
			cut.Markup.Should().Contain("&lt;script&gt;alert('x')&lt;/script&gt;");
			cut.Markup.Should().NotContain("<script>");
		});
	}

	[Fact]
	public void Contact_Renderiza_Mensaje_Escapado()
	{
		var mock = new MockHttpMessageHandler();
		mock.When("http://localhost/api/public/contact/messages")
			.Respond("application/json", "{ \"success\": true, \"data\": [{ \"nombre\": \"Ana\", \"email\": \"a@a.com\", \"asunto\": \"<b>bold</b>\", \"mensaje\": \"<img src=x onerror=alert(1)>\", \"fecha\": \"2024-01-01T00:00:00Z\" }] }");
		Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
		Services.AddScoped<ContactApiService>();

		var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();
		cut.WaitForAssertion(() =>
		{
			// Tanto el asunto como el mensaje deben estar escapados y no interpretados como HTML
			cut.Markup.Should().Contain("&lt;b&gt;bold&lt;/b&gt;");
			cut.Markup.Should().NotContain("<b>");

			cut.Markup.Should().Contain("&lt;img src=x onerror=alert(1)&gt;");
			// Al estar escapado, no debe existir una etiqueta <img> real
			cut.Markup.Should().NotContain("<img ");
		});
	}
}


