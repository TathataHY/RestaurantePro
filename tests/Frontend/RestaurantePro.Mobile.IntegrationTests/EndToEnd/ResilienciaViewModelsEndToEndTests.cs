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

public class ResilienciaViewModelsEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public ResilienciaViewModelsEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task LoadEstadisticas_Should_ShowError_When_ApiTimeoutOrFailure()
    {
        // HttpClient apuntando a host inalcanzable para forzar error de red/timeout
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.255.255.1/"),
            Timeout = TimeSpan.FromMilliseconds(200)
        };

        var api = new ApiService(httpClient);
        var secure = new FakeSecureStorageService();
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, secure, new FakeNavigationService());
        var dailyService = new DailyPreparationsService(api, auth, NullLogger<DailyPreparationsService>.Instance);

        var mockDialog = new Mock<IDialogService>();
        var vm = new DailyPreparationsViewModel(dailyService, mockDialog.Object, new FakeNavigationService());

        // Act: ejecución del comando que debe manejar el error y notificar al usuario
        await vm.LoadEstadisticasCommand.ExecuteAsync(null);

        // Assert: se muestra un error al usuario
        mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.AtLeastOnce());
    }
}


