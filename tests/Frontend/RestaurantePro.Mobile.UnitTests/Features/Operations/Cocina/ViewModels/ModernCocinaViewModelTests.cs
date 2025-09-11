using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Cocina.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Models.Common;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Cocina.ViewModels;

public class ModernCocinaViewModelTests
{
    private readonly Mock<IComandasService> _mockComandasService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IComandaRealtimeService> _mockRealtimeService;
    private readonly ModernCocinaViewModel _viewModel;

    public ModernCocinaViewModelTests()
    {
        _mockComandasService = new Mock<IComandasService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRealtimeService = new Mock<IComandaRealtimeService>();
        
        _viewModel = new ModernCocinaViewModel(
            _mockComandasService.Object,
            _mockDialogService.Object,
            _mockNotificationService.Object,
            _mockRealtimeService.Object);
    }

    [Fact]
    public void Constructor_WithValidServices_ShouldInitializeProperties()
    {
        // Act & Assert
        _viewModel.Comandas.Should().BeEmpty();
        _viewModel.ComandasFiltradas.Should().BeEmpty();
        _viewModel.Estadisticas.Should().BeNull();
        _viewModel.FiltroEstado.Should().Be("Todas");
        _viewModel.SoloPendientes.Should().BeTrue();
        _viewModel.IsRefreshing.Should().BeFalse();
        _viewModel.Title.Should().Be("Cocina");
        _viewModel.EstadosDisponibles.Should().HaveCount(4);
        _viewModel.EstadosDisponibles.Should().Contain("Todas", "Creada", "En Proceso", "Lista");
    }

