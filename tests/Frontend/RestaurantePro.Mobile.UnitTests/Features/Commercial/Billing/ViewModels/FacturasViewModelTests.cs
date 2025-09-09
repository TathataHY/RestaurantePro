using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Commercial.Billing.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Mobile.UnitTests.Features.Commercial.Billing.ViewModels;

public class FacturasViewModelTests
{
    private Mock<IFacturasService> _mockFacturasService;
    private Mock<IDialogService> _mockDialogService;
    private Mock<INavigationService> _mockNavigationService;
    private FacturasViewModel _viewModel;

    public FacturasViewModelTests()
    {
        _mockFacturasService = new Mock<IFacturasService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new FacturasViewModel(_mockFacturasService.Object, _mockDialogService.Object, _mockNavigationService.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Facturas.Should().NotBeNull();
        _viewModel.Busqueda.Should().BeEmpty();
        _viewModel.FechaSeleccionada.Should().Be(DateTime.Today);
        _viewModel.TotalFacturasHoy.Should().Be(0);
        _viewModel.TotalVentasHoy.Should().Be(0);
        _viewModel.PromedioFacturaHoy.Should().Be(0);
    }

    [Fact]
    public async Task OnAppearingAsync_WhenSuccessful_ShouldLoadFacturasAndStats()
    {
        // Arrange
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "F001", Total = 100.00m },
            new() { Id = Guid.NewGuid(), NumeroFactura = "F002", Total = 200.00m }
        };

        var facturasResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        var estadisticasResponse = ApiResponse<EstadisticasFacturasDto>.SuccessResponse(new EstadisticasFacturasDto
        {
            TotalFacturas = 2,
            TotalVentas = 300.00m,
            PromedioFactura = 150.00m
        });

