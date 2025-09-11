using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Services;

namespace RestaurantePro.Mobile.UnitTests.ViewModels;

public class DailyPreparationsViewModel_ActionsTests
{
    private readonly Mock<IDailyPreparationsService> _mockService = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();

    private DailyPreparationsViewModel CreateVm()
        => new DailyPreparationsViewModel(_mockService.Object, _mockDialog.Object, _mockNav.Object);

    [Fact]
    public async Task ConsumirPreparacion_ShouldValidateAndCallService()
    {
        var vm = CreateVm();
        var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza", CantidadDisponible = 5 };
        _mockDialog
            .Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("2");
        _mockService
            .Setup(x => x.ConsumirPreparacionDiariaAsync(prep.Id, 2, null))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Success(prep));

        await vm.ConsumirPreparacionCommand.ExecuteAsync(prep);

        _mockService.Verify(x => x.ConsumirPreparacionDiariaAsync(prep.Id, 2, null), Times.Once);
        _mockDialog.Verify(x => x.ShowSuccessAsync(It.Is<string>(m => m.Contains("consumida"))), Times.Once);
    }

    [Fact]
    public async Task MarcarComoDisponible_ShouldConfirmAndCallService()
    {
        var vm = CreateVm();
        var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza" };
        _mockDialog
            .Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockService
            .Setup(x => x.MarcarComoDisponibleAsync(prep.Id))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Success(prep));

        await vm.MarcarComoDisponibleCommand.ExecuteAsync(prep);

        _mockService.Verify(x => x.MarcarComoDisponibleAsync(prep.Id), Times.Once);
        _mockDialog.Verify(x => x.ShowSuccessAsync(It.Is<string>(m => m.Contains("disponible"))), Times.Once);
    }

    [Fact]
    public async Task EliminarPreparacion_ShouldConfirmAndCallService()
    {
        var vm = CreateVm();
        var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza" };
        _mockDialog
            .Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockService
            .Setup(x => x.EliminarPreparacionDiariaAsync(prep.Id))
            .ReturnsAsync(Result.Success());

        await vm.EliminarPreparacionCommand.ExecuteAsync(prep);

        _mockService.Verify(x => x.EliminarPreparacionDiariaAsync(prep.Id), Times.Once);
        _mockDialog.Verify(x => x.ShowSuccessAsync(It.Is<string>(m => m.Contains("eliminada"))), Times.Once);
    }
}