    [Fact]
    public void Constructor_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ModernCocinaViewModel(null!, _mockDialogService.Object, _mockNotificationService.Object, _mockRealtimeService.Object));
        Assert.Throws<ArgumentNullException>(() => 
            new ModernCocinaViewModel(_mockComandasService.Object, null!, _mockNotificationService.Object, _mockRealtimeService.Object));
        Assert.Throws<ArgumentNullException>(() => 
            new ModernCocinaViewModel(_mockComandasService.Object, _mockDialogService.Object, null!, _mockRealtimeService.Object));
        Assert.Throws<ArgumentNullException>(() => 
            new ModernCocinaViewModel(_mockComandasService.Object, _mockDialogService.Object, _mockNotificationService.Object, null!));
    }

    [Fact]
    public async Task LoadComandasAsync_WhenServiceReturnsSuccess_ShouldPopulateComandas()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("EnProceso", 2),
            CreateComandaDto("Lista", 3),
            CreateComandaDto("Entregada", 4) // Esta no debería aparecer
        };
        var apiResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(comandas);

        _mockComandasService.Setup(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert
        _mockComandasService.Verify(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Comandas.Should().HaveCount(3); // Solo Creada, EnProceso, Lista
        _viewModel.Comandas.Should().OnlyContain(c => c.Estado == "Creada" || c.Estado == "EnProceso" || c.Estado == "Lista");
    }

    [Fact]
    public async Task LoadComandasAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var apiResponse = ApiResponse<List<ComandaDto>>.Failure("Error al cargar comandas");

        _mockComandasService.Setup(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert
        _mockComandasService.Verify(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Error al cargar comandas", "OK"), Times.Once);
        _viewModel.Comandas.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadComandasAsync_WhenExceptionThrown_ShouldShowError()
    {
        // Arrange
        _mockComandasService.Setup(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        await _viewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Error inesperado: Error de red", "OK"), Times.Once);
        _viewModel.Comandas.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadComandasAsync_WithNewComandas_ShouldShowNotification()
    {
        // Arrange
        var existingComanda = CreateComandaDto("Creada", 1);
        _viewModel.Comandas.Add(existingComanda);

        var comandas = new List<ComandaDto>
        {
            existingComanda,
            CreateComandaDto("Creada", 2), // Nueva comanda
            CreateComandaDto("EnProceso", 3) // Nueva comanda
        };
        var apiResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(comandas);

        _mockComandasService.Setup(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert
        _mockNotificationService.Verify(x => x.VibrateAsync(120), Times.Once);
        _mockNotificationService.Verify(x => x.ShowToastAsync("2 nuevas comandas", 2000), Times.Once);
    }

    [Fact]
    public async Task LoadEstadisticasAsync_ShouldCalculateStatistics()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("Creada", 2),
            CreateComandaDto("EnProceso", 3),
            CreateComandaDto("Lista", 4)
        };
        
        foreach (var comanda in comandas)
        {
            _viewModel.Comandas.Add(comanda);
        }

        // Act
        await _viewModel.LoadEstadisticasCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Estadisticas.Should().NotBeNull();
        _viewModel.Estadisticas!.ComandasCreadas.Should().Be(2);
        _viewModel.Estadisticas.ComandasEnProceso.Should().Be(1);
        _viewModel.Estadisticas.ComandasListas.Should().Be(1);
        _viewModel.Estadisticas.TiempoPromedio.Should().Be(15);
    }

    [Fact]
    public async Task TomarComandaAsync_WithConfirmation_ShouldChangeStateToEnProceso()
    {
        // Arrange
        var comanda = CreateComandaDto("Creada", 1);
        var updatedComanda = CreateComandaDto("EnProceso", 1);
        var apiResponse = ApiResponse<ComandaDto>.SuccessResponse(updatedComanda);

        _mockDialogService.Setup(x => x.ShowConfirmAsync("Tomar Comanda", It.IsAny<string>(), "Sí", "No"))
            .ReturnsAsync(true);
        _mockComandasService.Setup(x => x.CambiarEstadoComandaAsync(comanda.Id, "enproceso", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.TomarComandaCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowConfirmAsync("Tomar Comanda", $"¿Está seguro que desea tomar la comanda #{comanda.NumeroDisplay} para preparar?", "Sí", "No"), Times.Once);
        _mockComandasService.Verify(x => x.CambiarEstadoComandaAsync(comanda.Id, "enproceso", "Tomada por cocina para preparar", It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowAlertAsync("Éxito", $"Comanda #{comanda.NumeroDisplay} tomada para preparar", "OK"), Times.Once);
    }

    [Fact]
    public async Task TomarComandaAsync_WithoutConfirmation_ShouldNotChangeState()
    {
        // Arrange
        var comanda = CreateComandaDto("Creada", 1);

        _mockDialogService.Setup(x => x.ShowConfirmAsync("Tomar Comanda", It.IsAny<string>(), "Sí", "No"))
            .ReturnsAsync(false);

        // Act
        await _viewModel.TomarComandaCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowConfirmAsync("Tomar Comanda", It.IsAny<string>(), "Sí", "No"), Times.Once);
        _mockComandasService.Verify(x => x.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TomarComandaAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var comanda = CreateComandaDto("Creada", 1);
        var apiResponse = ApiResponse<ComandaDto>.Failure("Error al cambiar estado");

        _mockDialogService.Setup(x => x.ShowConfirmAsync("Tomar Comanda", It.IsAny<string>(), "Sí", "No"))
            .ReturnsAsync(true);
        _mockComandasService.Setup(x => x.CambiarEstadoComandaAsync(comanda.Id, "enproceso", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.TomarComandaCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Error al cambiar estado", "OK"), Times.Once);
    }

    [Fact]
    public async Task MarcarListaAsync_WithConfirmation_ShouldChangeStateToLista()
    {
        // Arrange
        var comanda = CreateComandaDto("EnProceso", 1);
        var updatedComanda = CreateComandaDto("EnProceso", 1);
        var apiResponse = ApiResponse<ComandaDto>.SuccessResponse(updatedComanda);

        _mockDialogService.Setup(x => x.ShowConfirmAsync("Marcar como Lista", It.IsAny<string>(), "Sí", "No"))
            .ReturnsAsync(true);
        _mockComandasService.Setup(x => x.CambiarEstadoComandaAsync(comanda.Id, "lista", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.MarcarListaCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowConfirmAsync("Marcar como Lista", $"¿Está seguro que la comanda #{comanda.NumeroDisplay} está lista para entregar?", "Sí", "No"), Times.Once);
        _mockComandasService.Verify(x => x.CambiarEstadoComandaAsync(comanda.Id, "lista", "Comanda lista para entregar", It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowAlertAsync("Éxito", $"Comanda #{comanda.NumeroDisplay} marcada como lista", "OK"), Times.Once);
    }

    [Fact]
    public async Task MarcarListaAsync_WithoutConfirmation_ShouldNotChangeState()
    {
        // Arrange
        var comanda = CreateComandaDto("EnProceso", 1);

        _mockDialogService.Setup(x => x.ShowConfirmAsync("Marcar como Lista", It.IsAny<string>(), "Sí", "No"))
            .ReturnsAsync(false);

        // Act
        await _viewModel.MarcarListaCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowConfirmAsync("Marcar como Lista", It.IsAny<string>(), "Sí", "No"), Times.Once);
        _mockComandasService.Verify(x => x.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerDetalleAsync_ShouldShowComandaDetails()
    {
        // Arrange
        var comanda = CreateComandaDto("Creada", 1);
        comanda.MesaNumero = "Mesa 5";
        comanda.Total = 25.50m;
        comanda.Productos = new List<ComandaProductoDto>
        {
            new ComandaProductoDto { Nombre = "Pizza", Cantidad = 1 },
            new ComandaProductoDto { Nombre = "Coca Cola", Cantidad = 1 }
        };
        comanda.Observaciones = "Sin cebolla";

        // Act
        await _viewModel.VerDetalleCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Detalle de Comanda", 
            $"Comanda #{comanda.NumeroDisplay}\n" +
            $"Mesa: {comanda.MesaNumeroDisplay}\n" +
            $"Estado: {comanda.Estado}\n" +
            $"Total: {comanda.Total:C}\n" +
            $"Productos: {comanda.ProductosResumen}\n\n" +
            $"Observaciones:\n{comanda.Observaciones}", "OK"), Times.Once);
    }

    [Fact]
    public async Task VerDetalleAsync_WithoutObservations_ShouldShowDetailsWithoutObservations()
    {
        // Arrange
        var comanda = CreateComandaDto("Creada", 1);
        comanda.MesaNumero = "Mesa 5";
        comanda.Total = 25.50m;
        comanda.Productos = new List<ComandaProductoDto>
        {
            new ComandaProductoDto { Nombre = "Pizza", Cantidad = 1 },
            new ComandaProductoDto { Nombre = "Coca Cola", Cantidad = 1 }
        };
        comanda.Observaciones = string.Empty;

        // Act
        await _viewModel.VerDetalleCommand.ExecuteAsync(comanda);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Detalle de Comanda", 
            $"Comanda #{comanda.NumeroDisplay}\n" +
            $"Mesa: {comanda.MesaNumeroDisplay}\n" +
            $"Estado: {comanda.Estado}\n" +
            $"Total: {comanda.Total:C}\n" +
            $"Productos: {comanda.ProductosResumen}", "OK"), Times.Once);
    }

    [Fact]
    public async Task RefreshComandasAsync_ShouldReloadComandasAndStatistics()
    {
        // Arrange
        var comandas = new List<ComandaDto> { CreateComandaDto("Creada", 1) };
        var apiResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(comandas);

        _mockComandasService.Setup(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.RefreshComandasCommand.ExecuteAsync(null);

        // Assert
        _mockComandasService.Verify(x => x.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Comandas.Should().HaveCount(1);
        _viewModel.Estadisticas.Should().NotBeNull();
    }

    [Fact]
    public void AplicarFiltros_WithSoloPendientesTrue_ShouldFilterOnlyCreatedAndInProcess()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("EnProceso", 2),
            CreateComandaDto("Lista", 3)
        };
        
        foreach (var comanda in comandas)
        {
            _viewModel.Comandas.Add(comanda);
        }
        _viewModel.SoloPendientes = true;

        // Act - Simular cambio de filtro para activar AplicarFiltros
        _viewModel.FiltroEstado = "Todas";

        // Assert
        _viewModel.ComandasFiltradas.Should().HaveCount(2);
        _viewModel.ComandasFiltradas.Should().OnlyContain(c => c.Estado == "Creada" || c.Estado == "EnProceso");
    }

    [Fact]
    public void AplicarFiltros_WithSoloPendientesFalse_ShouldShowAllComandas()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("EnProceso", 2),
            CreateComandaDto("Lista", 3)
        };
        
        foreach (var comanda in comandas)
        {
            _viewModel.Comandas.Add(comanda);
        }
        _viewModel.SoloPendientes = false;

        // Act - Simular cambio de filtro para activar AplicarFiltros
        _viewModel.FiltroEstado = "Todas";

        // Assert
        _viewModel.ComandasFiltradas.Should().HaveCount(3);
    }

    [Fact]
    public void AplicarFiltros_WithSpecificEstado_ShouldFilterByEstado()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("Creada", 2),
            CreateComandaDto("EnProceso", 3),
            CreateComandaDto("Lista", 4)
        };
        
        foreach (var comanda in comandas)
        {
            _viewModel.Comandas.Add(comanda);
        }
        _viewModel.FiltroEstado = "Creada";

        // Act - Simular cambio de filtro para activar AplicarFiltros
        _viewModel.FiltroEstado = "Todas";

        // Assert
        _viewModel.ComandasFiltradas.Should().HaveCount(2);
        _viewModel.ComandasFiltradas.Should().OnlyContain(c => c.Estado == "Creada");
    }

    [Fact]
    public void OnFiltroEstadoChanged_ShouldApplyFilters()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("EnProceso", 2),
            CreateComandaDto("Lista", 3)
        };
        
        foreach (var comanda in comandas)
        {
            _viewModel.Comandas.Add(comanda);
        }

        // Act
        _viewModel.FiltroEstado = "EnProceso";

        // Assert
        _viewModel.ComandasFiltradas.Should().HaveCount(1);
        _viewModel.ComandasFiltradas.Should().OnlyContain(c => c.Estado == "EnProceso");
    }

    [Fact]
    public void OnSoloPendientesChanged_ShouldApplyFilters()
    {
        // Arrange
        var comandas = new List<ComandaDto>
        {
            CreateComandaDto("Creada", 1),
            CreateComandaDto("EnProceso", 2),
            CreateComandaDto("Lista", 3)
        };
        
        foreach (var comanda in comandas)
        {
            _viewModel.Comandas.Add(comanda);
        }

        // Act
        _viewModel.SoloPendientes = false;

        // Assert
        _viewModel.ComandasFiltradas.Should().HaveCount(3);
    }

    [Fact]
    public async Task TomarComandaAsync_WithNullComanda_ShouldNotExecute()
    {
        // Act
        await _viewModel.TomarComandaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), "Sí", "No"), Times.Never);
        _mockComandasService.Verify(x => x.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarcarListaAsync_WithNullComanda_ShouldNotExecute()
    {
        // Act
        await _viewModel.MarcarListaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), "Sí", "No"), Times.Never);
        _mockComandasService.Verify(x => x.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerDetalleAsync_WithNullComanda_ShouldNotExecute()
    {
        // Act
        await _viewModel.VerDetalleCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    private static ComandaDto CreateComandaDto(string estado, int numero)
    {
        return new ComandaDto
        {
            Id = Guid.NewGuid(),
            Estado = estado,
            Numero = numero.ToString(),
            MesaNumero = $"Mesa {numero}",
            Total = 15.99m,
            Productos = new List<ComandaProductoDto>
            {
                new ComandaProductoDto { Nombre = "Producto de prueba", Cantidad = 1 }
            },
            Observaciones = string.Empty,
            FechaCreacion = DateTime.Now.AddMinutes(-numero)
        };
    }
}
