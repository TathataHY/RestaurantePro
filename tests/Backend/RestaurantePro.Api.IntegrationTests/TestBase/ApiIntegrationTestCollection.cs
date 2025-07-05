using Xunit;

namespace RestaurantePro.Api.IntegrationTests.TestBase
{
    [CollectionDefinition("ApiIntegrationTestCollection")]
    public class ApiIntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
    {
        // No code needed here, just the definition for xUnit
    }
} 