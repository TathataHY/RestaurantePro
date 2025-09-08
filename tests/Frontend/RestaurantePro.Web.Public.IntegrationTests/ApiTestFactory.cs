using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Web.Public.IntegrationTests;

public class ApiTestFactory : IDisposable
{
	private readonly TestWebApplicationFactory _factory;
	public HttpClient Client { get; }

	public ApiTestFactory()
	{
		_factory = new TestWebApplicationFactory();
		Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});
	}

	public void Dispose()
	{
		Client.Dispose();
		_factory.Dispose();
	}
}
