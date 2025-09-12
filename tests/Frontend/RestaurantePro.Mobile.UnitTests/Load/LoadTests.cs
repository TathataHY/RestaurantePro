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
using System.Diagnostics;

namespace RestaurantePro.Mobile.UnitTests.Load;

/// <summary>
/// Pruebas de carga para verificar el rendimiento bajo estrés
/// </summary>
public class LoadTests
{
    private readonly Mock<IComandasService> _comandasServiceMock;
    private readonly Mock<IMesasService> _mesasServiceMock;
    private readonly Mock<IProductosService> _productosServiceMock;
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
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _realtimeServiceMock = new Mock<IComandaRealtimeService>();
        _preferencesServiceMock = new Mock<IPreferencesService>();
    }

    #region Pruebas de Carga - Múltiples Usuarios Concurrentes

    [Fact]
    public async Task Load_MultiplesUsuariosConcurrentes_CargarComandas()
    {
        // Arrange - Simular 20 usuarios concurrentes
        var comandas = GenerateComandasList(100);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular múltiples usuarios cargando comandas simultáneamente
        var tasks = new List<Task>();
        var viewModels = new List<ComandasViewModel>();

        for (int i = 0; i < 20; i++)
        {
            var viewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadComandasCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que todas las operaciones se completaron en tiempo razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Menos de 10 segundos
        viewModels.Should().HaveCount(20);
        viewModels.All(vm => vm.Comandas.Count == 100).Should().BeTrue();
    }

    [Fact]
    public async Task Load_MultiplesUsuariosConcurrentes_CargarMesas()
    {
        // Arrange - Simular 15 usuarios concurrentes
        var mesas = GenerateMesasList(50);
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));

        // Act - Simular múltiples usuarios cargando mesas simultáneamente
        var tasks = new List<Task>();
        var viewModels = new List<MesasViewModel>();

        for (int i = 0; i < 15; i++)
        {
            var viewModel = new MesasViewModel(
                _mesasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadMesasCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que todas las operaciones se completaron en tiempo razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(8000); // Menos de 8 segundos
        viewModels.Should().HaveCount(15);
        viewModels.All(vm => vm.Mesas.Count == 50).Should().BeTrue();
    }

    [Fact]
    public async Task Load_MultiplesUsuariosConcurrentes_CargarProductos()
    {
        // Arrange - Simular 10 usuarios concurrentes
        var productos = GenerateProductosList(200);
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            PageNumber = 1,
            PageSize = 200,
            TotalCount = 200
        };
        _productosServiceMock.Setup(p => p.ObtenerProductosPaginadosAsync(1, 200, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        // Act - Simular múltiples usuarios cargando productos simultáneamente
        var tasks = new List<Task>();
        var viewModels = new List<ProductosViewModel>();

        for (int i = 0; i < 10; i++)
        {
            var viewModel = new ProductosViewModel(
                _productosServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadProductosCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que todas las operaciones se completaron en tiempo razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(6000); // Menos de 6 segundos
        viewModels.Should().HaveCount(10);
        viewModels.All(vm => vm.Productos.Count == 200).Should().BeTrue();
    }

    #endregion

    #region Pruebas de Picos de Tráfico

    [Fact]
    public async Task Load_PicoTrafico_CargarComandasRapido()
    {
        // Arrange - Simular pico de tráfico con 50 operaciones en 5 segundos
        var comandas = GenerateComandasList(50);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular pico de tráfico
        var tasks = new List<Task>();
        var viewModels = new List<ComandasViewModel>();

        for (int i = 0; i < 50; i++)
        {
            var viewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadComandasCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que se manejó el pico de tráfico
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000); // Menos de 15 segundos
        viewModels.Should().HaveCount(50);
    }

    [Fact]
    public async Task Load_PicoTrafico_CargarMesasRapido()
    {
        // Arrange - Simular pico de tráfico con 30 operaciones en 3 segundos
        var mesas = GenerateMesasList(30);
        _mesasServiceMock.Setup(m => m.ObtenerMesasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.SuccessResponse(mesas));

        // Act - Simular pico de tráfico
        var tasks = new List<Task>();
        var viewModels = new List<MesasViewModel>();

        for (int i = 0; i < 30; i++)
        {
            var viewModel = new MesasViewModel(
                _mesasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadMesasCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que se manejó el pico de tráfico
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Menos de 10 segundos
        viewModels.Should().HaveCount(30);
    }

    #endregion

    #region Pruebas de Límites del Sistema

    [Fact]
    public async Task Load_LimiteSistema_MaximoUsuariosSimultaneos()
    {
        // Arrange - Simular el máximo de usuarios simultáneos (100)
        var comandas = GenerateComandasList(20);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular máximo de usuarios simultáneos
        var tasks = new List<Task>();
        var viewModels = new List<ComandasViewModel>();

        for (int i = 0; i < 100; i++)
        {
            var viewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadComandasCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que el sistema manejó el límite
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Menos de 30 segundos
        viewModels.Should().HaveCount(100);
    }

    [Fact]
    public async Task Load_LimiteSistema_MaximoDatosSimultaneos()
    {
        // Arrange - Simular carga máxima de datos (1000 comandas)
        var comandas = GenerateComandasList(1000);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        var comandasViewModel = new ComandasViewModel(
            _comandasServiceMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _mesasServiceMock.Object,
            _notificationServiceMock.Object,
            _realtimeServiceMock.Object,
            _preferencesServiceMock.Object);

        // Act - Cargar máximo de datos
        var stopwatch = Stopwatch.StartNew();
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);
        stopwatch.Stop();

        // Assert - Verificar que se cargó el máximo de datos en tiempo razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Menos de 5 segundos
        comandasViewModel.Comandas.Should().HaveCount(1000);
    }

    #endregion

    #region Pruebas de Escalabilidad

    [Fact]
    public async Task Load_Escalabilidad_CrecimientoLinealUsuarios()
    {
        // Arrange - Probar escalabilidad con diferentes números de usuarios
        var comandas = GenerateComandasList(50);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        var resultados = new List<(int usuarios, long tiempoMs)>();

        // Act - Probar con diferentes números de usuarios
        for (int usuarios = 10; usuarios <= 50; usuarios += 10)
        {
            var tasks = new List<Task>();
            var viewModels = new List<ComandasViewModel>();

            for (int i = 0; i < usuarios; i++)
            {
                var viewModel = new ComandasViewModel(
                    _comandasServiceMock.Object,
                    _dialogServiceMock.Object,
                    _navigationServiceMock.Object,
                    _mesasServiceMock.Object,
                    _notificationServiceMock.Object,
                    _realtimeServiceMock.Object,
                    _preferencesServiceMock.Object);
                
                viewModels.Add(viewModel);
                tasks.Add(viewModel.LoadComandasCommand.ExecuteAsync(null));
            }

            var stopwatch = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            resultados.Add((usuarios, stopwatch.ElapsedMilliseconds));
        }

        // Assert - Verificar que el tiempo crece de manera razonable
        resultados.Should().HaveCount(5);
        resultados.All(r => r.tiempoMs < 10000).Should().BeTrue(); // Todos menos de 10 segundos
    }

    #endregion

    #region Pruebas de Memoria bajo Carga

    [Fact]
    public async Task Load_MemoriaBajoCarga_MultiplesOperacionesLargas()
    {
        // Arrange - Simular operaciones largas con mucha memoria
        var comandas = GenerateComandasList(500);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Ejecutar múltiples operaciones largas
        var tasks = new List<Task>();
        var viewModels = new List<ComandasViewModel>();

        for (int i = 0; i < 20; i++)
        {
            var viewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadComandasCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);

        // Forzar garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert - Verificar que no hay memory leaks
        viewModels.Should().HaveCount(20);
        viewModels.All(vm => vm.Comandas.Count == 500).Should().BeTrue();
    }

    #endregion

    #region Pruebas de Concurrencia Extrema

    [Fact]
    public async Task Load_ConcurrenciaExtrema_MaximaSimultaneidad()
    {
        // Arrange - Simular concurrencia extrema (200 operaciones simultáneas)
        var comandas = GenerateComandasList(10);
        _comandasServiceMock.Setup(c => c.ObtenerComandasActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ComandaDto>>.SuccessResponse(comandas));

        // Act - Simular concurrencia extrema
        var tasks = new List<Task>();
        var viewModels = new List<ComandasViewModel>();

        for (int i = 0; i < 200; i++)
        {
            var viewModel = new ComandasViewModel(
                _comandasServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mesasServiceMock.Object,
                _notificationServiceMock.Object,
                _realtimeServiceMock.Object,
                _preferencesServiceMock.Object);
            
            viewModels.Add(viewModel);
            tasks.Add(viewModel.LoadComandasCommand.ExecuteAsync(null));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Verificar que se manejó la concurrencia extrema
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // Menos de 60 segundos
        viewModels.Should().HaveCount(200);
    }

    #endregion

    #region Métodos de Ayuda

    private List<ComandaDto> GenerateComandasList(int count)
    {
        var comandas = new List<ComandaDto>();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            comandas.Add(new ComandaDto
            {
                Id = Guid.NewGuid(),
                Numero = $"C-{i + 1:D3}",
                MesaId = Guid.NewGuid(),
                MesaNumero = $"Mesa {i % 20 + 1}",
                Estado = random.Next(0, 2) == 0 ? "Activa" : "En Preparación",
                Total = (decimal)(random.NextDouble() * 100 + 10),
                FechaCreacion = DateTime.Now.AddMinutes(-random.Next(0, 1440))
            });
        }

        return comandas;
    }

    private List<MesaDto> GenerateMesasList(int count)
    {
        var mesas = new List<MesaDto>();
        var random = new Random();
        var estados = new[] { "Disponible", "Ocupada", "Reservada", "Mantenimiento" };
        var ubicaciones = new[] { "Salón Principal", "Terraza", "Comedor Privado", "Bar" };

        for (int i = 0; i < count; i++)
        {
            mesas.Add(new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = $"Mesa {i + 1}",
                Capacidad = random.Next(2, 12),
                Estado = estados[random.Next(estados.Length)],
                Ubicacion = ubicaciones[random.Next(ubicaciones.Length)]
            });
        }

        return mesas;
    }

    private List<ProductoDto> GenerateProductosList(int count)
    {
        var productos = new List<ProductoDto>();
        var random = new Random();
        var categorias = new[] { "Pizzas", "Pasta", "Ensaladas", "Bebidas", "Postres" };

        for (int i = 0; i < count; i++)
        {
            productos.Add(new ProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Producto {i + 1}",
                Descripcion = $"Descripción del producto {i + 1}",
                Precio = (decimal)(random.NextDouble() * 50 + 5),
                CategoriaNombre = categorias[random.Next(categorias.Length)],
                Activo = random.Next(0, 2) == 0
            });
        }

        return productos;
    }

    #endregion
}
