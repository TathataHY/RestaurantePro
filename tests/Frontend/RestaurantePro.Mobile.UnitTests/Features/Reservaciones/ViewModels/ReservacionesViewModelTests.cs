using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Inventory.Reservaciones.ViewModels;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace RestaurantePro.Mobile.UnitTests.Features.Reservaciones.ViewModels
{
    public class ReservacionesViewModelTests
    {
        private Mock<IReservacionesService> _mockReservacionesService;
        private Mock<IDialogService> _mockDialogService;
        private ReservacionesViewModel _viewModel;

        public ReservacionesViewModelTests()
        {
            _mockReservacionesService = new Mock<IReservacionesService>();
            _mockDialogService = new Mock<IDialogService>();
            
            _viewModel = new ReservacionesViewModel(
                _mockReservacionesService.Object,
                _mockDialogService.Object);
        }

        [Fact]
        public async Task CargarReservacionesAsync_ConDatosValidos_DeberiaCargarReservaciones()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", Estado = "Pendiente" },
                new() { Id = Guid.NewGuid(), NombreCliente = "María García", Estado = "Confirmada" }
            };

            var response = ApiResponse<List<ReservacionDto>>.SuccessResponse(reservaciones);

            _mockReservacionesService.Setup(x => x.ObtenerReservacionesAsync())
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.CargarReservacionesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Reservaciones.Should().NotBeNull();
            _viewModel.Reservaciones.Count.Should().Be(2);
            _viewModel.IsLoading.Should().BeFalse();
            _mockReservacionesService.Verify(x => x.ObtenerReservacionesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarReservacionesAsync_ConError_DeberiaMostrarError()
        {
            // Arrange
            var response = ApiResponse<List<ReservacionDto>>.ErrorResponse(
                new List<string> { "Error al cargar" }, 
                "Error", 
                500);

            _mockReservacionesService.Setup(x => x.ObtenerReservacionesAsync())
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.CargarReservacionesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
            _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Error", "OK", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CargarReservacionesHoyAsync_DeberiaCargarReservacionesDelDia()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", FechaReservacion = DateTime.Today }
            };

            var response = ApiResponse<List<ReservacionDto>>.SuccessResponse(reservaciones);

            _mockReservacionesService.Setup(x => x.ObtenerReservacionesHoyAsync(It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.CargarReservacionesHoyCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Reservaciones.Should().NotBeNull();
            _viewModel.Reservaciones.Count.Should().Be(1);
            _mockReservacionesService.Verify(x => x.ObtenerReservacionesHoyAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CargarEstadisticasAsync_DeberiaCargarEstadisticas()
        {
            // Arrange
            var estadisticas = new EstadisticasReservacionesDto
            {
                TotalReservaciones = 10,
                ReservacionesConfirmadas = 5,
                ReservacionesPendientes = 3,
                ReservacionesCanceladas = 2
            };

            var response = ApiResponse<EstadisticasReservacionesDto>.SuccessResponse(estadisticas);

            _mockReservacionesService.Setup(x => x.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.CargarEstadisticasCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Estadisticas.Should().NotBeNull();
            _viewModel.Estadisticas!.TotalReservaciones.Should().Be(10);
            _mockReservacionesService.Verify(x => x.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarReservacionesAsync_ConTerminoValido_DeberiaBuscarReservaciones()
        {
            // Arrange
            _viewModel.TerminoBusqueda = "Juan";

            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", Estado = "Pendiente" }
            };

            var response = ApiResponse<List<ReservacionDto>>.SuccessResponse(reservaciones);

            _mockReservacionesService.Setup(x => x.BuscarReservacionesAsync("Juan", It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.BuscarReservacionesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.ReservacionesFiltradas.Should().NotBeNull();
            _viewModel.ReservacionesFiltradas.Count.Should().Be(1);
            _viewModel.IsLoading.Should().BeFalse();
            _mockReservacionesService.Verify(x => x.BuscarReservacionesAsync("Juan", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuscarReservacionesAsync_SinTermino_DeberiaAplicarFiltros()
        {
            // Arrange
            _viewModel.TerminoBusqueda = "";

            // Act
            await _viewModel.BuscarReservacionesCommand.ExecuteAsync(null);

            // Assert
            _mockReservacionesService.Verify(x => x.BuscarReservacionesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CambiarEstadoAsync_ConEstadoValido_DeberiaCambiarEstado()
        {
            // Arrange
            var reservacion = new ReservacionDto { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", Estado = "Pendiente" };

            var response = ApiResponse<ReservacionDto>.SuccessResponse(reservacion);

            _mockDialogService.Setup(x => x.ShowActionSheetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                             .ReturnsAsync("Confirmada");

            _mockReservacionesService.Setup(x => x.CambiarEstadoReservacionAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.CambiarEstadoCommand.ExecuteAsync(reservacion);

            // Assert
            _mockDialogService.Verify(x => x.ShowActionSheetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.AtLeastOnce);
            _mockReservacionesService.Verify(x => x.CambiarEstadoReservacionAsync(reservacion.Id, "Confirmada", It.IsAny<CancellationToken>()), Times.Once);
            _mockDialogService.Verify(x => x.ShowAlertAsync("Éxito", "Estado actualizado correctamente", "OK", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CambiarEstadoAsync_ConCancelacion_NoDeberiaCambiarEstado()
        {
            // Arrange
            var reservacion = new ReservacionDto { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", Estado = "Pendiente" };

            _mockDialogService.Setup(x => x.ShowActionSheetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                             .ReturnsAsync("Cancelar");

            // Act
            await _viewModel.CambiarEstadoCommand.ExecuteAsync(reservacion);

            // Assert
            _mockReservacionesService.Verify(x => x.CambiarEstadoReservacionAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AsignarMesaAsync_ConMesaValida_DeberiaAsignarMesa()
        {
            // Arrange
            var reservacion = new ReservacionDto { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez" };

            var response = ApiResponse<ReservacionDto>.SuccessResponse(reservacion);

            _mockDialogService.Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                             .ReturnsAsync("Mesa 5");

            _mockReservacionesService.Setup(x => x.AsignarMesaAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                                    .ReturnsAsync(response);

            // Act
            await _viewModel.AsignarMesaCommand.ExecuteAsync(reservacion);

            // Assert
            _mockDialogService.Verify(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.AtLeastOnce);
            _mockReservacionesService.Verify(x => x.AsignarMesaAsync(reservacion.Id, "Mesa 5", It.IsAny<CancellationToken>()), Times.Once);
            _mockDialogService.Verify(x => x.ShowAlertAsync("Éxito", "Mesa asignada correctamente", "OK", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AsignarMesaAsync_ConCancelacion_NoDeberiaAsignarMesa()
        {
            // Arrange
            var reservacion = new ReservacionDto { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez" };

            _mockDialogService.Setup(x => x.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                             .ReturnsAsync("Cancelar");

            // Act
            await _viewModel.AsignarMesaCommand.ExecuteAsync(reservacion);

            // Assert
            _mockReservacionesService.Verify(x => x.AsignarMesaAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void OnTerminoBusquedaChanged_DeberiaAplicarFiltros()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", Estado = "Pendiente" },
                new() { Id = Guid.NewGuid(), NombreCliente = "María García", Estado = "Confirmada" }
            };

            foreach (var reservacion in reservaciones)
            {
                _viewModel.Reservaciones.Add(reservacion);
            }

            // Act
            _viewModel.TerminoBusqueda = "Juan";

            // Assert
            _viewModel.ReservacionesFiltradas.Should().HaveCount(1);
            _viewModel.ReservacionesFiltradas.First().NombreCliente.Should().Be("Juan Pérez");
        }

        [Fact]
        public void OnEstadoFiltroChanged_DeberiaAplicarFiltros()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez", Estado = "Pendiente" },
                new() { Id = Guid.NewGuid(), NombreCliente = "María García", Estado = "Confirmada" }
            };

            foreach (var reservacion in reservaciones)
            {
                _viewModel.Reservaciones.Add(reservacion);
            }

            // Act
            _viewModel.EstadoFiltro = "Pendiente";

            // Assert
            _viewModel.ReservacionesFiltradas.Should().HaveCount(1);
            _viewModel.ReservacionesFiltradas.First().Estado.Should().Be("Pendiente");
        }

        [Fact]
        public void OnFechaSeleccionadaChanged_DeberiaCargarReservaciones()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NombreCliente = "Juan Pérez" }
            };

            var response = ApiResponse<List<ReservacionDto>>.SuccessResponse(reservaciones);

            _mockReservacionesService.Setup(x => x.ObtenerReservacionesAsync())
                                    .ReturnsAsync(response);

            // Act
            _viewModel.FechaSeleccionada = DateTime.Today.AddDays(1);

            // Assert
            _mockReservacionesService.Verify(x => x.ObtenerReservacionesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Estados_DeberiaContenerEstadosCorrectos()
        {
            // Assert
            _viewModel.Estados.Should().Contain("Todas");
            _viewModel.Estados.Should().Contain("Confirmada");
            _viewModel.Estados.Should().Contain("Pendiente");
            _viewModel.Estados.Should().Contain("Cancelada");
            _viewModel.Estados.Should().Contain("Completada");
            _viewModel.Estados.Should().HaveCount(5);
        }
    }
} 
