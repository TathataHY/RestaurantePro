# Arquitectura de la Aplicación Móvil - RestaurantePro

## Visión General

La aplicación móvil de RestaurantePro está desarrollada con .NET MAUI (Multi-platform App UI), permitiendo una única base de código para desplegar en iOS, Android y Windows. Esta aplicación es utilizada principalmente por meseros y personal de cocina para gestionar comandas, mesas y pedidos en tiempo real.

## Patrón MVVM

Se implementa el patrón MVVM (Model-View-ViewModel) utilizando CommunityToolkit.Mvvm para lograr una clara separación de responsabilidades:

![Diagrama MVVM](../diagramas/mvvm-pattern.png)

### Models

Representan los datos y la lógica de negocio:

```csharp
public class Mesa
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; }
    public string Ubicacion { get; set; }
    public bool Activa { get; set; }
}

public enum EstadoMesa
{
    Libre,
    Ocupada,
    Reservada
}
```

### ViewModels

Contienen la lógica de presentación y exponen propiedades y comandos para su uso en las vistas:

```csharp
public partial class MesasViewModel : ObservableObject
{
    private readonly IMesaService _mesaService;
    private readonly INavigationService _navigationService;
    
    public MesasViewModel(IMesaService mesaService, INavigationService navigationService)
    {
        _mesaService = mesaService;
        _navigationService = navigationService;
    }
    
    [ObservableProperty]
    private ObservableCollection<Mesa> _mesas = new();
    
    [ObservableProperty]
    private bool _isBusy;
    
    [RelayCommand]
    private async Task LoadMesasAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            var mesasData = await _mesaService.GetMesasAsync();
            Mesas = new ObservableCollection<Mesa>(mesasData);
        }
        catch (Exception ex)
        {
            // Manejo de errores
            Debug.WriteLine($"Error cargando mesas: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudieron cargar las mesas", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task NavigateToMesaDetail(Mesa mesa)
    {
        await _navigationService.NavigateToAsync($"//mesas/detail?id={mesa.Id}");
    }
}
```

### Views

Interfaces de usuario declarativas utilizando XAML:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:RestaurantePro.Mobile.ViewModels"
             x:Class="RestaurantePro.Mobile.Views.MesasPage"
             Title="Mesas">
    
    <ContentPage.BindingContext>
        <vm:MesasViewModel />
    </ContentPage.BindingContext>
    
    <Grid RowDefinitions="Auto,*">
        <HorizontalStackLayout Spacing="10" Padding="10">
            <Button Text="Actualizar" 
                    Command="{Binding LoadMesasCommand}" 
                    IsEnabled="{Binding !IsBusy}" />
            <ActivityIndicator IsRunning="{Binding IsBusy}" IsVisible="{Binding IsBusy}" />
        </HorizontalStackLayout>
        
        <CollectionView Grid.Row="1" 
                        ItemsSource="{Binding Mesas}" 
                        SelectionMode="None">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid Padding="10" 
                          ColumnDefinitions="Auto,*,Auto" 
                          RowDefinitions="Auto,Auto">
                        <Frame BorderColor="{Binding Estado, Converter={StaticResource EstadoColorConverter}}"
                               CornerRadius="25" 
                               HeightRequest="50" 
                               WidthRequest="50">
                            <Label Text="{Binding Numero}" 
                                   VerticalOptions="Center" 
                                   HorizontalOptions="Center" />
                        </Frame>
                        
                        <Label Grid.Column="1" 
                               Text="{Binding Estado, Converter={StaticResource EstadoTextConverter}}" 
                               VerticalOptions="Center" 
                               Margin="10,0,0,0" />
                        
                        <Label Grid.Column="1" 
                               Grid.Row="1" 
                               Text="{Binding Capacidad, StringFormat='Capacidad: {0} personas'}" 
                               FontSize="Small" 
                               TextColor="Gray" 
                               Margin="10,0,0,0" />
                        
                        <Button Grid.Column="2" 
                                Text="Ver" 
                                Command="{Binding Source={RelativeSource AncestorType={x:Type vm:MesasViewModel}}, Path=NavigateToMesaDetailCommand}" 
                                CommandParameter="{Binding}" />
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

## Estructura del Proyecto

```
RestaurantePro.Mobile/
├── Models/               # Clases de modelo
├── ViewModels/           # ViewModels para MVVM
├── Views/                # Páginas XAML
├── Services/             # Servicios para lógica de negocio y datos
├── Helpers/              # Clases auxiliares
├── Converters/           # Convertidores de valor para XAML
├── Controls/             # Controles personalizados
├── Resources/            # Recursos (imágenes, estilos, etc.)
├── Platforms/            # Código específico de plataforma
└── MauiProgram.cs        # Configuración de la aplicación
```

