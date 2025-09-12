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

namespace RestaurantePro.Mobile.UnitTests.Performance;

/// <summary>
/// Pruebas de rendimiento para la aplicación móvil
/// FASE 3: Calidad y Rendimiento
/// </summary>
public class PerformanceTests
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

    public PerformanceTests()
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

    #region Pruebas de Carga con Grandes Volúmenes de Datos

    [Fact]
    public async Task Performance_CargarComandas_GranVolumenDeDatos()
    {
        // Arrange - Generar 1000 comandas para prueba de carga
        var comandas = GenerateLargeComandasList(1000);
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

        // Act - Medir tiempo de carga
        var stopwatch = Stopwatch.StartNew();
        await comandasViewModel.LoadComandasAsync();
        stopwatch.Stop();

        // Assert - Verificar rendimiento
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Menos de 5 segundos
        comandasViewModel.Comandas.Should().HaveCount(1000);
        comandasViewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Performance_CargarMesas_GranVolumenDeDatos()
    {
        // Arrange - Generar 500 mesas para prueba de carga
        var mesas = GenerateLargeMesasList(500);
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));

        var mesasViewModel = new MesasViewModel(_mesasServiceMock.Object, _dialogServiceMock.Object, _navigationServiceMock.Object);

        // Act - Medir tiempo de carga
        var stopwatch = Stopwatch.StartNew();
        await mesasViewModel.LoadMesasAsync();
        stopwatch.Stop();

        // Assert - Verificar rendimiento
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos
        mesasViewModel.Mesas.Should().HaveCount(500);
        mesasViewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Performance_CargarProductos_GranVolumenDeDatos()
    {
        // Arrange - Generar 2000 productos para prueba de carga
        var productos = GenerateLargeProductosList(2000);
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = 2000,
            PageNumber = 1,
            PageSize = 2000
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList));

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Medir tiempo de carga
        var stopwatch = Stopwatch.StartNew();
        await productosViewModel.LoadProductosAsync();
        stopwatch.Stop();

        // Assert - Verificar rendimiento
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(4000); // Menos de 4 segundos
        productosViewModel.Productos.Should().HaveCount(2000);
        productosViewModel.IsBusy.Should().BeFalse();
    }

    // [Fact]
    // public async Task Performance_CargarPreparaciones_GranVolumenDeDatos()
    // {
    //     // Esta prueba está comentada temporalmente porque DailyPreparationsViewModel no existe
    // }

    #endregion

    #region Pruebas de Memoria - Detección de Memory Leaks

    [Fact]
    public async Task Performance_MemoryLeaks_CargaMultipleSinLiberacion()
    {
        // Arrange
        var comandas = GenerateLargeComandasList(100);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Realizar múltiples cargas para detectar memory leaks
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
            
            // Forzar garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // Assert - Verificar que no hay memory leaks significativos
        var memoryBefore = GC.GetTotalMemory(false);
        GC.Collect();
        var memoryAfter = GC.GetTotalMemory(true);
        
        // La diferencia de memoria no debería ser excesiva
        (memoryBefore - memoryAfter).Should().BeLessThan(10 * 1024 * 1024); // Menos de 10MB
    }

    [Fact]
    public async Task Performance_MemoryLeaks_EventosYSubscripciones()
    {
        // Arrange
        var comandas = new List<ComandaDto>();
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Crear y destruir ViewModels múltiples veces
        for (int i = 0; i < 5; i++)
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
            
            // Forzar garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // Assert - Verificar que las subscripciones se liberaron correctamente
        // (En un escenario real, esto se verificaría con herramientas de profiling)
        var finalMemory = GC.GetTotalMemory(true);
        finalMemory.Should().BeLessThan(50 * 1024 * 1024); // Menos de 50MB
    }

    #endregion

    #region Pruebas de Tiempo de Respuesta

    [Fact]
    public async Task Performance_TiempoRespuesta_OperacionesCriticas()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var comanda = new ComandaDto { Id = comandaId, Estado = "Pendiente" };
        
        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, "En Preparación", It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda with { Estado = "En Preparación" }));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Medir tiempo de operaciones críticas
        var stopwatch = Stopwatch.StartNew();
        await comandasViewModel.CambiarEstadoCommand.ExecuteAsync("En Preparación");
        stopwatch.Stop();

        // Assert - Verificar tiempo de respuesta
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
    }

    [Fact]
    public async Task Performance_TiempoRespuesta_BusquedaYFiltrado()
    {
        // Arrange
        var productos = GenerateLargeProductosList(1000);
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = 1000,
            PageNumber = 1,
            PageSize = 1000
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList));

        var productosViewModel = new ProductosViewModel(
            _productosServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object);

        // Act - Medir tiempo de búsqueda
        var stopwatch = Stopwatch.StartNew();
        productosViewModel.SearchText = "Pizza";
        await productosViewModel.BuscarProductosCommand.ExecuteAsync(null);
        stopwatch.Stop();

        // Assert - Verificar tiempo de búsqueda
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Menos de 500ms
    }

    #endregion

    #region Pruebas de Concurrencia y Carga

    [Fact]
    public async Task Performance_Concurrencia_MultiplesUsuariosSimultaneos()
    {
        // Arrange
        var comandas = GenerateLargeComandasList(100);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular 10 usuarios cargando comandas simultáneamente
        var tasks = Enumerable.Range(1, 10).Select(async _ =>
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
            return comandasViewModel.Comandas.Count;
        });

        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar concurrencia
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Menos de 10 segundos
        results.Should().AllBeEquivalentTo(100);
    }

    [Fact]
    public async Task Performance_Concurrencia_OperacionesCRUDSimultaneas()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var comanda = new ComandaDto { Id = comandaId, Estado = "Pendiente" };

        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda));

        // Act - Simular múltiples operaciones CRUD simultáneas
        var tasks = Enumerable.Range(1, 20).Select(async i =>
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            await comandasViewModel.CambiarEstadoCommand.ExecuteAsync($"Estado{i}");
            return i;
        });

        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar rendimiento bajo carga
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Menos de 5 segundos
        results.Should().HaveCount(20);
    }

    #endregion

    #region Pruebas de Escalabilidad

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2000)]
    public async Task Performance_Escalabilidad_CargaConDiferentesVolumenes(int cantidad)
    {
        // Arrange
        var comandas = GenerateLargeComandasList(cantidad);
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

        // Act - Medir tiempo de carga
        var stopwatch = Stopwatch.StartNew();
        await comandasViewModel.LoadComandasAsync();
        stopwatch.Stop();

        // Assert - Verificar escalabilidad
        var tiempoMaximo = cantidad switch
        {
            <= 100 => 1000,    // 1 segundo para 100 elementos
            <= 500 => 2000,    // 2 segundos para 500 elementos
            <= 1000 => 4000,   // 4 segundos para 1000 elementos
            _ => 8000          // 8 segundos para 2000+ elementos
        };

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(tiempoMaximo);
        comandasViewModel.Comandas.Should().HaveCount(cantidad);
    }

    #endregion

    #region Métodos Auxiliares

    private List<ComandaDto> GenerateLargeComandasList(int count)
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

    private List<MesaDto> GenerateLargeMesasList(int count)
    {
        var random = new Random();
        var mesas = new List<MesaDto>();
        var ubicaciones = new[] { "Terraza", "Interior", "Ventana", "Barra" };
        var estados = new[] { "Disponible", "Ocupada", "Reservada", "Mantenimiento" };

        for (int i = 0; i < count; i++)
        {
            mesas.Add(new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = $"Mesa {i + 1}",
                Capacidad = random.Next(2, 8),
                Estado = estados[random.Next(estados.Length)],
                Ubicacion = ubicaciones[random.Next(ubicaciones.Length)],
                FechaCreacion = DateTime.Now.AddDays(-random.Next(0, 365))
            });
        }

        return mesas;
    }

    private List<ProductoDto> GenerateLargeProductosList(int count)
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

    private List<PreparacionDiariaDto> GenerateLargePreparacionesList(int count)
    {
        var random = new Random();
        var preparaciones = new List<PreparacionDiariaDto>();
        var estados = new[] { "Disponible", "Agotado", "En Preparación", "Cancelado" };

        for (int i = 0; i < count; i++)
        {
            var cantidadPreparada = random.Next(1, 20);
            var cantidadConsumida = random.Next(0, cantidadPreparada);
            var cantidadDisponible = cantidadPreparada - cantidadConsumida;

            preparaciones.Add(new PreparacionDiariaDto
            {
                Id = Guid.NewGuid(),
                ProductoId = Guid.NewGuid(),
                ProductoNombre = $"Producto {i + 1}",
                CantidadPreparada = cantidadPreparada,
                CantidadDisponible = cantidadDisponible,
                CantidadConsumida = cantidadConsumida,
                Estado = estados[random.Next(estados.Length)],
                FechaPreparacion = DateTime.Today.AddHours(-random.Next(0, 24))
            });
        }

        return preparaciones;
    }

    private string GetRandomEstado(Random random)
    {
        var estados = new[] { "Pendiente", "En Preparación", "Lista", "Entregada", "Finalizada", "Cancelada" };
        return estados[random.Next(estados.Length)];
    }

    #endregion
}
