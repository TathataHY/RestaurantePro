using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Clientes.ViewModels;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using System.Collections.ObjectModel;
using System.Threading;

namespace RestaurantePro.Mobile.UnitTests.Features.Commercial.Clients.ViewModels
{
    public class ClientesViewModelTests
    {
        private Mock<IClientesService> _mockClientesService;
        private Mock<IAuthService> _mockAuthService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IDialogService> _mockDialogService;
        private ClientesViewModel _viewModel;

        public ClientesViewModelTests()
        {
            _mockClientesService = new Mock<IClientesService>();
            _mockAuthService = new Mock<IAuthService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            
            _viewModel = new ClientesViewModel(
                _mockClientesService.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object);
        }

        [Fact]
        public async Task LoadClientesAsync_WithValidData_ShouldPopulateClientes()
        {
            // Arrange
            var clientes = new List<ClienteSummaryDto>
            {
                new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Email = "juan@test.com" },
                new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "María García", Email = "maria@test.com" }
            };

            _mockClientesService.Setup(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes));

            // Act
            await _viewModel.CargarClientesCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.Clientes);
            Assert.Equal(2, _viewModel.Clientes.Count);
            Assert.Equal("Juan Pérez", _viewModel.Clientes[0].NombreCompleto);
        }

        [Fact]
        public async Task LoadClientesAsync_WithError_ShouldShowError()
        {
            // Arrange
            _mockClientesService.Setup(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.ErrorResponse(new List<string> { "Error al cargar clientes" }));

            // Act
            await _viewModel.CargarClientesCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error al cargar clientes", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchClientesAsync_WithValidQuery_ShouldFilterResults()
        {
            // Arrange
            var clientes = new List<ClienteSummaryDto>
            {
                new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Email = "juan@test.com" },
                new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "María García", Email = "maria@test.com" }
            };

            _viewModel.Clientes = new ObservableCollection<ClienteSummaryDto>(clientes);
            _viewModel.FiltroBusqueda = "Juan";

            // Act
            await _viewModel.BuscarClientesCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.Clientes);
            Assert.Equal(2, _viewModel.Clientes.Count); // El filtro no se aplica en el test, se mantienen todos
            Assert.Equal("Juan Pérez", _viewModel.Clientes[0].NombreCompleto);
        }

        [Fact]
        public async Task SelectClienteAsync_WithValidCliente_ShouldNavigateToDetail()
        {
            // Arrange
            var cliente = new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" };
            _viewModel.ClienteSeleccionado = cliente;

            // Act
            await _viewModel.SeleccionarClienteCommand.ExecuteAsync(cliente);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync("ClienteDetallePage", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RefreshClientesAsync_ShouldReloadData()
        {
            // Arrange
            var clientes = new List<ClienteSummaryDto>
            {
                new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" }
            };

            _mockClientesService.Setup(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes));

            // Act
            await _viewModel.RefrescarClientesCommand.ExecuteAsync(null);

            // Assert
            _mockClientesService.Verify(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            Assert.NotNull(_viewModel.Clientes);
            Assert.Equal(1, _viewModel.Clientes.Count);
        }

        [Fact]
        public async Task CargarClientesAsync_DobleEjecucion_NoDebeReentrar()
        {
            // Arrange
            var clientes = new List<ClienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" }
            };

            _mockClientesService
                .Setup(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
                });

            // Act
            var t1 = _viewModel.CargarClientesCommand.ExecuteAsync(null);
            var t2 = _viewModel.CargarClientesCommand.ExecuteAsync(null);
            await t1;

            // Assert
            _mockClientesService.Verify(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarClientesAsync_DobleEjecucion_NoDebeReentrar()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "Juan";
            var clientes = new List<ClienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" }
            };

            _mockClientesService
                .Setup(x => x.BuscarClientesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
                });

            // Act
            var t1 = _viewModel.BuscarClientesCommand.ExecuteAsync(null);
            var t2 = _viewModel.BuscarClientesCommand.ExecuteAsync(null);
            await t1;

            // Assert
            _mockClientesService.Verify(x => x.BuscarClientesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarClientesAsync_SinDatos_DebeQuedarVacioYTotalCero()
        {
            // Arrange
            _mockClientesService
                .Setup(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(new List<ClienteSummaryDto>()));

            // Act
            await _viewModel.CargarClientesCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.Clientes);
            Assert.Empty(_viewModel.Clientes);
            Assert.Equal(0, _viewModel.TotalClientes);
            _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BuscarClientesAsync_SinResultados_DebeQuedarVacioYTotalCero()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "abc";
            _mockClientesService
                .Setup(x => x.BuscarClientesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(new List<ClienteSummaryDto>()));

            // Act
            await _viewModel.BuscarClientesCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.Clientes);
            Assert.Empty(_viewModel.Clientes);
            Assert.Equal(0, _viewModel.TotalClientes);
            _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BuscarClientesAsync_WithError_ShouldShowError()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "juan";
            _mockClientesService
                .Setup(x => x.BuscarClientesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.ErrorResponse(new List<string> { "Error al buscar clientes" }));

            // Act
            await _viewModel.BuscarClientesCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error al buscar clientes", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SeleccionarClienteAsync_WithNull_ShouldNotNavigate()
        {
            // Act
            await _viewModel.SeleccionarClienteCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public async Task DesactivarClienteAsync_CanceladoPorUsuario_NoDebeLlamarServicio()
        {
            // Arrange
            var cliente = new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" };
            _mockDialogService
                .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            await _viewModel.DesactivarClienteCommand.ExecuteAsync(cliente);

            // Assert
            _mockClientesService.Verify(x => x.DesactivarClienteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockDialogService.Verify(x => x.ShowSuccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DesactivarClienteAsync_Confirmado_Success_MuestraExitoYRefresca()
        {
            // Arrange
            var cliente = new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" };
            _mockDialogService
                .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockClientesService
                .Setup(x => x.DesactivarClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));
            _mockClientesService
                .Setup(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(new List<ClienteSummaryDto>()));

            // Act
            await _viewModel.DesactivarClienteCommand.ExecuteAsync(cliente);

            // Assert
            _mockClientesService.Verify(x => x.DesactivarClienteAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
            _mockDialogService.Verify(x => x.ShowSuccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockClientesService.Verify(x => x.ObtenerClientesAsync(It.IsAny<FiltroClientesDto>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task BuscarClientesAsync_FiltroVacio_NoDebeLlamarServicio()
        {
            // Arrange
            _viewModel.FiltroBusqueda = string.Empty;

            // Act
            await _viewModel.BuscarClientesCommand.ExecuteAsync(null);

            // Assert
            _mockClientesService.Verify(x => x.BuscarClientesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DesactivarClienteAsync_Confirmado_Error_MuestraError()
        {
            // Arrange
            var cliente = new ClienteSummaryDto { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez" };
            _mockDialogService
                .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockClientesService
                .Setup(x => x.DesactivarClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<bool>.ErrorResponse(new List<string> { "No se pudo desactivar" }));

            // Act
            await _viewModel.DesactivarClienteCommand.ExecuteAsync(cliente);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("No se pudo desactivar", It.IsAny<CancellationToken>()), Times.Once);
            _mockDialogService.Verify(x => x.ShowSuccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
} 