## Navegación con Shell

Se utiliza .NET MAUI Shell para facilitar la navegación entre páginas:

```csharp
public class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Registrar rutas para navegación
        Routing.RegisterRoute("mesas/detail", typeof(MesaDetailPage));
        Routing.RegisterRoute("comandas/nueva", typeof(NuevaComandaPage));
        Routing.RegisterRoute("comandas/detail", typeof(ComandaDetailPage));
    }
}
```

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:RestaurantePro.Mobile.Views"
       x:Class="RestaurantePro.Mobile.AppShell"
       Title="RestaurantePro">
    
    <TabBar>
        <ShellContent Title="Mesas"
                      Icon="icon_table.png"
                      ContentTemplate="{DataTemplate views:MesasPage}"
                      Route="mesas" />
        
        <ShellContent Title="Comandas"
                      Icon="icon_order.png"
                      ContentTemplate="{DataTemplate views:ComandasPage}"
                      Route="comandas" />
        
        <ShellContent Title="Productos"
                      Icon="icon_product.png"
                      ContentTemplate="{DataTemplate views:ProductosPage}"
                      Route="productos" />
        
        <ShellContent Title="Perfil"
                      Icon="icon_profile.png"
                      ContentTemplate="{DataTemplate views:PerfilPage}"
                      Route="perfil" />
    </TabBar>
</Shell>
```

## Comunicación con la API

### Servicios HTTP

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ITokenService _tokenService;
    
    public ApiService(ITokenService tokenService, ISettingsService settingsService)
    {
        _tokenService = tokenService;
        _baseUrl = settingsService.ApiUrl;
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }
    
    public async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetAsync(endpoint);
    }
    
    public async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.PostAsync(endpoint, content);
    }
    
    public async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.PutAsync(endpoint, content);
    }
    
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.DeleteAsync(endpoint);
    }
    
    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
```

### Servicios Específicos

```csharp
public class MesaService : IMesaService
{
    private readonly ApiService _apiService;
    
    public MesaService(ApiService apiService)
    {
        _apiService = apiService;
    }
    
    public async Task<IEnumerable<Mesa>> GetMesasAsync()
    {
        var response = await _apiService.GetAsync("api/mesas");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<Mesa>>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    
    public async Task<Mesa> GetMesaAsync(int id)
    {
        var response = await _apiService.GetAsync($"api/mesas/{id}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Mesa>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    
    public async Task<bool> UpdateMesaEstadoAsync(int id, EstadoMesa estado)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { Estado = estado }),
            Encoding.UTF8,
            "application/json");
            
        var response = await _apiService.PutAsync($"api/mesas/{id}/estado", content);
        return response.IsSuccessStatusCode;
    }
}
```

## Almacenamiento Local con SQLite

Para operaciones offline y caché de datos:

```csharp
public class LocalDatabase
{
    private readonly SQLiteAsyncConnection _database;
    
    public LocalDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<MesaLocal>().Wait();
        _database.CreateTableAsync<ProductoLocal>().Wait();
        _database.CreateTableAsync<ComandaLocal>().Wait();
        _database.CreateTableAsync<ComandaDetalleLocal>().Wait();
    }
    
    // Métodos para mesas
    public Task<List<MesaLocal>> GetMesasAsync()
    {
        return _database.Table<MesaLocal>().ToListAsync();
    }
    
    public Task<MesaLocal> GetMesaAsync(int id)
    {
        return _database.Table<MesaLocal>()
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public Task<int> SaveMesaAsync(MesaLocal mesa)
    {
        if (mesa.Id != 0)
        {
            return _database.UpdateAsync(mesa);
        }
        else
        {
            return _database.InsertAsync(mesa);
        }
    }
    
    // Métodos para comandas
    public Task<List<ComandaLocal>> GetComandasPendientesAsync()
    {
        return _database.Table<ComandaLocal>()
            .Where(c => c.Sincronizada == false)
            .ToListAsync();
    }
    
    public async Task<int> SaveComandaConDetallesAsync(ComandaLocal comanda, List<ComandaDetalleLocal> detalles)
    {
        await _database.RunInTransactionAsync(tran => {
            _database.InsertOrReplaceWithChildrenAsync(comanda, recursive: true);
        });
        
        return comanda.Id;
    }
}
```

## Comunicación en Tiempo Real con SignalR

Para notificaciones push y actualizaciones en tiempo real:

