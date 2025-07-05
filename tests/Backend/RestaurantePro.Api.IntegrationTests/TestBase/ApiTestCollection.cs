using Xunit;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

[CollectionDefinition("ApiTestCollection")]
public class ApiTestCollection : ICollectionFixture<TestWebApplicationFactory> { } 