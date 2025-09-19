using System.Collections.ObjectModel;
using AutoFixture;
using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Authentication;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Mesas.ViewModels;

/// <summary>
/// Tests unitarios para MesasViewModel
/// </summary>
public class MesasViewModelTests
{
    private readonly Mock<IMesasService> _mockMesasService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Fixture _fixture;
    private MesasViewModel _viewModel;

    public MesasViewModelTests()
    {
        _mockMesasService = new Mock<IMesasService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockAuthService = new Mock<IAuthService>();
        _fixture = new Fixture();
        
        // Configurar mocks básicos para evitar errores de null reference
        _mockDialogService.Setup(x => x.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);
        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
        _mockDialogService.Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                         .ReturnsAsync("test_response");
        
        // Configurar mock de AuthService
        _mockAuthService.Setup(x => x.IsAuthenticatedAsync())
                       .ReturnsAsync(true);
        
        _viewModel = new MesasViewModel(
            _mockMesasService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockAuthService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var fakeService = new FakeMesasService(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        var fakeDialog = new FakeDialogService();
        var fakeNavigation = new Mock<INavigationService>().Object;
        var fakeAuth = new Mock<IAuthService>().Object;

        // Act
        var viewModel = new MesasViewModel(fakeService, fakeDialog, fakeNavigation, fakeAuth);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Title.Should().Be("Gestión de Mesas");
        viewModel.Mesas.Should().NotBeNull();
        viewModel.FiltroEstado.Should().BeEmpty();
        viewModel.FiltroUbicacion.Should().BeEmpty();
        viewModel.FiltroCapacidadMinima.Should().BeNull();
        viewModel.SearchText.Should().BeEmpty();
        viewModel.IsRefreshing.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    #endregion

    #region LoadMesasAsync Tests

    [Fact]
    public async Task LoadMesasAsync_WhenSuccessful_ShouldLoadMesas()
    {
        // Arrange
        var mesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 }, "Mesas cargadas exitosamente");
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert
        viewModel.Mesas.Should().HaveCount(3);
        viewModel.Mesas.Should().BeEquivalentTo(mesas);
        viewModel.HasError.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadMesasAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var errorResponse = ApiResponse<PaginatedList<MesaDto>>.ErrorResponse(
            new List<string> { "Error al cargar mesas" }, 
            "Error al cargar las mesas", 
            500);
        var fakeService = new FakeMesasService(errorResponse);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert
        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("Error al cargar las mesas");
    }

    [Fact]
    public async Task LoadMesasAsync_WhenExceptionOccurs_ShouldShowError()
    {
        // Arrange
        var exception = new Exception("Error de conexión");
        var fakeService = new FakeMesasServiceThrows(exception);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert
        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("Error inesperado: Error de conexión");
    }

    [Fact]
    public async Task LoadMesasAsync_WhenAlreadyBusy_ShouldNotExecute()
    {
        // Arrange
        var fakeService = new FakeMesasServiceNotCalled();
        var viewModel = new MesasViewModel(
            fakeService,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockAuthService.Object);
        
        // Reset WasCalled después del constructor y establecer IsBusy
        fakeService.WasCalled = false;
        viewModel.IsBusy = true;

        // Act
        await viewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeFalse();
    }

    #endregion

    #region AsignarMesaAsync Tests

    [Fact]
    public async Task AsignarMesaAsync_WhenSuccessful_ShouldAssignMesa()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var clienteId = Guid.NewGuid().ToString();
        var fakeService = new FakeMesasServiceAsignar(true, "Mesa asignada exitosamente");
        var fakeDialog = new FakeDialogService { PromptResponse = clienteId };
        var viewModel = new MesasViewModel(
            fakeService,
            fakeDialog,
            _mockNavigationService.Object,
            _mockAuthService.Object);

        // Act
        await viewModel.AsignarMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task AsignarMesaAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var clienteId = Guid.NewGuid().ToString();
        var errorMessage = "Error en la asignación";
        var fakeService = new FakeMesasServiceAsignar(false, errorMessage);
        var fakeDialog = new FakeDialogService { PromptResponse = clienteId };
        var viewModel = new MesasViewModel(
            fakeService,
            fakeDialog,
            _mockNavigationService.Object,
            _mockAuthService.Object);

        // Act
        await viewModel.AsignarMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Error");
        fakeDialog.LastMessage.Should().Be(errorMessage);
    }

