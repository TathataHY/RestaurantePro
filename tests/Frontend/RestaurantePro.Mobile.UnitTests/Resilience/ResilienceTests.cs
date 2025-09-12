using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
// using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels; // Comentado temporalmente
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Preferences;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using Xunit;
using System.Net;

namespace RestaurantePro.Mobile.UnitTests.Resilience;

/// <summary>
/// Pruebas de resilencia para fallos de servicios y recuperación automática
/// FASE 3: Calidad y Rendimiento
/// </summary>
public class ResilienceTests
{
    private readonly Mock<IComandasService> _comandasServiceMock;
    private readonly Mock<IMesasService> _mesasServiceMock;
    private readonly Mock<IProductosService> _productosServiceMock;
    private readonly Mock<IDailyPreparationsService> _dailyPreparationsServiceMock;
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
        _dailyPreparationsServiceMock = new Mock<IDailyPreparationsService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _realtimeServiceMock = new Mock<IComandaRealtimeService>();
        _preferencesServiceMock = new Mock<IPreferencesService>();
    }

    #region Pruebas de Fallos de Red

    [Fact]
    public async Task Resilience_FalloRed_RecuperacionAutomatica()
    {
        // Arrange - Configurar fallo de red seguido de recuperación
        var callCount = 0;
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Network error");
                }
                return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
            });

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Primera llamada (falla)
        await comandasViewModel.LoadComandasAsync();
        comandasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();

        // Segunda llamada (recuperación)
        await comandasViewModel.LoadComandasAsync();

        // Assert - Verificar recuperación
        comandasViewModel.ErrorMessage.Should().BeNullOrEmpty();
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Resilience_Timeout_ReintentoAutomatico()
    {
        // Arrange - Configurar timeout seguido de éxito
        var callCount = 0;
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 2)
                {
                    throw new TaskCanceledException("Request timeout");
                }
                return Task.FromResult(ApiResponse<List<MesaDto>>.SuccessResponse(new List<MesaDto>()));
            });

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        // Act - Intentar cargar mesas (debe fallar las primeras veces)
        await mesasViewModel.LoadMesasAsync();
        mesasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();

        // Reintentar (debe funcionar)
        await mesasViewModel.LoadMesasAsync();

        // Assert - Verificar que eventualmente funciona
        mesasViewModel.ErrorMessage.Should().BeNullOrEmpty();
        _mesasServiceMock.Verify(m => m.ObtenerMesasAsync(), Times.AtLeast(2));
    }

    [Fact]
    public async Task Resilience_SocketException_RecuperacionGradual()
    {
        // Arrange - Configurar fallo de socket seguido de recuperación
        var callCount = 0;
        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new SocketException(10054); // Connection reset
                }
                return Task.FromResult(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(new PaginatedList<ProductoDto>
                {
                    Items = new List<ProductoDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 20
                }));
            });

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Primera llamada (falla)
        await productosViewModel.LoadProductosAsync();
        productosViewModel.ErrorMessage.Should().NotBeNullOrEmpty();

        // Segunda llamada (recuperación)
        await productosViewModel.LoadProductosAsync();

        // Assert - Verificar recuperación
        productosViewModel.ErrorMessage.Should().BeNullOrEmpty();
        _productosServiceMock.Verify(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
    }

    #endregion

    #region Pruebas de Fallos de Servicios

    [Fact]
    public async Task Resilience_FalloServicioComandas_DegradacionElegante()
    {
        // Arrange - Configurar fallo del servicio de comandas
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        _comandasServiceMock.Setup(c => c.ObtenerEstadisticasAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas
        await comandasViewModel.LoadComandasAsync();
        await comandasViewModel.LoadEstadisticasAsync();

        // Assert - Verificar degradación elegante
        comandasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        comandasViewModel.Comandas.Should().BeEmpty();
        comandasViewModel.Estadisticas.Should().BeNull();
        comandasViewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Resilience_FalloServicioMesas_DegradacionElegante()
    {
        // Arrange - Configurar fallo del servicio de mesas
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        _mesasServiceMock.Setup(m => m.ObtenerEstadoOcupacionAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        // Act - Intentar cargar mesas
        await mesasViewModel.LoadMesasAsync();

        // Assert - Verificar degradación elegante
        mesasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        mesasViewModel.Mesas.Should().BeEmpty();
        mesasViewModel.EstadoMesas.Should().BeNull();
        mesasViewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Resilience_FalloServicioProductos_DegradacionElegante()
    {
        // Arrange - Configurar fallo del servicio de productos
        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        _productosServiceMock.Setup(p => p.ObtenerCategoriasAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar cargar productos
        await productosViewModel.LoadProductosAsync();

        // Assert - Verificar degradación elegante
        productosViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        productosViewModel.Productos.Should().BeEmpty();
        productosViewModel.IsBusy.Should().BeFalse();
    }

    #endregion

    #region Pruebas de Fallos Parciales

    [Fact]
    public async Task Resilience_FalloParcial_AlgunosServiciosFuncionan()
    {
        // Arrange - Configurar fallo parcial (comandas falla, mesas funciona)
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ThrowsAsync(new Exception("Comandas service unavailable"));

        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "Mesa 1", Estado = "Disponible" }
        };
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        // Act - Cargar ambos servicios
        await comandasViewModel.LoadComandasAsync();
        await mesasViewModel.LoadMesasAsync();

        // Assert - Verificar fallo parcial
        comandasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        comandasViewModel.Comandas.Should().BeEmpty();

        mesasViewModel.ErrorMessage.Should().BeNullOrEmpty();
        mesasViewModel.Mesas.Should().HaveCount(1);
    }

    [Fact]
    public async Task Resilience_FalloParcial_OperacionesCRUD()
    {
        // Arrange - Configurar fallo parcial en operaciones CRUD
        var comandaId = Guid.NewGuid();
        var comanda = new ComandaDto { Id = comandaId, Estado = "Pendiente" };

        // Crear funciona, actualizar falla
        _comandasServiceMock.Setup(c => c.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda));

        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Update service unavailable"));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar operaciones
        // Crear comanda (debe funcionar)
        await comandasViewModel.CrearComandaCommand.ExecuteAsync(null);

        // Cambiar estado (debe fallar)
        await comandasViewModel.CambiarEstadoCommand.ExecuteAsync("En Preparación");

        // Assert - Verificar fallo parcial
        _comandasServiceMock.Verify(c => c.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()), Times.Once);
        _comandasServiceMock.Verify(c => c.CambiarEstadoComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Pruebas de Recuperación Automática

    [Fact]
    public async Task Resilience_RecuperacionAutomatica_DespuesDeFallo()
    {
        // Arrange - Configurar fallo temporal seguido de recuperación
        var callCount = 0;
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Numero = "C001", Estado = "Pendiente" }
        };

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 3)
                {
                    throw new Exception($"Service error {callCount}");
                }
                return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
            });

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas múltiples veces
        for (int i = 0; i < 5; i++)
        {
            await comandasViewModel.LoadComandasAsync();
        }

        // Assert - Verificar recuperación
        comandasViewModel.Comandas.Should().HaveCount(1);
        comandasViewModel.ErrorMessage.Should().BeNullOrEmpty();
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(), Times.Exactly(5));
    }

    [Fact]
    public async Task Resilience_RecuperacionAutomatica_ConBackoff()
    {
        // Arrange - Configurar fallo con patrón de backoff
        var callCount = 0;
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "Mesa 1", Estado = "Disponible" }
        };

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 2)
                {
                    throw new Exception($"Service error {callCount}");
                }
                return Task.FromResult(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));
            });

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        // Act - Intentar cargar mesas con reintentos
        await mesasViewModel.LoadMesasAsync(); // Falla
        await Task.Delay(100); // Simular backoff
        await mesasViewModel.LoadMesasAsync(); // Falla
        await Task.Delay(100); // Simular backoff
        await mesasViewModel.LoadMesasAsync(); // Funciona

        // Assert - Verificar recuperación con backoff
        mesasViewModel.Mesas.Should().HaveCount(1);
        mesasViewModel.ErrorMessage.Should().BeNullOrEmpty();
        _mesasServiceMock.Verify(m => m.ObtenerMesasAsync(), Times.Exactly(3));
    }

    #endregion

    #region Pruebas de Degradación Elegante

    [Fact]
    public async Task Resilience_DegradacionElegante_DatosSimulados()
    {
        // Arrange - Configurar fallo total de servicios
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ThrowsAsync(new Exception("All services unavailable"));

        _comandasServiceMock.Setup(c => c.ObtenerEstadisticasAsync())
            .ThrowsAsync(new Exception("All services unavailable"));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar datos
        await comandasViewModel.LoadComandasAsync();
        await comandasViewModel.LoadEstadisticasAsync();

        // Assert - Verificar degradación elegante
        comandasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        comandasViewModel.Comandas.Should().BeEmpty();
        comandasViewModel.Estadisticas.Should().BeNull();
        comandasViewModel.IsBusy.Should().BeFalse();

        // Verificar que se mostró error al usuario
        _dialogServiceMock.Verify(d => d.ShowErrorAsync(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Resilience_DegradacionElegante_FuncionalidadLimitada()
    {
        // Arrange - Configurar fallo de servicios específicos
        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Productos service unavailable"));

        _productosServiceMock.Setup(p => p.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Search service unavailable"));

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Intentar operaciones
        await productosViewModel.LoadProductosAsync();
        await productosViewModel.BuscarProductosCommand.ExecuteAsync("test");

        // Assert - Verificar degradación elegante
        productosViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        productosViewModel.Productos.Should().BeEmpty();
        productosViewModel.IsBusy.Should().BeFalse();
    }

    #endregion

    #region Pruebas de Resilencia de Datos

    [Fact]
    public async Task Resilience_DatosCorruptos_ManejoElegante()
    {
        // Arrange - Configurar respuesta con datos corruptos
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(null!)); // Datos nulos

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Intentar cargar comandas
        await comandasViewModel.LoadComandasAsync();

        // Assert - Verificar manejo de datos corruptos
        comandasViewModel.Comandas.Should().BeEmpty();
        comandasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Resilience_DatosInconsistentes_ManejoElegante()
    {
        // Arrange - Configurar datos inconsistentes
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.Empty, Numero = "", Estado = null! }, // Datos inválidos
            new ComandaDto { Id = Guid.NewGuid(), Numero = "C001", Estado = "Pendiente" } // Datos válidos
        };

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Cargar comandas
        await comandasViewModel.LoadComandasAsync();

        // Assert - Verificar manejo de datos inconsistentes
        comandasViewModel.Comandas.Should().HaveCount(2); // Debe cargar todos los datos
        comandasViewModel.IsBusy.Should().BeFalse();
    }

    #endregion

    #region Pruebas de Resilencia de Concurrencia

    [Fact]
    public async Task Resilience_Concurrencia_FallosSimultaneos()
    {
        // Arrange - Configurar fallos concurrentes
        var callCount = 0;
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount % 2 == 0)
                {
                    throw new Exception($"Concurrent error {callCount}");
                }
                return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));
            });

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Ejecutar operaciones concurrentes
        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            await comandasViewModel.LoadComandasAsync();
        });

        await Task.WhenAll(tasks);

        // Assert - Verificar manejo de fallos concurrentes
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(), Times.Exactly(10));
        comandasViewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Resilience_Concurrencia_RecuperacionSimultanea()
    {
        // Arrange - Configurar recuperación concurrente
        var callCount = 0;
        var comandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Numero = "C001", Estado = "Pendiente" }
        };

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 5)
                {
                    throw new Exception($"Recovery error {callCount}");
                }
                return Task.FromResult(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
            });

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Ejecutar operaciones concurrentes con recuperación
        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            await comandasViewModel.LoadComandasAsync();
        });

        await Task.WhenAll(tasks);

        // Assert - Verificar recuperación concurrente
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(), Times.Exactly(10));
        comandasViewModel.IsBusy.Should().BeFalse();
    }

    #endregion
}
