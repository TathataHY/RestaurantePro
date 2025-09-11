using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Inventory.Ingredients.ViewModels;
using RestaurantePro.Mobile.Core.Services.Inventory;
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

namespace RestaurantePro.Mobile.UnitTests.Features.Inventory.Ingredients.ViewModels
{
    public class IngredientesViewModelTests
    {
        private Mock<IIngredientesService> _mockIngredientesService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IDialogService> _mockDialogService;
        private IngredientesViewModel _viewModel;

        public IngredientesViewModelTests()
        {
            _mockIngredientesService = new Mock<IIngredientesService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            _viewModel = new IngredientesViewModel(
                _mockIngredientesService.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object);
        }

        [Fact]
        public async Task CargarIngredientesCommand_WithValidData_ShouldPopulateIngredientes()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50, StockMinimo = 10 },
                new() { Id = Guid.NewGuid(), Nombre = "Cebolla", StockActual = 30, StockMinimo = 15 }
            };

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CargarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Ingredientes.Should().NotBeNull();
            _viewModel.Ingredientes.Should().HaveCount(2);
            _viewModel.Ingredientes.First().Nombre.Should().Be("Tomate");
            _mockIngredientesService.Verify(x => x.ObtenerIngredientesAsync(_viewModel.SoloDisponibles, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarIngredientesCommand_WithError_ShouldShowError()
        {
            // Arrange
            var response = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(
                new List<string> { "Error al cargar ingredientes" }, 
                "Error", 
                500);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CargarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarIngredientesCommand_WithValidQuery_ShouldFilterResults()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50 },
                new() { Id = Guid.NewGuid(), Nombre = "Cebolla", StockActual = 30 }
            };

            _viewModel.Busqueda = "Tomate";

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(
                ingredientes.Where(i => i.Nombre == "Tomate").ToList());

            _mockIngredientesService.Setup(x => x.BuscarIngredientesAsync("Tomate", It.IsAny<string>(), It.IsAny<bool?>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.BuscarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Ingredientes.Should().HaveCount(1);
            _viewModel.Ingredientes.First().Nombre.Should().Be("Tomate");
            _mockIngredientesService.Verify(x => x.BuscarIngredientesAsync("Tomate", It.IsAny<string>(), It.IsAny<bool?>()), Times.Once);
        }

        [Fact]
        public async Task SeleccionarIngredienteCommand_WithValidIngrediente_ShouldNavigateToDetail()
        {
            // Arrange
            var ingrediente = new IngredienteSummaryDto { Id = Guid.NewGuid(), Nombre = "Tomate" };

            // Act
            await _viewModel.SeleccionarIngredienteCommand.ExecuteAsync(ingrediente);

            // Assert
            _viewModel.IngredienteSeleccionado.Should().Be(ingrediente);
            _mockNavigationService.Verify(x => x.NavigateToAsync("ingredientedetalle", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task SeleccionarIngredienteCommand_WithNullIngrediente_ShouldNotNavigate()
        {
            // Act
            await _viewModel.SeleccionarIngredienteCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public async Task VerMovimientosCommand_WithValidIngrediente_ShouldNavigateToMovimientos()
        {
            // Arrange
            var ingrediente = new IngredienteSummaryDto { Id = Guid.NewGuid(), Nombre = "Tomate" };
            var movimientos = new List<MovimientoInventarioDto>
            {
                new() { Id = Guid.NewGuid(), TipoMovimiento = "Entrada", Cantidad = 10 }
            };

            var response = ApiResponse<List<MovimientoInventarioDto>>.SuccessResponse(movimientos);

            _mockIngredientesService.Setup(x => x.ObtenerMovimientosAsync(ingrediente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.VerMovimientosCommand.ExecuteAsync(ingrediente);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync("movimientosingrediente", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task VerMovimientosCommand_WithError_ShouldShowError()
        {
            // Arrange
            var ingrediente = new IngredienteSummaryDto { Id = Guid.NewGuid(), Nombre = "Tomate" };

            var response = ApiResponse<List<MovimientoInventarioDto>>.ErrorResponse(
                new List<string> { "Error al cargar movimientos" }, 
                "Error", 
                500);

            _mockIngredientesService.Setup(x => x.ObtenerMovimientosAsync(ingrediente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.VerMovimientosCommand.ExecuteAsync(ingrediente);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerBajoStockCommand_ShouldSetSoloBajoStockAndReload()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 5, StockMinimo = 10 },
                new() { Id = Guid.NewGuid(), Nombre = "Cebolla", StockActual = 30, StockMinimo = 15 }
            };

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(
                ingredientes.Where(i => i.StockActual < i.StockMinimo).ToList());

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.VerBajoStockCommand.ExecuteAsync(null);

            // Assert
            _viewModel.SoloBajoStock.Should().BeTrue();
            _viewModel.Ingredientes.Should().HaveCount(1);
            _viewModel.Ingredientes.First().Nombre.Should().Be("Tomate");
        }

        [Fact]
        public async Task RefrescarCommand_ShouldReloadIngredientesAndEstadisticas()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50 }
            };

            var estadisticas = new EstadisticasIngredientesDto
            {
                TotalIngredientes = 1,
                IngredientesDisponibles = 1,
                IngredientesBajoStock = 0
            };

            var ingredientesResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
            var estadisticasResponse = ApiResponse<EstadisticasIngredientesDto>.SuccessResponse(estadisticas);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesResponse);
            _mockIngredientesService.Setup(x => x.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(estadisticasResponse);

            // Act
            await _viewModel.RefrescarCommand.ExecuteAsync(null);

            // Assert
            _viewModel.TotalIngredientes.Should().Be(1);
            _viewModel.IngredientesDisponibles.Should().Be(1);
            _viewModel.IngredientesBajoStock.Should().Be(0);
            _viewModel.Ingredientes.Should().HaveCount(1);
        }

        [Fact]
        public async Task CambiarFiltroDisponiblesCommand_ShouldReloadIngredientes()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50 }
            };

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CambiarFiltroDisponiblesCommand.ExecuteAsync(null);

            // Assert
            _mockIngredientesService.Verify(x => x.ObtenerIngredientesAsync(_viewModel.SoloDisponibles, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CambiarFiltroBajoStockCommand_ShouldReloadIngredientes()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50 }
            };

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CambiarFiltroBajoStockCommand.ExecuteAsync(null);

            // Assert
            _mockIngredientesService.Verify(x => x.ObtenerIngredientesAsync(_viewModel.SoloDisponibles, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnAppearingCommand_ShouldLoadIngredientesAndEstadisticas()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50 }
            };

            var estadisticas = new EstadisticasIngredientesDto
            {
                TotalIngredientes = 1,
                IngredientesDisponibles = 1,
                IngredientesBajoStock = 0
            };

            var ingredientesResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
            var estadisticasResponse = ApiResponse<EstadisticasIngredientesDto>.SuccessResponse(estadisticas);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesResponse);
            _mockIngredientesService.Setup(x => x.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(estadisticasResponse);

            // Act
            await _viewModel.OnAppearingAsync();

            // Assert
            _mockIngredientesService.Verify(x => x.ObtenerIngredientesAsync(_viewModel.SoloDisponibles, It.IsAny<CancellationToken>()), Times.Once);
            _mockIngredientesService.Verify(x => x.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            // Assert
            _viewModel.Busqueda.Should().BeEmpty();
            _viewModel.SoloDisponibles.Should().BeFalse();
            _viewModel.SoloBajoStock.Should().BeFalse();
            _viewModel.TotalIngredientes.Should().Be(0);
            _viewModel.IngredientesDisponibles.Should().Be(0);
            _viewModel.IngredientesBajoStock.Should().Be(0);
            _viewModel.IngredienteSeleccionado.Should().BeNull();
        }

        [Fact]
        public async Task CargarIngredientesAsync_SinDatos_DebeQuedarVacio()
        {
            // Arrange
            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(new List<IngredienteSummaryDto>());

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CargarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Ingredientes.Should().BeEmpty();
        }

        [Fact]
        public async Task BuscarIngredientesAsync_SinResultados_DebeQuedarVacio()
        {
            // Arrange
            _viewModel.Busqueda = "Inexistente";
            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(new List<IngredienteSummaryDto>());

            _mockIngredientesService.Setup(x => x.BuscarIngredientesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.BuscarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Ingredientes.Should().BeEmpty();
        }

        [Fact]
        public async Task CargarIngredientesAsync_DosVeces_NoDebeDuplicarResultados()
        {
            // Arrange
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50, StockMinimo = 10 }
            };

            var response = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.CargarIngredientesCommand.ExecuteAsync(null);
            await _viewModel.CargarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Ingredientes.Should().HaveCount(1);
            _viewModel.Ingredientes.First().Nombre.Should().Be("Tomate");
            _mockIngredientesService.Verify(x => x.ObtenerIngredientesAsync(It.IsAny<bool>()), Times.Exactly(2));
        }

        [Fact]
        public async Task BuscarIngredientesCommand_WithError_ShouldShowError()
        {
            // Arrange
            _viewModel.Busqueda = "Tomate";
            var response = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(
                new List<string> { "Error al buscar ingredientes" },
                "Error",
                500);

            _mockIngredientesService.Setup(x => x.BuscarIngredientesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.BuscarIngredientesCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerIngredienteCommand_WithValidItem_ShouldShowDetailsDialog()
        {
            // Arrange
            var ingrediente = new IngredienteSummaryDto { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 42 };

            // Act
            await _viewModel.VerIngredienteCommand.ExecuteAsync(ingrediente);

            // Assert
            _mockDialogService.Verify(x => x.ShowAlertAsync(
                It.Is<string>(t => t.Contains("Detalles")),
                It.Is<string>(m => m.Contains("Nombre:") && m.Contains("Tomate") && m.Contains("Stock:") && m.Contains("42")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task VerIngredienteCommand_WithNull_ShouldNotShowDialog()
        {
            // Act
            await _viewModel.VerIngredienteCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CrearIngredienteCommand_ShouldShowNoDisponible()
        {
            // Act
            await _viewModel.CrearIngredienteCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowAlertAsync(
                It.Is<string>(t => t.Contains("Función no disponible")),
                It.Is<string>(m => m.Contains("creación de ingredientes") || m.Contains("no está disponible")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task EditarIngredienteCommand_ShouldShowNoDisponible()
        {
            // Arrange
            var ingrediente = new IngredienteSummaryDto { Id = Guid.NewGuid(), Nombre = "Tomate" };

            // Act
            await _viewModel.EditarIngredienteCommand.ExecuteAsync(ingrediente);

            // Assert
            _mockDialogService.Verify(x => x.ShowAlertAsync(
                It.Is<string>(t => t.Contains("Función no disponible")),
                It.Is<string>(m => m.Contains("edición de ingredientes") || m.Contains("no está disponible")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AjustarStockCommand_ShouldShowNoDisponible()
        {
            // Arrange
            var ingrediente = new IngredienteSummaryDto { Id = Guid.NewGuid(), Nombre = "Tomate" };

            // Act
            await _viewModel.AjustarStockCommand.ExecuteAsync(ingrediente);

            // Assert
            _mockDialogService.Verify(x => x.ShowAlertAsync(
                It.Is<string>(t => t.Contains("Función no disponible")),
                It.Is<string>(m => m.Contains("ajuste de stock") || m.Contains("no está disponible")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoadAlertasStockCommand_WithError_ShouldShowError()
        {
            // Arrange
            var response = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(
                new List<string> { "Error al cargar alertas" }, "Error", 500);

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesBajoStockAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.LoadAlertasStockCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarIngredientesAsync_DobleEjecucion_NoDebeReentrar()
        {
            var ingredientes = new List<IngredienteSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50, StockMinimo = 10 }
            };

            _mockIngredientesService.Setup(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
                });

            var t1 = _viewModel.CargarIngredientesCommand.ExecuteAsync(null);
            var t2 = _viewModel.CargarIngredientesCommand.ExecuteAsync(null);
            await t1;

            _mockIngredientesService.Verify(x => x.ObtenerIngredientesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarIngredientesAsync_DobleEjecucion_NoDebeReentrar()
        {
            _viewModel.Busqueda = "Tomate";
            var lista = new List<IngredienteSummaryDto> { new() { Id = Guid.NewGuid(), Nombre = "Tomate" } };

            _mockIngredientesService
                .Setup(x => x.BuscarIngredientesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(lista);
                });

            var t1 = _viewModel.BuscarIngredientesCommand.ExecuteAsync(null);
            var t2 = _viewModel.BuscarIngredientesCommand.ExecuteAsync(null);
            await t1;

            _mockIngredientesService.Verify(x => x.BuscarIngredientesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()), Times.Once);
        }
    }
} 