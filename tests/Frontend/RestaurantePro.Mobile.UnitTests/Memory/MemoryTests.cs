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
using System.Diagnostics;

namespace RestaurantePro.Mobile.UnitTests.Memory;

/// <summary>
/// Pruebas de memoria para detección de memory leaks y uso eficiente de recursos
/// FASE 3: Calidad y Rendimiento
/// </summary>
public class MemoryTests
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

    public MemoryTests()
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

    #region Pruebas de Memory Leaks

    [Fact]
    public async Task MemoryTest_ViewModels_CreacionYDestruccion()
    {
        // Arrange
        var comandas = GenerateComandasList(100);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear y destruir ViewModels múltiples veces
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 50; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            // Simular navegación y destrucción del ViewModel
            comandasViewModel = null;
            
            // Forzar garbage collection cada 10 iteraciones
            if (i % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        // Forzar garbage collection final
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar que no hay memory leaks significativos
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // Menos de 10MB
    }

    [Fact]
    public async Task MemoryTest_Eventos_SubscripcionesYLiberacion()
    {
        // Arrange
        var comandas = new List<ComandaDto>();
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear ViewModels con eventos y verificar liberación
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 20; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            // Simular suscripción a eventos
            comandasViewModel.PropertyChanged += (sender, e) => { };
            
            // Simular destrucción
            comandasViewModel = null;
            
            // Forzar garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar liberación de eventos
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(5 * 1024 * 1024); // Menos de 5MB
    }

    [Fact]
    public async Task MemoryTest_ObservableCollections_LiberacionCorrecta()
    {
        // Arrange
        var comandas = GenerateComandasList(1000);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear ViewModels con colecciones grandes
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 10; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            // Verificar que las colecciones se llenaron
            comandasViewModel.Comandas.Should().HaveCount(1000);
            
            // Simular destrucción
            comandasViewModel = null;
            
            // Forzar garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar liberación de colecciones
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(20 * 1024 * 1024); // Menos de 20MB
    }

    #endregion

    #region Pruebas de Uso Eficiente de Memoria

    [Fact]
    public async Task MemoryTest_UsoEficiente_CargaIncremental()
    {
        // Arrange
        var comandas = GenerateComandasList(5000);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Cargar datos grandes y verificar uso de memoria
        var initialMemory = GC.GetTotalMemory(false);
        
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        await comandasViewModel.LoadComandasAsync();
        
        var afterLoadMemory = GC.GetTotalMemory(false);
        
        // Simular operaciones adicionales
        for (int i = 0; i < 10; i++)
        {
            await comandasViewModel.LoadComandasAsync();
        }
        
        var afterOperationsMemory = GC.GetTotalMemory(false);

        // Assert - Verificar uso eficiente de memoria
        var loadMemoryIncrease = afterLoadMemory - initialMemory;
        var operationsMemoryIncrease = afterOperationsMemory - afterLoadMemory;
        
        loadMemoryIncrease.Should().BeLessThan(50 * 1024 * 1024); // Menos de 50MB para carga inicial
        operationsMemoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // Menos de 10MB para operaciones adicionales
    }

    [Fact]
    public async Task MemoryTest_UsoEficiente_FiltradoYBusqueda()
    {
        // Arrange
        var productos = GenerateProductosList(2000);
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = 2000,
            PageNumber = 1,
            PageSize = 2000
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList));

        _productosServiceMock.Setup(p => p.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos.Take(100).ToList()));

        // Act - Realizar operaciones de filtrado y búsqueda
        var initialMemory = GC.GetTotalMemory(false);
        
        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        await productosViewModel.LoadProductosAsync();
        var afterLoadMemory = GC.GetTotalMemory(false);
        
        // Realizar múltiples búsquedas
        for (int i = 0; i < 20; i++)
        {
            productosViewModel.SearchText = $"Producto {i}";
            await productosViewModel.BuscarProductosCommand.ExecuteAsync(null);
        }
        
        var afterSearchMemory = GC.GetTotalMemory(false);

        // Assert - Verificar uso eficiente de memoria
        var loadMemoryIncrease = afterLoadMemory - initialMemory;
        var searchMemoryIncrease = afterSearchMemory - afterLoadMemory;
        
        loadMemoryIncrease.Should().BeLessThan(30 * 1024 * 1024); // Menos de 30MB para carga
        searchMemoryIncrease.Should().BeLessThan(5 * 1024 * 1024); // Menos de 5MB para búsquedas
    }

    #endregion

    #region Pruebas de Garbage Collection

    [Fact]
    public async Task MemoryTest_GarbageCollection_RecoleccionAutomatica()
    {
        // Arrange
        var comandas = GenerateComandasList(100);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear objetos temporales y verificar recolección
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 100; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            // Simular destrucción inmediata
            comandasViewModel = null;
        }

        // Forzar garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar recolección automática
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(5 * 1024 * 1024); // Menos de 5MB
    }

    [Fact]
    public async Task MemoryTest_GarbageCollection_PresionDeMemoria()
    {
        // Arrange
        var comandas = GenerateComandasList(1000);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear presión de memoria
        var initialMemory = GC.GetTotalMemory(false);
        var memoryPeak = 0L;
        
        for (int i = 0; i < 50; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            var currentMemory = GC.GetTotalMemory(false);
            memoryPeak = Math.Max(memoryPeak, currentMemory);
            
            // Simular destrucción
            comandasViewModel = null;
            
            // Forzar garbage collection cada 10 iteraciones
            if (i % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar manejo de presión de memoria
        var memoryIncrease = finalMemory - initialMemory;
        var peakIncrease = memoryPeak - initialMemory;
        
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // Menos de 10MB final
        peakIncrease.Should().BeLessThan(100 * 1024 * 1024); // Menos de 100MB pico
    }

    #endregion

    #region Pruebas de Memory Leaks Específicos

    [Fact]
    public async Task MemoryTest_MemoryLeaks_EventosRealtime()
    {
        // Arrange
        var comandas = new List<ComandaDto>();
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear ViewModels con eventos realtime
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 30; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            // Simular suscripción a eventos realtime
            _realtimeServiceMock.Raise(r => r.OnNuevaComanda += null);
            _realtimeServiceMock.Raise(r => r.OnComandaActualizada += null);
            
            // Simular destrucción
            comandasViewModel = null;
            
            // Forzar garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar liberación de eventos realtime
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(5 * 1024 * 1024); // Menos de 5MB
    }

    [Fact]
    public async Task MemoryTest_MemoryLeaks_Preferencias()
    {
        // Arrange
        var comandas = new List<ComandaDto>();
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        _preferencesServiceMock.Setup(p => p.GetAsync<bool>("SoloActivas", true))
            .ReturnsAsync(true);
        _preferencesServiceMock.Setup(p => p.GetAsync<string>("FiltroEstado", ""))
            .ReturnsAsync("Pendiente");

        // Act - Crear ViewModels con preferencias
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 20; i++)
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.LoadComandasAsync();
            
            // Simular guardado de preferencias
            _preferencesServiceMock.Setup(p => p.SetAsync("FiltroEstado", $"Estado{i}"))
                .Returns(Task.CompletedTask);
            
            comandasViewModel.FiltroEstado = $"Estado{i}";
            await comandasViewModel.AplicarFiltrosCommand.ExecuteAsync(null);
            
            // Simular destrucción
            comandasViewModel = null;
            
            // Forzar garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Verificar liberación de preferencias
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(3 * 1024 * 1024); // Menos de 3MB
    }

    #endregion

    #region Pruebas de Memoria con Datos Grandes

    [Fact]
    public async Task MemoryTest_DatosGrandes_ManejoEficiente()
    {
        // Arrange - Configurar datos muy grandes
        var comandas = GenerateComandasList(10000);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Cargar datos grandes
        var initialMemory = GC.GetTotalMemory(false);
        
        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        await comandasViewModel.LoadComandasAsync();
        
        var afterLoadMemory = GC.GetTotalMemory(false);
        
        // Simular operaciones con datos grandes
        for (int i = 0; i < 5; i++)
        {
            await comandasViewModel.LoadComandasAsync();
        }
        
        var afterOperationsMemory = GC.GetTotalMemory(false);

        // Assert - Verificar manejo eficiente de datos grandes
        var loadMemoryIncrease = afterLoadMemory - initialMemory;
        var operationsMemoryIncrease = afterOperationsMemory - afterLoadMemory;
        
        loadMemoryIncrease.Should().BeLessThan(100 * 1024 * 1024); // Menos de 100MB para carga
        operationsMemoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // Menos de 10MB para operaciones
    }

    #endregion

    #region Métodos Auxiliares

    private List<ComandaDto> GenerateComandasList(int count)
    {
        var random = new Random();
        var comandas = new List<ComandaDto>();

        for (int i = 0; i < count; i++)
        {
            comandas.Add(new ComandaDto
            {
                Id = Guid.NewGuid(),
                Numero = $"C{i:D4}",
                MesaId = Guid.NewGuid(),
                Estado = GetRandomEstado(random),
                Total = (decimal)(random.NextDouble() * 100 + 10),
                FechaCreacion = DateTime.Now.AddMinutes(-random.Next(0, 1440)),
                ClienteNombre = $"Cliente {i}",
                Observaciones = $"Observaciones para comanda {i}"
            });
        }

        return comandas;
    }

    private List<ProductoDto> GenerateProductosList(int count)
    {
        var random = new Random();
        var productos = new List<ProductoDto>();
        var categorias = new[] { "Pizzas", "Bebidas", "Postres", "Entradas", "Platos Principales" };

        for (int i = 0; i < count; i++)
        {
            productos.Add(new ProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Producto {i + 1}",
                Descripcion = $"Descripción del producto {i + 1}",
                Precio = (decimal)(random.NextDouble() * 50 + 5),
                CategoriaId = Guid.NewGuid(),
                CategoriaNombre = categorias[random.Next(categorias.Length)],
                Activo = random.Next(0, 2) == 1,
                FechaCreacion = DateTime.Now.AddDays(-random.Next(0, 365))
            });
        }

        return productos;
    }

    private string GetRandomEstado(Random random)
    {
        var estados = new[] { "Pendiente", "En Preparación", "Lista", "Entregada", "Finalizada", "Cancelada" };
        return estados[random.Next(estados.Length)];
    }

    #endregion
}