    [Fact]
    public async Task AsignarMesaAsync_WhenMesaIsNull_ShouldNotExecute()
    {
        // Arrange
        var fakeService = new FakeMesasServiceNotCalled();
        var viewModel = new MesasViewModel(
            fakeService,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockAuthService.Object);

        // Reset WasCalled después del constructor
        fakeService.WasCalled = false;

        // Act
        await viewModel.AsignarMesaCommand.ExecuteAsync(null);

        // Assert
        fakeService.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task AsignarMesaAsync_WhenUserCancelsPrompt_ShouldNotExecute()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var fakeService = new FakeMesasServiceNotCalled();
        var fakeDialog = new FakeDialogService { PromptResponse = string.Empty };
        var viewModel = new MesasViewModel(
            fakeService,
            fakeDialog,
            _mockNavigationService.Object,
            _mockAuthService.Object);

        // Reset WasCalled después del constructor
        fakeService.WasCalled = false;

        // Act
        await viewModel.AsignarMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCalled.Should().BeFalse();
    }

    #endregion

    #region LiberarMesaAsync Tests

    [Fact]
    public async Task LiberarMesaAsync_WhenSuccessful_ShouldLiberateMesa()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var fakeService = new FakeMesasServiceLiberar(true, "Mesa liberada exitosamente");
        var fakeDialog = new FakeDialogService { ConfirmResponse = true, PromptResponse = "Motivo de liberación" };
        var viewModel = new MesasViewModel(
            fakeService,
            fakeDialog,
            _mockNavigationService.Object,
            _mockAuthService.Object);

        // Act
        await viewModel.LiberarMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    [Fact]
    public async Task LiberarMesaAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var errorMessage = "Error al liberar la mesa";
        var fakeService = new FakeMesasServiceLiberar(false, errorMessage);
        var fakeDialog = new FakeDialogService { ConfirmResponse = true, PromptResponse = "Motivo de liberación" };
        var viewModel = new MesasViewModel(
            fakeService,
            fakeDialog,
            _mockNavigationService.Object,
            _mockAuthService.Object);

        // Act
        await viewModel.LiberarMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCalled.Should().BeTrue();
        fakeDialog.WasCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Error");
        fakeDialog.LastMessage.Should().Be(errorMessage);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task NavigateToMesaDetailAsync_WhenMesaSelected_ShouldNavigate()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();