```csharp
public class SignalRService
{
    private HubConnection _hubConnection;
    private readonly ITokenService _tokenService;
    private readonly ISettingsService _settingsService;
    
    public event Action<ComandaDto> NuevaComandaRecibida;
    public event Action<int, string> ComandaActualizada;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    
    public SignalRService(ITokenService tokenService, ISettingsService settingsService)
    {
        _tokenService = tokenService;
        _settingsService = settingsService;
    }
    
    public async Task InitializeAsync()
    {
        if (_hubConnection != null)
        {
            return;
        }
        
        var token = await _tokenService.GetTokenAsync();
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_settingsService.ApiUrl}/hubs/comandas", options => {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();
            
        _hubConnection.On<ComandaDto>("RecibirNuevaComanda", (comanda) => {
            NuevaComandaRecibida?.Invoke(comanda);
        });
        
        _hubConnection.On<int, string>("ComandaActualizada", (comandaId, estado) => {
            ComandaActualizada?.Invoke(comandaId, estado);
        });
        
        await _hubConnection.StartAsync();
    }
    
    public async Task JoinGroupAsync(string group)
    {
        if (IsConnected)
        {
            await _hubConnection.InvokeAsync("JoinGroup", group);
        }
    }
    
    public async Task EnviarNuevaComandaAsync(ComandaDto comanda)
    {
        if (IsConnected)
        {
            await _hubConnection.InvokeAsync("NuevaComanda", comanda);
        }
    }
    
    public async Task ActualizarEstadoComandaAsync(int comandaId, string estado)
    {
        if (IsConnected)
        {
            await _hubConnection.InvokeAsync("ActualizarEstadoComanda", comandaId, estado);
        }
    }
    
    public async Task DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}
```

## Autenticación y Seguridad

### Servicio de Tokens

```csharp
public class TokenService : ITokenService
{
    private readonly ISecureStorage _secureStorage;
    private readonly ApiService _apiService;
    private readonly ISettingsService _settingsService;
    
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    
    public TokenService(ISecureStorage secureStorage, ApiService apiService, ISettingsService settingsService)
    {
        _secureStorage = secureStorage;
        _apiService = apiService;
        _settingsService = settingsService;
    }
    
    public async Task<string> GetTokenAsync()
    {
        var token = await _secureStorage.GetAsync(TokenKey);
        
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        
        // Validar si el token ha expirado
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expiry = jwtToken.ValidTo;
        if (expiry < DateTime.UtcNow)
        {
            // Token expirado, intentar renovarlo
            return await RefreshTokenAsync();
        }
        
        return token;
    }
    
    public async Task SaveTokensAsync(string token, string refreshToken)
    {
        await _secureStorage.SetAsync(TokenKey, token);
        await _secureStorage.SetAsync(RefreshTokenKey, refreshToken);
    }
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { username, password }),
            Encoding.UTF8,
            "application/json");
            
        var response = await _apiService.PostAsync("api/auth/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
            await SaveTokensAsync(authResponse.Token, authResponse.RefreshToken);
            return true;
        }
        
        return false;
    }
    
    public async Task LogoutAsync()
    {
        _secureStorage.Remove(TokenKey);
        _secureStorage.Remove(RefreshTokenKey);
    }
    
    private async Task<string> RefreshTokenAsync()
    {
        var refreshToken = await _secureStorage.GetAsync(RefreshTokenKey);
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return null;
        }
        
        var content = new StringContent(
            JsonSerializer.Serialize(new { refreshToken }),
            Encoding.UTF8,
            "application/json");
            
        var response = await _apiService.PostAsync("api/auth/refresh", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
            await SaveTokensAsync(authResponse.Token, authResponse.RefreshToken);
            return authResponse.Token;
        }
        
        // Si falla la renovación, limpiar tokens y forzar nuevo login
        await LogoutAsync();
        return null;
    }
}
```

## Manejo de Errores y Conectividad

```csharp
public class ConnectivityService : IConnectivityService
{
    private readonly IConnectivity _connectivity;
    
    public ConnectivityService()
    {
        _connectivity = NetworkAccess.Internet;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }
    
    public bool IsConnected => _connectivity.NetworkAccess == NetworkAccess.Internet;
    
    public event Action<bool> ConnectivityChanged;
    
    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        ConnectivityChanged?.Invoke(e.NetworkAccess == NetworkAccess.Internet);
    }
}
```

