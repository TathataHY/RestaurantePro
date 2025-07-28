using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Preparaciones.ViewModels;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace RestaurantePro.Mobile.UnitTests.Features.Preparaciones.ViewModels
{
    public class PreparacionesViewModelTests
    {
        private Mock<IPreparacionesService> _mockPreparacionesService;
        private Mock<IDialogService> _mockDialogService;
        private Mock<INavigationService> _mockNavigationService;
        private PreparacionesViewModel _viewModel;

        public PreparacionesViewModelTests()
        {
            _mockPreparacionesService = new Mock<IPreparacionesService>();
            _mockDialogService = new Mock<IDialogService>();
            _mockNavigationService = new Mock<INavigationService>();
            
            _viewModel = new PreparacionesViewModel(
                _mockPreparacionesService.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object);
        }

        [Fact]
        public async Task CargarPreparacionesAsync_ConDatosValidos_DebeCargarPreparaciones()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new PreparacionDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    NombreProducto = "Pizza Margherita",
                    Descripcion = "Pizza tradicional italiana",
                    Categoria = "Pizzas",
                    Precio = 15.99m,
                    TiempoPreparacionMinutos = 15,
                    Disponible = true,
                    FechaCreacion = DateTime.Now
                }
            };

            var result = ApiResponse<List<PreparacionDto>>.SuccessResponse(preparaciones);

            _mockPreparacionesService.Setup(x => x.ObtenerPreparacionesAsync(It.IsAny<bool>()))
                                    .ReturnsAsync(result);

            // Act
            await _viewModel.CargarPreparacionesAsync();

            // Assert
            _viewModel.Preparaciones.Should().HaveCount(1);
            _viewModel.Preparaciones.First().NombreProducto.Should().Be("Pizza Margherita");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task CargarPreparacionesAsync_ConError_DebeMostrarError()
        {
            // Arrange
            var result = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                new List<string> { "Error de conexión" }, 
                "Error", 
                500);

            _mockPreparacionesService.Setup(x => x.ObtenerPreparacionesAsync(It.IsAny<bool>()))
                                    .ReturnsAsync(result);

            // Act
            await _viewModel.CargarPreparacionesAsync();

            // Assert
            _mockDialogService.Verify(x => x.ShowErrorAsync("Error de conexión"), Times.Once);
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task CargarPreparacionesAsync_ConFiltros_DebeAplicarFiltros()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "Pizza";
            _viewModel.MostrarSoloPendientes = true;

            var preparaciones = new List<PreparacionDto>
            {
                new PreparacionDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    NombreProducto = "Pizza Margherita",
                    Descripcion = "Pizza tradicional italiana",
                    Categoria = "Pizzas",
                    Precio = 15.99m,
                    TiempoPreparacionMinutos = 15,
                    Disponible = true
                }
            };

            var result = ApiResponse<List<PreparacionDto>>.SuccessResponse(preparaciones);

            _mockPreparacionesService.Setup(x => x.ObtenerPreparacionesAsync(It.IsAny<bool>()))
                                    .ReturnsAsync(result);

            // Act
            await _viewModel.CargarPreparacionesAsync();

            // Assert
            _viewModel.Preparaciones.Should().HaveCount(1);
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task IniciarPreparacionAsync_ConConfirmacion_DebeIniciarPreparacion()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            var preparacionIniciada = new PreparacionDto
            {
                Id = preparacion.Id,
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(true);

            _mockPreparacionesService.Setup(x => x.IniciarPreparacionAsync(preparacion.Id, It.IsAny<IniciarPreparacionDto>()))
                                    .ReturnsAsync(ApiResponse<PreparacionDto>.SuccessResponse(preparacionIniciada));

            // Act
            await _viewModel.IniciarPreparacionCommand.ExecuteAsync(preparacion);

            // Assert
            _mockDialogService.Verify(x => x.ShowSuccessAsync("Preparación iniciada exitosamente"), Times.AtLeastOnce);
            _mockPreparacionesService.Verify(x => x.IniciarPreparacionAsync(preparacion.Id, It.IsAny<IniciarPreparacionDto>()), Times.Once);
        }

        [Fact]
        public async Task IniciarPreparacionAsync_SinConfirmacion_NoDebeIniciarPreparacion()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(false);

            // Act
            await _viewModel.IniciarPreparacionCommand.ExecuteAsync(preparacion);

            // Assert
            _mockPreparacionesService.Verify(x => x.IniciarPreparacionAsync(It.IsAny<Guid>(), It.IsAny<IniciarPreparacionDto>()), Times.Never);
        }

        [Fact]
        public async Task CompletarPreparacionAsync_ConConfirmacion_DebeCompletarPreparacion()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            var preparacionCompletada = new PreparacionDto
            {
                Id = preparacion.Id,
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(true);

            _mockPreparacionesService.Setup(x => x.CompletarPreparacionAsync(preparacion.Id))
                                    .ReturnsAsync(ApiResponse<PreparacionDto>.SuccessResponse(preparacionCompletada));

            // Act
            await _viewModel.CompletarPreparacionCommand.ExecuteAsync(preparacion);

            // Assert
            _mockDialogService.Verify(x => x.ShowSuccessAsync("Preparación completada exitosamente"), Times.AtLeastOnce);
            _mockPreparacionesService.Verify(x => x.CompletarPreparacionAsync(preparacion.Id), Times.Once);
        }

        [Fact]
        public async Task CancelarPreparacionAsync_ConMotivoYConfirmacion_DebeCancelarPreparacion()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            var preparacionCancelada = new PreparacionDto
            {
                Id = preparacion.Id,
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = false
            };

            _mockDialogService.Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                             .ReturnsAsync("Sin ingredientes");

            _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(true);

            _mockPreparacionesService.Setup(x => x.CancelarPreparacionAsync(preparacion.Id, It.IsAny<CancelarPreparacionDto>()))
                                    .ReturnsAsync(ApiResponse<PreparacionDto>.SuccessResponse(preparacionCancelada));

            // Act
            await _viewModel.CancelarPreparacionCommand.ExecuteAsync(preparacion);

            // Assert
            _mockDialogService.Verify(x => x.ShowSuccessAsync("Preparación cancelada exitosamente"), Times.AtLeastOnce);
            _mockPreparacionesService.Verify(x => x.CancelarPreparacionAsync(preparacion.Id, It.IsAny<CancelarPreparacionDto>()), Times.Once);
        }

        [Fact]
        public async Task CancelarPreparacionAsync_SinMotivo_NoDebeCancelarPreparacion()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            _mockDialogService.Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                             .ReturnsAsync(string.Empty);

            // Act
            await _viewModel.CancelarPreparacionCommand.ExecuteAsync(preparacion);

            // Assert
            _mockPreparacionesService.Verify(x => x.CancelarPreparacionAsync(It.IsAny<Guid>(), It.IsAny<CancelarPreparacionDto>()), Times.Never);
        }

        [Fact]
        public async Task CargarColaPreparacionesAsync_ConDatosValidos_DebeCargarCola()
        {
            // Arrange
            var colaPreparaciones = new List<PreparacionDto>
            {
                new PreparacionDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    NombreProducto = "Pizza Margherita",
                    Descripcion = "Pizza tradicional italiana",
                    Categoria = "Pizzas",
                    Precio = 15.99m,
                    TiempoPreparacionMinutos = 15,
                    Disponible = true
                },
                new PreparacionDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pasta Carbonara",
                    NombreProducto = "Pasta Carbonara",
                    Descripcion = "Pasta italiana con salsa carbonara",
                    Categoria = "Pastas",
                    Precio = 12.99m,
                    TiempoPreparacionMinutos = 10,
                    Disponible = true
                }
            };

            var result = ApiResponse<List<PreparacionDto>>.SuccessResponse(colaPreparaciones);

            _mockPreparacionesService.Setup(x => x.ObtenerColaPreparacionesAsync())
                                    .ReturnsAsync(result);

            // Act
            await _viewModel.CargarColaPreparacionesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Preparaciones.Should().HaveCount(2);
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task CargarPreparacionesPendientesAsync_ConDatosValidos_DebeCargarPendientes()
        {
            // Arrange
            var preparacionesPendientes = new List<PreparacionDto>
            {
                new PreparacionDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    NombreProducto = "Pizza Margherita",
                    Descripcion = "Pizza tradicional italiana",
                    Categoria = "Pizzas",
                    Precio = 15.99m,
                    TiempoPreparacionMinutos = 15,
                    Disponible = true
                }
            };

            var result = ApiResponse<List<PreparacionDto>>.SuccessResponse(preparacionesPendientes);

            _mockPreparacionesService.Setup(x => x.ObtenerPreparacionesPorEstadoAsync("Pendiente"))
                                    .ReturnsAsync(result);

            // Act
            await _viewModel.CargarPreparacionesPendientesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Preparaciones.Should().HaveCount(1);
            _viewModel.Preparaciones.First().Nombre.Should().Be("Pizza Margherita");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task SeleccionarPreparacionAsync_ConPreparacion_DebeNavegarADetalle()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            // Act
            await _viewModel.SeleccionarPreparacionCommand.ExecuteAsync(preparacion);

            // Assert
            _viewModel.PreparacionSeleccionada.Should().Be(preparacion);
            _mockNavigationService.Verify(x => x.NavigateToAsync("preparaciondetalle", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task OnDisappearingAsync_DebeLimpiarSeleccion()
        {
            // Arrange
            _viewModel.PreparacionSeleccionada = new PreparacionDto 
            { 
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            };

            // Act
            await _viewModel.OnDisappearingAsync();

            // Assert
            _viewModel.PreparacionSeleccionada.Should().BeNull();
        }

        [Fact]
        public void CrearFiltros_ConFiltrosConfigurados_DebeRetornarFiltrosCorrectos()
        {
            // Arrange
            _viewModel.FiltroBusqueda = "Pizza";
            _viewModel.MostrarSoloPendientes = true;

            // Act
            var filtros = _viewModel.GetType().GetMethod("CrearFiltros", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(_viewModel, null) as FiltroPreparacionesDto;

            // Assert
            filtros.Should().NotBeNull();
            filtros!.SearchTerm.Should().Be("Pizza");
            filtros.Estado.Should().Be("Pendiente");
        }
    }
} 