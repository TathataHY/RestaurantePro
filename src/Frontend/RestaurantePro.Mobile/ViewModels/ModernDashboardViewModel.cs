using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.ViewModels;

public class ModernDashboardViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private readonly IDashboardService _dashboardService;
    private bool _isLoading;
    private ObservableCollection<RestaurantePro.Mobile.Core.Models.DTOs.OrderItem> _recentOrders;
    
    // Propiedades para datos reales
    private decimal _todaySales;
    private decimal _salesChangePercentage;
    private int _activeOrdersCount;
    private int _pendingOrdersCount;
    private EstadoMesasDto _tableStatus;

    public bool IsLoading
    {
        get => _isLoading;
        set 
        { 
            SetProperty(ref _isLoading, value);
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }

    public bool IsNotLoading => !IsLoading;

    public ObservableCollection<RestaurantePro.Mobile.Core.Models.DTOs.OrderItem> RecentOrders
    {
        get => _recentOrders;
        set => SetProperty(ref _recentOrders, value);
    }
    
    public decimal TodaySales
    {
        get => _todaySales;
        set => SetProperty(ref _todaySales, value);
    }
    
    public decimal SalesChangePercentage
    {
        get => _salesChangePercentage;
        set => SetProperty(ref _salesChangePercentage, value);
    }
    
    public int ActiveOrdersCount
    {
        get => _activeOrdersCount;
        set => SetProperty(ref _activeOrdersCount, value);
    }
    
    public int PendingOrdersCount
    {
        get => _pendingOrdersCount;
        set => SetProperty(ref _pendingOrdersCount, value);
    }

    public EstadoMesasDto TableStatus
    {
        get => _tableStatus;
        set 
        { 
            SetProperty(ref _tableStatus, value);
            OnPropertyChanged(nameof(MesasOcupadasDisplay));
            OnPropertyChanged(nameof(PorcentajeOcupacionDisplay));
        }
    }

    public string MesasOcupadasDisplay
    {
        get
        {
            if (TableStatus?.Estadisticas != null)
            {
                return $"{TableStatus.Estadisticas.MesasOcupadas}/{TableStatus.TotalMesas}";
            }
            return "0/0";
        }
    }

    public string PorcentajeOcupacionDisplay
    {
        get
        {
            if (TableStatus?.Estadisticas != null)
            {
                return $"{TableStatus.Estadisticas.PorcentajeOcupacion:F0}% ocupación";
            }
            return "0% ocupación";
        }
    }

    public ICommand CreateOrderCommand { get; }
    public ICommand AssignTableCommand { get; }
    public ICommand ViewInventoryCommand { get; }
    public ICommand ViewReportsCommand { get; }
    public ICommand LogoutCommand { get; }

    public ModernDashboardViewModel(IAuthService authService, IDashboardService dashboardService)
    {
        _authService = authService;
        _dashboardService = dashboardService;
        CreateOrderCommand = new Command(async () => await CreateOrder());
        AssignTableCommand = new Command(async () => await AssignTable());
        ViewInventoryCommand = new Command(async () => await ViewInventory());
        ViewReportsCommand = new Command(async () => await ViewReports());
        LogoutCommand = new Command(async () => await Logout());

        LoadData();
    }

    /// <summary>
    /// Método público para refrescar datos del dashboard
    /// </summary>
    public void RefreshData()
    {
        System.Diagnostics.Debug.WriteLine("🔄 [ModernDashboardViewModel] RefreshData solicitado");
        LoadData();
    }

    public async void LoadData()
    {
        IsLoading = true;

        try
        {
            // Cargar datos reales del dashboard
            var todaySalesTask = _dashboardService.GetTodaySalesAsync();
            var salesChangeTask = _dashboardService.GetSalesChangePercentageAsync();
            var activeOrdersTask = _dashboardService.GetActiveOrdersCountAsync();
            var pendingOrdersTask = _dashboardService.GetPendingOrdersCountAsync();
            var recentOrdersTask = _dashboardService.GetRecentOrdersAsync();
            var tableStatusTask = _dashboardService.GetTableStatusAsync();

            // Esperar a que todas las tareas se completen
            await Task.WhenAll(todaySalesTask, salesChangeTask, activeOrdersTask, pendingOrdersTask, recentOrdersTask, tableStatusTask);

            // Asignar los datos obtenidos
            TodaySales = await todaySalesTask;
            SalesChangePercentage = await salesChangeTask;
            ActiveOrdersCount = await activeOrdersTask;
            PendingOrdersCount = await pendingOrdersTask;
            TableStatus = await tableStatusTask;
            
            var recentOrders = await recentOrdersTask;
            System.Diagnostics.Debug.WriteLine($"🔍 [ModernDashboardViewModel] Comandas recientes obtenidas: {recentOrders?.Count ?? 0}");
            RecentOrders = new ObservableCollection<RestaurantePro.Mobile.Core.Models.DTOs.OrderItem>(recentOrders);
            System.Diagnostics.Debug.WriteLine($"🔍 [ModernDashboardViewModel] RecentOrders.Count: {RecentOrders.Count}");
        }
        catch (Exception ex)
        {
            // En caso de error, mostrar datos por defecto
            TodaySales = 0m;
            SalesChangePercentage = 0m;
            ActiveOrdersCount = 0;
            PendingOrdersCount = 0;
            TableStatus = new EstadoMesasDto();
            RecentOrders = new ObservableCollection<RestaurantePro.Mobile.Core.Models.DTOs.OrderItem>();
            
            // Log del error (en una app real, usarías un logger)
            System.Diagnostics.Debug.WriteLine($"Error cargando datos del dashboard: {ex.Message}");
        }

        IsLoading = false;
    }

    /// <summary>
    /// Filtra las comandas por estado seleccionado en el tab
    /// </summary>
    public async Task FilterOrdersByStatusAsync(string status)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ModernDashboardViewModel] Filtrando comandas por estado: {status}");
            
            if (status == "Todas")
            {
                // Mostrar todas las comandas recientes (sin filtro)
                var allOrders = await _dashboardService.GetRecentOrdersAsync();
                RecentOrders = new ObservableCollection<RestaurantePro.Mobile.Core.Models.DTOs.OrderItem>(allOrders);
                System.Diagnostics.Debug.WriteLine($"🔍 [ModernDashboardViewModel] Mostrando todas las comandas: {RecentOrders.Count}");
            }
            else
            {
                // Filtrar por estado específico
                var filteredOrders = await _dashboardService.GetOrdersByStatusAsync(status);
                RecentOrders = new ObservableCollection<RestaurantePro.Mobile.Core.Models.DTOs.OrderItem>(filteredOrders);
                System.Diagnostics.Debug.WriteLine($"🔍 [ModernDashboardViewModel] Comandas filtradas por '{status}': {RecentOrders.Count}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ModernDashboardViewModel] Error filtrando comandas: {ex.Message}");
        }
    }

    private async Task CreateOrder()
    {
        try
        {
            // Navegar a la página de Comandas
            await Shell.Current.GoToAsync("//comandas");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al navegar a comandas: {ex.Message}", "OK");
        }
    }

    private async Task AssignTable()
    {
        try
        {
            // Navegar a la página de Mesas
            await Shell.Current.GoToAsync("//mesas");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al navegar a mesas: {ex.Message}", "OK");
        }
    }

    private async Task ViewInventory()
    {
        try
        {
            // Navegar a la página de Productos (inventario)
            await Shell.Current.GoToAsync("//productos");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al navegar a productos: {ex.Message}", "OK");
        }
    }

    private async Task ViewReports()
    {
        try
        {
            // Navegar a la página de Analytics (reportes)
            await Shell.Current.GoToAsync("//analytics");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al navegar a analytics: {ex.Message}", "OK");
        }
    }

    private async Task Logout()
    {
        try
        {
            // Mostrar confirmación
            var result = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión", 
                "¿Estás seguro de que quieres cerrar sesión?", 
                "Sí", 
                "Cancelar");

            if (result)
            {
                // Limpiar la sesión usando el AuthService
                await _authService.LogoutAsync();
                
                // Navegar de vuelta a la página de login
                await Shell.Current.GoToAsync("//login");
                
                // Mostrar mensaje de confirmación
                await Application.Current.MainPage.DisplayAlert(
                    "Sesión Cerrada", 
                    "Has cerrado sesión exitosamente", 
                    "OK");
            }

        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", 
                $"Error al cerrar sesión: {ex.Message}", 
                "OK");
        }
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}

public class OrderItem
{
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public string Status { get; set; }
    public decimal Total { get; set; }
    public Color StatusColor { get; set; }
} 
