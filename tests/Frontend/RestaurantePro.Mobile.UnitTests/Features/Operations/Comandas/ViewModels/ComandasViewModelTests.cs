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

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Comandas.ViewModels;

public class ComandasViewModelTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        vm.Should().NotBeNull();
        vm.Title.Should().Be("Gestión de Comandas");
        vm.Comandas.Should().NotBeNull();
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        await Task.Delay(100);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(2);
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadComandasAsync_ErrorResponse_SetsErrorMessage()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.ErrorResponse(new List<string>{"err"}, "Error al cargar las comandas", 500));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        await Task.Delay(100);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.ErrorMessage.Should().Be("Error al cargar las comandas");
    }

    [Fact]
    public async Task CrearComandaAsync_WhenSuccessful_ShouldCreateComanda()
    {
        // Arrange
        var mesaId = Guid.NewGuid().ToString();
        var fakeService = new FakeComandasServiceCrear(true, "Comanda creada exitosamente");
        var fakeDialog = new FakeDialogService { PromptResponse = mesaId };
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);

        // Act
        await vm.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task CrearComandaAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var mesaId = Guid.NewGuid().ToString();
        var errorMessage = "Error en la creación";
        var fakeService = new FakeComandasServiceCrear(false, errorMessage);
        var fakeDialog = new FakeDialogService { PromptResponse = mesaId };
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);

        // Act
        await vm.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Error");
    }

    [Fact]
    public async Task CrearComandaAsync_WhenUserCancelsPrompt_ShouldNotExecute()
    {
        // Arrange
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { PromptResponse = string.Empty };
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);

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
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);

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
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);

        // Act
        await vm.CambiarEstadoComandaCommand.ExecuteAsync(comanda);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_WhenComandaIsNull_ShouldNotExecute()
    {
        // Arrange
        var fakeService = new FakeComandasServiceNotCalled();
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);

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
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);
        await vm.FinalizarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task FinalizarComandaAsync_WhenUserCancels_ShouldNotExecute()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { ConfirmResponse = false };
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);
        await vm.CancelarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task CancelarComandaAsync_WhenUserCancels_ShouldNotExecute()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasServiceNotCalled();
        var fakeDialog = new FakeDialogService { ConfirmResponse = false };
        var vm = new ComandasViewModel(fakeService, fakeDialog, _mockNav.Object);
        fakeService.WasCalled = false;
        await vm.CancelarComandaCommand.ExecuteAsync(comanda);
        fakeService.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToComandaDetailAsync_WhenComandaSelected_ShouldNavigate()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        await vm.NavigateToComandaDetailCommand.ExecuteAsync(comanda);
        _mockNav.Verify(x => x.NavigateToAsync("comanda-detalle", It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task NavigateToComandaDetailAsync_WhenComandaIsNull_ShouldNotNavigate()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        await vm.NavigateToComandaDetailCommand.ExecuteAsync(null);
        _mockNav.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public async Task NavigateToAgregarProductosAsync_WhenComandaSelected_ShouldNavigate()
    {
        var comanda = new ComandaDto { Id = Guid.NewGuid(), Estado = "pendiente", Numero = "1" };
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        await vm.NavigateToAgregarProductosCommand.ExecuteAsync(comanda);
        _mockNav.Verify(x => x.NavigateToAsync("productos", It.IsAny<Dictionary<string, object>>()), Times.Once);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        await vm.RefreshComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(2);
        vm.IsRefreshing.Should().BeFalse();
    }

    [Fact]
    public async Task LoadEstadisticasAsync_WhenSuccessful_ShouldLoadEstadisticas()
    {
        var estadisticas = new EstadisticasComandasDto { TotalComandasActivas = 5 };
        var fakeService = new FakeComandasServiceEstadisticas(true, estadisticas, "Estadísticas cargadas");
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        vm.SelectedComanda = comanda;
        vm.SelectedComanda.Should().Be(comanda);
    }

    [Fact]
    public void FiltroEstado_WhenSet_ShouldUpdateProperty()
    {
        var estado = "Pendiente";
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        vm.FiltroEstado = estado;
        vm.FiltroEstado.Should().Be(estado);
    }

    [Fact]
    public void SoloActivas_WhenSet_ShouldUpdateProperty()
    {
        var soloActivas = false;
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
        vm.SoloActivas = false;
        await vm.LoadComandasCommand.ExecuteAsync(null);
        vm.Comandas.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadComandasAsync_WithEmptyList_ShouldSetComandasEmpty()
    {
        var fakeService = new FakeComandasService(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, _mockDialog.Object, _mockNav.Object);
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
        var vm = new ComandasViewModel(fakeService, mockDialog.Object, _mockNav.Object);
        await vm.LoadComandasCommand.ExecuteAsync(null);
        mockDialog.Verify(x => x.ShowAlertAsync(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Error")), It.IsAny<string>()), Times.AtLeastOnce);
    }

    // --- Fakes mínimos para compilar ---
    private class FakeComandasService : IComandasService
    {
        private readonly ApiResponse<List<ComandaDto>> _response;
        public FakeComandasService(ApiResponse<List<ComandaDto>> response) { _response = response; }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null)
            => Task.FromResult(_response);
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync() => throw new NotImplementedException();
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

        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request)
        {
            WasCalled = true;
            return Task.FromResult(_success 
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null)
        {
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        }

        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync() => throw new NotImplementedException();
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

        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null)
        {
            WasCalled = true;
            return Task.FromResult(_success 
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null)
        {
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        }

        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync() => throw new NotImplementedException();
    }

    private class FakeComandasServiceNotCalled : IComandasService
    {
        public bool WasCalled { get; set; }

        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null)
        {
            WasCalled = true;
            return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        }

        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request)
        {
            WasCalled = true;
            return Task.FromResult(ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto()));
        }

        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync() => throw new NotImplementedException();
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
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null)
        {
            WasCalled = true;
            return Task.FromResult(_success
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null) => Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync() => throw new NotImplementedException();
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
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo)
        {
            WasCalled = true;
            return Task.FromResult(_success
                ? ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto(), _message)
                : ApiResponse<ComandaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null) => Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync() => throw new NotImplementedException();
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
        public Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync()
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
        public Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(string? estado = null, Guid? mesaId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? clienteNombre = null) => Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        public Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId) => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync() => throw new NotImplementedException();
        public Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null) => throw new NotImplementedException();
        public Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo) => throw new NotImplementedException();
    }
} 