```csharp
public class ApiExceptionHandler
{
    private readonly IConnectivityService _connectivityService;
    
    public ApiExceptionHandler(IConnectivityService connectivityService)
    {
        _connectivityService = connectivityService;
    }
    
    public async Task<bool> HandleExceptionAsync(Exception ex)
    {
        if (!_connectivityService.IsConnected)
        {
            await Shell.Current.DisplayAlert("Sin conexión", 
                "No hay conexión a Internet. Por favor verifica tu conexión e intenta nuevamente.", 
                "OK");
            return true;
        }
        
        if (ex is HttpRequestException httpEx)
        {
            switch (httpEx.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    // Manejar error de autenticación
                    await Shell.Current.GoToAsync("//login");
                    return true;
                
                case System.Net.HttpStatusCode.NotFound:
                    await Shell.Current.DisplayAlert("Recurso no encontrado", 
                        "El recurso solicitado no existe o fue eliminado.", 
                        "OK");
                    return true;
                
                case System.Net.HttpStatusCode.BadRequest:
                    // Intentar extraer errores de validación
                    if (httpEx.Data.Contains("ValidationErrors"))
                    {
                        var errors = httpEx.Data["ValidationErrors"] as Dictionary<string, string[]>;
                        var errorMessages = errors.SelectMany(e => e.Value).ToList();
                        await Shell.Current.DisplayAlert("Error de validación",
                            string.Join("\n", errorMessages),
                            "OK");
                        return true;
                    }
                    break;
            }
        }
        
        // Error genérico
        Debug.WriteLine($"Error en API: {ex.Message}");
        await Shell.Current.DisplayAlert("Error", 
            "Ocurrió un error al procesar la solicitud. Por favor intenta nuevamente.", 
            "OK");
        return true;
    }
}
```

## Inyección de Dependencias

Configuración de servicios en MauiProgram.cs:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
            
        // Configurar base de datos local
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "restaurantepro.db3");
        builder.Services.AddSingleton(new LocalDatabase(dbPath));
        
        // Servicios de la aplicación
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<ApiExceptionHandler>();
        builder.Services.AddSingleton<SignalRService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        
        // Servicios de negocio
        builder.Services.AddSingleton<IMesaService, MesaService>();
        builder.Services.AddSingleton<IComandaService, ComandaService>();
        builder.Services.AddSingleton<IProductoService, ProductoService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        
        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MesasViewModel>();
        builder.Services.AddTransient<MesaDetailViewModel>();
        builder.Services.AddTransient<NuevaComandaViewModel>();
        builder.Services.AddTransient<ComandasViewModel>();
        builder.Services.AddTransient<ComandaDetailViewModel>();
        builder.Services.AddTransient<ProductosViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        
        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MesasPage>();
        builder.Services.AddTransient<MesaDetailPage>();
        builder.Services.AddTransient<NuevaComandaPage>();
        builder.Services.AddTransient<ComandasPage>();
        builder.Services.AddTransient<ComandaDetailPage>();
        builder.Services.AddTransient<ProductosPage>();
        builder.Services.AddTransient<PerfilPage>();
        
        return builder.Build();
    }
}
```

## Código Específico de Plataforma

Ejemplo de implementación específica para notificaciones en Android:

```csharp
#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace RestaurantePro.Mobile.Platforms.Android.Services
{
    [Service]
    public class NotificationService : Service
    {
        private const string ChannelId = "restaurantepro_channel";
        private const int NotificationId = 100;
        
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();
            
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");
            
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.AddFlags(ActivityFlags.ClearTop);
            
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            
            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.notification_icon)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .Build();
                
            StartForeground(NotificationId, notification);
            
            return StartCommandResult.Sticky;
        }
        
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }
            
            var channelName = "RestaurantePro";
            var channelDescription = "Notificaciones de RestaurantePro";
            var importance = NotificationImportance.High;
            
            var channel = new NotificationChannel(ChannelId, channelName, importance)
            {
                Description = channelDescription
            };
            
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
#endif
```

## Interfaz de Usuario Personalizada

Temas y estilos globales:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <!-- Colores -->
    <Color x:Key="Primary">#512DA8</Color>
    <Color x:Key="PrimaryDark">#311B92</Color>
    <Color x:Key="Accent">#FF4081</Color>
    <Color x:Key="LightPrimary">#D1C4E9</Color>
    <Color x:Key="TextPrimary">#212121</Color>
    <Color x:Key="TextSecondary">#757575</Color>
    <Color x:Key="DividerColor">#BDBDBD</Color>
    
    <!-- Estados de mesas -->
    <Color x:Key="MesaLibre">#4CAF50</Color>
    <Color x:Key="MesaOcupada">#F44336</Color>
    <Color x:Key="MesaReservada">#FFC107</Color>
    
    <!-- Estilos de botones -->
    <Style TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="CornerRadius" Value="5" />
        <Setter Property="Padding" Value="14,10" />
    </Style>
    
    <Style x:Key="DangerButton" TargetType="Button">
        <Setter Property="BackgroundColor" Value="#F44336" />
        <Setter Property="TextColor" Value="White" />
    </Style>
    
    <Style x:Key="SuccessButton" TargetType="Button">
        <Setter Property="BackgroundColor" Value="#4CAF50" />
        <Setter Property="TextColor" Value="White" />
    </Style>
    
    <!-- Estilos de tarjetas -->
    <Style TargetType="Frame">
        <Setter Property="CornerRadius" Value="10" />
        <Setter Property="HasShadow" Value="True" />
        <Setter Property="BorderColor" Value="{StaticResource DividerColor}" />
        <Setter Property="Padding" Value="15" />
        <Setter Property="Margin" Value="10" />
    </Style>
    
    <!-- Estilos de etiquetas -->
    <Style x:Key="PageTitleLabel" TargetType="Label">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="Margin" Value="20,10" />
    </Style>
    
    <Style x:Key="SectionTitleLabel" TargetType="Label">
        <Setter Property="FontSize" Value="18" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="Margin" Value="15,10,15,5" />
    </Style>
</ResourceDictionary>
```

