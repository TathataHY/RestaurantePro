# Capa Mobile - RestaurantePro

Esta capa implementa la aplicación móvil multiplataforma utilizando .NET MAUI, organizada en módulos que reflejan las áreas funcionales del negocio.

## Estructura de Módulos Principales

```
Mobile/
├── Features/                 # Funcionalidades organizadas por módulos
│   ├── Comercial/            # Módulo Comercial
│   │   ├── Clientes/         # Gestión de clientes
│   │   │   ├── Views/        # Páginas XAML
│   │   │   ├── ViewModels/   # ViewModels
│   │   │   └── Models/       # Modelos específicos
│   │   │
│   │   └── Promociones/      # Gestión de promociones
│   │
│   ├── Operaciones/          # Módulo Operaciones
│   │   ├── Comandas/         # Gestión de comandas
│   │   │   ├── Views/        # Páginas XAML
│   │   │   ├── ViewModels/   # ViewModels
│   │   │   └── Components/   # Componentes reutilizables
│   │   │
│   │   ├── Mesas/            # Gestión de mesas
│   │   └── Reservaciones/    # Sistema de reservaciones
│   │
│   ├── Inventario/           # Módulo Inventario
│   │   ├── Productos/        # Inventario de productos
│   │   └── Movimientos/      # Movimientos de inventario
│   │
│   ├── Catalogo/             # Módulo Catálogo
│   │   ├── Productos/        # Productos y platillos
│   │   └── Categorias/       # Categorías de productos
│   │
│   ├── Finanzas/             # Módulo Finanzas
│   │   ├── Pagos/            # Procesamiento de pagos
│   │   └── Reportes/         # Reportes financieros
│   │
│   └── Account/              # Gestión de cuenta y autenticación
│       ├── Login/            # Inicio de sesión
│       ├── Profile/          # Perfil de usuario
│       └── Settings/         # Configuración de usuario
│
├── Core/                     # Componentes centrales
│   ├── Services/             # Servicios comunes
│   │   ├── Api/              # Servicios de comunicación con API
│   │   ├── Navigation/       # Servicio de navegación
│   │   ├── Dialog/           # Servicio de diálogos
│   │   └── Authentication/   # Servicio de autenticación
│   │
│   ├── Helpers/              # Clases auxiliares
│   ├── Extensions/           # Extensiones útiles
│   ├── Converters/           # Convertidores de valores
│   ├── Behaviors/            # Comportamientos
│   └── Constants/            # Constantes de la aplicación
│
├── Shared/                   # Elementos compartidos
│   ├── Controls/             # Controles personalizados
│   ├── Templates/            # Plantillas de datos
│   ├── Themes/               # Temas y estilos
│   ├── Fonts/                # Fuentes personalizadas
│   └── Icons/                # Iconos personalizados
│
├── Resources/                # Recursos de la aplicación
│   ├── Images/               # Imágenes
│   ├── Styles/               # Estilos globales
│   ├── Fonts/                # Fuentes
│   └── Raw/                  # Archivos sin procesar
│
├── Config/                   # Configuración de la aplicación
│   ├── AppSettings.cs        # Configuración general
│   ├── ThemeConfig.cs        # Configuración de temas
│   └── ApiEndpoints.cs       # Configuración de endpoints
│
└── Platforms/                # Código específico de plataforma
    ├── Android/              # Configuración y código para Android
    ├── iOS/                  # Configuración y código para iOS
    ├── Windows/              # Configuración y código para Windows
    └── MacCatalyst/          # Configuración y código para Mac
```

## Arquitectura MVVM

Cada módulo funcional sigue el patrón MVVM (Model-View-ViewModel):

- **Views**: Páginas XAML que definen la interfaz de usuario.
- **ViewModels**: Clases que manejan la lógica de presentación y el estado.
- **Models**: Clases que representan los datos específicos del dominio.

## Comunicación con API

La aplicación móvil se comunica con la API RESTful a través de servicios dedicados:

```csharp
// ApiClient.cs
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ITokenService _tokenService;

    public ApiClient(ITokenService tokenService, IAppSettings appSettings)
    {
        _tokenService = tokenService;
        _baseUrl = appSettings.ApiBaseUrl;
        _httpClient = new HttpClient();
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        await SetAuthorizationHeader();
        var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        await SetAuthorizationHeader();
        var jsonContent = JsonSerializer.Serialize(data);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", stringContent);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private async Task SetAuthorizationHeader()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
```

## Inyección de Dependencias

La aplicación utiliza la inyección de dependencias nativa de MAUI:

```csharp
// MauiProgram.cs
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

        // Registrar servicios
        builder.Services.AddSingleton<IAppSettings, AppSettings>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        
        // Registrar clientes API por módulo
        builder.Services.AddSingleton<IClientesApiClient, ClientesApiClient>();
        builder.Services.AddSingleton<IComandasApiClient, ComandasApiClient>();
        
        // Registrar ViewModels y Pages
        RegisterViewsAndViewModels(builder.Services);
        
        return builder.Build();
    }
    
    private static void RegisterViewsAndViewModels(IServiceCollection services)
    {
        // Comercial - Clientes
        services.AddTransient<ClientesListViewModel>();
        services.AddTransient<ClientesListPage>();
        services.AddTransient<ClienteDetailViewModel>();
        services.AddTransient<ClienteDetailPage>();
        
        // Operaciones - Comandas
        services.AddTransient<ComandasListViewModel>();
        services.AddTransient<ComandasListPage>();
        services.AddTransient<ComandaDetailViewModel>();
        services.AddTransient<ComandaDetailPage>();
        services.AddTransient<NuevaComandaViewModel>();
        services.AddTransient<NuevaComandaPage>();
        
        // Account
        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginPage>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<ProfilePage>();
    }
}
```

## Ejemplo de ViewModel

```csharp
// ClientesListViewModel.cs
public class ClientesListViewModel : BaseViewModel
{
    private readonly IClientesApiClient _clientesApiClient;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    
    private ObservableCollection<ClienteDto> _clientes;
    public ObservableCollection<ClienteDto> Clientes
    {
        get => _clientes;
        set => SetProperty(ref _clientes, value);
    }
    
    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }
    
    public Command LoadClientsCommand { get; }
    public Command<int> ViewClientDetailCommand { get; }
    public Command AddClientCommand { get; }
    
    public ClientesListViewModel(
        IClientesApiClient clientesApiClient,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _clientesApiClient = clientesApiClient;
        _navigationService = navigationService;
        _dialogService = dialogService;
        
        Title = "Clientes";
        Clientes = new ObservableCollection<ClienteDto>();
        
        LoadClientsCommand = new Command(async () => await LoadClientsAsync());
        ViewClientDetailCommand = new Command<int>(async (id) => await ViewClientDetailAsync(id));
        AddClientCommand = new Command(async () => await AddClientAsync());
    }
    
    async Task LoadClientsAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            IsRefreshing = true;
            
            var clientes = await _clientesApiClient.GetAllClientes();
            
            Clientes.Clear();
            foreach (var cliente in clientes)
            {
                Clientes.Add(cliente);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"No se pudieron cargar los clientes: {ex.Message}", "Aceptar");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
    
    async Task ViewClientDetailAsync(int id)
    {
        await _navigationService.NavigateToAsync<ClienteDetailViewModel>(new Dictionary<string, object>
        {
            { "Id", id }
        });
    }
    
    async Task AddClientAsync()
    {
        await _navigationService.NavigateToAsync<ClienteAddViewModel>();
    }
    
    public override Task InitializeAsync(IDictionary<string, object> parameters)
    {
        return LoadClientsAsync();
    }
}
``` 