        _mockFacturasService.Setup(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(facturasResponse);
        _mockFacturasService.Setup(x => x.ObtenerEstadisticasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(estadisticasResponse);

                    // Act
            await _viewModel.OnAppearingAsync();

        // Assert
        _viewModel.Facturas.Should().HaveCount(2);
        _viewModel.TotalFacturasHoy.Should().Be(2);
        _viewModel.TotalVentasHoy.Should().Be(300.00m);
        _viewModel.PromedioFacturaHoy.Should().Be(150.00m);
        _mockFacturasService.Verify(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFacturasService.Verify(x => x.ObtenerEstadisticasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CargarFacturasAsync_WhenSuccessful_ShouldLoadFacturas()
    {
        // Arrange
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "F001", Total = 100.00m },
            new() { Id = Guid.NewGuid(), NumeroFactura = "F002", Total = 200.00m }
        };

        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        _mockFacturasService.Setup(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.CargarFacturasCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Facturas.Should().HaveCount(2);
        _mockFacturasService.Verify(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CargarFacturasAsync_WhenError_ShouldShowErrorMessage()
    {
        // Arrange
        var errorMessage = "Error en la operación";
        var response = ApiResponse<List<FacturaDto>>.ErrorResponse(errorMessage);
        _mockFacturasService.Setup(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.CargarFacturasCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Facturas.Should().BeEmpty();
        _mockDialogService.Verify(x => x.ShowErrorAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task BuscarFacturasAsync_WhenSuccessful_ShouldLoadFilteredFacturas()
    {
        // Arrange
        _viewModel.Busqueda = "F001";
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "F001", Total = 100.00m }
        };

        var response = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        _mockFacturasService.Setup(x => x.BuscarFacturasAsync("F001", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.BuscarFacturasCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Facturas.Should().HaveCount(1);
        _viewModel.Facturas.First().NumeroFactura.Should().Be("F001");
        _mockFacturasService.Verify(x => x.BuscarFacturasAsync("F001", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarFacturasAsync_WhenError_ShouldShowErrorMessage()
    {
        // Arrange
        _viewModel.Busqueda = "F001";
        var response = ApiResponse<List<FacturaDto>>.ErrorResponse("Error en la operación");
        _mockFacturasService.Setup(x => x.BuscarFacturasAsync("F001", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.BuscarFacturasCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Facturas.Should().BeEmpty();
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
    }

    [Fact]
    public async Task CambiarFechaAsync_ShouldReloadFacturasAndStats()
    {
        // Arrange
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "F001", Total = 100.00m }
        };

        var facturasResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        var estadisticasResponse = ApiResponse<EstadisticasFacturasDto>.SuccessResponse(new EstadisticasFacturasDto
        {
            TotalFacturas = 1,
            TotalVentas = 100.00m,
            PromedioFactura = 100.00m
        });

        _mockFacturasService.Setup(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(facturasResponse);
        _mockFacturasService.Setup(x => x.ObtenerEstadisticasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(estadisticasResponse);

        // Act
        await _viewModel.CambiarFechaCommand.ExecuteAsync(null);

        // Assert
        _mockFacturasService.Verify(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFacturasService.Verify(x => x.ObtenerEstadisticasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SeleccionarFacturaAsync_WhenValidFactura_ShouldNavigateToDetail()
    {
        // Arrange
        var factura = new FacturaDto { Id = Guid.NewGuid(), NumeroFactura = "F001" };

        // Act
        await _viewModel.SeleccionarFacturaCommand.ExecuteAsync(factura);

        // Assert
        _viewModel.FacturaSeleccionada.Should().Be(factura);
        _mockNavigationService.Verify(x => x.NavigateToAsync("facturadetalle", It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task SeleccionarFacturaAsync_WhenNullFactura_ShouldNotNavigate()
    {
        // Act
        await _viewModel.SeleccionarFacturaCommand.ExecuteAsync(null);

        // Assert
        _viewModel.FacturaSeleccionada.Should().BeNull();
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public async Task ImprimirFacturaAsync_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var factura = new FacturaDto { Id = Guid.NewGuid(), NumeroFactura = "F001" };
        var response = ApiResponse<bool>.SuccessResponse(true);
        _mockFacturasService.Setup(x => x.ImprimirFacturaAsync(factura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.ImprimirFacturaCommand.ExecuteAsync(factura);

        // Assert
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Factura enviada a impresión"), Times.Once);
        _mockFacturasService.Verify(x => x.ImprimirFacturaAsync(factura.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ImprimirFacturaAsync_WhenError_ShouldShowErrorMessage()
    {
        // Arrange
        var factura = new FacturaDto { Id = Guid.NewGuid(), NumeroFactura = "F001" };
        var response = ApiResponse<bool>.ErrorResponse("Error al imprimir");
        _mockFacturasService.Setup(x => x.ImprimirFacturaAsync(factura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.ImprimirFacturaCommand.ExecuteAsync(factura);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
    }

    [Fact]
    public async Task ImprimirFacturaAsync_WhenNullFactura_ShouldNotCallService()
    {
        // Act
        await _viewModel.ImprimirFacturaCommand.ExecuteAsync(null);

        // Assert
        _mockFacturasService.Verify(x => x.ImprimirFacturaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefrescarAsync_ShouldReloadFacturasAndStats()
    {
        // Arrange
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "F001", Total = 100.00m }
        };

        var facturasResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        var estadisticasResponse = ApiResponse<EstadisticasFacturasDto>.SuccessResponse(new EstadisticasFacturasDto
        {
            TotalFacturas = 1,
            TotalVentas = 100.00m,
            PromedioFactura = 100.00m
        });

        _mockFacturasService.Setup(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(facturasResponse);
        _mockFacturasService.Setup(x => x.ObtenerEstadisticasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(estadisticasResponse);

        // Act
        await _viewModel.RefrescarCommand.ExecuteAsync(null);

        // Assert
        _mockFacturasService.Verify(x => x.ObtenerFacturasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFacturasService.Verify(x => x.ObtenerEstadisticasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Busqueda_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var busqueda = "F001";

        // Act
        _viewModel.Busqueda = busqueda;

        // Assert
        _viewModel.Busqueda.Should().Be(busqueda);
    }

    [Fact]
    public void FechaSeleccionada_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);

        // Act
        _viewModel.FechaSeleccionada = fecha;

        // Assert
        _viewModel.FechaSeleccionada.Should().Be(fecha);
    }

    [Fact]
    public void Commands_ShouldBeInitialized()
    {
        // Assert
                    // El método OnAppearingAsync es público y existe
        _viewModel.CargarFacturasCommand.Should().NotBeNull();
        _viewModel.BuscarFacturasCommand.Should().NotBeNull();
        _viewModel.CambiarFechaCommand.Should().NotBeNull();
        _viewModel.SeleccionarFacturaCommand.Should().NotBeNull();
        _viewModel.ImprimirFacturaCommand.Should().NotBeNull();
        _viewModel.RefrescarCommand.Should().NotBeNull();
    }

    [Fact]
    public void Commands_ShouldBeAsyncRelayCommands()
    {
        // Assert
                    // El método OnAppearingAsync es público y existe
        _viewModel.CargarFacturasCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.BuscarFacturasCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.CambiarFechaCommand.Should().BeOfType<AsyncRelayCommand>();
        _viewModel.SeleccionarFacturaCommand.Should().BeOfType<AsyncRelayCommand<FacturaDto>>();
        _viewModel.ImprimirFacturaCommand.Should().BeOfType<AsyncRelayCommand<FacturaDto>>();
        _viewModel.RefrescarCommand.Should().BeOfType<AsyncRelayCommand>();
    }
} 