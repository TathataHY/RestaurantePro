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

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_Success_ShouldPopulateAndToggleBusy()
        {
            var data = new List<PreparacionDiariaDto>
            {
                new() { Id = Guid.NewGuid(), NombreProducto = "Ceviche", NombreChef = "Ana" }
            };
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(data));

            var vm = CreateVm();
            vm.IsBusy.Should().BeFalse();

            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            vm.IsBusy.Should().BeFalse();
            vm.PreparacionesDiarias.Should().HaveCount(1);
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_Error_ShouldShowError()
        {
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Failure("Error"));

            var vm = CreateVm();
            await vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            _mockDialog.Verify(d => d.ShowErrorAsync(It.Is<string>(m => m.Contains("Error"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LoadPreparacionesDiariasAsync_DoubleExecution_ShouldNotReenter()
        {
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .Returns(async () =>
                        {
                            await Task.Delay(200);
                            return Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>());
                        });

            var vm = CreateVm();
            var t1 = vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);
            var t2 = vm.LoadPreparacionesDiariasCommand.ExecuteAsync(null);

            await t1;
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Once);
        }

        [Fact]
        public async Task FiltrarPorEstadoAsync_Todos_ShouldReloadAllAndSetFiltro()
        {
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();
            await vm.FiltrarPorEstadoCommand.ExecuteAsync("Todos");

            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.Once);
            vm.FiltroEstado.Should().Be("Todos");
        }

        [Fact]
        public async Task FiltrarPorEstadoAsync_Especifico_ShouldCallByEstadoAndSetFiltro()
        {
            _mockService.Setup(s => s.GetPreparacionesDiariasPorEstadoAsync("Pendiente"))
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();
            await vm.FiltrarPorEstadoCommand.ExecuteAsync("Pendiente");

            _mockService.Verify(s => s.GetPreparacionesDiariasPorEstadoAsync("Pendiente"), Times.Once);
            vm.FiltroEstado.Should().Be("Pendiente");
        }

        [Fact]
        public async Task MarcarComoDisponibleAsync_Confirmed_ShouldCallServiceAndReload()
        {
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Arroz" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.MarcarComoDisponibleAsync(prep.Id))
                        .ReturnsAsync(Result<PreparacionDiariaDto>.Success(prep));
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();
            await vm.MarcarComoDisponibleCommand.ExecuteAsync(prep);

            _mockService.Verify(s => s.MarcarComoDisponibleAsync(prep.Id), Times.Once);
            _mockDialog.Verify(d => d.ShowSuccessAsync(It.Is<string>(m => m.Contains("disponible"))), Times.AtLeastOnce);
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task EliminarPreparacionAsync_Confirmed_ShouldCallServiceAndReload()
        {
            var prep = new PreparacionDiariaDto { Id = Guid.NewGuid(), NombreProducto = "Causa" };
            _mockDialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
            _mockService.Setup(s => s.EliminarPreparacionDiariaAsync(prep.Id))
                        .ReturnsAsync(Result.Success());
            _mockService.Setup(s => s.GetPreparacionesDiariasAsync())
                        .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

            var vm = CreateVm();
            await vm.EliminarPreparacionCommand.ExecuteAsync(prep);

            _mockService.Verify(s => s.EliminarPreparacionDiariaAsync(prep.Id), Times.Once);
            _mockDialog.Verify(d => d.ShowSuccessAsync(It.Is<string>(m => m.Contains("eliminada"))), Times.AtLeastOnce);
            _mockService.Verify(s => s.GetPreparacionesDiariasAsync(), Times.AtLeastOnce);
        }
    }
}


