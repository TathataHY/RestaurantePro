using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class CreateDailyPreparationEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public CreateDailyPreparationEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CrearPreparacion_EndToEnd_ShouldSucceed()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var daily = new DailyPreparationsService(api, auth, NullLogger<DailyPreparationsService>.Instance);
        var productosService = new RestaurantePro.Mobile.Core.Services.Productos.ProductosService(api, auth);
        var dialog = new Mock<IDialogService>();
        var nav = new FakeNavigationService();

        // Login
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);

        // Obtener un producto válido del seed (usando ProductosService que maneja paginación)
        var productosResponse = await productosService.ObtenerProductosPaginadosAsync(1, 1, null, true);
        productosResponse.Success.Should().BeTrue();
        productosResponse.Data.Should().NotBeNull();
        productosResponse.Data!.Any().Should().BeTrue();
        var productoId = productosResponse.Data!.First().Id;

        // Crear VM de creación
        var vm = new CreateDailyPreparationViewModel(daily, auth, dialog.Object, nav, productosService)
        {
            ProductoIdText = productoId.ToString(),
            Cantidad = 2,
            FechaVencimiento = DateTime.Today.AddDays(1),
            Observaciones = "test"
        };

        await vm.CrearPreparacionCommand.ExecuteAsync(null);

        dialog.Verify(x => x.ShowSuccessAsync(It.Is<string>(m => m.Contains("Preparación creada"))), Times.Once);
    }
}


