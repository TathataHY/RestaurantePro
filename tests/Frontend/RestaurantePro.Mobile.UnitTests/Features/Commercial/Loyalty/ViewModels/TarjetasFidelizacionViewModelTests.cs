using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Commercial.Loyalty.ViewModels;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.UnitTests.Features.Commercial.Loyalty.ViewModels
{
    public class TarjetasFidelizacionViewModelTests
    {
        private Mock<ITarjetasFidelizacionService> _mockTarjetasService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IDialogService> _mockDialogService;
        private TarjetasFidelizacionViewModel _viewModel;

        public TarjetasFidelizacionViewModelTests()
        {
            _mockTarjetasService = new Mock<ITarjetasFidelizacionService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            
            _viewModel = new TarjetasFidelizacionViewModel(
                _mockTarjetasService.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object);
        }

        [Fact]
        public async Task OnAppearingAsync_ShouldClearState()
        {
            // Arrange
            _viewModel.TarjetaActual = new TarjetaFidelizacionDto { Id = Guid.NewGuid(), CodigoTarjeta = "123456789" };
            _viewModel.Historial.Add(new TransaccionPuntosDto { Id = Guid.NewGuid() });
            _viewModel.CodigoTarjeta = "123456789";

            // Act
            await _viewModel.OnAppearingAsync();

            // Assert
            Assert.Null(_viewModel.TarjetaActual);
            Assert.Empty(_viewModel.Historial);
            Assert.Equal(string.Empty, _viewModel.CodigoTarjeta);
        }

        [Fact]
        public async Task BuscarTarjetaAsync_WithValidCode_ShouldFindTarjeta()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var tarjeta = new TarjetaFidelizacionDto { Id = tarjetaId, CodigoTarjeta = "123456789", PuntosDisponibles = 100 };
            _viewModel.CodigoTarjeta = "123456789";

            _mockTarjetasService.Setup(x => x.ObtenerTarjetaPorCodigoAsync("123456789", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta));

            _mockTarjetasService.Setup(x => x.ObtenerHistorialAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(new List<TransaccionPuntosDto>()));

            // Act
            await _viewModel.BuscarTarjetaCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.TarjetaActual);
            Assert.Equal("123456789", _viewModel.TarjetaActual.CodigoTarjeta);
            Assert.Equal(100, _viewModel.TarjetaActual.PuntosDisponibles);
        }

        [Fact]
        public async Task BuscarTarjetaAsync_WithEmptyCode_ShouldShowError()
        {
            // Arrange
            _viewModel.CodigoTarjeta = "";

            // Act
            await _viewModel.BuscarTarjetaCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Ingrese un código de tarjeta válido"), Times.Once);
        }

        [Fact]
        public async Task BuscarTarjetaAsync_WithInvalidCode_ShouldShowError()
        {
            // Arrange
            _viewModel.CodigoTarjeta = "999999999";

            _mockTarjetasService.Setup(x => x.ObtenerTarjetaPorCodigoAsync("999999999", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TarjetaFidelizacionDto>.Failure("Tarjeta no encontrada"));

            // Act
            await _viewModel.BuscarTarjetaCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        }

        [Fact]
        public async Task ActivarTarjetaAsync_WithValidCode_ShouldActivateTarjeta()
        {
            // Arrange
            var tarjeta = new TarjetaFidelizacionDto { Id = Guid.NewGuid(), CodigoTarjeta = "123456789", PuntosDisponibles = 0 };
            _viewModel.CodigoTarjeta = "123456789";

            _mockTarjetasService.Setup(x => x.ActivarTarjetaAsync("123456789", "Cliente", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta));

            // Act
            await _viewModel.ActivarTarjetaCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.TarjetaActual);
            _mockDialogService.Verify(x => x.ShowSuccessAsync("Tarjeta activada correctamente"), Times.Once);
        }

        [Fact]
        public async Task ActivarTarjetaAsync_WithEmptyCode_ShouldShowError()
        {
            // Arrange
            _viewModel.CodigoTarjeta = "";

            // Act
            await _viewModel.ActivarTarjetaCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Ingrese un código de tarjeta válido"), Times.Once);
        }

        [Fact]
        public async Task AplicarDescuentoAsync_WithValidTarjeta_ShouldNavigate()
        {
            // Arrange
            var tarjeta = new TarjetaFidelizacionDto { Id = Guid.NewGuid(), CodigoTarjeta = "123456789" };
            _viewModel.TarjetaActual = tarjeta;

            // Act
            await _viewModel.AplicarDescuentoCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync("aplicardescuento", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task AplicarDescuentoAsync_WithNoTarjeta_ShouldShowError()
        {
            // Arrange
            _viewModel.TarjetaActual = null;

            // Act
            await _viewModel.AplicarDescuentoCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Debe seleccionar una tarjeta primero"), Times.Once);
        }

        [Fact]
        public async Task RefrescarAsync_WithValidTarjeta_ShouldReloadHistorial()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var tarjeta = new TarjetaFidelizacionDto { Id = tarjetaId, CodigoTarjeta = "123456789" };
            _viewModel.TarjetaActual = tarjeta;

            var historial = new List<TransaccionPuntosDto>
            {
                new TransaccionPuntosDto { Id = Guid.NewGuid(), Puntos = 100 }
            };

            _mockTarjetasService.Setup(x => x.ObtenerHistorialAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(historial));

            // Act
            await _viewModel.RefrescarCommand.ExecuteAsync(null);

            // Assert
            _mockTarjetasService.Verify(x => x.ObtenerHistorialAsync(tarjetaId, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Single(_viewModel.Historial);
        }

        [Fact]
        public async Task BloquearTarjetaAsync_WithConfirmation_ShouldBlockTarjeta()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var tarjeta = new TarjetaFidelizacionDto { Id = tarjetaId, CodigoTarjeta = "123456789" };
            _viewModel.TarjetaActual = tarjeta;

            _mockDialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockTarjetasService.Setup(x => x.BloquearTarjetaAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));

            _mockTarjetasService.Setup(x => x.ObtenerTarjetaAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta));

            // Act
            await _viewModel.BloquearTarjetaCommand.ExecuteAsync(null);

            // Assert
            _mockTarjetasService.Verify(x => x.BloquearTarjetaAsync(tarjetaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockDialogService.Verify(x => x.ShowSuccessAsync("Tarjeta bloqueada correctamente"), Times.Once);
        }

        [Fact]
        public async Task BuscarTarjetaAsync_DobleEjecucion_NoDebeReentrar()
        {
            // Arrange
            _viewModel.CodigoTarjeta = "123";
            var tarjeta = new TarjetaFidelizacionDto { Id = Guid.NewGuid(), CodigoTarjeta = "123" };
            _mockTarjetasService.Setup(x => x.ObtenerTarjetaPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
                });
            _mockTarjetasService.Setup(x => x.ObtenerHistorialAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(new List<TransaccionPuntosDto>()));

            // Act
            var t1 = _viewModel.BuscarTarjetaCommand.ExecuteAsync(null);
            var t2 = _viewModel.BuscarTarjetaCommand.ExecuteAsync(null);
            await t1;

            // Assert
            _mockTarjetasService.Verify(x => x.ObtenerTarjetaPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarHistorialAsync_SinTransacciones_NoDebeFallarYQuedaVacio()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            _viewModel.TarjetaActual = new TarjetaFidelizacionDto { Id = tarjetaId, CodigoTarjeta = "123" };
            _mockTarjetasService.Setup(x => x.ObtenerHistorialAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(new List<TransaccionPuntosDto>()));

            // Act
            await _viewModel.CargarHistorialCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.Historial);
            _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BloquearTarjetaAsync_Cancelado_NoDebeInvocarServicio()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            _viewModel.TarjetaActual = new TarjetaFidelizacionDto { Id = tarjetaId, CodigoTarjeta = "123" };
            _mockDialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            await _viewModel.BloquearTarjetaCommand.ExecuteAsync(null);

            // Assert
            _mockTarjetasService.Verify(x => x.BloquearTarjetaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockDialogService.Verify(x => x.ShowSuccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BuscarTarjetaAsync_ErrorServicio_MuestraError()
        {
            // Arrange
            _viewModel.CodigoTarjeta = "123";
            _mockTarjetasService.Setup(x => x.ObtenerTarjetaPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<TarjetaFidelizacionDto>.Failure("Fallo de servicio"));

            // Act
            await _viewModel.BuscarTarjetaCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 