## Pruebas Unitarias

Ejemplo de pruebas para ViewModel:

```csharp
[TestClass]
public class MesasViewModelTests
{
    private Mock<IMesaService> _mesaServiceMock;
    private Mock<INavigationService> _navigationServiceMock;
    private MesasViewModel _viewModel;
    
    [TestInitialize]
    public void Initialize()
    {
        _mesaServiceMock = new Mock<IMesaService>();
        _navigationServiceMock = new Mock<INavigationService>();
        
        _viewModel = new MesasViewModel(_mesaServiceMock.Object, _navigationServiceMock.Object);
    }
    
    [TestMethod]
    public async Task LoadMesasCommand_ShouldUpdateMesasCollection()
    {
        // Arrange
        var mesas = new List<Mesa>
        {
            new Mesa { Id = 1, Numero = "1", Capacidad = 4, Estado = EstadoMesa.Libre },
            new Mesa { Id = 2, Numero = "2", Capacidad = 2, Estado = EstadoMesa.Ocupada }
        };
        
        _mesaServiceMock.Setup(m => m.GetMesasAsync())
            .ReturnsAsync(mesas);
            
        // Act
        await _viewModel.LoadMesasCommand.ExecuteAsync(null);
        
        // Assert
        Assert.AreEqual(2, _viewModel.Mesas.Count);
        Assert.AreEqual("1", _viewModel.Mesas[0].Numero);
        Assert.AreEqual("2", _viewModel.Mesas[1].Numero);
        Assert.IsFalse(_viewModel.IsBusy);
    }
    
    [TestMethod]
    public async Task NavigateToMesaDetailCommand_ShouldNavigateToCorrectRoute()
    {
        // Arrange
        var mesa = new Mesa { Id = 1, Numero = "1", Capacidad = 4, Estado = EstadoMesa.Libre };
        
        // Act
        await _viewModel.NavigateToMesaDetailCommand.ExecuteAsync(mesa);
        
        // Assert
        _navigationServiceMock.Verify(n => n.NavigateToAsync("//mesas/detail?id=1"), Times.Once);
    }
}
```

## Consideraciones para Publicación

### Android

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application 
        android:allowBackup="true" 
        android:icon="@mipmap/appicon" 
        android:roundIcon="@mipmap/appicon_round" 
        android:supportsRtl="true">
    </application>
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.VIBRATE" />
</manifest>
```

### iOS

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>UIDeviceFamily</key>
    <array>
        <integer>1</integer>
        <integer>2</integer>
    </array>
    <key>UISupportedInterfaceOrientations</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
    <key>UISupportedInterfaceOrientations~ipad</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
    <key>LSRequiresIPhoneOS</key>
    <true/>
    <key>UILaunchStoryboardName</key>
    <string>LaunchScreen</string>
    <key>NSLocationWhenInUseUsageDescription</key>
    <string>Esta aplicación requiere acceso a la ubicación para mejorar la experiencia del usuario.</string>
    <key>NSAppTransportSecurity</key>
    <dict>
        <key>NSAllowsArbitraryLoads</key>
        <true/>
    </dict>
</dict>
</plist>
``` 