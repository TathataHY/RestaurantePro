using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Cocina.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Cocina.ViewModels;

public class ModernCocinaViewModelTests
{
    private readonly Mock<IComandasService> _mockComandasService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IComandaRealtimeService> _mockRealtimeService;
    private readonly ModernCocinaViewModel _viewModel;

    public ModernCocinaViewModelTests()
    {
        _mockComandasService = new Mock<IComandasService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRealtimeService = new Mock<IComandaRealtimeService>();

        _viewModel = new ModernCocinaViewModel(
            _mockComandasService.Object,
            _mockDialogService.Object,
            _mockNotificationService.Object,
            _mockRealtimeService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.Equal("Cocina", _viewModel.Title);
        Assert.NotNull(_viewModel.Comandas);
        Assert.NotNull(_viewModel.ComandasFiltradas);
        Assert.Equal("Todas", _viewModel.FiltroEstado);
        Assert.True(_viewModel.SoloPendientes);
        Assert.False(_viewModel.IsRefreshing);
    }

    [Fact]
    public void Constructor_ShouldInitializeEstadosDisponibles()
    {
        // Act
        var estados = _viewModel.EstadosDisponibles;

        // Assert
        Assert.Equal(4, estados.Count);
        Assert.Contains("Todas", estados);
        Assert.Contains("Creada", estados);
        Assert.Contains("En Proceso", estados);
        Assert.Contains("Lista", estados);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void SoloPendientes_WhenSetToFalse_ShouldUpdateProperty()
    {
        // Act
        _viewModel.SoloPendientes = false;

        // Assert
        Assert.False(_viewModel.SoloPendientes);
    }

    [Fact]
    public void FiltroEstado_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var estado = "Creada";

        // Act
        _viewModel.FiltroEstado = estado;

        // Assert
        Assert.Equal(estado, _viewModel.FiltroEstado);
    }

    [Fact]
    public void IsRefreshing_WhenSetToTrue_ShouldUpdateProperty()
    {
        // Act
        _viewModel.IsRefreshing = true;

        // Assert
        Assert.True(_viewModel.IsRefreshing);
    }

    #endregion

    #region EstadisticasCocinaDto Tests

    [Fact]
    public void EstadisticasCocinaDto_ShouldAllowSettingProperties()
    {
        // Arrange
        var estadisticas = new EstadisticasCocinaDto
        {
            ComandasCreadas = 5,
            ComandasEnProceso = 3,
            ComandasListas = 2,
            TiempoPromedio = 15
        };

        // Act
        _viewModel.Estadisticas = estadisticas;

        // Assert
        Assert.Equal(estadisticas, _viewModel.Estadisticas);
        Assert.Equal(5, _viewModel.Estadisticas.ComandasCreadas);
        Assert.Equal(3, _viewModel.Estadisticas.ComandasEnProceso);
        Assert.Equal(2, _viewModel.Estadisticas.ComandasListas);
        Assert.Equal(15, _viewModel.Estadisticas.TiempoPromedio);
    }

    #endregion

    #region Helper Methods

    private ComandaDto CreateComandaDto(string numero, string estado)
    {
        return new ComandaDto
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            Estado = estado,
            MesaNumero = "Mesa 1",
            Total = 25.50m,
            FechaCreacion = DateTime.Now
        };
    }

    #endregion
}