using RestaurantePro.Api.IntegrationTests.TestBase;
using System.Text.Json;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Public;

[Collection(nameof(ApiIntegrationTestCollection))]
public class PromocionesPublicControllerTests : ApiIntegrationTestBase
{
    public PromocionesPublicControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ObtenerPromocionesPublicas_DeberiaRetornar200YLista()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync("/api/public/promociones?soloVigentes=true");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var success = root.GetProperty("Success").GetBoolean();
        var data = root.GetProperty("Data");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        success.Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }
}