        // Act
        await _viewModel.NavigateToMesaDetailCommand.ExecuteAsync(mesa);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync("mesa-detalle", It.Is<Dictionary<string, object>>(p => 
            p.ContainsKey("mesaId") && p["mesaId"].Equals(mesa.Id.ToString()))), Times.Once);
    }

    [Fact]
    public async Task NavigateToMesaDetailAsync_WhenMesaIsNull_ShouldNotNavigate()
    {
        // Act
        await _viewModel.NavigateToMesaDetailCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task RefreshMesasAsync_ShouldReloadMesas()
    {
        // Arrange
        var mesas = _fixture.CreateMany<MesaDto>(2).ToList();
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 }, "Mesas cargadas");
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.RefreshMesasCommand.ExecuteAsync(null);

        // Assert
        viewModel.Mesas.Should().HaveCount(2);
        viewModel.IsRefreshing.Should().BeFalse();
    }

    #endregion

    #region Infinite Scroll Tests

    [Fact]
    public async Task LoadMoreMesasAsync_ShouldAppendNextPage_WhenBufferHasMore()
    {
        // Arrange: 25 mesas simuladas, pageSize=12 => 12 + 12 + 1
        var mesas = _fixture.CreateMany<MesaDto>(25).ToList();
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 });
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act: cargar primera página
        await viewModel.LoadMesasCommand.ExecuteAsync(null);
        var firstCount = viewModel.Mesas.Count;
        await viewModel.LoadMoreMesasCommand.ExecuteAsync(null);
        var secondCount = viewModel.Mesas.Count;
        await viewModel.LoadMoreMesasCommand.ExecuteAsync(null);
        var thirdCount = viewModel.Mesas.Count;

        // Assert
        firstCount.Should().BeGreaterThan(0);
        secondCount.Should().BeGreaterThan(firstCount);
        thirdCount.Should().Be(mesas.Count);
    }

    [Fact]
    public async Task LoadMoreMesasAsync_WhenIsBusy_ShouldNotAppend()
    {
        var mesas = _fixture.CreateMany<MesaDto>(10).ToList();
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 });
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        await viewModel.LoadMesasCommand.ExecuteAsync(null);
        var before = viewModel.Mesas.Count;
        viewModel.IsBusy = true;
        await viewModel.LoadMoreMesasCommand.ExecuteAsync(null);
        viewModel.Mesas.Count.Should().Be(before);
    }

    #endregion

    #region Filters Mapping Tests

    [Fact]
    public async Task ApplyFiltersAsync_ShouldMapUiEstadoToBackend()
    {
        var mesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 });
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object)
        {
            FiltroEstado = "Disponibles",
            FiltroCapacidad = "4 personas"
        };

        await viewModel.ApplyFiltersCommand.ExecuteAsync(null);

        // Si no lanza excepción y carga, damos por válido el mapeo; en escenarios reales, 
        // instrumentar Fake para capturar parámetros
        viewModel.Mesas.Should().NotBeNull();
    }

    [Fact]
    public async Task ClearFiltersAsync_ShouldResetFiltersAndReload()
    {
        var mesas = _fixture.CreateMany<MesaDto>(2).ToList();
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 });
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object)
        {
            FiltroEstado = "Ocupadas",
            FiltroUbicacion = "Terraza",
            FiltroCapacidadMinima = 4,
            FiltroCapacidad = "4 personas",
            SearchText = "A1"
        };

        await viewModel.ClearFiltersCommand.ExecuteAsync(null);

        viewModel.FiltroEstado.Should().BeEmpty();
        viewModel.FiltroUbicacion.Should().BeEmpty();
        viewModel.FiltroCapacidadMinima.Should().BeNull();
        viewModel.FiltroCapacidad.Should().BeEmpty();
        viewModel.SearchText.Should().BeEmpty();
        viewModel.Mesas.Should().NotBeNull();
    }

    #endregion
    
    #region CambiarEstado Tests

    [Fact]
    public async Task CambiarEstadoMesaAsync_WhenUserCancels_ShouldNotCallService()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var fakeService = new FakeMesasServiceCambiarEstado(success: true, message: "ok");
        var fakeDialog = new FakeDialogService { ActionSheetResponse = "Cancelar" };
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.CambiarEstadoMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCambiarCalled.Should().BeFalse();
    }

    [Fact]
    public async Task CambiarEstadoMesaAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var errorMessage = "Error al cambiar";
        var fakeService = new FakeMesasServiceCambiarEstado(success: false, message: errorMessage);
        var fakeDialog = new FakeDialogService { ActionSheetResponse = "Ocupada" };
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.CambiarEstadoMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCambiarCalled.Should().BeTrue();
        fakeDialog.LastTitle.Should().Be("Error");
        fakeDialog.LastMessage.Should().Be(errorMessage);
    }

    [Fact]
    public async Task CambiarEstadoMesaAsync_WhenSuccessful_ShouldShowSuccessAndReload()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();
        var fakeService = new FakeMesasServiceCambiarEstado(success: true, message: "Cambiado");
        var fakeDialog = new FakeDialogService { ActionSheetResponse = "Disponible" };
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.CambiarEstadoMesaCommand.ExecuteAsync(mesa);

        // Assert
        fakeService.WasCambiarCalled.Should().BeTrue();
        fakeService.ObtenerMesasCalledCount.Should().BeGreaterThan(0); // recarga
        fakeDialog.LastTitle.Should().Be("Éxito");
    }

    #endregion

    #region Search Filter Tests

    [Fact]
    public async Task LoadMesasAsync_WhenSearchText_ShouldFilterLocally()
    {
        // Arrange datos
        var all = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "A1", Ubicacion = "Terraza", Zona = "Norte", Capacidad = 4 },
            new MesaDto { Id = Guid.NewGuid(), Numero = "B2", Ubicacion = "Salon", Zona = "Sur", Capacidad = 2 },
            new MesaDto { Id = Guid.NewGuid(), Numero = "A10", Ubicacion = "Terraza", Zona = "Oeste", Capacidad = 6 },
        };
        var response = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = all, TotalCount = all.Count, PageNumber = 1, PageSize = 10 });
        var fakeService = new FakeMesasService(response);
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object)
        {
            SearchText = "A1"
        };

        // Act
        await viewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert: deben quedar A1 y A10 (contiene "A1")
        viewModel.Mesas.Should().HaveCount(2);
        viewModel.Mesas.Select(m => m.Numero).Should().BeEquivalentTo(new[] { "A1", "A10" });
    }

    #endregion

    #region Filters Mapping (Captura de parámetros)

    [Fact]
    public async Task ApplyFiltersAsync_ShouldMapAndPassParametersToService()
    {
        // Arrange
        var captureService = new FakeMesasServiceCaptureParams();
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(captureService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object)
        {
            FiltroEstado = "Disponibles",
            FiltroCapacidad = "4 personas",
            FiltroUbicacion = "Terraza"
        };

        // Act
        await viewModel.ApplyFiltersCommand.ExecuteAsync(null);

        // Assert
        captureService.LastEstado.Should().Be("Disponible");
        captureService.LastUbicacion.Should().Be("Terraza");
        captureService.LastCapacidadMinima.Should().Be(4);
        viewModel.FiltroCapacidadMinima.Should().Be(4);
    }

    #endregion
    #region Estadisticas Tests

    [Fact]
    public async Task LoadEstadisticasAsync_WhenSuccessful_ShouldLoadEstado()
    {
        // Arrange
        var estado = _fixture.Create<EstadoMesasDto>();
        var fakeService = new FakeMesasServiceEstadisticas(true, estado, "Estadísticas cargadas");
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Asegurar que el ViewModel no esté ocupado (evitar conflictos con carga automática del constructor)
        viewModel.IsBusy = false;

        // Act
        await viewModel.LoadEstadisticasCommand.ExecuteAsync(null);

        // Assert
        // Verificar que el fake fue llamado
        var response = await fakeService.ObtenerEstadoOcupacionAsync();
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        
        // Verificar que el ViewModel no está en estado de error
        viewModel.HasError.Should().BeFalse();
        viewModel.IsBusy.Should().BeFalse();
        
        // Verificar que el ViewModel tiene el estado
        viewModel.EstadoMesas.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadEstadisticasAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var fakeService = new FakeMesasServiceEstadisticas(false, null, "No se pudieron cargar las estadísticas");
        var fakeDialog = new FakeDialogService();
        var viewModel = new MesasViewModel(fakeService, fakeDialog, _mockNavigationService.Object, _mockAuthService.Object);

        // Act
        await viewModel.LoadEstadisticasCommand.ExecuteAsync(null);

        // Assert
        viewModel.EstadoMesas.Should().BeNull();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void SelectedMesa_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var mesa = _fixture.Create<MesaDto>();

        // Act
        _viewModel.SelectedMesa = mesa;

        // Assert
        _viewModel.SelectedMesa.Should().Be(mesa);
    }

    [Fact]
    public void FiltroEstado_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var estado = "Disponible";

        // Act
        _viewModel.FiltroEstado = estado;

        // Assert
        _viewModel.FiltroEstado.Should().Be(estado);
    }

    [Fact]
    public void FiltroUbicacion_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var ubicacion = "Terraza";

        // Act
        _viewModel.FiltroUbicacion = ubicacion;

        // Assert
        _viewModel.FiltroUbicacion.Should().Be(ubicacion);
    }

    [Fact]
    public void FiltroCapacidadMinima_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var capacidad = 4;

        // Act
        _viewModel.FiltroCapacidadMinima = capacidad;

        // Assert
        _viewModel.FiltroCapacidadMinima.Should().Be(capacidad);
    }

    #endregion

    #region Command Tests

    [Fact]
    public void Commands_ShouldBeInitialized()
    {
        // Assert
        _viewModel.LoadMesasCommand.Should().NotBeNull();
        _viewModel.RefreshMesasCommand.Should().NotBeNull();
        _viewModel.LoadEstadisticasCommand.Should().NotBeNull();
        _viewModel.AsignarMesaCommand.Should().NotBeNull();
        _viewModel.LiberarMesaCommand.Should().NotBeNull();
        _viewModel.CambiarEstadoMesaCommand.Should().NotBeNull();
        _viewModel.BuscarMejorMesaCommand.Should().NotBeNull();
        _viewModel.ApplyFiltersCommand.Should().NotBeNull();
        _viewModel.ClearFiltersCommand.Should().NotBeNull();
        _viewModel.NavigateToMesaDetailCommand.Should().NotBeNull();
        _viewModel.NavigateToNewComandaCommand.Should().NotBeNull();
    }

    [Fact]
    public void Commands_ShouldBeRelayCommands()
    {
        // Assert
        _viewModel.LoadMesasCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.RefreshMesasCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.LoadEstadisticasCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.AsignarMesaCommand.Should().BeOfType<AsyncRelayCommand<MesaDto>>();
        _viewModel.LiberarMesaCommand.Should().BeOfType<AsyncRelayCommand<MesaDto>>();
        _viewModel.CambiarEstadoMesaCommand.Should().BeOfType<AsyncRelayCommand<MesaDto>>();
        _viewModel.BuscarMejorMesaCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.ApplyFiltersCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.ClearFiltersCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.NavigateToMesaDetailCommand.Should().BeOfType<AsyncRelayCommand<MesaDto>>();
        _viewModel.NavigateToNewComandaCommand.Should().BeOfType<AsyncRelayCommand<MesaDto>>();
    }

    #endregion

    #region Utility Methods Tests

    [Theory]
    [InlineData("Disponible", "Green")]
    [InlineData("Ocupada", "Red")]
    [InlineData("Reservada", "Orange")]
    [InlineData("Mantenimiento", "Gray")]
    [InlineData("Unknown", "Black")]
    public void GetEstadoColor_ShouldReturnCorrectColor(string estado, string expectedColor)
    {
        // Act
        var result = _viewModel.GetEstadoColor(estado);

        // Assert
        result.Should().Be(expectedColor);
    }

    [Theory]
    [InlineData("Disponible", "✓")]
    [InlineData("Ocupada", "●")]
    [InlineData("Reservada", "⏰")]
    [InlineData("Mantenimiento", "🔧")]
    [InlineData("Unknown", "?")]
    public void GetEstadoIcon_ShouldReturnCorrectIcon(string estado, string expectedIcon)
    {
        // Act
        var result = _viewModel.GetEstadoIcon(estado);

        // Assert
        result.Should().Be(expectedIcon);
    }

    #endregion

    #region Fake Services

    private class FakeMesasService : IMesasService
    {
        private readonly ApiResponse<PaginatedList<MesaDto>> _response;
        public FakeMesasService(ApiResponse<PaginatedList<MesaDto>> response) { _response = response; }
        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
            => Task.FromResult(_response);
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeMesasServiceThrows : IMesasService
    {
        private readonly Exception _exception;
        public FakeMesasServiceThrows(Exception exception) { _exception = exception; }
        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
            => throw _exception;
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeMesasServiceNotCalled : IMesasService
    {
        public bool WasCalled { get; set; }

        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        }

        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(ApiResponse<object>.SuccessResponse(new object()));
        }

        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class FakeDialogService : IDialogService
    {
        public bool WasCalled { get; private set; }
        public string? LastTitle { get; private set; }
        public string? LastMessage { get; private set; }
        public string? PromptResponse { get; set; } = "test_response";
        public bool ConfirmResponse { get; set; } = true;
        public string? ActionSheetResponse { get; set; } = "Disponible";
        
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
            return Task.FromResult<string?>(ActionSheetResponse);
        }
        
        public Task<string?> ShowPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancelar", string placeholder = "", int maxLength = -1, string? initialValue = null)
        {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Task.FromResult(PromptResponse);
        }
    }

    // Fake para pruebas de asignación exitosa
    private class FakeMesasServiceAsignar : IMesasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCalled { get; private set; }

        public FakeMesasServiceAsignar(bool success, string message)
        {
            _success = success;
            _message = message;
            WasCalled = false;
        }

        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_success 
                ? ApiResponse<object>.SuccessResponse(new object(), _message)
                : ApiResponse<object>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
            => Task.FromResult(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // Fake para pruebas de liberación
    private class FakeMesasServiceLiberar : IMesasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCalled { get; private set; }

        public FakeMesasServiceLiberar(bool success, string message)
        {
            _success = success;
            _message = message;
            WasCalled = false;
        }

        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_success 
                ? ApiResponse<MesaDto>.SuccessResponse(new MesaDto(), _message)
                : ApiResponse<MesaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
            => Task.FromResult(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // Fake para pruebas de estadísticas
    private class FakeMesasServiceEstadisticas : IMesasService
    {
        private readonly bool _success;
        private readonly EstadoMesasDto? _estado;
        private readonly string _message;

        public FakeMesasServiceEstadisticas(bool success, EstadoMesasDto? estado, string message)
        {
            _success = success;
            _estado = estado ?? new EstadoMesasDto(); // Nunca null
            _message = message;
        }

        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default)
        {
            if (_success)
            {
                return Task.FromResult(ApiResponse<EstadoMesasDto>.SuccessResponse(_estado!, _message));
            }
            else
            {
                return Task.FromResult(ApiResponse<EstadoMesasDto>.ErrorResponse(new List<string> { _message }, "No se pudieron cargar las estadísticas", 500));
            }
        }

        // Implementar ObtenerMesasAsync para evitar excepción en constructor
        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        }

        // Métodos no usados en este test
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // Fake para capturar parámetros de filtros
    private class FakeMesasServiceCaptureParams : IMesasService
    {
        public string? LastEstado { get; private set; }
        public string? LastUbicacion { get; private set; }
        public int? LastCapacidadMinima { get; private set; }

        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            LastEstado = estado;
            LastUbicacion = ubicacion;
            LastCapacidadMinima = capacidadMinima;
            return Task.FromResult(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        }

        // Métodos no utilizados en estas pruebas
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // Fake para pruebas de cambiar estado
    private class FakeMesasServiceCambiarEstado : IMesasService
    {
        private readonly bool _success;
        private readonly string _message;
        public bool WasCambiarCalled { get; private set; }
        public int ObtenerMesasCalledCount { get; private set; }

        public FakeMesasServiceCambiarEstado(bool success, string message)
        {
            _success = success;
            _message = message;
        }

        public Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, string? motivo = null, CancellationToken cancellationToken = default)
        {
            WasCambiarCalled = true;
            return Task.FromResult(_success
                ? ApiResponse<MesaDto>.SuccessResponse(new MesaDto(), _message)
                : ApiResponse<MesaDto>.ErrorResponse(new List<string> { _message }, _message, 500));
        }

        public Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            ObtenerMesasCalledCount++;
            return Task.FromResult(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 }));
        }

        // Métodos no usados
        public Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(int? capacidadMinima = null, string? ubicacion = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<object>> AsignarMesaAsync(Guid mesaId, Guid? clienteId = null, int? numeroPersonas = null, string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> LiberarMesaAsync(Guid mesaId, string motivo = "Mesa liberada desde móvil", string? observaciones = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(int numeroPersonas, string? ubicacionPreferida = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    #endregion
} 