using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
// using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels; // Comentado temporalmente
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Preferences;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Integration;

/// <summary>
/// Pruebas de integración simuladas para flujos completos de usuario
/// FASE 3: Calidad y Rendimiento
/// </summary>
public class FlujosCompletosIntegrationTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IComandasService> _comandasServiceMock;
    private readonly Mock<IMesasService> _mesasServiceMock;
    private readonly Mock<IProductosService> _productosServiceMock;
    private readonly Mock<IDailyPreparationsService> _dailyPreparationsServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<IDashboardService> _dashboardServiceMock;
    private readonly Mock<IAnalyticsService> _analyticsServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IComandaRealtimeService> _realtimeServiceMock;
    private readonly Mock<IPreferencesService> _preferencesServiceMock;

    public FlujosCompletosIntegrationTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _comandasServiceMock = new Mock<IComandasService>();
        _mesasServiceMock = new Mock<IMesasService>();
        _productosServiceMock = new Mock<IProductosService>();
        _dailyPreparationsServiceMock = new Mock<IDailyPreparationsService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _dashboardServiceMock = new Mock<IDashboardService>();
        _analyticsServiceMock = new Mock<IAnalyticsService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _realtimeServiceMock = new Mock<IComandaRealtimeService>();
        _preferencesServiceMock = new Mock<IPreferencesService>();
    }

    #region Flujo 1: Autenticación y Navegación al Dashboard

    [Fact]
    public async Task FlujoCompleto_LoginExitoso_NavegacionAlDashboard()
    {
        // Arrange - Configurar mocks para flujo de login exitoso
        var email = "mesero@restaurante.com";
        var password = "password123";
        var userId = Guid.NewGuid().ToString();
        var token = "jwt-token-123";

        _authServiceMock.Setup(a => a.LoginAsync(email, password, false))
            .ReturnsAsync(ApiResponse<AuthResponse>.SuccessResponse(new AuthResponse
            {
                Token = token,
                RefreshToken = "refresh-token",
                User = new AuthUser { Id = userId, Email = email, Name = "Mesero Test" }
            }));

        _authServiceMock.Setup(a => a.IsAuthenticatedAsync()).ReturnsAsync(true);
        _authServiceMock.Setup(a => a.GetTokenAsync()).ReturnsAsync(token);
        _authServiceMock.Setup(a => a.GetUserIdAsync()).ReturnsAsync(userId);

        // Configurar datos del dashboard
        _dashboardServiceMock.Setup(d => d.GetTodaySalesAsync()).ReturnsAsync(1250.50m);
        _dashboardServiceMock.Setup(d => d.GetSalesChangePercentageAsync()).ReturnsAsync(15.5m);
        _dashboardServiceMock.Setup(d => d.GetActiveOrdersCountAsync()).ReturnsAsync(8);
        _dashboardServiceMock.Setup(d => d.GetPendingOrdersCountAsync()).ReturnsAsync(3);

        // Act - Simular flujo completo de login
        var loginViewModel = new LoginViewModel(_authServiceMock.Object, _navigationServiceMock.Object);
        loginViewModel.Email = email;
        loginViewModel.Password = password;
        loginViewModel.Recordarme = false;

        await loginViewModel.LoginCommand.ExecuteAsync(null);

        // Assert - Verificar que se ejecutó el flujo completo
        _authServiceMock.Verify(a => a.LoginAsync(email, password, false), Times.Once);
        _navigationServiceMock.Verify(n => n.NavigateToAsync("//main/dashboard"), Times.Once);
        loginViewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task FlujoCompleto_LoginFallido_ManejoDeErrores()
    {
        // Arrange - Configurar mock para login fallido
        var email = "mesero@restaurante.com";
        var password = "password-incorrecto";

        _authServiceMock.Setup(a => a.LoginAsync(email, password, false))
            .ReturnsAsync(ApiResponse<AuthResponse>.ErrorResponse("Credenciales inválidas"));

        // Act - Simular login fallido
        var loginViewModel = new LoginViewModel(_authServiceMock.Object, _navigationServiceMock.Object);
        loginViewModel.Email = email;
        loginViewModel.Password = password;

        await loginViewModel.LoginCommand.ExecuteAsync(null);

        // Assert - Verificar manejo de errores
        _authServiceMock.Verify(a => a.LoginAsync(email, password, false), Times.Once);
        _navigationServiceMock.Verify(n => n.NavigateToAsync(It.IsAny<string>()), Times.Never);
        loginViewModel.IsLoading.Should().BeFalse();
        loginViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Flujo 2: Gestión de Mesas - Asignar Mesa y Crear Comanda

    [Fact]
    public async Task FlujoCompleto_GestionMesas_AsignarMesaYCrearComanda()
    {
        // Arrange - Configurar datos de mesas
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto
        {
            Id = mesaId,
            Numero = "Mesa 5",
            Capacidad = 4,
            Estado = "Disponible",
            Ubicacion = "Terraza"
        };

        var mesasDisponibles = new List<MesaDto> { mesa };
        _mesasServiceMock.Setup(m => m.ObtenerMesasDisponiblesAsync(It.IsAny<int?>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesasDisponibles));

        _mesasServiceMock.Setup(m => m.AsignarMesaAsync(mesaId, It.IsAny<string>(), 4, It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));

        // Configurar productos para la comanda
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", Precio = 15.99m, Activo = true },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Coca Cola", Precio = 2.50m, Activo = true }
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosDisponiblesParaComandasAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        // Configurar creación de comanda
        var comandaId = Guid.NewGuid();
        var comandaCreada = new ComandaDto
        {
            Id = comandaId,
            MesaId = mesaId,
            Numero = "C001",
            Estado = "Pendiente",
            Total = 18.49m
        };

        _comandasServiceMock.Setup(c => c.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comandaCreada));

        // Act - Simular flujo completo de gestión de mesas
        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);
        
        // 1. Cargar mesas disponibles
        await mesasViewModel.LoadMesasAsync();
        
        // 2. Seleccionar mesa
        mesasViewModel.SelectedMesa = mesa;
        
        // 3. Asignar mesa
        await mesasViewModel.AsignarMesaCommand.ExecuteAsync(null);

        // Verificar que se asignó la mesa
        _mesasServiceMock.Verify(m => m.AsignarMesaAsync(mesaId, It.IsAny<string>(), 4, It.IsAny<string>()), Times.Once);

        // 4. Crear comanda para la mesa asignada
        var crearComandaViewModel = new CrearComandaViewModel(
            _comandasServiceMock.Object,
            _productosServiceMock.Object,
            _mesasServiceMock.Object,
            _dailyPreparationsServiceMock.Object,
            _navigationServiceMock.Object,
            _dialogServiceMock.Object);

        crearComandaViewModel.Mesa = mesa;
        await crearComandaViewModel.LoadProductosAsync();

        // Simular agregar productos al carrito
        if (productos.Count >= 2)
        {
            crearComandaViewModel.AgregarProductoCommand.Execute(productos[0]);
            crearComandaViewModel.AgregarProductoCommand.Execute(productos[1]);
        }

        // Crear la comanda
        await crearComandaViewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert - Verificar flujo completo
        _comandasServiceMock.Verify(c => c.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()), Times.Once);
        crearComandaViewModel.Carrito.Should().HaveCount(2);
    }

    #endregion

    #region Flujo 3: Gestión de Comandas - Ciclo de Vida Completo

    [Fact]
    public async Task FlujoCompleto_GestionComandas_CicloDeVidaCompleto()
    {
        // Arrange - Configurar datos iniciales
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var comanda = new ComandaDto
        {
            Id = comandaId,
            MesaId = mesaId,
            Numero = "C001",
            Estado = "Pendiente",
            Total = 25.99m,
            FechaCreacion = DateTime.Now
        };

        var comandas = new List<ComandaDto> { comanda };
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Configurar estadísticas
        var estadisticas = new EstadisticasComandasDto
        {
            TotalComandasActivas = 1,
            ComandasPendientes = 1,
            ComandasEnPreparacion = 0,
            ComandasListas = 0
        };
        _comandasServiceMock.Setup(c => c.ObtenerEstadisticasAsync())
            .ReturnsAsync(ApiResponse<EstadisticasComandasDto>.SuccessResponse(estadisticas));

        // Act - Simular flujo completo de gestión de comandas
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // 1. Cargar comandas activas
        await comandasViewModel.LoadComandasAsync();
        comandasViewModel.Comandas.Should().HaveCount(1);

        // 2. Seleccionar comanda
        comandasViewModel.SelectedComanda = comanda;

        // 3. Cambiar estado a "En Preparación"
        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, "En Preparación", It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda with { Estado = "En Preparación" }));

        await comandasViewModel.CambiarEstadoCommand.ExecuteAsync("En Preparación");

        // 4. Cambiar estado a "Lista"
        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, "Lista", It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda with { Estado = "Lista" }));

        await comandasViewModel.CambiarEstadoCommand.ExecuteAsync("Lista");

        // 5. Finalizar comanda
        _comandasServiceMock.Setup(c => c.FinalizarComandaAsync(comandaId, "Efectivo", It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda with { Estado = "Finalizada" }));

        await comandasViewModel.FinalizarComandaCommand.ExecuteAsync("Efectivo");

        // Assert - Verificar que se ejecutaron todos los cambios de estado
        _comandasServiceMock.Verify(c => c.CambiarEstadoComandaAsync(comandaId, "En Preparación", It.IsAny<string>()), Times.Once);
        _comandasServiceMock.Verify(c => c.CambiarEstadoComandaAsync(comandaId, "Lista", It.IsAny<string>()), Times.Once);
        _comandasServiceMock.Verify(c => c.FinalizarComandaAsync(comandaId, "Efectivo", It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Flujo 4: Gestión de Preparaciones Diarias - COMENTADO TEMPORALMENTE

    // [Fact]
    // public async Task FlujoCompleto_GestionPreparaciones_CicloCompleto()
    // {
    //     // Esta prueba está comentada temporalmente porque DailyPreparationsViewModel no existe
    //     // Se puede implementar cuando se cree el ViewModel correspondiente
    // }

    #endregion

    #region Flujo 5: Gestión de Productos - CRUD Completo

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
                Descripcion = "Pizza con tomate, mozzarella y albahaca",
                Precio = 15.99m,
                CategoriaId = Guid.NewGuid(),
                CategoriaNombre = "Pizzas",
                Activo = true
            }
        };

        var categorias = new List<CategoriaProductoDto>
        {
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Pizzas" },
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas" }
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(1, 20, It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(new PaginatedList<ProductoDto>
            {
                Items = productos,
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 20
            }));

        _productosServiceMock.Setup(p => p.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

        // Act - Simular flujo completo de gestión de productos
        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // 1. Cargar productos
        await productosViewModel.LoadProductosAsync();
        productosViewModel.Productos.Should().HaveCount(1);

        // 2. Crear nuevo producto
        var nuevoProducto = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Pepperoni",
            Descripcion = "Pizza con pepperoni y queso",
            Precio = 17.99m,
            CategoriaId = categorias[0].Id,
            Activo = true
        };

        _productosServiceMock.Setup(p => p.CrearProductoAsync(It.IsAny<CrearProductoRequest>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(nuevoProducto));

        // Simular navegación a editor de producto
        await productosViewModel.CrearProductoCommand.ExecuteAsync(null);

        // 3. Editar producto existente
        var productoExistente = productos[0];
        _productosServiceMock.Setup(p => p.ActualizarProductoAsync(productoExistente.Id, It.IsAny<ActualizarProductoRequest>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(productoExistente with { Precio = 16.99m }));

        // Simular edición
        await productosViewModel.EditarProductoCommand.ExecuteAsync(productoExistente);

        // 4. Eliminar producto
        _productosServiceMock.Setup(p => p.EliminarProductoAsync(productoExistente.Id))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));

        await productosViewModel.EliminarProductoCommand.ExecuteAsync(productoExistente);

        // Assert - Verificar operaciones CRUD
        _productosServiceMock.Verify(p => p.CrearProductoAsync(It.IsAny<CrearProductoRequest>()), Times.Once);
        _productosServiceMock.Verify(p => p.ActualizarProductoAsync(productoExistente.Id, It.IsAny<ActualizarProductoRequest>()), Times.Once);
        _productosServiceMock.Verify(p => p.EliminarProductoAsync(productoExistente.Id), Times.Once);
    }

    #endregion

    #region Flujo 6: Flujo de Error y Recuperación

    [Fact]
    public async Task FlujoCompleto_ManejoDeErrores_RecuperacionAutomatica()
    {
        // Arrange - Configurar fallos de servicios
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ThrowsAsync(new HttpRequestException("Error de red"));

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act - Simular flujos con errores
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // 1. Intentar cargar comandas (debe fallar)
        await comandasViewModel.LoadComandasAsync();
        comandasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();

        // 2. Intentar cargar mesas (debe fallar)
        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);
        await mesasViewModel.LoadMesasAsync();
        mesasViewModel.ErrorMessage.Should().NotBeNullOrEmpty();

        // 3. Verificar que se mostraron errores al usuario
        _dialogServiceMock.Verify(d => d.ShowErrorAsync(It.IsAny<string>()), Times.AtLeastOnce);

        // Assert - Verificar manejo de errores
        comandasViewModel.IsBusy.Should().BeFalse();
        mesasViewModel.IsBusy.Should().BeFalse();
    }

    #endregion

    #region Flujo 7: Flujo de Concurrencia - Múltiples Operaciones Simultáneas

    [Fact]
    public async Task FlujoCompleto_Concurrencia_MultiplesOperacionesSimultaneas()
    {
        // Arrange - Configurar mocks para operaciones concurrentes
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(new List<MesaDto>()));

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(new PaginatedList<ProductoDto>
            {
                Items = new List<ProductoDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 20
            }));

        // Act - Simular operaciones concurrentes
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Ejecutar operaciones concurrentemente
        var tasks = new[]
        {
            comandasViewModel.LoadComandasAsync(),
            mesasViewModel.LoadMesasAsync(),
            productosViewModel.LoadProductosAsync(),
            comandasViewModel.LoadEstadisticasAsync(),
            mesasViewModel.LoadMesasAsync() // Segunda llamada para probar concurrencia
        };

        await Task.WhenAll(tasks);

        // Assert - Verificar que todas las operaciones se completaron
        comandasViewModel.IsBusy.Should().BeFalse();
        mesasViewModel.IsBusy.Should().BeFalse();
        productosViewModel.IsBusy.Should().BeFalse();

        // Verificar que se llamaron los servicios
        _comandasServiceMock.Verify(c => c.ObtenerComandasActivasAsync(), Times.Once);
        _mesasServiceMock.Verify(m => m.ObtenerMesasAsync(), Times.Exactly(2));
        _productosServiceMock.Verify(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    #endregion

    #region Flujo 8: Flujo de Persistencia - Estado de Aplicación

    [Fact]
    public async Task FlujoCompleto_Persistencia_EstadoDeAplicacion()
    {
        // Arrange - Configurar preferencias
        var filtroEstado = "Pendiente";
        var filtroUbicacion = "Terraza";
        var soloActivas = true;

        _preferencesServiceMock.Setup(p => p.GetAsync<bool>("SoloActivas", true))
            .ReturnsAsync(soloActivas);
        _preferencesServiceMock.Setup(p => p.GetAsync<string>("FiltroEstado", ""))
            .ReturnsAsync(filtroEstado);
        _preferencesServiceMock.Setup(p => p.GetAsync<string>("FiltroUbicacion", ""))
            .ReturnsAsync(filtroUbicacion);

        // Configurar datos
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto>()));

        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(new List<MesaDto>()));

        // Act - Simular restauración de estado
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        // Cargar datos (debe restaurar preferencias)
        await comandasViewModel.LoadComandasAsync();
        await mesasViewModel.LoadMesasAsync();

        // Simular guardado de preferencias
        _preferencesServiceMock.Setup(p => p.SetAsync("FiltroEstado", "En Preparación"))
            .Returns(Task.CompletedTask);

        comandasViewModel.FiltroEstado = "En Preparación";
        await comandasViewModel.AplicarFiltrosCommand.ExecuteAsync(null);

        // Assert - Verificar persistencia
        _preferencesServiceMock.Verify(p => p.GetAsync<bool>("SoloActivas", true), Times.Once);
        _preferencesServiceMock.Verify(p => p.GetAsync<string>("FiltroEstado", ""), Times.Once);
        _preferencesServiceMock.Verify(p => p.GetAsync<string>("FiltroUbicacion", ""), Times.Once);
        _preferencesServiceMock.Verify(p => p.SetAsync("FiltroEstado", "En Preparación"), Times.Once);
    }

    #endregion
}
