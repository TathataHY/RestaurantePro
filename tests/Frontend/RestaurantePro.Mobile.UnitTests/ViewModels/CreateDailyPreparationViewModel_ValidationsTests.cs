using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.ViewModels;

public class CreateDailyPreparationViewModel_ValidationsTests
{
    private readonly Mock<IDailyPreparationsService> _mockService = new();
    private readonly Mock<IAuthService> _mockAuth = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();
    private readonly Mock<IProductosService> _mockProductos = new();

    private CreateDailyPreparationViewModel CreateVm()
    {
        _mockAuth.Setup(x => x.GetUserIdAsync()).ReturnsAsync(Guid.NewGuid().ToString());
        return new CreateDailyPreparationViewModel(_mockService.Object, _mockAuth.Object, _mockDialog.Object, _mockNav.Object, _mockProductos.Object);
    }

    [Fact]
    public async Task CrearPreparacion_InvalidCantidad_ShouldShowError()
    {
        var vm = CreateVm();
        vm.ProductoIdText = Guid.NewGuid().ToString();
        vm.Cantidad = 0;

        await vm.CrearPreparacionCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowErrorAsync(It.Is<string>(m => m.Contains("mayor que 0"))), Times.Once);
        _mockService.Verify(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()), Times.Never);
    }

    [Fact]
    public async Task CrearPreparacion_PastDate_ShouldShowError()
    {
        var vm = CreateVm();
        vm.ProductoIdText = Guid.NewGuid().ToString();
        vm.Cantidad = 1;
        vm.FechaVencimiento = DateTime.Today.AddDays(-1);

        await vm.CrearPreparacionCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowErrorAsync(It.Is<string>(m => m.Contains("fecha de vencimiento"))), Times.Once);
        _mockService.Verify(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()), Times.Never);
    }
}


