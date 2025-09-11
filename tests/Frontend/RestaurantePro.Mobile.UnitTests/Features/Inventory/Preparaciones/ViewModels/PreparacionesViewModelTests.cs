using System.ComponentModel;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Inventory.Preparaciones.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Inventory;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Features.Inventory.Preparaciones.ViewModels;

public class PreparacionesViewModelTests
{
    private readonly Mock<IPreparacionesService> _mockService = new();
    private readonly Mock<IDialogService> _mockDialog = new();

    private PreparacionesViewModel CreateVm() => new PreparacionesViewModel(_mockService.Object, _mockDialog.Object);

    [Fact]
    public async Task CargarPreparacionesAsync_Success_ShouldPopulateAndToggleIsLoading()
    {
        var data = new List<PreparacionDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ceviche", Categoria = "Entradas", Disponible = true },
            new() { Id = Guid.NewGuid(), Nombre = "Lomo Saltado", Categoria = "Platos Principales", Disponible = true }
        };
        _mockService
            .Setup(s => s.ObtenerPreparacionesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDto>>.SuccessResponse(data));

        var vm = CreateVm();
        vm.IsLoading.Should().BeFalse();

        await vm.CargarPreparacionesCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.Preparaciones.Should().HaveCount(2);
        vm.PreparacionesFiltradas.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CargarPreparacionesAsync_Error_ShouldShowDialog()
    {
        _mockService
            .Setup(s => s.ObtenerPreparacionesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDto>>.ErrorResponse("Error de servicio"));

        var vm = CreateVm();
        await vm.CargarPreparacionesCommand.ExecuteAsync(null);

        _mockDialog.Verify(d => d.ShowAlertAsync(
            It.Is<string>(t => t == "Error"),
            It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarPreparacionesAsync_WithTerm_ShouldPopulateFiltered()
    {
        var data = new List<PreparacionDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ceviche", Categoria = "Entradas" },
            new() { Id = Guid.NewGuid(), Nombre = "Arroz con mariscos", Categoria = "Platos Principales" }
        };
        _mockService
            .Setup(s => s.BuscarPreparacionesAsync(It.Is<string>(t => t == "ar"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDto>>.SuccessResponse(data.Where(p => p.Nombre.Contains("ar", StringComparison.OrdinalIgnoreCase)).ToList()));

        var vm = CreateVm();
        vm.TerminoBusqueda = "ar";
        await vm.BuscarPreparacionesCommand.ExecuteAsync(null);

        vm.PreparacionesFiltradas.Should().NotBeEmpty();
        vm.PreparacionesFiltradas.Should().OnlyContain(p => p.Nombre.Contains("ar", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PropertyChanged_TerminoBusqueda_Categoria_ShouldRaise()
    {
        var vm = CreateVm();
        var changes = new List<string>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changes.Add(e.PropertyName!);

        vm.TerminoBusqueda = "cev";
        vm.CategoriaSeleccionada = "Entradas";

        changes.Should().Contain(nameof(PreparacionesViewModel.TerminoBusqueda));
        changes.Should().Contain(nameof(PreparacionesViewModel.CategoriaSeleccionada));
    }

    [Fact]
    public async Task CambiarDisponibilidadAsync_Success_ShouldToggleAndShowSuccess()
    {
        var item = new PreparacionDto { Id = Guid.NewGuid(), Disponible = true };
        _mockService
            .Setup(s => s.CambiarDisponibilidadAsync(item.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDto>.SuccessResponse(item));

        var vm = CreateVm();
        await vm.CambiarDisponibilidadCommand.ExecuteAsync(item);

        _mockService.Verify(s => s.CambiarDisponibilidadAsync(item.Id, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockDialog.Verify(d => d.ShowAlertAsync(It.Is<string>(t => t == "Éxito"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CambiarDisponibilidadAsync_Error_ShouldShowError()
    {
        var item = new PreparacionDto { Id = Guid.NewGuid(), Disponible = true };
        _mockService
            .Setup(s => s.CambiarDisponibilidadAsync(item.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDto>.ErrorResponse("fallo"));

        var vm = CreateVm();
        await vm.CambiarDisponibilidadCommand.ExecuteAsync(item);

        _mockDialog.Verify(d => d.ShowAlertAsync(It.Is<string>(t => t == "Error"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
