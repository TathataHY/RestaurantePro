using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoFixture;
using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using Xunit;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Preferences;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Comandas.ViewModels;

public class ComandasViewModelTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();
    private readonly Mock<IMesasService> _mockMesas = new();
    private readonly Mock<INotificationService> _mockNotif = new();
    private readonly Mock<IComandaRealtimeService> _mockRealtime = new();
    private readonly Mock<IPreferencesService> _mockPrefs = new();

    private ComandasViewModel CreateVm(IComandasService service, IDialogService dialog)
        => new ComandasViewModel(service, dialog, _mockNav.Object, _mockMesas.Object, _mockNotif.Object, _mockRealtime.Object, _mockPrefs.Object);

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.Should().NotBeNull();
        vm.Title.Should().Be("Gestión de Comandas");
        vm.Comandas.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadComandasAsync_WhenIsBusy_ShouldNotCallService()
    {
        // Arrange
        var mockService = new Mock<IComandasService>();
        var calls = 0;
        mockService
            .Setup(s => s.BuscarComandasAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()))
            .Callback(() => calls++);
        mockService
            .Setup(s => s.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadisticasComandasDto>.SuccessResponse(new EstadisticasComandasDto()));

        var vm = CreateVm(mockService.Object, _mockDialog.Object);
        await Task.Delay(50); // permitir llamadas iniciales del constructor
        var baseline = calls;

        // Act
        vm.IsBusy = true;
        await vm.LoadComandasCommand.ExecuteAsync(null);

        // Assert
        calls.Should().Be(baseline);
    }

    [Fact]
    public async Task LoadEstadisticasAsync_WhenIsBusy_ShouldNotCallService()
    {
        // Arrange
        var mockService = new Mock<IComandasService>();
        var statCalls = 0;
        mockService
            .Setup(s => s.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadisticasComandasDto>.SuccessResponse(new EstadisticasComandasDto()))
            .Callback(() => statCalls++);
        mockService
            .Setup(s => s.BuscarComandasAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));

        var vm = CreateVm(mockService.Object, _mockDialog.Object);
        await Task.Delay(50); // permitir llamadas iniciales del constructor
        var baseline = statCalls;

        // Act
        vm.IsBusy = true;
        await vm.LoadEstadisticasCommand.ExecuteAsync(null);

        // Assert
        statCalls.Should().Be(baseline);
    }

    [Fact]
    public async Task LoadComandasAsync_SuccessfulLoad_PopulatesComandas()
    {
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" },
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "2" }
        };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await Task.Delay(100);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(2);
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadComandasAsync_ErrorResponse_SetsErrorMessage()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.ErrorResponse(new List<string>{"err"}, "Error al cargar las comandas", 500));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await Task.Delay(100);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.ErrorMessage.Should().Be("Error al cargar las comandas");
    }

    [Fact]
    public async Task CrearComandaAsync_WhenSuccessful_ShouldCreateComanda()
    {
        // Arrange: Mesas disponibles y selección
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "10", Ubicacion = "Salón", Capacidad = 4 }
        };
        _mockMesas
            .Setup(x => x.ObtenerMesasDisponiblesAsync(It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));
        var label = $"Mesa {mesas[0].Numero} — {mesas[0].Ubicacion} (Cap: {mesas[0].Capacidad})";
        _mockDialog
            .Setup(d => d.ShowActionSheetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(label);

        IDictionary<string, object>? capturedParams = null;
        _mockNav
            .Setup(n => n.NavigateToAsync("crear-comanda", It.IsAny<IDictionary<string, object>>()))
            .Callback((string route, IDictionary<string, object> p) => capturedParams = p)
            .Returns(Task.CompletedTask);

        var vm = CreateVm(new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>())), _mockDialog.Object);

        // Act
        await vm.CrearComandaCommand.ExecuteAsync(null);

        // Assert: navega a crear-comanda con mesaId
        _mockNav.Verify(x => x.NavigateToAsync("crear-comanda", It.IsAny<IDictionary<string, object>>()), Times.Once);
        capturedParams.Should().NotBeNull();
        capturedParams!.ContainsKey("mesaId").Should().BeTrue();
        Guid.TryParse(capturedParams["mesaId"].ToString(), out var gid).Should().BeTrue();
        gid.Should().Be(mesas[0].Id);
    }

    [Fact]
    public async Task CrearComandaAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange: error al cargar mesas disponibles
        _mockMesas
            .Setup(x => x.ObtenerMesasDisponiblesAsync(It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.ErrorResponse(new List<string>{"err"}, "No se pudieron cargar las mesas disponibles", 500));

        var vm = CreateVm(new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>())), _mockDialog.Object);

        // Act
        await vm.CrearComandaCommand.ExecuteAsync(null);

        // Assert: muestra error
        _mockDialog.Verify(d => d.ShowAlertAsync("Error", It.Is<string>(m => m.Contains("No se pudieron cargar")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CrearComandaAsync_WhenUserCancelsPrompt_ShouldNotExecute()
    {
        // Arrange
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { PromptResponse = string.Empty };
        var vm = CreateVm(fakeService, fakeDialog);

        // Reset WasCalled después del constructor
        fakeService.WasCalled = false;

        // Act
        await vm.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task CrearComandaAsync_WhenInvalidMesaId_ShouldShowError()
    {
        // Arrange
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { PromptResponse = "invalid_guid" };
        var vm = CreateVm(fakeService, fakeDialog);

        // Reset WasCalled después del constructor
        fakeService.WasCalled = false;

        // Act
        await vm.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeFalse();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Error");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_WhenSuccessful_ShouldChangeEstado()
    {
        // Arrange
        var comanda = _fixture.Create<ComandaDto>();
        comanda.Estado = "Pendiente";
        var fakeService = new FakeComandasServiceCambiarEstado(true, "Estado cambiado exitosamente");
        var fakeDialog = new FakeDialogService { ActionSheetResponse = "En Preparación" };
        var vm = CreateVm(fakeService, fakeDialog);

        // Act
        await vm.CambiarEstadoComandaCommand.ExecuteAsync(comanda);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_WhenUserCancelsAction_ShouldNotExecute()
    {
        var comanda = _fixture.Create<ComandaDto>();
        comanda.Estado = "Pendiente";
        var mockService = new Mock<IComandasService>();
        var fakeDialog = new FakeDialogService { ActionSheetResponse = "Cancelar" };
        var vm = CreateVm(mockService.Object, fakeDialog);

        await vm.CambiarEstadoComandaCommand.ExecuteAsync(comanda);
        mockService.Verify(s => s.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_WhenServiceFails_ShouldShowError()
    {
        var comanda = _fixture.Create<ComandaDto>();
        comanda.Estado = "Pendiente";
        var fakeService = new FakeComandasServiceCambiarEstado(false, "fallo");
        var fakeDialog = new FakeDialogService { ActionSheetResponse = "En Preparación" };
        var vm = CreateVm(fakeService, fakeDialog);

        await vm.CambiarEstadoComandaCommand.ExecuteAsync(comanda);
        fakeDialog.LastTitle.Should().Be("Error");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_WhenComandaIsNull_ShouldNotExecute()
    {
        // Arrange
        var fakeService = new FakeComandasServiceNotCalled();
        var vm = CreateVm(fakeService, _mockDialog.Object);

        // Reset WasCalled después del constructor
        fakeService.WasCalled = false;

        // Act
        await vm.CambiarEstadoComandaCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task FinalizarComandaAsync_WhenSuccessful_ShouldFinalizeComanda()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceFinalizar(true, "Comanda finalizada exitosamente");
        var fakeDialog = new FakeDialogService { ConfirmResponse = true, ActionSheetResponse = "Efectivo" };
        var vm = CreateVm(fakeService, fakeDialog);
        await vm.FinalizarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task FinalizarComandaAsync_WhenServiceFails_ShouldShowError()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceFinalizar(false, "fallo al finalizar");
        var fakeDialog = new FakeDialogService { ConfirmResponse = true, ActionSheetResponse = "Efectivo" };
        var vm = CreateVm(fakeService, fakeDialog);
        await vm.FinalizarComandaCommand.ExecuteAsync(comanda);
        fakeDialog.LastTitle.Should().Be("Error");
    }

    [Fact]
    public async Task FinalizarComandaAsync_WhenUserCancels_ShouldNotExecute()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { ConfirmResponse = false };
        var vm = CreateVm(fakeService, fakeDialog);
        fakeService.WasCalled = false;
        await vm.FinalizarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task CancelarComandaAsync_WhenSuccessful_ShouldCancelComanda()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceCancelar(true, "Comanda cancelada exitosamente");
        var fakeDialog = new FakeDialogService { ConfirmResponse = true, PromptResponse = "Motivo de cancelación" };
        var vm = CreateVm(fakeService, fakeDialog);
        await vm.CancelarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task CancelarComandaAsync_WhenServiceFails_ShouldShowError()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceCancelar(false, "fallo al cancelar");
        var fakeDialog = new FakeDialogService { ConfirmResponse = true, PromptResponse = "Motivo" };
        var vm = CreateVm(fakeService, fakeDialog);
        await vm.CancelarComandaCommand.ExecuteAsync(comanda);
        fakeDialog.LastTitle.Should().Be("Error");
    }

    [Fact]
    public async Task CancelarComandaAsync_WhenUserCancels_ShouldNotExecute()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { ConfirmResponse = false };
        var vm = CreateVm(fakeService, fakeDialog);
        fakeService.WasCalled = false;
        await vm.CancelarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToComandaDetailAsync_WhenComandaSelected_ShouldNavigate()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await vm.NavigateToComandaDetailCommand.ExecuteAsync(comanda);
        _mockNav.Verify(x => x.NavigateToAsync("comanda-detalle", It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task NavigateToComandaDetailAsync_WhenComandaIsNull_ShouldNotNavigate()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await vm.NavigateToComandaDetailCommand.ExecuteAsync(null);
        _mockNav.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public async Task NavigateToAgregarProductosAsync_WhenComandaSelected_ShouldNavigate()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await vm.NavigateToAgregarProductosCommand.ExecuteAsync(comanda);
        _mockNav.Verify(x => x.NavigateToAsync("///productos", It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task RefreshComandasAsync_ShouldReloadComandas()
    {
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" },
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "2" }
        };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await vm.RefreshComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(2);
        vm.IsRefreshing.Should().BeFalse();
    }

    [Fact]
    public async Task LoadEstadisticasAsync_WhenSuccessful_ShouldLoadEstadisticas()
    {
        var estadisticas = new EstadisticasComandasDto { TotalComandasActivas = 5 };
        var fakeService = new FakeComandasServiceEstadisticas(true, estadisticas, "Estadísticas cargadas");
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.IsBusy = false;
        await vm.LoadEstadisticasCommand.ExecuteAsync(null);
        vm.Estadisticas.Should().NotBeNull();
        vm.Estadisticas!.TotalComandasActivas.Should().Be(5);
    }

    [Fact]
    public void SelectedComanda_WhenSet_ShouldUpdateProperty()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.SelectedComanda = comanda;
        vm.SelectedComanda.Should().Be(comanda);
    }

    [Fact]
    public void FiltroEstado_WhenSet_ShouldUpdateProperty()
    {
        var estado = "Pendiente";
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.FiltroEstado = estado;
        vm.FiltroEstado.Should().Be(estado);
    }

    [Fact]
    public void SoloActivas_WhenSet_ShouldUpdateProperty()
    {
        var soloActivas = false;
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.SoloActivas = soloActivas;
        vm.SoloActivas.Should().Be(soloActivas);
    }

    [Theory]
    [InlineData("Pendiente", "Orange")]
    [InlineData("En Preparación", "Blue")]
    [InlineData("Lista", "Green")]
    [InlineData("Entregada", "Purple")]
    [InlineData("Finalizada", "Gray")]
    [InlineData("Cancelada", "Red")]
    [InlineData("Unknown", "Black")]
    public void GetEstadoColor_ShouldReturnCorrectColor(string estado, string expectedColor)
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        var result = vm.GetEstadoColor(estado);
        result.Should().Be(expectedColor);
    }

    [Theory]
    [InlineData("Pendiente", "⏳")]
    [InlineData("En Preparación", "👨‍🍳")]
    [InlineData("Lista", "✅")]
    [InlineData("Entregada", "🍽️")]
    [InlineData("Finalizada", "✔️")]
    [InlineData("Cancelada", "❌")]
    [InlineData("Unknown", "?")]
    public void GetEstadoIcon_ShouldReturnCorrectIcon(string estado, string expectedIcon)
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        var result = vm.GetEstadoIcon(estado);
        result.Should().Be(expectedIcon);
    }

    [Fact]
    public async Task LoadComandasAsync_WithSoloActivasTrue_ShouldOnlyShowActivas()
    {
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" },
            new ComandaDto { Id = Guid.NewGuid(), Estado = "finalizada", Numero = "2" },
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "3" }
        };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.SoloActivas = true;
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().OnlyContain(c => c.Estado == "pendiente");
    }

    [Fact]
    public async Task LoadComandasAsync_WithSoloActivasFalse_ShouldShowAll()
    {
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" },
            new ComandaDto { Id = Guid.NewGuid(), Estado = "finalizada", Numero = "2" },
            new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "3" }
        };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.SoloActivas = false;
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadComandasAsync_WithEmptyList_ShouldSetComandasEmpty()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadComandasAsync_WithUnknownEstado_ShouldNotThrow()
    {
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Estado = "desconocido", Numero = "1" }
        };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
        var vm = CreateVm(fakeService, _mockDialog.Object);
        vm.SoloActivas = false;
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(1);
        var color = vm.GetEstadoColor("desconocido");
        var icon = vm.GetEstadoIcon("desconocido");
        color.Should().Be("Black");
        icon.Should().Be("?");
    }

    [Fact]
    public async Task LoadComandasAsync_WhenServiceReturnsError_ShouldShowErrorDialog()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.ErrorResponse(new List<string>{"Error de servicio"}, "Error", 500));
        var mockDialog = new Mock<IDialogService>();
        var vm = CreateVm(fakeService, mockDialog.Object);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        mockDialog.Verify(x => x.ShowAlertAsync(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Error")), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_WhenNoTransitions_ShouldShowInfoAndNotCallService()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "Finalizada" };
        var mockService = new Mock<IComandasService>();
        var mockDialog = new Mock<IDialogService>();
        mockDialog.Setup(x => x.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        var vm = CreateVm(mockService.Object, mockDialog.Object);
        await vm.CambiarEstadoComandaCommand.ExecuteAsync(comanda);
        mockService.Verify(s => s.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        mockDialog.Verify(x => x.ShowAlertAsync(It.Is<string>(t => t == "Información"), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ToggleActivasAsync_ShouldFlipAndReload()
    {
        var mockService = new Mock<IComandasService>();
        var calls = 0;
        mockService
            .Setup(s => s.BuscarComandasAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()))
            .Callback(() => calls++);
        mockService
            .Setup(s => s.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadisticasComandasDto>.SuccessResponse(new EstadisticasComandasDto()));
        var vm = CreateVm(mockService.Object, _mockDialog.Object);
        await Task.Delay(50);
        var baseline = calls;
        await vm.ToggleActivasCommand.ExecuteAsync(null);
        calls.Should().BeGreaterThan(baseline);
    }

    [Fact]
    public async Task BuscarComandaAsync_WithEmptySearchText_ShouldShowInfoAndNotSearch()
    {
        var mockService = new Mock<IComandasService>();
        var calls = 0;
        mockService
            .Setup(s => s.BuscarComandasAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()))
            .Callback(() => calls++);
        mockService
            .Setup(s => s.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadisticasComandasDto>.SuccessResponse(new EstadisticasComandasDto()));

        var vm = CreateVm(mockService.Object, _mockDialog.Object);
        await Task.Delay(50); // permitir llamadas iniciales
        var baseline = calls;
        vm.SearchText = string.Empty;
        await vm.BuscarComandaCommand.ExecuteAsync(null);
        calls.Should().Be(baseline);
        _mockDialog.Verify(d => d.ShowAlertAsync(It.Is<string>(t => t == "Información"), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task BuscarComandaAsync_WhenFirstSearchEmpty_ShouldFallbackAndFilterByNumero()
    {
        var vmSearchText = "001";
        var service = new FakeComandasServiceSearchFlow(vmSearchText);
        var vm = CreateVm(service, _mockDialog.Object);
        vm.SearchText = vmSearchText;

        await vm.BuscarComandaCommand.ExecuteAsync(null);

        vm.Comandas.Should().NotBeEmpty();
        vm.Comandas.Should().OnlyContain(c =>
            (c.NumeroDisplay != null && c.NumeroDisplay.Contains(vmSearchText))
            || (c.Numero != null && c.Numero.Contains(vmSearchText))
        );
    }

    // --- Fakes mínimos para compilar ---
    private class FakeComandasService : IComandasService
    {
        private readonly ApiResponse<List<ComandaDto>> _response;
        public FakeComandasService(ApiResponse<List<ComandaDto>> response) { _response = response; }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default)
            => Task.FromResult(_response);
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // --- Fakes específicos para tests ---
    private class FakeComandasServiceCrear : IComandasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCalled { get; private set; }

        public FakeComandasServiceCrear(bool success, string message)
        {
            _success = success;
            _message = message;
            WasCalled = false;
        }

        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_success 
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        }

        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => Task.FromResult(ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto()));
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeComandasServiceCambiarEstado : IComandasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCalled { get; private set; }

        public FakeComandasServiceCambiarEstado(bool success, string message)
        {
            _success = success;
            _message = message;
            WasCalled = false;
        }

        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_success 
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        }

        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeComandasServiceNotCalled : IComandasService
    {
        public bool WasCalled { get; set; }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        }

        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto()));
        }

        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeComandasServiceFinalizar : IComandasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCalled { get; private set; }
        public FakeComandasServiceFinalizar(bool success, string message)
        {
            _success = success;
            _message = message;
            WasCalled = false;
        }
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_success
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default) => Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeComandasServiceCancelar : IComandasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCalled { get; private set; }
        public FakeComandasServiceCancelar(bool success, string message)
        {
            _success = success;
            _message = message;
            WasCalled = false;
        }
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_success
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default) => Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeDialogService : IDialogService
    {
        public bool WasCalled { get; private set; }
        public string? LastTitle { get; private set; }
        public string? LastMessage { get; private set; }
        public string? PromptResponse { get; set; } = "test_response";
        public bool ConfirmResponse { get; set; } = true;
        public string? ActionSheetResponse { get; set; } = "test_option";
        
        public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Task.CompletedTask;
        }
        
        public Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
        {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Task.FromResult(ConfirmResponse);
        }
        
        public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Sí", string cancel = "No")
        {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Task.FromResult(ConfirmResponse);
        }
        
        public Task ShowErrorAsync(string message)
        {
            WasCalled = true;
            LastMessage = message;
            return Task.CompletedTask;
        }
        
        public Task ShowSuccessAsync(string message)
        {
            WasCalled = true;
            LastMessage = message;
            return Task.CompletedTask;
        }
        
        public Task<string?> ShowActionSheetAsync(string title, string message, string cancel, params string[] buttons)
        {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Task.FromResult(ActionSheetResponse);
        }
        
        public Task<string?> ShowPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancelar", string placeholder = "", int maxLength = -1, string? initialValue = null)
        {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Task.FromResult(PromptResponse);
        }
    }

    private class FakeComandasServiceEstadisticas : IComandasService
    {
        private readonly bool _success;
        private readonly EstadisticasComandasDto? _estadisticas;
        private readonly string _message;
        public FakeComandasServiceEstadisticas(bool success, EstadisticasComandasDto? estadisticas, string message)
        {
            _success = success;
            _estadisticas = estadisticas ?? new EstadisticasComandasDto();
            _message = message;
        }
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            if (_success)
            {
                return Task.FromResult(ApiResponse<EstadisticasComandasDto>.SuccessResponse(_estadisticas!, _message));
            }
            else
            {
                return Task.FromResult(ApiResponse<EstadisticasComandasDto>.ErrorResponse(new List<string> { _message }, "No se pudieron cargar las estadísticas", 500));
            }
        }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default) => Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // Fake para flujo de búsqueda con fallback
    private class FakeComandasServiceSearchFlow : IComandasService
    {
        private readonly string _term;
        private int _calls;
        public FakeComandasServiceSearchFlow(string term) { _term = term; _calls = 0; }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null, CancellationToken cancellationToken = default)
        {
            _calls++;
            // 1ra llamada (constructor) → lista vacía
            if (_calls == 1)
                return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
            // 2da llamada (primer intento con clienteNombre) → vacía
            if (_calls == 2 && clienteNombre == _term)
                return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
            // 3ra llamada (fallback estado null) → devolver dataset con coincidencias
            var data = new List<ComandaDto>
            {
                new ComandaDto { Id = Guid.NewGuid(), Numero = _term, Estado = "pendiente" },
                new ComandaDto { Id = Guid.NewGuid(), Numero = "XYZ", Estado = "pendiente" }
            };
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(data));
        }

        // Métodos no usados en estas pruebas
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default) => Task.FromResult(ApiResponse<EstadisticasComandasDto>.SuccessResponse(new EstadisticasComandasDto()));
    }
} 