using System.ComponentModel;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Inventory.Reservaciones.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Inventory;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Features.Inventory.Reservaciones.ViewModels;

public class ReservacionesViewModelTests
{
    private readonly Mock<IReservacionesService> _mockService = new();
    private readonly Mock<IDialogService> _mockDialog = new();

    private ReservacionesViewModel CreateVm() => new ReservacionesViewModel(_mockService.Object, _mockDialog.Object);

    [Fact]
    public async Task CargarReservacionesAsync_Success_ShouldPopulateAndToggleIsLoading()
    {
        var data = new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = "Confirmada", NombreCliente = "Ana" },
            new() { Id = Guid.NewGuid(), Estado = "Pendiente", NombreCliente = "Luis" }
        };
        _mockService
            .Setup(s => s.ObtenerReservacionesAsync())
            .ReturnsAsync(ApiResponse<List<ReservacionDto>>.SuccessResponse(data));

        var vm = CreateVm();
        vm.IsLoading.Should().BeFalse();

        await vm.CargarReservacionesCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.Reservaciones.Should().HaveCount(2);
        vm.ReservacionesFiltradas.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CargarReservacionesAsync_SinDatos_DebeQuedarVacio()
    {
        _mockService
            .Setup(s => s.ObtenerReservacionesAsync())
            .ReturnsAsync(ApiResponse<List<ReservacionDto>>.SuccessResponse(new List<ReservacionDto>()));

        var vm = CreateVm();
        await vm.CargarReservacionesCommand.ExecuteAsync(null);

        vm.Reservaciones.Should().BeEmpty();
        vm.ReservacionesFiltradas.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task BuscarReservacionesAsync_SinResultados_DebeQuedarVacio()
    {
        _mockService
            .Setup(s => s.BuscarReservacionesAsync("nope"))
            .ReturnsAsync(ApiResponse<List<ReservacionDto>>.SuccessResponse(new List<ReservacionDto>()));

        var vm = CreateVm();
        vm.TerminoBusqueda = "nope";

        await vm.BuscarReservacionesCommand.ExecuteAsync(null);

        vm.ReservacionesFiltradas.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task CargarReservacionesAsync_DobleEjecucion_NoDebeReentrar()
    {
        var data = new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = "Confirmada", NombreCliente = "Ana" }
        };

        _mockService
            .Setup(s => s.ObtenerReservacionesAsync())
            .Returns(async () =>
            {
                await Task.Delay(200);
                return ApiResponse<List<ReservacionDto>>.SuccessResponse(data);
            });

        var vm = CreateVm();

        var t1 = vm.CargarReservacionesCommand.ExecuteAsync(null);
        var t2 = vm.CargarReservacionesCommand.ExecuteAsync(null);

        await t1; // La segunda ejecución debe ser ignorada mientras la primera corre

        _mockService.Verify(s => s.ObtenerReservacionesAsync(), Times.Once);
    }

    [Fact]
    public async Task BuscarReservacionesAsync_DobleEjecucion_NoDebeReentrar()
    {
        var data = new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = "Pendiente", NombreCliente = "Luis" }
        };

        _mockService
            .Setup(s => s.BuscarReservacionesAsync(It.IsAny<string>()))
            .Returns(async (string term) =>
            {
                await Task.Delay(200);
                return ApiResponse<List<ReservacionDto>>.SuccessResponse(data);
            });

        var vm = CreateVm();
        vm.TerminoBusqueda = "lu";

        var t1 = vm.BuscarReservacionesCommand.ExecuteAsync(null);
        var t2 = vm.BuscarReservacionesCommand.ExecuteAsync(null);

        await t1;

        _mockService.Verify(s => s.BuscarReservacionesAsync("lu"), Times.Once);
    }

    [Fact]
    public async Task CargarReservacionesAsync_Error_ShouldShowDialogError()
    {
        _mockService
            .Setup(s => s.ObtenerReservacionesAsync())
            .ReturnsAsync(ApiResponse<List<ReservacionDto>>.ErrorResponse("Error de servicio"));

        var vm = CreateVm();

        await vm.CargarReservacionesCommand.ExecuteAsync(null);

        _mockDialog.Verify(d => d.ShowAlertAsync(
            It.Is<string>(t => t == "Error"),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CargarReservacionesHoyAsync_Success_ShouldPopulate()
    {
        var data = new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = "Confirmada", NombreCliente = "Ana" }
        };
        _mockService
            .Setup(s => s.ObtenerReservacionesHoyAsync())
            .ReturnsAsync(ApiResponse<List<ReservacionDto>>.SuccessResponse(data));

        var vm = CreateVm();
        await vm.CargarReservacionesHoyCommand.ExecuteAsync(null);

        vm.Reservaciones.Should().HaveCount(1);
        vm.ReservacionesFiltradas.Should().HaveCount(1);
    }

    [Fact]
    public void PropertyChanged_TerminoBusqueda_ShouldRaiseEvent()
    {
        var vm = CreateVm();
        string? lastProp = null;
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => lastProp = e.PropertyName;

        vm.TerminoBusqueda = "mesa";

        lastProp.Should().Be(nameof(ReservacionesViewModel.TerminoBusqueda));
    }

    [Fact]
    public async Task Filtros_EstadoYBusqueda_ShouldFilterResultados()
    {
        var data = new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = "Confirmada", NombreCliente = "Ana" },
            new() { Id = Guid.NewGuid(), Estado = "Pendiente", NombreCliente = "Luis" },
            new() { Id = Guid.NewGuid(), Estado = "Confirmada", NombreCliente = "Andrés" }
        };
        _mockService
            .Setup(s => s.ObtenerReservacionesAsync())
            .ReturnsAsync(ApiResponse<List<ReservacionDto>>.SuccessResponse(data));

        var vm = CreateVm();
        vm.EstadoFiltro = "Confirmada";
        vm.TerminoBusqueda = "An"; // Coincide con Ana y Andrés

        await vm.CargarReservacionesCommand.ExecuteAsync(null);

        vm.ReservacionesFiltradas.Should().HaveCount(2);
        vm.ReservacionesFiltradas.Should().OnlyContain(r => r.Estado == "Confirmada" && r.NombreCliente!.StartsWith("An"));
    }

    [Fact]
    public async Task VerReservacionAsync_DebeMostrarDetallesEnDialogo()
    {
        var vm = CreateVm();
        var r = new ReservacionDto
        {
            Id = Guid.NewGuid(),
            NombreCliente = "Juan Pérez",
            FechaReservacion = DateTime.Today,
            HoraReservacion = new TimeSpan(19, 30, 0),
            NumeroPersonas = 4
        };

        await vm.VerReservacionCommand.ExecuteAsync(r);

        _mockDialog.Verify(d => d.ShowAlertAsync(
            It.Is<string>(t => t.Contains("Detalles")),
            It.Is<string>(m => m.Contains("Juan Pérez") && m.Contains("19:30")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EditarReservacionAsync_DebeInformarNoDisponible()
    {
        var vm = CreateVm();
        var r = new ReservacionDto { Id = Guid.NewGuid(), NombreCliente = "Ana" };

        await vm.EditarReservacionCommand.ExecuteAsync(r);

        _mockDialog.Verify(d => d.ShowAlertAsync(
            It.Is<string>(t => t.Contains("Función no disponible")),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }
}
