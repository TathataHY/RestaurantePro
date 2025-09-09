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

public class ConsumeDailyPreparationEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public ConsumeDailyPreparationEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ConsumirPreparacion_EndToEnd_ShouldDecreaseDisponible()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var daily = new DailyPreparationsService(api, auth, NullLogger<DailyPreparationsService>.Instance);
        var dialog = new Mock<IDialogService>();
        var vm = new DailyPreparationsViewModel(daily, dialog.Object, new FakeNavigationService());

        // Login
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);

        // Cargar lista
        await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

        // Si no hay, crear una
        if (vm.PreparacionesDiarias.Count == 0)
        {
            var productos = new RestaurantePro.Mobile.Core.Services.Productos.ProductosService(api, auth);
            var prod = await productos.ObtenerProductosPaginadosAsync(1, 1, null, true);
            Assert.True(prod.Success);
            var createVm = new CreateDailyPreparationViewModel(daily, auth, dialog.Object, new FakeNavigationService(), productos)
            {
                ProductoIdText = prod.Data!.First().Id.ToString(),
                Cantidad = 3,
                FechaVencimiento = DateTime.Today.AddDays(1)
            };
            await createVm.CrearPreparacionCommand.ExecuteAsync(null);
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);
        }

        var prep = vm.PreparacionesDiarias.First();
        var disponibleAntes = prep.CantidadDisponible;

        // Consumir 1 unidad
        dialog
            .Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("1");

        await vm.ConsumirPreparacionCommand.ExecuteAsync(prep);

        // Recargar y verificar
        await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);
        var prepActualizada = vm.PreparacionesDiarias.First(p => p.Id == prep.Id);
        Assert.True(prepActualizada.CantidadDisponible <= disponibleAntes - 1);
    }
}


