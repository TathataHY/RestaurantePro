using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RestaurantePro.Mobile.ViewModels;

public class ModernDashboardViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private ObservableCollection<OrderItem> _recentOrders;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsNotLoading => !IsLoading;

    public ObservableCollection<OrderItem> RecentOrders
    {
        get => _recentOrders;
        set => SetProperty(ref _recentOrders, value);
    }

    public ICommand CreateOrderCommand { get; }
    public ICommand AssignTableCommand { get; }
    public ICommand ViewInventoryCommand { get; }
    public ICommand ViewReportsCommand { get; }

    public ModernDashboardViewModel()
    {
        CreateOrderCommand = new Command(async () => await CreateOrder());
        AssignTableCommand = new Command(async () => await AssignTable());
        ViewInventoryCommand = new Command(async () => await ViewInventory());
        ViewReportsCommand = new Command(async () => await ViewReports());

        LoadData();
    }

    private async void LoadData()
    {
        IsLoading = true;

        // Simular carga de datos
        await Task.Delay(2000);

        RecentOrders = new ObservableCollection<OrderItem>
        {
            new OrderItem 
            { 
                OrderNumber = "ORD-001", 
                CustomerName = "Juan Pérez", 
                Status = "En Progreso", 
                Total = 45.50m,
                StatusColor = Color.FromArgb("#3498DB")
            },
            new OrderItem 
            { 
                OrderNumber = "ORD-002", 
                CustomerName = "María García", 
                Status = "Pendiente", 
                Total = 32.75m,
                StatusColor = Color.FromArgb("#F39C12")
            },
            new OrderItem 
            { 
                OrderNumber = "ORD-003", 
                CustomerName = "Carlos López", 
                Status = "Lista", 
                Total = 28.90m,
                StatusColor = Color.FromArgb("#27AE60")
            },
            new OrderItem 
            { 
                OrderNumber = "ORD-004", 
                CustomerName = "Ana Martínez", 
                Status = "En Progreso", 
                Total = 67.25m,
                StatusColor = Color.FromArgb("#3498DB")
            }
        };

        IsLoading = false;
    }

    private async Task CreateOrder()
    {
        // Implementar navegación a crear comanda
        await Application.Current.MainPage.DisplayAlert("Acción", "Crear nueva comanda", "OK");
    }

    private async Task AssignTable()
    {
        // Implementar navegación a asignar mesa
        await Application.Current.MainPage.DisplayAlert("Acción", "Asignar mesa", "OK");
    }

    private async Task ViewInventory()
    {
        // Implementar navegación a inventario
        await Application.Current.MainPage.DisplayAlert("Acción", "Ver inventario", "OK");
    }

    private async Task ViewReports()
    {
        // Implementar navegación a reportes
        await Application.Current.MainPage.DisplayAlert("Acción", "Ver reportes", "OK");
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