using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Public;

[Collection(nameof(ApiIntegrationTestCollection))]
public class ClientesPublicControllerTests : ApiIntegrationTestBase
{
    public ClientesPublicControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RegistrarClientePublico_DeberiaCrearYRetornar201()
    {
        // Arrange
        // Autenticación no requerida, pero removemos header por claridad
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var body = new
        {
            nombre = "Cliente Público",
            email = GenerarEmailValido(),
            telefono = "+56912345678",
            fechaNacimiento = DateTime.Today.AddYears(-25)
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/public/clientes", body);
        var apiResponse = await DeserializeFromContentAsync<ApiResponse<object>>(response.Content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task RegistrarClientePublico_DatosInvalidos_DeberiaRetornar400()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var body = new
        {
            nombre = "",
            email = "correo-invalido",
            telefono = "123",
            fechaNacimiento = DateTime.Today.AddYears(1)
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/public/clientes", body);
        var apiResponse = await DeserializeFromContentAsync<ApiResponse<object>>(response.Content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }
}


