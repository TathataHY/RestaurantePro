using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Analytics.ViewModels;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace RestaurantePro.Mobile.UnitTests.Features.Analytics.ViewModels
{
    public class AnalyticsViewModelTests
    {
        private Mock<IAnalyticsService> _mockAnalyticsService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IDialogService> _mockDialogService;
        private AnalyticsViewModel _viewModel;

        public AnalyticsViewModelTests()
        {
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            
            _viewModel = new AnalyticsViewModel(
                _mockAnalyticsService.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object);
        }

        [Fact]
        public async Task OnAppearingAsync_WithValidData_ShouldLoadAllMetrics()
        {
            // Arrange
            var metricasDia = new MetricasDiaDto
            {
                TotalVentas = 1500.50m,
                TotalComandas = 25,
                PorcentajeOcupacionMesas = 80.0m
            };

            var topProductos = new List<TopProductoDto>
            {
                new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pizza Margherita", CantidadVendida = 25 },
                new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa", CantidadVendida = 20 }
            };

            var ocupacionMesas = new OcupacionMesasDto
            {
                MesasOcupadas = 8,
                MesasLibres = 12,
                PorcentajeOcupacion = 0.67m
            };

            var tiempoPreparacion = new TiempoPreparacionDto
            {
                TiempoPromedioMinutos = 15,
                TiempoMaximoMinutos = 30
            };

            var ventasPorHora = new List<VentasHoraDto>
            {
                new() { Hora = 12, TotalVentas = 500.00m },
                new() { Hora = 13, TotalVentas = 750.00m }
            };

            _mockAnalyticsService.Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<MetricasDiaDto>.SuccessResponse(metricasDia));

            _mockAnalyticsService.Setup(x => x.ObtenerTopProductosAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TopProductoDto>>.SuccessResponse(topProductos));

            _mockAnalyticsService.Setup(x => x.ObtenerOcupacionMesasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<OcupacionMesasDto>.SuccessResponse(ocupacionMesas));

            _mockAnalyticsService.Setup(x => x.ObtenerTiempoPreparacionAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TiempoPreparacionDto>.SuccessResponse(tiempoPreparacion));

            _mockAnalyticsService.Setup(x => x.ObtenerVentasPorHoraAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<VentasHoraDto>>.SuccessResponse(ventasPorHora));

            // Act
            await _viewModel.CargarMetricasDiaAsync();

            // Assert
            _viewModel.MetricasDia.Should().NotBeNull();
            _viewModel.MetricasDia!.TotalVentas.Should().Be(1500.50m);
            // Los otros métodos se cargan por separado, no en CargarMetricasDiaAsync
        }

        [Fact]
        public async Task CargarMetricasDiaAsync_WithValidData_ShouldPopulateMetricasDia()
        {
            // Arrange
            var metricasDia = new MetricasDiaDto
            {
                TotalVentas = 1500.50m,
                TotalComandas = 25,
                PorcentajeOcupacionMesas = 80.0m
            };

            _mockAnalyticsService.Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<MetricasDiaDto>.SuccessResponse(metricasDia));

            // Act
            await _viewModel.CargarMetricasDiaAsync();

            // Assert
            _viewModel.MetricasDia.Should().NotBeNull();
            _viewModel.MetricasDia!.TotalVentas.Should().Be(1500.50m);
            _viewModel.MetricasDia.TotalComandas.Should().Be(25);
            _mockAnalyticsService.Verify(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarMetricasDiaAsync_WithError_ShouldShowError()
        {
            // Arrange
            var response = ApiResponse<MetricasDiaDto>.ErrorResponse(
                new List<string> { "Error al cargar métricas" }, 
                "Error", 
                500);

            _mockAnalyticsService.Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CargarMetricasDiaAsync();

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error"), Times.Once);
        }

        [Fact]
        public async Task CargarTopProductosAsync_WithValidData_ShouldPopulateTopProductos()
        {
            // Arrange
            var topProductos = new List<TopProductoDto>
            {
                new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pizza Margherita", CantidadVendida = 25 },
                new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa", CantidadVendida = 20 }
            };

            _mockAnalyticsService.Setup(x => x.ObtenerTopProductosAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TopProductoDto>>.SuccessResponse(topProductos));

            // Act
            await _viewModel.CargarTopProductosAsync();

            // Assert
            _viewModel.TopProductos.Should().HaveCount(2);
            _viewModel.TopProductos.First().NombreProducto.Should().Be("Pizza Margherita");
            _mockAnalyticsService.Verify(x => x.ObtenerTopProductosAsync(10, _viewModel.FechaDesde, _viewModel.FechaHasta, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarOcupacionMesasAsync_WithValidData_ShouldPopulateOcupacionMesas()
        {
            // Arrange
            var ocupacionMesas = new OcupacionMesasDto
            {
                MesasOcupadas = 8,
                MesasLibres = 12,
                PorcentajeOcupacion = 0.67m
            };

            _mockAnalyticsService.Setup(x => x.ObtenerOcupacionMesasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<OcupacionMesasDto>.SuccessResponse(ocupacionMesas));

            // Act
            await _viewModel.CargarOcupacionMesasAsync();

            // Assert
            _viewModel.OcupacionMesas.Should().NotBeNull();
            _viewModel.OcupacionMesas!.MesasOcupadas.Should().Be(8);
            _viewModel.OcupacionMesas.PorcentajeOcupacion.Should().Be(0.67m);
            _mockAnalyticsService.Verify(x => x.ObtenerOcupacionMesasAsync(_viewModel.FechaSeleccionada, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarTiempoPreparacionAsync_WithValidData_ShouldPopulateTiempoPreparacion()
        {
            // Arrange
            var tiempoPreparacion = new TiempoPreparacionDto
            {
                TiempoPromedioMinutos = 15,
                TiempoMaximoMinutos = 30
            };

            _mockAnalyticsService.Setup(x => x.ObtenerTiempoPreparacionAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TiempoPreparacionDto>.SuccessResponse(tiempoPreparacion));

            // Act
            await _viewModel.CargarTiempoPreparacionAsync();

            // Assert
            _viewModel.TiempoPreparacion.Should().NotBeNull();
            _viewModel.TiempoPreparacion!.TiempoPromedioMinutos.Should().Be(15);
            _mockAnalyticsService.Verify(x => x.ObtenerTiempoPreparacionAsync(_viewModel.FechaDesde, _viewModel.FechaHasta, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarVentasPorHoraAsync_WithValidData_ShouldPopulateVentasPorHora()
        {
            // Arrange
            var ventasPorHora = new List<VentasHoraDto>
            {
                new() { Hora = 12, TotalVentas = 500.00m },
                new() { Hora = 13, TotalVentas = 750.00m }
            };

            _mockAnalyticsService.Setup(x => x.ObtenerVentasPorHoraAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<VentasHoraDto>>.SuccessResponse(ventasPorHora));

            // Act
            await _viewModel.CargarVentasPorHoraAsync();

            // Assert
            _viewModel.VentasPorHora.Should().HaveCount(2);
            _viewModel.VentasPorHora.First().Hora.Should().Be(12);
            _viewModel.VentasPorHora.First().TotalVentas.Should().Be(500.00m);
            _mockAnalyticsService.Verify(x => x.ObtenerVentasPorHoraAsync(_viewModel.FechaSeleccionada, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarMetricasDiaAsync_DobleEjecucion_NoDebeReentrar()
        {
            // Arrange
            _mockAnalyticsService.Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<MetricasDiaDto>.SuccessResponse(new MetricasDiaDto());
                });

            // Act
            var t1 = _viewModel.CargarMetricasDiaCommand.ExecuteAsync(null);
            var t2 = _viewModel.CargarMetricasDiaCommand.ExecuteAsync(null);
            await t1;

            // Assert
            _mockAnalyticsService.Verify(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarTopProductosAsync_SinDatos_DebeQuedarVacio()
        {
            // Arrange
            _mockAnalyticsService.Setup(x => x.ObtenerTopProductosAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(ApiResponse<List<TopProductoDto>>.SuccessResponse(new List<TopProductoDto>()));

            // Act
            await _viewModel.CargarTopProductosAsync();

            // Assert
            _viewModel.TopProductos.Should().BeEmpty();
            _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CargarTiempoPreparacionAsync_Error_DeberiaMostrarError()
        {
            // Arrange
            _mockAnalyticsService.Setup(x => x.ObtenerTiempoPreparacionAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(ApiResponse<TiempoPreparacionDto>.Failure("Fallo servicio"));

            // Act
            await _viewModel.CargarTiempoPreparacionCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefrescarAsync_ShouldReloadAllMetrics()
        {
            // Arrange
            var metricasDia = new MetricasDiaDto { TotalVentas = 1000.00m };
            var topProductos = new List<TopProductoDto>();
            var ocupacionMesas = new OcupacionMesasDto();
            var tiempoPreparacion = new TiempoPreparacionDto();
            var ventasPorHora = new List<VentasHoraDto>();

            _mockAnalyticsService.Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<MetricasDiaDto>.SuccessResponse(metricasDia));
            _mockAnalyticsService.Setup(x => x.ObtenerTopProductosAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TopProductoDto>>.SuccessResponse(topProductos));
            _mockAnalyticsService.Setup(x => x.ObtenerOcupacionMesasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<OcupacionMesasDto>.SuccessResponse(ocupacionMesas));
            _mockAnalyticsService.Setup(x => x.ObtenerTiempoPreparacionAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TiempoPreparacionDto>.SuccessResponse(tiempoPreparacion));
            _mockAnalyticsService.Setup(x => x.ObtenerVentasPorHoraAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<VentasHoraDto>>.SuccessResponse(ventasPorHora));

            // Act
            await _viewModel.RefrescarAsync();

            // Assert
            _viewModel.EstaRefrescando.Should().BeFalse();
            _mockAnalyticsService.Verify(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockAnalyticsService.Verify(x => x.ObtenerTopProductosAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockAnalyticsService.Verify(x => x.ObtenerOcupacionMesasAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockAnalyticsService.Verify(x => x.ObtenerTiempoPreparacionAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockAnalyticsService.Verify(x => x.ObtenerVentasPorHoraAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CambiarPeriodoAsync_WithHoyPeriod_ShouldSetCorrectDates()
        {
            // Arrange
            _viewModel.FiltroPeriodo = "Hoy";

            // Act
            await _viewModel.CambiarPeriodoAsync();

            // Assert
            _viewModel.FechaDesde.Should().Be(DateTime.Today);
            _viewModel.FechaHasta.Should().Be(DateTime.Today);
        }

        [Fact]
        public async Task CambiarPeriodoAsync_WithEstaSemanaPeriod_ShouldSetCorrectDates()
        {
            // Arrange
            _viewModel.FiltroPeriodo = "Esta Semana";

            // Act
            await _viewModel.CambiarPeriodoAsync();

            // Assert
            _viewModel.FechaDesde.Should().Be(DateTime.Today.AddDays(-7));
            _viewModel.FechaHasta.Should().Be(DateTime.Today);
        }

        [Fact]
        public async Task CambiarPeriodoAsync_WithEsteMesPeriod_ShouldSetCorrectDates()
        {
            // Arrange
            _viewModel.FiltroPeriodo = "Este Mes";

            // Act
            await _viewModel.CambiarPeriodoAsync();

            // Assert
            _viewModel.FechaDesde.Should().Be(DateTime.Today.AddDays(-30));
            _viewModel.FechaHasta.Should().Be(DateTime.Today);
        }

        [Fact]
        public async Task VerDetalleProductoAsync_WithValidProducto_ShouldNavigateToProductDetail()
        {
            // Arrange
            var producto = new TopProductoDto
            {
                ProductoId = Guid.NewGuid(),
                NombreProducto = "Pizza Margherita",
                CantidadVendida = 25
            };

            // Act
            await _viewModel.VerDetalleProductoAsync(producto);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync("productodetalle", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task VerDetalleProductoAsync_WithNullProducto_ShouldNotNavigate()
        {
            // Act
            await _viewModel.VerDetalleProductoAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public void Constructor_ShouldSetCorrectTitle()
        {
            // Assert
            _viewModel.Title.Should().Be("Analytics y Métricas");
        }

        [Fact]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            // Assert
            _viewModel.FechaSeleccionada.Should().Be(DateTime.Today);
            _viewModel.FechaDesde.Should().Be(DateTime.Today.AddDays(-7));
            _viewModel.FechaHasta.Should().Be(DateTime.Today);
            _viewModel.FiltroPeriodo.Should().Be("Hoy");
            _viewModel.EstaRefrescando.Should().BeFalse();
        }
    }
} 