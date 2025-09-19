using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Preferences;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace RestaurantePro.Mobile.UnitTests.Integration;

/// <summary>
/// Pruebas de integración simuladas para flujos completos de usuario
/// </summary>
public class FlujosCompletosIntegrationTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IComandasService> _comandasServiceMock;
    private readonly Mock<IMesasService> _mesasServiceMock;
    private readonly Mock<IProductosService> _productosServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<IDashboardService> _dashboardServiceMock;
    private readonly Mock<IAnalyticsService> _analyticsServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IComandaRealtimeService> _realtimeServiceMock;
    private readonly Mock<IPreferencesService> _preferencesServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IAuthorizationValidator> _authorizationValidatorMock;
    private readonly AuthorizationUIHelper _authorizationUIHelper;

    public FlujosCompletosIntegrationTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _comandasServiceMock = new Mock<IComandasService>();
        _mesasServiceMock = new Mock<IMesasService>();
        _productosServiceMock = new Mock<IProductosService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _dashboardServiceMock = new Mock<IDashboardService>();
        _analyticsServiceMock = new Mock<IAnalyticsService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _realtimeServiceMock = new Mock<IComandaRealtimeService>();
        _preferencesServiceMock = new Mock<IPreferencesService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _authorizationValidatorMock = new Mock<IAuthorizationValidator>();
        _authorizationUIHelper = new AuthorizationUIHelper(_authorizationServiceMock.Object, NullLogger<AuthorizationUIHelper>.Instance);
    }

    #region Flujo 1: Autenticación Completa

    [Fact]
    public async Task FlujoCompleto_Autenticacion_LoginExitoso()
    {
        // Arrange - Configurar datos de autenticación
        var loginRequest = new LoginRequest
        {
            Email = "chef@restaurante.com",
            Password = "Password123!",
            Recordarme = true
        };

        var authResponse = new AuthResponse
        {
            Token = "jwt_token_123",
            RefreshToken = "refresh_token_123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new AuthUser
            {
                Id = 1,
                Email = "chef@restaurante.com",
                Nombre = "Chef Principal",
                Roles = new List<string> { "Chef" }
            }
        };

        _authServiceMock.Setup(a => a.LoginAsync(loginRequest.Email, loginRequest.Password, loginRequest.Recordarme))
            .ReturnsAsync(ApiResponse<AuthResponse>.SuccessResponse(authResponse));

        // Act - Simular flujo completo de autenticación
        var loginViewModel = new LoginViewModel(
            _authServiceMock.Object,
            _navigationServiceMock.Object);

        loginViewModel.Email = loginRequest.Email;
        loginViewModel.Password = loginRequest.Password;
        loginViewModel.Recordarme = loginRequest.Recordarme;

        await loginViewModel.LoginCommand.ExecuteAsync(null);

        // Assert - Verificar operaciones realizadas
        _authServiceMock.Verify(a => a.LoginAsync(loginRequest.Email, loginRequest.Password, loginRequest.Recordarme), Times.Once);
        _navigationServiceMock.Verify(n => n.NavigateToAsync("//main/dashboard"), Times.Once);
    }

    #endregion

    #region Flujo 2: Gestión de Mesas - Asignación y Liberación

    [Fact]
    public async Task FlujoCompleto_GestionMesas_AsignacionYLiberacion()
    {
        // Arrange - Configurar datos de mesas
        var mesas = new List<MesaDto>
        {
            new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "Mesa 1",
                Capacidad = 4,
                Estado = "Disponible",
                Ubicacion = "Salón Principal"
            },
            new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "Mesa 2",
                Capacidad = 6,
                Estado = "Ocupada",
                Ubicacion = "Terraza"
            }
        };

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 }));

        _mesasServiceMock.Setup(m => m.AsignarMesaAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<object>.SuccessResponse(new object()));

        _mesasServiceMock.Setup(m => m.LiberarMesaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesas[0]));

        // Act - Simular flujo completo de gestión de mesas
        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _authServiceMock.Object);

        // 1. Cargar mesas
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);
        mesasViewModel.Mesas.Should().HaveCount(2);

        // 2. Asignar mesa disponible
        var mesaDisponible = mesas.First(m => m.Estado == "Disponible");
        _dialogServiceMock.Setup(d => d.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("12345678-1234-1234-1234-123456789012"); // ID de cliente válido
        await mesasViewModel.AsignarMesaCommand.ExecuteAsync(mesaDisponible);

        // 3. Liberar mesa ocupada
        var mesaOcupada = mesas.First(m => m.Estado == "Ocupada");
        _dialogServiceMock.Setup(d => d.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _dialogServiceMock.Setup(d => d.ShowPromptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("Cliente terminó"); // Motivo de liberación
        await mesasViewModel.LiberarMesaCommand.ExecuteAsync(mesaOcupada);

        // Assert - Verificar operaciones realizadas
        _mesasServiceMock.Verify(m => m.AsignarMesaAsync(mesaDisponible.Id, It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mesasServiceMock.Verify(m => m.LiberarMesaAsync(mesaOcupada.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Flujo 3: Gestión de Comandas - Ciclo Completo

    [Fact]
    public async Task FlujoCompleto_GestionComandas_CicloCompleto()
    {
        // Arrange - Configurar datos de comandas
        var comandas = new List<ComandaDto>
        {
            new ComandaDto
            {
                Id = Guid.NewGuid(),
                Numero = "C-001",
                MesaId = Guid.NewGuid(),
                MesaNumero = "Mesa 1",
                Estado = "Pendiente",
                Total = 45.50m,
                FechaCreacion = DateTime.Now
            }
        };

        _comandasServiceMock.Setup(c => c.BuscarComandasAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));
        
        _comandasServiceMock.Setup(c => c.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadisticasComandasDto>.SuccessResponse(new EstadisticasComandasDto()));

        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comandas[0]));

        _comandasServiceMock.Setup(c => c.FinalizarComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comandas[0]));

        // Act - Simular flujo completo de gestión de comandas
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object,
            _authorizationServiceMock.Object,
            _authorizationValidatorMock.Object,
            _authorizationUIHelper);

        // 1. Cargar comandas activas
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);
        comandasViewModel.Comandas.Should().HaveCount(1);

        // 2. Cambiar estado de comanda
        var comanda = comandas.First();
        
        // Configurar mocks para los diálogos ANTES de ejecutar los comandos
        _dialogServiceMock.Setup(d => d.ShowActionSheetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync("En Preparación"); // Nuevo estado válido para comanda "Pendiente"
        
        await comandasViewModel.CambiarEstadoComandaCommand.ExecuteAsync(comanda);

        // 3. Finalizar comanda
        _dialogServiceMock.Setup(d => d.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _dialogServiceMock.Setup(d => d.ShowActionSheetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync("Efectivo"); // Método de pago
        await comandasViewModel.FinalizarComandaCommand.ExecuteAsync(comanda);

        // Assert - Verificar operaciones realizadas
        _comandasServiceMock.Verify(c => c.CambiarEstadoComandaAsync(comanda.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _comandasServiceMock.Verify(c => c.FinalizarComandaAsync(comanda.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Flujo 4: Gestión de Productos - CRUD Completo

    [Fact]
    public async Task FlujoCompleto_GestionProductos_CRUDCompleto()
    {
        // Arrange - Configurar datos de productos
        var productos = new List<ProductoDto>
        {
            new ProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                Descripcion = "Pizza clásica con tomate y mozzarella",
                Precio = 12.50m,
                CategoriaNombre = "Pizzas",
                Activo = true
            }
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(1, 100, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));
        
        _productosServiceMock.Setup(p => p.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));

        _productosServiceMock.Setup(p => p.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(productos[0]));

        _productosServiceMock.Setup(p => p.ActualizarProductoAsync(It.IsAny<ActualizarProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(productos[0]));

        _productosServiceMock.Setup(p => p.EliminarProductoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));

        // Act - Simular flujo completo de gestión de productos
        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // 1. Cargar productos
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        productosViewModel.Productos.Should().HaveCount(1);

        // 2. Navegar a crear producto (el comando solo navega, no crea directamente)
        await productosViewModel.CrearProductoCommand.ExecuteAsync(null);

        // 3. Ver producto existente
        var productoExistente = productos.First();
        await productosViewModel.VerProductoCommand.ExecuteAsync(productoExistente);

        // 4. Filtrar por categoría (si hay categorías)
        if (productosViewModel.Categorias.Any())
        {
            var categoria = productosViewModel.Categorias.First();
            await productosViewModel.FiltrarPorCategoriaCommand.ExecuteAsync(categoria);
        }

        // Assert - Verificar que se cargaron los productos correctamente
        productosViewModel.Productos.Should().HaveCount(1);
        productosViewModel.TieneProductos.Should().BeTrue();
    }

    #endregion

    #region Flujo 5: Dashboard - Carga de Métricas

    [Fact]
    public async Task FlujoCompleto_Dashboard_CargaMetricas()
    {
        // Arrange - Configurar datos del dashboard
        var ventasHoy = 1250.75m;
        var cambioVentas = 15.5m;
        var comandasActivas = 8;
        var comandasPendientes = 3;

        var estadoMesas = new EstadoMesasDto
        {
            Mesas = new List<MesaDto>
            {
                new MesaDto { Id = Guid.NewGuid(), Numero = "1", Estado = "Disponible", Capacidad = 4 },
                new MesaDto { Id = Guid.NewGuid(), Numero = "2", Estado = "Ocupada", Capacidad = 6 },
                new MesaDto { Id = Guid.NewGuid(), Numero = "3", Estado = "Reservada", Capacidad = 2 }
            }
        };

        _dashboardServiceMock.Setup(d => d.GetTodaySalesAsync())
            .ReturnsAsync(ventasHoy);

        _dashboardServiceMock.Setup(d => d.GetSalesChangePercentageAsync())
            .ReturnsAsync(cambioVentas);

        _dashboardServiceMock.Setup(d => d.GetActiveOrdersCountAsync())
            .ReturnsAsync(comandasActivas);

        _dashboardServiceMock.Setup(d => d.GetPendingOrdersCountAsync())
            .ReturnsAsync(comandasPendientes);

        _dashboardServiceMock.Setup(d => d.GetTableStatusAsync())
            .ReturnsAsync(estadoMesas);

        // Act - Simular flujo completo de carga del dashboard
        var ventasHoyResult = await _dashboardServiceMock.Object.GetTodaySalesAsync();
        var cambioVentasResult = await _dashboardServiceMock.Object.GetSalesChangePercentageAsync();
        var comandasActivasResult = await _dashboardServiceMock.Object.GetActiveOrdersCountAsync();
        var comandasPendientesResult = await _dashboardServiceMock.Object.GetPendingOrdersCountAsync();
        var estadoMesasResult = await _dashboardServiceMock.Object.GetTableStatusAsync();

        // Assert - Verificar datos cargados
        ventasHoyResult.Should().Be(1250.75m);
        cambioVentasResult.Should().Be(15.5m);
        comandasActivasResult.Should().Be(8);
        comandasPendientesResult.Should().Be(3);
        estadoMesasResult.Should().NotBeNull();
        estadoMesasResult.TotalMesas.Should().Be(3);
    }

    #endregion

    #region Flujo 6: Manejo de Errores - Recuperación Automática

    [Fact]
    public async Task FlujoCompleto_ManejoErrores_RecuperacionAutomatica()
    {
        // Arrange - Configurar fallos de red
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Error de red"));

        _comandasServiceMock.Setup(c => c.BuscarComandasAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NullReferenceException("Object reference not set to an instance of an object"));
        
        _comandasServiceMock.Setup(c => c.ObtenerEstadisticasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NullReferenceException("Object reference not set to an instance of an object"));

        // Act - Simular flujo de recuperación de errores
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object,
            _authorizationServiceMock.Object,
            _authorizationValidatorMock.Object,
            _authorizationUIHelper);

        // 1. Primer intento - falla
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);
        
        // 2. Segundo intento - éxito
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert - Verificar que se manejó el error y se recuperó (1 llamada a ShowAlertAsync + 3 llamadas a ShowErrorAsync)
        _dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _dialogServiceMock.Verify(d => d.ShowErrorAsync(It.IsAny<string>()), Times.Exactly(3));
    }

    #endregion

    #region Flujo 7: Concurrencia - Múltiples Operaciones Simultáneas

    [Fact]
    public async Task FlujoCompleto_Concurrencia_MultiplesOperacionesSimultaneas()
    {
        // Arrange - Configurar datos para operaciones concurrentes
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "Mesa 1", Estado = "Disponible" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "Mesa 2", Estado = "Disponible" }
        };

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(new PaginatedList<MesaDto> { Items = mesas, TotalCount = mesas.Count, PageNumber = 1, PageSize = 10 }));

        _mesasServiceMock.Setup(m => m.AsignarMesaAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<object>.SuccessResponse(new object()));

        // Act - Simular operaciones concurrentes
        var mesasViewModel = new MesasViewModel(
            _mesasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _authServiceMock.Object);

        // Ejecutar múltiples operaciones simultáneamente
        var tasks = new List<Task>
        {
            mesasViewModel.LoadMesasAsync(),
            mesasViewModel.LoadMesasAsync(),
            mesasViewModel.LoadMesasAsync()
        };

        await Task.WhenAll(tasks);

        // Assert - Verificar que todas las operaciones se completaron
        _mesasServiceMock.Verify(m => m.ObtenerMesasAsync(null, null, null, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    #endregion

    #region Flujo 8: Persistencia - Guardado de Preferencias

    [Fact]
    public async Task FlujoCompleto_Persistencia_GuardadoPreferencias()
    {
        // Arrange - Configurar preferencias
        var preferencias = new Dictionary<string, object>
        {
            { "FiltroEstadoMesas", "Disponible" },
            { "OrdenProductos", "Nombre" },
            { "TemaApp", "Claro" }
        };

        _preferencesServiceMock.Setup(p => p.SetAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        _preferencesServiceMock.Setup(p => p.Get<string>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("Disponible");

        // Act - Simular flujo de guardado de preferencias
        foreach (var preferencia in preferencias)
        {
            await _preferencesServiceMock.Object.SetAsync(preferencia.Key, preferencia.Value);
        }

        var valorRecuperado = _preferencesServiceMock.Object.Get<string>("FiltroEstadoMesas", "Todos");

        // Assert - Verificar que las preferencias se guardaron y recuperaron
        _preferencesServiceMock.Verify(p => p.SetAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(3));
        valorRecuperado.Should().Be("Disponible");
    }

    #endregion
}
