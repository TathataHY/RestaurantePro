using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RestaurantePro.Mobile.Services;

namespace RestaurantePro.Mobile.ViewModels;

/// <summary>
/// ViewModel para el monitor de performance - V4
/// </summary>
public class PerformanceMonitorViewModel : INotifyPropertyChanged
{
    private readonly PerformanceService _performanceService;
    private readonly CacheService _cacheService;
    private readonly LazyLoadingService _lazyLoadingService;
    private readonly Timer _updateTimer;

    private MemoryInfo _memoryInfo;
    private CacheStatistics _cacheStats;
    private LazyLoadingStatistics _lazyLoadingStats;
    private List<OperationMetrics> _slowOperations;

    public PerformanceMonitorViewModel(
        PerformanceService performanceService,
        CacheService cacheService,
        LazyLoadingService lazyLoadingService)
    {
        _performanceService = performanceService;
        _cacheService = cacheService;
        _lazyLoadingService = lazyLoadingService;

        // Inicializar propiedades
        _memoryInfo = new MemoryInfo();
        _cacheStats = new CacheStatistics();
        _lazyLoadingStats = new LazyLoadingStatistics();
        _slowOperations = new List<OperationMetrics>();

        // Configurar timer para actualizaciones cada 2 segundos
        _updateTimer = new Timer(UpdateMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

        // Configurar comandos
        ClearCacheCommand = new Command(async () => await ClearCacheAsync());
        ClearLazyLoadingCommand = new Command(async () => await ClearLazyLoadingAsync());
        GenerateReportCommand = new Command(async () => await GenerateReportAsync());

        // Cargar métricas iniciales
        UpdateMetrics(null);
    }

    #region Properties

    public MemoryInfo MemoryInfo
    {
        get => _memoryInfo;
        set => SetProperty(ref _memoryInfo, value);
    }

    public CacheStatistics CacheStats
    {
        get => _cacheStats;
        set => SetProperty(ref _cacheStats, value);
    }

    public LazyLoadingStatistics LazyLoadingStats
    {
        get => _lazyLoadingStats;
        set => SetProperty(ref _lazyLoadingStats, value);
    }

    public List<OperationMetrics> SlowOperations
    {
        get => _slowOperations;
        set => SetProperty(ref _slowOperations, value);
    }

    #endregion

    #region Commands

    public ICommand ClearCacheCommand { get; }
    public ICommand ClearLazyLoadingCommand { get; }
    public ICommand GenerateReportCommand { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Actualiza las métricas de performance
    /// </summary>
    private void UpdateMetrics(object? state)
    {
        try
        {
            // Actualizar información de memoria
            MemoryInfo = _performanceService.GetMemoryInfo();

            // Actualizar estadísticas de cache
            CacheStats = _cacheService.GetStatistics();

            // Actualizar estadísticas de lazy loading
            LazyLoadingStats = _lazyLoadingService.GetStatistics();

            // Actualizar operaciones lentas
            var report = _performanceService.GenerateReport();
            SlowOperations = report.Operations
                .Where(op => op.AverageTime > 100) // Operaciones que toman más de 100ms
                .OrderByDescending(op => op.AverageTime)
                .Take(10) // Top 10 operaciones más lentas
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating performance metrics: {ex.Message}");
        }
    }

    /// <summary>
    /// Limpia el cache
    /// </summary>
    private async Task ClearCacheAsync()
    {
        try
        {
            _cacheService.Clear();
            await Application.Current.MainPage.DisplayAlert("Cache Limpiado", 
                "El cache ha sido limpiado exitosamente.", "OK");
            
            // Actualizar métricas
            UpdateMetrics(null);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al limpiar cache: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Limpia el lazy loading
    /// </summary>
    private async Task ClearLazyLoadingAsync()
    {
        try
        {
            _lazyLoadingService.Clear();
            await Application.Current.MainPage.DisplayAlert("Lazy Loading Limpiado", 
                "El lazy loading ha sido limpiado exitosamente.", "OK");
            
            // Actualizar métricas
            UpdateMetrics(null);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al limpiar lazy loading: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Genera un reporte de performance
    /// </summary>
    private async Task GenerateReportAsync()
    {
        try
        {
            var report = _performanceService.GenerateReport();
            
            var reportText = $"Reporte de Performance - {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}\n\n" +
                           $"Memoria:\n" +
                           $"  Working Set: {report.MemoryInfo.WorkingSet / 1024 / 1024:F1} MB\n" +
                           $"  Private Memory: {report.MemoryInfo.PrivateMemory / 1024 / 1024:F1} MB\n" +
                           $"  Virtual Memory: {report.MemoryInfo.VirtualMemory / 1024 / 1024:F1} MB\n\n" +
                           $"Operaciones ({report.Operations.Count}):\n";

            foreach (var operation in report.Operations.OrderByDescending(op => op.AverageTime))
            {
                reportText += $"  {operation.Name}: {operation.AverageTime:F0}ms (min: {operation.MinTime}ms, max: {operation.MaxTime}ms, count: {operation.Count})\n";
            }

            await Application.Current.MainPage.DisplayAlert("Reporte de Performance", reportText, "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al generar reporte: {ex.Message}", "OK");
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _updateTimer?.Dispose();
    }

    #endregion
} 