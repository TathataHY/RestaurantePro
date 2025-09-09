using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.ViewModels;

public class CreateDailyPreparationViewModelTests
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
    public async Task CrearPreparacionAsync_WithInvalidProductoId_ShouldShowError()
    {
        var vm = CreateVm();
        vm.ProductoIdText = "not-a-guid";
        vm.Cantidad = 1;

        await vm.CrearPreparacionCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowErrorAsync(It.Is<string>(m => m.Contains("ProductoId inválido"))), Times.Once);
        _mockService.Verify(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()), Times.Never);
    }

    [Fact]
    public async Task CrearPreparacionAsync_WithValidData_ShouldCallServiceAndNavigateBack()
    {
        var vm = CreateVm();
        vm.ProductoIdText = Guid.NewGuid().ToString();
        vm.Cantidad = 3;
        vm.FechaVencimiento = DateTime.Today.AddDays(1);
        vm.Observaciones = "Obs";

        _mockService
            .Setup(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Success(new PreparacionDiariaDto { Id = Guid.NewGuid() }));

        await vm.CrearPreparacionCommand.ExecuteAsync(null);

        _mockService.Verify(x => x.CrearPreparacionDiariaAsync(It.Is<CrearPreparacionDiariaCommand>(c => c.Cantidad == 3)), Times.Once);
        _mockDialog.Verify(x => x.ShowSuccessAsync(It.Is<string>(m => m.Contains("creada"))), Times.Once);
        _mockNav.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_ShouldNavigateBack()
    {
        var vm = CreateVm();

        await vm.CancelarCommand.ExecuteAsync(null);

        _mockNav.Verify(x => x.GoBackAsync(), Times.Once);
    }
}


