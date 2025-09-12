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

namespace RestaurantePro.Mobile.UnitTests.Load;

/// <summary>
/// Pruebas de carga para múltiples usuarios concurrentes y picos de tráfico
/// FASE 3: Calidad y Rendimiento
/// </summary>
public class LoadTests
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

    public LoadTests()
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

    #region Pruebas de Carga Básica

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task LoadTest_UsuariosConcurrentes_CargaBasica(int usuarios)
    {
        // Arrange - Configurar datos para múltiples usuarios
        var comandas = GenerateComandasList(50);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        var mesas = GenerateMesasList(20);
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync())
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));

        var productos = GenerateProductosList(100);
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 100
        };
        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList));

        // Act - Simular múltiples usuarios cargando datos simultáneamente
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, usuarios).Select(async _ =>
        {
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

            await Task.WhenAll(
                comandasViewModel.LoadComandasAsync(),
                mesasViewModel.LoadMesasAsync(),
                productosViewModel.LoadProductosAsync()
            );

            return new { Comandas = comandasViewModel.Comandas.Count, Mesas = mesasViewModel.Mesas.Count, Productos = productosViewModel.Productos.Count };
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar rendimiento bajo carga
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GetMaxTimeForUsers(usuarios));
        results.Should().HaveCount(usuarios);
        results.Should().AllSatisfy(r => r.Comandas.Should().Be(50));
        results.Should().AllSatisfy(r => r.Mesas.Should().Be(20));
        results.Should().AllSatisfy(r => r.Productos.Should().Be(100));
    }

    #endregion

    #region Pruebas de Picos de Tráfico

    [Fact]
    public async Task LoadTest_PicoTrafico_OperacionesCRUD()
    {
        // Arrange - Configurar operaciones CRUD bajo carga
        var comandaId = Guid.NewGuid();
        var comanda = new ComandaDto { Id = comandaId, Estado = "Pendiente" };

        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda));

        _comandasServiceMock.Setup(c => c.FinalizarComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda with { Estado = "Finalizada" }));

        // Act - Simular pico de tráfico con 200 operaciones CRUD simultáneas
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 200).Select(async i =>
        {
            var comandasViewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);

            if (i % 2 == 0)
            {
                await comandasViewModel.CambiarEstadoCommand.ExecuteAsync("En Preparación");
            }
            else
            {
                await comandasViewModel.FinalizarComandaCommand.ExecuteAsync("Efectivo");
            }

            return i;
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar rendimiento bajo pico de tráfico
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000); // Menos de 15 segundos
        results.Should().HaveCount(200);
    }

    [Fact]
    public async Task LoadTest_PicoTrafico_BusquedasSimultaneas()
    {
        // Arrange - Configurar búsquedas bajo carga
        var productos = GenerateProductosList(500);
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = 500,
            PageNumber = 1,
            PageSize = 500
        };

        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList));

        _productosServiceMock.Setup(p => p.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos.Take(10).ToList()));

        // Act - Simular 100 búsquedas simultáneas
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var productosViewModel = new ProductosViewModel(
                _productosServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object);

            await productosViewModel.LoadProductosAsync();
            productosViewModel.SearchText = $"Producto {i}";
            await productosViewModel.BuscarProductosCommand.ExecuteAsync(null);

            return productosViewModel.Productos.Count;
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar rendimiento de búsquedas
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Menos de 10 segundos
        results.Should().HaveCount(100);
    }

    #endregion

    #region Pruebas de Límites del Sistema

    [Fact]
    public async Task LoadTest_LimiteSistema_MaximoUsuariosSimultaneos()
    {
        // Arrange - Configurar para máximo número de usuarios
        var maxUsuarios = 500;
        var comandas = GenerateComandasList(100);

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular máximo número de usuarios simultáneos
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, maxUsuarios).Select(async _ =>
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

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar límites del sistema
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Menos de 30 segundos
        results.Should().HaveCount(maxUsuarios);
        results.Should().AllSatisfy(r => r.Should().Be(100));
    }

    [Fact]
    public async Task LoadTest_LimiteSistema_OperacionesIntensivas()
    {
        // Arrange - Configurar operaciones intensivas
        var comandaId = Guid.NewGuid();
        var comanda = new ComandaDto { Id = comandaId, Estado = "Pendiente" };

        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda));

        // Act - Simular operaciones intensivas (1000 operaciones)
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 1000).Select(async i =>
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

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar límites de operaciones intensivas
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // Menos de 1 minuto
        results.Should().HaveCount(1000);
    }

    #endregion

    #region Pruebas de Escalabilidad

    [Theory]
    [InlineData(1, 1000)]
    [InlineData(5, 2000)]
    [InlineData(10, 5000)]
    [InlineData(20, 10000)]
    public async Task LoadTest_Escalabilidad_DatosVsUsuarios(int usuarios, int datos)
    {
        // Arrange - Configurar datos escalables
        var comandas = GenerateComandasList(datos);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular carga escalable
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, usuarios).Select(async _ =>
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

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar escalabilidad
        var tiempoMaximo = GetMaxTimeForDataAndUsers(usuarios, datos);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(tiempoMaximo);
        results.Should().HaveCount(usuarios);
        results.Should().AllSatisfy(r => r.Should().Be(datos));
    }

    #endregion

    #region Pruebas de Memoria bajo Carga

    [Fact]
    public async Task LoadTest_Memoria_CargaProlongada()
    {
        // Arrange - Configurar para carga prolongada
        var comandas = GenerateComandasList(100);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular carga prolongada (100 ciclos)
        var memoryBefore = GC.GetTotalMemory(false);
        var stopwatch = Stopwatch.StartNew();

        for (int cycle = 0; cycle < 100; cycle++)
        {
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

            await Task.WhenAll(tasks);

            // Forzar garbage collection cada 10 ciclos
            if (cycle % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        stopwatch.Stop();
        var memoryAfter = GC.GetTotalMemory(true);

        // Assert - Verificar uso de memoria
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // Menos de 1 minuto
        var memoryIncrease = memoryAfter - memoryBefore;
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024); // Menos de 50MB de aumento
    }

    #endregion

    #region Pruebas de Concurrencia Extrema

    [Fact]
    public async Task LoadTest_ConcurrenciaExtrema_OperacionesMixtas()
    {
        // Arrange - Configurar operaciones mixtas
        var comandaId = Guid.NewGuid();
        var comanda = new ComandaDto { Id = comandaId, Estado = "Pendiente" };

        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync())
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(new List<ComandaDto> { comanda }));

        _comandasServiceMock.Setup(c => c.CambiarEstadoComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda));

        _comandasServiceMock.Setup(c => c.FinalizarComandaAsync(comandaId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comanda));

        // Act - Simular concurrencia extrema con operaciones mixtas
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();

        // 50 tareas de carga
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
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
            }));
        }

        // 50 tareas de cambio de estado
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var comandasViewModel = new ComandasViewModel(
                    _comandasServiceMock.Object,
                    _dialogServiceMock.Object,
                    _navigationServiceMock.Object,
                    _mesasServiceMock.Object,
                    _notificationServiceMock.Object,
                    _realtimeServiceMock.Object,
                    _preferencesServiceMock.Object);

                await comandasViewModel.CambiarEstadoCommand.ExecuteAsync("En Preparación");
            }));
        }

        // 50 tareas de finalización
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var comandasViewModel = new ComandasViewModel(
                    _comandasServiceMock.Object,
                    _dialogServiceMock.Object,
                    _navigationServiceMock.Object,
                    _mesasServiceMock.Object,
                    _notificationServiceMock.Object,
                    _realtimeServiceMock.Object,
                    _preferencesServiceMock.Object);

                await comandasViewModel.FinalizarComandaCommand.ExecuteAsync("Efectivo");
            }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar concurrencia extrema
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Menos de 30 segundos
        tasks.Should().HaveCount(150);
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

    private List<MesaDto> GenerateMesasList(int count)
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

    private int GetMaxTimeForUsers(int usuarios)
    {
        return usuarios switch
        {
            <= 10 => 5000,    // 5 segundos para 10 usuarios
            <= 25 => 10000,   // 10 segundos para 25 usuarios
            <= 50 => 15000,   // 15 segundos para 50 usuarios
            _ => 30000        // 30 segundos para 100+ usuarios
        };
    }

    private int GetMaxTimeForDataAndUsers(int usuarios, int datos)
    {
        var baseTime = usuarios * 100; // 100ms por usuario
        var dataTime = datos / 100; // 1ms por 100 datos
        return Math.Max(baseTime + dataTime, 5000); // Mínimo 5 segundos
    }

    #endregion
}
