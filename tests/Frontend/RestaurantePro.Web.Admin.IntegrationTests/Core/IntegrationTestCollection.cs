using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Core;

/// <summary>
/// Colección de tests de integración que comparten la misma instancia de WebApplicationFactory
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<WebApplicationFactory>
{
    // Esta clase no necesita implementar nada, solo define la colección
}
