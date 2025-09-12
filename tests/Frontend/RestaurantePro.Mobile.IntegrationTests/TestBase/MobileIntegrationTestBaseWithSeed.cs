using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

/// <summary>
/// Clase base para tests de integración móvil que incluye seed automático de datos
/// </summary>
public abstract class MobileIntegrationTestBaseWithSeed : IClassFixture<MobileIntegrationTestFixture>
{
    protected readonly MobileIntegrationTestFixture Fixture;

    protected MobileIntegrationTestBaseWithSeed(MobileIntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Configura el test con seed de datos automático
    /// </summary>
    protected async Task SetupAsync()
    {
        await Fixture.SetupTestWithSeedAsync();
    }
}
