using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Categorias.ViewModels;
using RestaurantePro.Mobile.Core.Services.Categorias;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantePro.Mobile.UnitTests.Features.Inventory.Categorias.ViewModels
{
    public class CategoriasViewModelTests
    {
        private Mock<ICategoriasService> _mockCategoriasService;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IDialogService> _mockDialogService;
        private CategoriasViewModel _viewModel;

        public CategoriasViewModelTests()
        {
            _mockCategoriasService = new Mock<ICategoriasService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            
            _viewModel = new CategoriasViewModel(
                _mockCategoriasService.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object);
        }

        [Fact]
        public async Task OnAppearingAsync_WithValidData_ShouldPopulateCategorias()
        {
            // Arrange
            var categorias = new List<CategoriaProductoDto>
            {
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada", Activa = true },
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Platos Principales", Descripcion = "Platos fuertes", Activa = true }
            };

            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

            // Act
            await _viewModel.CargarCategoriasAsync();

            // Assert
            Assert.NotNull(_viewModel.Categorias);
            Assert.Equal(2, _viewModel.Categorias.Count);
            Assert.Equal("Entradas", _viewModel.Categorias[0].Nombre);
        }

        [Fact]
        public async Task CargarCategoriasAsync_WithError_ShouldShowError()
        {
            // Arrange
            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.ErrorResponse("Error al cargar categorías"));

            // Act
            await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        }

        [Fact]
        public async Task BuscarCategoriasAsync_WithValidQuery_ShouldFilterResults()
        {
            // Arrange
            var categorias = new List<CategoriaProductoDto>
            {
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada", Activa = true }
            };

            _viewModel.FiltroBusqueda = "Entradas";

            _mockCategoriasService.Setup(x => x.BuscarCategoriasAsync("Entradas", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

            // Act
            await _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(_viewModel.Categorias);
            Assert.Single(_viewModel.Categorias);
            Assert.Equal("Entradas", _viewModel.Categorias[0].Nombre);
        }

        [Fact]
        public async Task SeleccionarCategoriaAsync_WithValidCategoria_ShouldNavigateToProductos()
        {
            // Arrange
            var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas" };

            // Act
            await _viewModel.SeleccionarCategoriaCommand.ExecuteAsync(categoria);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateToAsync("productos", It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Equal(categoria, _viewModel.CategoriaSeleccionada);
        }

        [Fact]
        public async Task RefrescarAsync_ShouldReloadData()
        {
            // Arrange
            var categorias = new List<CategoriaProductoDto>
            {
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada", Activa = true }
            };

            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

            // Act
            await _viewModel.RefrescarCommand.ExecuteAsync(null);

            // Assert
            _mockCategoriasService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(_viewModel.Categorias);
            Assert.Single(_viewModel.Categorias);
        }

        [Fact]
        public async Task CambiarFiltroActivasAsync_ShouldReloadData()
        {
            // Arrange
            var categorias = new List<CategoriaProductoDto>
            {
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada", Activa = true }
            };

            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

            // Act
            await _viewModel.CambiarFiltroActivasCommand.ExecuteAsync(null);

            // Assert
            _mockCategoriasService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActualizarEstadisticas_WithCategorias_ShouldUpdateCounts()
        {
            // Arrange
            var categorias = new List<CategoriaProductoDto>
            {
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true },
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Platos Principales", Activa = true },
                new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Postres", Activa = false }
            };

            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

            // Act
            await _viewModel.CargarCategoriasAsync();

            // Assert
            Assert.Equal(3, _viewModel.TotalCategorias);
            Assert.Equal(2, _viewModel.CategoriasActivas);
            Assert.Equal(1, _viewModel.CategoriasInactivas);
        }

        [Fact]
        public void MostrarSoloActivas_WhenChanged_ShouldUpdateFilter()
        {
            // Arrange
            _viewModel.MostrarSoloActivas = true;

            // Act
            _viewModel.MostrarSoloActivas = false;

            // Assert
            Assert.False(_viewModel.MostrarSoloActivas);
        }

        [Fact]
        public void FiltroBusqueda_WhenSet_ShouldUpdateProperty()
        {
            // Arrange
            var busqueda = "Entradas";

            // Act
            _viewModel.FiltroBusqueda = busqueda;

            // Assert
            Assert.Equal(busqueda, _viewModel.FiltroBusqueda);
        }

        [Fact]
        public async Task CargarCategoriasAsync_SinDatos_DebeQuedarVacioYEstadisticasEnCero()
        {
            // Arrange
            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));

            // Act
            await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.Categorias);
            Assert.Equal(0, _viewModel.TotalCategorias);
            Assert.Equal(0, _viewModel.CategoriasActivas);
            Assert.Equal(0, _viewModel.CategoriasInactivas);
        }

        [Fact]
        public async Task BuscarCategoriasAsync_SinResultados_DebeQuedarVacio()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "Inexistente";
            _mockCategoriasService.Setup(x => x.BuscarCategoriasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));

            // Act
            await _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.Categorias);
        }

        [Fact]
        public async Task CargarCategoriasAsync_DobleEjecucion_NoDebeReentrar()
        {
            var categorias = new List<CategoriaProductoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true }
            };

            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
                });

            var t1 = _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
            var t2 = _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
            await t1;

            _mockCategoriasService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarCategoriasAsync_DobleEjecucion_NoDebeReentrar()
        {
            _viewModel.FiltroBusqueda = "Entradas";
            var categorias = new List<CategoriaProductoDto> { new() { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true } };

            _mockCategoriasService.Setup(x => x.BuscarCategoriasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
                });

            var t1 = _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);
            var t2 = _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);
            await t1;

            _mockCategoriasService.Verify(x => x.BuscarCategoriasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarCategoriasAsync_WithError_ShouldShowError()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "Entradas";
            _mockCategoriasService.Setup(x => x.BuscarCategoriasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.ErrorResponse("Error al buscar categorías"));

            // Act
            await _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(d => d.ShowErrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarCategoriasAsync_EmptyQuery_NoDebeInvocarServicio()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "   ";

            // Act
            await _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);

            // Assert
            _mockCategoriasService.Verify(s => s.BuscarCategoriasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SeleccionarCategoriaAsync_Null_NoDebeNavegar()
        {
            // Act
            await _viewModel.SeleccionarCategoriaCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(n => n.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public async Task CargarCategoriasAsync_Exception_ShouldSetHasError()
        {
            // Arrange
            _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Fallo inesperado"));

            // Act
            await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.HasError);
            Assert.Equal("Fallo inesperado", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task BuscarCategoriasAsync_SuccessPeroDataNull_ShouldShowError()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "Entradas";
            var response = new ApiResponse<List<CategoriaProductoDto>>
            {
                Success = true,
                Data = null,
                Message = null
            };
            _mockCategoriasService.Setup(s => s.BuscarCategoriasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.BuscarCategoriasCommand.ExecuteAsync(null);

            // Assert
            _mockDialogService.Verify(d => d.ShowErrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 