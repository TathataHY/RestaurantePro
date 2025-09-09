using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Services;

namespace RestaurantePro.Mobile.UnitTests.ViewModels;

public class DailyPreparationsViewModel_CreateNavigationTests
{
    private readonly Mock<IDailyPreparationsService> _mockService = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();

    private DailyPreparationsViewModel CreateVm()
    {
        return new DailyPreparationsViewModel(_mockService.Object, _mockDialog.Object, _mockNav.Object);
    }

    [Fact]
    public async Task CrearNuevaPreparacionCommand_ShouldNavigateToCreatePage()
    {
        var vm = CreateVm();

        await vm.CrearNuevaPreparacionCommand.ExecuteAsync(null);

        _mockNav.Verify(x => x.NavigateToAsync("crear-preparacion-diaria"), Times.Once);
    }
}


