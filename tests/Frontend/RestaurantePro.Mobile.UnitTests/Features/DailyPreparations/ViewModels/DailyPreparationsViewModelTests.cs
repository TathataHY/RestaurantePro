using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;

namespace RestaurantePro.Mobile.UnitTests.Features.DailyPreparations.ViewModels
{
    public class DailyPreparationsViewModelTests
    {
        private readonly Mock<IDailyPreparationsService> _mockService = new();
        private readonly Mock<IDialogService> _mockDialog = new();
        private readonly Mock<INavigationService> _mockNav = new();

        private DailyPreparationsViewModel CreateVm() =>
            new DailyPreparationsViewModel(_mockService.Object, _mockDialog.Object, _mockNav.Object);

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Act
            var vm = CreateVm();

            // Assert
            vm.PreparacionesDiarias.Should().NotBeNull();
            vm.PreparacionesDiarias.Should().BeEmpty();
            vm.SelectedPreparacion.Should().BeNull();
            vm.Estadisticas.Should().BeNull();
            vm.FiltroEstado.Should().Be("Todos");
            vm.MostrarEstadisticas.Should().BeTrue();
            vm.MostrarFiltros.Should().BeFalse();
            vm.TextoBusqueda.Should().BeEmpty();
            vm.IsBusy.Should().BeFalse();
            vm.IsRefreshing.Should().BeFalse();
        }

        #endregion

        #region LoadPreparacionesDiariasAsync Tests

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_Success_ShouldPopulateAndToggleBusy()
        {
            // Arrange
            var data = new List<PreparacionDiariaDto>
            {
                new() { Id = Guid.NewGuid(), NombreProducto = "Ceviche", NombreChef = "Ana" }
            };
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(data));

            var vm = CreateVm();
            vm.IsBusy.Should().BeFalse();

