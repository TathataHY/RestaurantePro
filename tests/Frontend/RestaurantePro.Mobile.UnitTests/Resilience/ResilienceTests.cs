using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Preferences;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Net;

namespace RestaurantePro.Mobile.UnitTests.Resilience;

/// <summary>
/// Pruebas de resilencia para manejo de fallos y recuperación
/// </summary>
public class ResilienceTests
{
    private readonly Mock<IComandasService> _comandasServiceMock;
    private readonly Mock<IMesasService> _mesasServiceMock;
    private readonly Mock<IProductosService> _productosServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IComandaRealtimeService> _realtimeServiceMock;
    private readonly Mock<IPreferencesService> _preferencesServiceMock;

    public ResilienceTests()
    {
        _comandasServiceMock = new Mock<IComandasService>();
        _mesasServiceMock = new Mock<IMesasService>();
        _productosServiceMock = new Mock<IProductosService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _realtimeServiceMock = new Mock<IComandaRealtimeService>();
        _preferencesServiceMock = new Mock<IPreferencesService>();
    }

    #region Pruebas de Fallos de Red

    [Fact]
    public async Task Resilience_FalloRed_ComandasService_RecuperacionAutomatica()
    {
        // Arrange - Configurar fallo de red seguido de éxito
        _comandasServiceMock.SetupSequence(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas (falla y luego recupera)
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null); // Primer intento - falla
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null); // Segundo intento - éxito

        // Assert - Verificar que se manejó el error y se recuperó
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        comandasViewModel.Comandas.Should().NotBeNull();
    }

    [Fact]
    public async Task Resilience_FalloRed_MesasService_RecuperacionAutomatica()
    {
        // Arrange - Configurar fallo de red seguido de éxito
        _mesasServiceMock.SetupSequence(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(new List<MesaDto>()));

        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar mesas (falla y luego recupera)
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null); // Primer intento - falla
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null); // Segundo intento - éxito

        // Assert - Verificar que se manejó el error y se recuperó
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mesasViewModel.Mesas.Should().NotBeNull();
    }

    [Fact]
    public async Task Resilience_FalloRed_ProductosService_RecuperacionAutomatica()
    {
        // Arrange - Configurar fallo de red seguido de éxito
        var paginatedList = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 10, TotalCount = 0 };
        _productosServiceMock.SetupSequence(p => p.ObtenerProductosPaginadosAsync(1, 10, null, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar productos (falla y luego recupera)
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null); // Primer intento - falla
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null); // Segundo intento - éxito

        // Assert - Verificar que se manejó el error y se recuperó
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        productosViewModel.Productos.Should().NotBeNull();
    }

    #endregion

    #region Pruebas de Fallos de Servicio

    [Fact]
    public async Task Resilience_FalloServicio_ComandasService_Error500()
    {
        // Arrange - Configurar error 500 del servidor
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.ErrorResponse(new List<string> { "Error interno del servidor" }, "Error interno del servidor"));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el error
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        comandasViewModel.Comandas.Should().BeEmpty();
    }

    [Fact]
    public async Task Resilience_FalloServicio_MesasService_Error503()
    {
        // Arrange - Configurar error 503 del servidor
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.ErrorResponse(new List<string> { "Servicio no disponible" }, "Servicio no disponible"));

        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar mesas
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el error
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mesasViewModel.Mesas.Should().BeEmpty();
    }

    #endregion

    #region Pruebas de Fallos Parciales

    [Fact]
    public async Task Resilience_FalloParcial_ComandasService_DatosIncompletos()
    {
        // Arrange - Configurar respuesta con datos incompletos
        var comandasIncompletas = new List<ComandaDto>
        {
            new ComandaDto
            {
                Id = Guid.NewGuid(),
                Numero = "C-001",
                // Faltan campos requeridos
                Estado = "Activa"
            }
        };

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandasIncompletas));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Cargar comandas con datos incompletos
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó la situación
        comandasViewModel.Comandas.Should().HaveCount(1);
        comandasViewModel.Comandas.First().Numero.Should().Be("C-001");
    }

    [Fact]
    public async Task Resilience_FalloParcial_MesasService_DatosInconsistentes()
    {
        // Arrange - Configurar respuesta con datos inconsistentes
        var mesasInconsistentes = new List<MesaDto>
        {
            new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "Mesa 1",
                Capacidad = -1, // Capacidad inválida
                Estado = "EstadoInvalido",
                Ubicacion = null // Ubicación nula
            }
        };

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesasInconsistentes));

        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Cargar mesas con datos inconsistentes
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó la situación
        mesasViewModel.Mesas.Should().HaveCount(1);
        mesasViewModel.Mesas.First().Numero.Should().Be("Mesa 1");
    }

    #endregion

    #region Pruebas de Recuperación Automática

    [Fact]
    public async Task Resilience_RecuperacionAutomatica_ComandasService_ReintentoExitoso()
    {
        // Arrange - Configurar fallo seguido de éxito
        _comandasServiceMock.SetupSequence(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas múltiples veces
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null); // Fallo
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null); // Fallo
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null); // Éxito

        // Assert - Verificar que se recuperó después de múltiples fallos
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Resilience_RecuperacionAutomatica_MesasService_ReintentoExitoso()
    {
        // Arrange - Configurar fallo seguido de éxito
        _mesasServiceMock.SetupSequence(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ThrowsAsync(new HttpRequestException("Error de red"))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(new List<MesaDto>()));

        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar mesas múltiples veces
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null); // Fallo
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null); // Fallo
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null); // Éxito

        // Assert - Verificar que se recuperó después de múltiples fallos
        _mesasServiceMock.Verify(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Exactly(3));
    }

    #endregion

    #region Pruebas de Datos Corruptos

    [Fact]
    public async Task Resilience_DatosCorruptos_ComandasService_RespuestaInvalida()
    {
        // Arrange - Configurar respuesta con datos corruptos
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Datos corruptos"));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas con datos corruptos
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el error
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        comandasViewModel.Comandas.Should().BeEmpty();
    }

    [Fact]
    public async Task Resilience_DatosCorruptos_MesasService_RespuestaInvalida()
    {
        // Arrange - Configurar respuesta con datos corruptos
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ThrowsAsync(new InvalidOperationException("Datos corruptos"));

        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar mesas con datos corruptos
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el error
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mesasViewModel.Mesas.Should().BeEmpty();
    }

    #endregion

    #region Pruebas de Concurrencia con Fallos

    [Fact]
    public async Task Resilience_ConcurrenciaConFallos_MultiplesOperacionesSimultaneas()
    {
        // Arrange - Configurar fallos intermitentes
        var callCount = 0;
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount % 2 == 0)
                {
                    return ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>());
                }
                else
                {
                    throw new HttpRequestException("Error de red");
                }
            });

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Ejecutar múltiples operaciones simultáneamente
        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(comandasViewModel.LoadComandasCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);

        // Assert - Verificar que se manejaron los fallos
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()), Times.Exactly(5));
    }

    #endregion

    #region Pruebas de Timeout

    [Fact]
    public async Task Resilience_Timeout_ComandasService_OperacionLenta()
    {
        // Arrange - Configurar timeout
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Operación cancelada por timeout"));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas con timeout
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el timeout
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        comandasViewModel.Comandas.Should().BeEmpty();
    }

    [Fact]
    public async Task Resilience_Timeout_MesasService_OperacionLenta()
    {
        // Arrange - Configurar timeout
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ThrowsAsync(new TaskCanceledException("Operación cancelada por timeout"));

        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar mesas con timeout
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el timeout
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mesasViewModel.Mesas.Should().BeEmpty();
    }

    #endregion
}
