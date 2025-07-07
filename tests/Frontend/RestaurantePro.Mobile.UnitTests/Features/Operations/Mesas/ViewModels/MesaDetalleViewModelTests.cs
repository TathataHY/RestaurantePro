using AutoFixture;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Mesas.ViewModels;

public class MesaDetalleViewModelTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IMesasService> _mockMesasService = new();
    private readonly Mock<IComandasService> _mockComandasService = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.Title.Should().Be("Detalle de Mesa");
        vm.Mesa.Should().NotBeNull();
        vm.ComandasActivas.Should().NotBeNull();
        vm.MesaId.Should().Be(Guid.Empty);
        vm.PuedeAsignar.Should().BeFalse();
        vm.PuedeLiberar.Should().BeFalse();
        vm.TieneComandasActivas.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithMesaId_ShouldSetMesaIdAndLoadData()
    {
        var mesaId = Guid.NewGuid();
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.InitializeAsync(mesaId);

        vm.MesaId.Should().Be(mesaId);
        _mockMesasService.Verify(x => x.ObtenerMesaAsync(mesaId), Times.Once);
        _mockComandasService.Verify(x => x.ObtenerComandasPorMesaAsync(mesaId), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithEmptyMesaId_ShouldNotLoadData()
    {
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.InitializeAsync(Guid.Empty);

        _mockMesasService.Verify(x => x.ObtenerMesaAsync(It.IsAny<Guid>()), Times.Never);
        _mockComandasService.Verify(x => x.ObtenerComandasPorMesaAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task LoadMesaAsync_SuccessfulLoad_ShouldUpdateMesa()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Numero = "1", Estado = "disponible", Capacidad = 4 };
        _mockMesasService.Setup(x => x.ObtenerMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        await vm.LoadMesaCommand.ExecuteAsync(null);

        vm.Mesa.Should().Be(mesa);
        vm.PuedeAsignar.Should().BeTrue();
        vm.PuedeLiberar.Should().BeFalse();
    }

    [Fact]
    public async Task LoadMesaAsync_ErrorResponse_ShouldShowError()
    {
        var mesaId = Guid.NewGuid();
        _mockMesasService.Setup(x => x.ObtenerMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<MesaDto>.ErrorResponse(new List<string> { "Error" }, "Error", 500));

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        await vm.LoadMesaCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadComandasActivasAsync_SuccessfulLoad_ShouldUpdateComandas()
    {
        var mesaId = Guid.NewGuid();
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), MesaId = mesaId, Estado = "pendiente", Numero = "1" },
            new ComandaDto { Id = Guid.NewGuid(), MesaId = mesaId, Estado = "en preparación", Numero = "2" },
            new ComandaDto { Id = Guid.NewGuid(), MesaId = mesaId, Estado = "finalizada", Numero = "3" }
        };
        _mockComandasService.Setup(x => x.ObtenerComandasPorMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        await vm.LoadComandasActivasCommand.ExecuteAsync(null);

        vm.ComandasActivas.Should().HaveCount(2); // Solo pendiente y en preparación
        vm.TieneComandasActivas.Should().BeTrue();
    }

    [Fact]
    public async Task LoadComandasActivasAsync_EmptyResponse_ShouldClearComandas()
    {
        var mesaId = Guid.NewGuid();
        _mockComandasService.Setup(x => x.ObtenerComandasPorMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.ComandasActivas.Add(new ComandaDto()); // Agregar una comanda para verificar que se limpia
        await vm.LoadComandasActivasCommand.ExecuteAsync(null);

        vm.ComandasActivas.Should().BeEmpty();
        vm.TieneComandasActivas.Should().BeFalse();
    }

    [Fact]
    public async Task AsignarMesaAsync_WithValidData_ShouldAssignMesa()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "disponible" };
        _mockMesasService.Setup(x => x.ObtenerMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));
        _mockMesasService.Setup(x => x.AsignarMesaAsync(
            It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<object>.SuccessResponse(new { mesa.Id, mesa.Estado }));
        
        int promptCall = 0;
        _mockDialog.Setup(x => x.ShowPromptAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                promptCall++;
                return promptCall == 1 ? "Juan Pérez" : "4";
            });

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.AsignarMesaCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.AsignarMesaAsync(
            mesaId, null, 4, It.Is<string>(s => s.Contains("Juan Pérez"))), Times.Once);
        _mockDialog.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_WhenUserCancelsName_ShouldNotAssign()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "disponible" };
        _mockDialog.Setup(x => x.ShowPromptAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.AsignarMesaCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.AsignarMesaAsync(
            It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AsignarMesaAsync_WithInvalidPersonas_ShouldUseDefault()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "disponible" };
        _mockMesasService.Setup(x => x.AsignarMesaAsync(
            It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<object>.SuccessResponse(new { mesa.Id, mesa.Estado }));
        
        int promptCall = 0;
        _mockDialog.Setup(x => x.ShowPromptAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                promptCall++;
                return promptCall == 1 ? "Juan Pérez" : "invalid";
            });

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.AsignarMesaCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.AsignarMesaAsync(
            mesaId, null, 2, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LiberarMesaAsync_WithConfirmation_ShouldLiberateMesa()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "ocupada" };
        _mockMesasService.Setup(x => x.ObtenerMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));
        _mockMesasService.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));
        
        // Configurar el mock de comandas para evitar error al cargar comandas
        _mockComandasService.Setup(x => x.ObtenerComandasPorMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
        
        // Configurar ambos diálogos que se llaman en LiberarMesaAsync
        _mockDialog.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockDialog.Setup(x => x.ShowPromptAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("Finalización del servicio");

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.LiberarMesaCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.LiberarMesaAsync(
            mesaId, "Finalización del servicio", It.IsAny<string>()), Times.Once);
        _mockDialog.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LiberarMesaAsync_WhenUserCancels_ShouldNotLiberate()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "ocupada" };
        _mockDialog.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.LiberarMesaCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.LiberarMesaAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstadoAsync_WithValidEstado_ShouldChangeEstado()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "disponible" };
        _mockMesasService.Setup(x => x.ObtenerMesaAsync(mesaId))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));
        _mockMesasService.Setup(x => x.CambiarEstadoMesaAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));
        _mockDialog.Setup(x => x.ShowActionSheetAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync("mantenimiento");

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.CambiarEstadoCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.CambiarEstadoMesaAsync(
            mesaId, "mantenimiento", It.IsAny<string>()), Times.Once);
        _mockDialog.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_WhenUserCancels_ShouldNotChange()
    {
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto { Id = mesaId, Estado = "disponible" };
        _mockDialog.Setup(x => x.ShowActionSheetAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync((string?)null);

        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;
        vm.Mesa = mesa;
        await vm.CambiarEstadoCommand.ExecuteAsync(null);

        _mockMesasService.Verify(x => x.CambiarEstadoMesaAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task VerComandasAsync_ShouldNavigateToComandas()
    {
        var mesaId = Guid.NewGuid();
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MesaId = mesaId;

        await vm.VerComandasCommand.ExecuteAsync(null);

        _mockNav.Verify(x => x.NavigateToAsync("comandas", It.Is<Dictionary<string, object>>(p => p.ContainsKey("mesaId"))), Times.Once);
    }

    [Fact]
    public void Cleanup_ShouldClearCollections()
    {
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.ComandasActivas.Add(new ComandaDto());

        vm.Cleanup();

        vm.ComandasActivas.Should().BeEmpty();
    }

    [Theory]
    [InlineData("disponible", true, false)]
    [InlineData("ocupada", false, true)]
    [InlineData("mantenimiento", false, false)]
    [InlineData("reservada", false, false)]
    public void PropertiesCalculadas_ShouldReturnCorrectValues(string estado, bool puedeAsignar, bool puedeLiberar)
    {
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        vm.Mesa = new MesaDto { Estado = estado };

        vm.PuedeAsignar.Should().Be(puedeAsignar);
        vm.PuedeLiberar.Should().Be(puedeLiberar);
    }

    [Fact]
    public void MesaId_WhenSet_ShouldUpdateProperty()
    {
        var mesaId = Guid.NewGuid();
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.MesaId = mesaId;
        
        vm.MesaId.Should().Be(mesaId);
    }

    [Fact]
    public void Mesa_WhenSet_ShouldUpdateProperty()
    {
        var mesa = new MesaDto { Id = Guid.NewGuid(), Numero = "1", Estado = "disponible" };
        var vm = new MesaDetalleViewModel(_mockMesasService.Object, _mockComandasService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.Mesa = mesa;
        
        vm.Mesa.Should().Be(mesa);
    }
} 