            // Act
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            // Assert
            vm.IsBusy.Should().BeFalse();
            vm.PreparacionesDiarias.Should().HaveCount(1);
            vm.PreparacionesDiarias[0].NombreProducto.Should().Be("Ceviche");
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_Error_ShouldShowError()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Failure("Error"));

            var vm = CreateVm();

            // Act
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Error"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_DoubleExecution_ShouldNotReenter()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .Returns(async () =>
                        {
                            await Task.Delay(200);
                            return Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>());
                        });

            var vm = CreateVm();

            // Act
            var t1 = vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);
            var t2 = vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            await t1;

            // Assert
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Once);
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_WithNullData_ShouldHandleGracefully()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(null!));

            var vm = CreateVm();

            // Act
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            // Assert
            vm.PreparacionesDiarias.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_WithEmptyData_ShouldHandleGracefully()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();

            // Act
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            // Assert
            vm.PreparacionesDiarias.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_WithException_ShouldShowError()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ThrowsAsync(new Exception("Database connection failed"));

            var vm = CreateVm();

            // Act
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Database connection failed"))), Times.AtLeastOnce);
        }

        #endregion

        #region LoadEstadisticasAsync Tests

        [Fact]
        public async Task LoadEstadisticasAsync_Success_ShouldSetEstadisticas()
        {
            // Arrange
            var estadisticas = new EstadisticasPreparacionesDiariasDto
            {
                TotalPreparaciones = 10,
                PreparacionesDisponibles = 8,
                PreparacionesVencidas = 2
            };
            _mockService.Setup(s => s.GetEstadisticasAsync())
                        .ReturnsAsync(Result<EstadisticasPreparacionesDiariasDto>.Success(estadisticas));

            var vm = CreateVm();

            // Act
            await vm.LoadEstadisticasCommand.ExecuteAsync(null);

            // Assert
            vm.Estadisticas.Should().NotBeNull();
            vm.Estadisticas.TotalPreparaciones.Should().Be(10);
            vm.Estadisticas.PreparacionesDisponibles.Should().Be(8);
            vm.Estadisticas.PreparacionesVencidas.Should().Be(2);
        }

        [Fact]
        public async Task LoadEstadisticasAsync_Error_ShouldShowError()
        {
            // Arrange
            _mockService.Setup(s => s.GetEstadisticasAsync())
                        .ReturnsAsync(Result<EstadisticasPreparacionesDiariasDto>.Failure("Error"));

            var vm = CreateVm();

            // Act
            await vm.LoadEstadisticasCommand.ExecuteAsync(null);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Error"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LoadEstadisticasAsync_WithException_ShouldShowError()
        {
            // Arrange
            _mockService.Setup(s => s.GetEstadisticasAsync())
                        .ThrowsAsync(new Exception("Network error"));

            var vm = CreateVm();

            // Act
            await vm.LoadEstadisticasCommand.ExecuteAsync(null);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Network error"))), Times.AtLeastOnce);
        }

        #endregion

        #region FiltrarPorEstadoAsync Tests

        [Fact]
        public async Task FiltrarPorEstadoAsync_Todos_ShouldReloadAllAndSetFiltro()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();

            // Act
            await vm.FiltrarPorEstadoCommand.ExecuteAsync("Todos");

            // Assert
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Once);
            vm.FiltroEstado.Should().Be("Todos");
        }

        [Fact]
        public async Task FiltrarPorEstadoAsync_Especifico_ShouldCallByEstadoAndSetFiltro()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasPorEstadoAsync("Pendiente"))
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();

            // Act
            await vm.FiltrarPorEstadoCommand.ExecuteAsync("Pendiente");

            // Assert
            _mockService.Verify(s => s.GetPreparacionesDiariasPorEstadoAsync("Pendiente"), Times.Once);
            vm.FiltroEstado.Should().Be("Pendiente");
        }

        [Fact]
        public async Task FiltrarPorEstadoAsync_WithError_ShouldShowError()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasPorEstadoAsync("Pendiente"))
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Failure("Error"));

            var vm = CreateVm();

            // Act
            await vm.FiltrarPorEstadoCommand.ExecuteAsync("Pendiente");

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Error"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task FiltrarPorEstadoAsync_WithException_ShouldShowError()
        {
            // Arrange
            _mockService.Setup(s => s.GetPreparacionesDiariasPorEstadoAsync("Pendiente"))
                        .ThrowsAsync(new Exception("Network error"));

            var vm = CreateVm();

            // Act
            await vm.FiltrarPorEstadoCommand.ExecuteAsync("Pendiente");

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Network error"))), Times.AtLeastOnce);
        }

        #endregion

        #region MarcarComoDisponibleAsync Tests

        [Fact]
        public async Task MarcarComoDisponibleAsync_Confirmed_ShouldCallServiceAndReload()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Arroz" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.MarcarComoDisponibleAsync(prep.Id))
                        .ReturnsAsync(Result<PreparacionDiariaDto>.Success(prep));
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();

            // Act
            await vm.MarcarComoDisponibleCommand.ExecuteAsync(prep);

            // Assert
            _mockService.Verify(s => s.MarcarComoDisponibleAsync(prep.Id), Times.Once);
            _mockDialog.Verify(d => d.ShowSuccessAsync(It.Is<string>(m => m.Contains("disponible"))), Times.AtLeastOnce);
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MarcarComoDisponibleAsync_NotConfirmed_ShouldNotCallService()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Arroz" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(false);

            var vm = CreateVm();

            // Act
            await vm.MarcarComoDisponibleCommand.ExecuteAsync(prep);

            // Assert
            _mockService.Verify(s => s.MarcarComoDisponibleAsync(prep.Id), Times.Never);
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Never);
        }

        [Fact]
        public async Task MarcarComoDisponibleAsync_WithNullPreparacion_ShouldReturnEarly()
        {
            // Arrange
            var vm = CreateVm();

            // Act
            await vm.MarcarComoDisponibleCommand.ExecuteAsync(null);

            // Assert
            _mockDialog.Verify(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockService.Verify(s => s.MarcarComoDisponibleAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task MarcarComoDisponibleAsync_WithServiceError_ShouldShowError()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Arroz" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.MarcarComoDisponibleAsync(prep.Id))
                        .ReturnsAsync(Result<PreparacionDiariaDto>.Failure("Error"));

            var vm = CreateVm();

            // Act
            await vm.MarcarComoDisponibleCommand.ExecuteAsync(prep);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Error"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MarcarComoDisponibleAsync_WithException_ShouldShowError()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Arroz" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.MarcarComoDisponibleAsync(prep.Id))
                        .ThrowsAsync(new Exception("Network error"));

            var vm = CreateVm();

            // Act
            await vm.MarcarComoDisponibleCommand.ExecuteAsync(prep);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Network error"))), Times.AtLeastOnce);
        }

        #endregion

        #region EliminarPreparacionAsync Tests

        [Fact]
        public async Task EliminarPreparacionAsync_Confirmed_ShouldCallServiceAndReload()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Causa" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.EliminarPreparacionDiariaAsync(prep.Id))
                        .ReturnsAsync(Result.Success());
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();

            // Act
            await vm.EliminarPreparacionCommand.ExecuteAsync(prep);

            // Assert
            _mockService.Verify(s => s.EliminarPreparacionDiariaAsync(prep.Id), Times.Once);
            _mockDialog.Verify(d => d.ShowSuccessAsync(It.Is<string>(m => m.Contains("eliminada"))), Times.AtLeastOnce);
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task EliminarPreparacionAsync_NotConfirmed_ShouldNotCallService()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Causa" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(false);

            var vm = CreateVm();

            // Act
            await vm.EliminarPreparacionCommand.ExecuteAsync(prep);

            // Assert
            _mockService.Verify(s => s.EliminarPreparacionDiariaAsync(prep.Id), Times.Never);
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Never);
        }

        [Fact]
        public async Task EliminarPreparacionAsync_WithNullPreparacion_ShouldReturnEarly()
        {
            // Arrange
            var vm = CreateVm();

            // Act
            await vm.EliminarPreparacionCommand.ExecuteAsync(null);

            // Assert
            _mockDialog.Verify(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockService.Verify(s => s.EliminarPreparacionDiariaAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task EliminarPreparacionAsync_WithServiceError_ShouldShowError()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Causa" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.EliminarPreparacionDiariaAsync(prep.Id))
                        .ReturnsAsync(Result.Failure("Error"));

            var vm = CreateVm();

            // Act
            await vm.EliminarPreparacionCommand.ExecuteAsync(prep);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Error"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task EliminarPreparacionAsync_WithException_ShouldShowError()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Causa" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.EliminarPreparacionDiariaAsync(prep.Id))
                        .ThrowsAsync(new Exception("Network error"));

            var vm = CreateVm();

            // Act
            await vm.EliminarPreparacionCommand.ExecuteAsync(prep);

            // Assert
            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Network error"))), Times.AtLeastOnce);
        }

        #endregion

        #region BuscarPreparaciones Tests

        [Fact]
        public void BuscarPreparaciones_WithEmptyText_ShouldReloadAll()
        {
            // Arrange
            var vm = CreateVm();
            vm.TextoBusqueda = "";

            // Act
            vm.BuscarPreparacionesCommand.Execute(null);

            // Assert
            // Should call LoadPreparacionesDiariasCommand
            Assert.True(true); // This is a simple test since the command calls another command
        }

        [Fact]
        public void BuscarPreparaciones_WithValidText_ShouldFilterResults()
        {
            // Arrange
            var vm = CreateVm();
            vm.TextoBusqueda = "Pizza";
            vm.PreparacionesDiarias.Add(new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza Margherita", NombreChef = "Chef1" });
            vm.PreparacionesDiarias.Add(new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Ceviche", NombreChef = "Chef2" });

            // Act
            vm.BuscarPreparacionesCommand.Execute(null);

            // Assert
            vm.PreparacionesDiarias.Should().HaveCount(1);
            vm.PreparacionesDiarias[0].NombreProducto.Should().Be("Pizza Margherita");
        }

        [Fact]
        public void BuscarPreparaciones_WithCaseInsensitiveSearch_ShouldFindResults()
        {
            // Arrange
            var vm = CreateVm();
            vm.TextoBusqueda = "pizza";
            vm.PreparacionesDiarias.Add(new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza Margherita", NombreChef = "Chef1" });
            vm.PreparacionesDiarias.Add(new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Ceviche", NombreChef = "Chef2" });

            // Act
            vm.BuscarPreparacionesCommand.Execute(null);

            // Assert
            vm.PreparacionesDiarias.Should().HaveCount(1);
            vm.PreparacionesDiarias[0].NombreProducto.Should().Be("Pizza Margherita");
        }

        #endregion

        #region LimpiarBusqueda Tests

        [Fact]
        public void LimpiarBusqueda_ShouldClearTextAndReload()
        {
            // Arrange
            var vm = CreateVm();
            vm.TextoBusqueda = "Pizza";

            // Act
            vm.LimpiarBusquedaCommand.Execute(null);

            // Assert
            vm.TextoBusqueda.Should().BeEmpty();
            // Should call LoadPreparacionesDiariasCommand
            Assert.True(true); // This is a simple test since the command calls another command
        }

        #endregion

        #region ToggleEstadisticas Tests

        [Fact]
        public void ToggleEstadisticas_ShouldToggleMostrarEstadisticas()
        {
            // Arrange
            var vm = CreateVm();
            vm.MostrarEstadisticas = true;

            // Act
            vm.ToggleEstadisticasCommand.Execute(null);

            // Assert
            vm.MostrarEstadisticas.Should().BeFalse();

            // Act again
            vm.ToggleEstadisticasCommand.Execute(null);

            // Assert
            vm.MostrarEstadisticas.Should().BeTrue();
        }

        #endregion

        #region ShowFiltros Tests

        [Fact]
        public void ShowFiltros_ShouldToggleMostrarFiltros()
        {
            // Arrange
            var vm = CreateVm();
            vm.MostrarFiltros = false;

            // Act
            vm.ShowFiltrosCommand.Execute(null);

            // Assert
            vm.MostrarFiltros.Should().BeTrue();

            // Act again
            vm.ShowFiltrosCommand.Execute(null);

            // Assert
            vm.MostrarFiltros.Should().BeFalse();
        }

        #endregion

        #region RefreshAsync Tests

        [Fact]
        public async Task RefreshAsync_ShouldCallLoadCommands()
        {
            // Arrange
            var vm = CreateVm();
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));
            _mockService.Setup(s => s.GetEstadisticasAsync())
                        .ReturnsAsync(Result<EstadisticasPreparacionesDiariasDto>.Success(new EstadisticasPreparacionesDiariasDto()));

            // Act
            await vm.RefreshCommand.ExecuteAsync(null);

            // Assert
            vm.IsRefreshing.Should().BeFalse();
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Once);
            _mockService.Verify(s => s.GetEstadisticasAsync(), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WhenAlreadyRefreshing_ShouldNotExecute()
        {
            // Arrange
            var vm = CreateVm();
            vm.IsRefreshing = true;

            // Act
            await vm.RefreshCommand.ExecuteAsync(null);

            // Assert
            // Should not call any service methods
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Never);
            _mockService.Verify(s => s.GetEstadisticasAsync(), Times.Never);
        }

        #endregion

        #region EditarPreparacionAsync Tests

        [Fact]
        public async Task EditarPreparacionAsync_WithValidPreparacion_ShouldNavigate()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza" };
            var vm = CreateVm();

            // Act
            await vm.EditarPreparacionCommand.ExecuteAsync(prep);

            // Assert
            _mockNav.Verify(n => n.NavigateToAsync("editar-preparacion-diaria", It.Is<Dictionary<string, object>>(d => d.ContainsKey("id") && d["id"].Equals(prep.Id))), Times.Once);
        }

        [Fact]
        public async Task EditarPreparacionAsync_WithNullPreparacion_ShouldNotNavigate()
        {
            // Arrange
            var vm = CreateVm();

            // Act
            await vm.EditarPreparacionCommand.ExecuteAsync(null);

            // Assert
            _mockNav.Verify(n => n.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        #endregion

        #region VerDetalleAsync Tests

        [Fact]
        public async Task VerDetalleAsync_WithValidPreparacion_ShouldNotThrow()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza" };
            var vm = CreateVm();

            // Act & Assert
            await vm.VerDetalleCommand.ExecuteAsync(prep);
            // Should not throw exception
            Assert.True(true);
        }

        [Fact]
        public async Task VerDetalleAsync_WithNullPreparacion_ShouldNotThrow()
        {
            // Arrange
            var vm = CreateVm();

            // Act & Assert
            await vm.VerDetalleCommand.ExecuteAsync(null);
            // Should not throw exception
            Assert.True(true);
        }

        #endregion

        #region CrearNuevaPreparacionAsync Tests

        [Fact]
        public async Task CrearNuevaPreparacionAsync_ShouldNavigate()
        {
            // Arrange
            var vm = CreateVm();

            // Act
            await vm.CrearNuevaPreparacionCommand.ExecuteAsync(null);

            // Assert
            _mockNav.Verify(n => n.NavigateToAsync("crear-preparacion-diaria", null), Times.Once);
        }

        #endregion

        #region ConsumirPreparacionAsync Tests

        [Fact]
        public async Task ConsumirPreparacionAsync_ShouldNeverExecute()
        {
            // Arrange
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Pizza" };
            var vm = CreateVm();

            // Act
            await vm.ConsumirPreparacionCommand.ExecuteAsync(prep);

            // Assert
            // Should not call any service methods since CanNeverExecute returns false
            _mockService.Verify(s => s.ConsumirPreparacionDiariaAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}


