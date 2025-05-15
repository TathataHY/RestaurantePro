# Arquitectura de la Aplicación Web - RestaurantePro (Parte 2)

## Estructura del Proyecto

```
RestaurantePro.Web/
├── wwwroot/              # Recursos estáticos (CSS, JS, imágenes)
├── Pages/                # Páginas Razor
├── Shared/               # Componentes compartidos
├── Components/           # Componentes reutilizables
│   ├── Dashboard/        # Componentes para dashboard
│   ├── Reports/          # Componentes para reportes
│   ├── Management/       # Componentes para gestión
│   └── Auth/             # Componentes de autenticación
├── Models/               # Modelos de datos
├── Services/             # Servicios para comunicación con API
├── Authentication/       # Lógica de autenticación
├── Helpers/              # Utilidades
└── Program.cs            # Punto de entrada
```

## Comunicación con la API

Se implementa una capa de servicios que se comunica con el backend:

```csharp
public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync(DateTime startDate, DateTime endDate);
    Task<List<VentasPorMesa>> GetVentasPorMesaAsync(DateTime startDate, DateTime endDate);
    Task<List<VentasPorMesero>> GetVentasPorMeseroAsync(DateTime startDate, DateTime endDate);
}

public class DashboardService : IDashboardService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    
    public DashboardService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }
    
    public async Task<DashboardStats> GetDashboardStatsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DashboardStats>(
                $"api/dashboard/stats?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                
            return response ?? new DashboardStats();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error obteniendo estadísticas: {ex.Message}");
            throw;
        }
    }
    
    public async Task<List<VentasPorMesa>> GetVentasPorMesaAsync(DateTime startDate, DateTime endDate)
    {
        var response = await _httpClient.GetFromJsonAsync<List<VentasPorMesa>>(
            $"api/dashboard/ventas-por-mesa?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
            
        return response ?? new List<VentasPorMesa>();
    }
    
    public async Task<List<VentasPorMesero>> GetVentasPorMeseroAsync(DateTime startDate, DateTime endDate)
    {
        var response = await _httpClient.GetFromJsonAsync<List<VentasPorMesero>>(
            $"api/dashboard/ventas-por-mesero?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
            
        return response ?? new List<VentasPorMesero>();
    }
}
```

## Autenticación y Autorización

### Proveedor de Autenticación

```csharp
public class AuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;
    
    public AuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        
        if (string.IsNullOrWhiteSpace(token))
            return _anonymous;
            
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("bearer", token);
            
        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }
    
    public void NotifyUserAuthentication(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(
            new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }
    
    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }
    
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);
        
        if (roles != null)
        {
            if (roles.ToString().Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                
                foreach (var role in parsedRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
            }
            
            keyValuePairs.Remove(ClaimTypes.Role);
        }
        
        claims.AddRange(keyValuePairs.Select(kvp => 
            new Claim(kvp.Key, kvp.Value.ToString())));
            
        return claims;
    }
    
    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
```

### Servicio de Autenticación

```csharp
public interface IAuthService
{
    Task<AuthResponseDto> Login(LoginDto loginModel);
    Task Logout();
    Task<bool> RefreshToken();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;
    
    public AuthService(HttpClient httpClient, 
                       AuthenticationStateProvider authenticationStateProvider,
                       ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
    }
    
    public async Task<AuthResponseDto> Login(LoginDto loginModel)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        if (!response.IsSuccessStatusCode)
        {
            return content;
        }
        
        await _localStorage.SetItemAsync("authToken", content.Token);
        await _localStorage.SetItemAsync("refreshToken", content.RefreshToken);
        
        ((AuthenticationStateProvider)_authenticationStateProvider)
            .NotifyUserAuthentication(content.Token);
            
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("bearer", content.Token);
            
        return content;
    }
    
    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        
        ((AuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogout();
        
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
    
    public async Task<bool> RefreshToken()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
        
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            return false;
            
        var refreshRequest = new RefreshTokenDto
        {
            Token = token,
            RefreshToken = refreshToken
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", refreshRequest);
        
        if (!response.IsSuccessStatusCode)
            return false;
            
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        await _localStorage.SetItemAsync("authToken", content.Token);
        await _localStorage.SetItemAsync("refreshToken", content.RefreshToken);
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("bearer", content.Token);
            
        return true;
    }
}
```

## Gestión de Estado

Para aplicaciones Blazor, la gestión del estado es crucial. Se implementan varias estrategias:

### Estado en Memoria

```csharp
public class AppState
{
    // Estado global de la aplicación
    public UserInfo CurrentUser { get; private set; }
    public RestauranteInfo RestauranteInfo { get; private set; }
    
    // Eventos para notificar cambios
    public event Action OnChange;
    
    public void SetCurrentUser(UserInfo user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }
    
    public void SetRestauranteInfo(RestauranteInfo info)
    {
        RestauranteInfo = info;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

### Uso del Estado en Componentes

```razor
@inject AppState State
@implements IDisposable

<div>
    @if (State.CurrentUser != null)
    {
        <MudText>Bienvenido, @State.CurrentUser.Nombre</MudText>
    }
    
    @if (State.RestauranteInfo != null)
    {
        <MudText>@State.RestauranteInfo.Nombre</MudText>
    }
</div>

@code {
    protected override void OnInitialized()
    {
        // Suscribirse a cambios en el estado
        State.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        // Cancelar suscripción al desmontar el componente
        State.OnChange -= StateHasChanged;
    }
}
```

## Manejo de Errores y Excepciones

### Interceptor HTTP

```csharp
public class HttpInterceptorService
{
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _navigationManager;
    
    public HttpInterceptorService(ISnackbar snackbar, NavigationManager navigationManager)
    {
        _snackbar = snackbar;
        _navigationManager = navigationManager;
    }
    
    public async Task<HttpResponseMessage> Send(HttpRequestMessage request, 
                                               CancellationToken cancellationToken, 
                                               HttpMessageHandler handler)
    {
        var response = await handler.SendAsync(request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    _snackbar.Add(error?.Message ?? "Solicitud inválida", Severity.Error);
                    break;
                    
                case HttpStatusCode.Unauthorized:
                    _snackbar.Add("Sesión expirada. Por favor vuelva a iniciar sesión.", Severity.Warning);
                    _navigationManager.NavigateTo("/login");
                    break;
                    
                case HttpStatusCode.Forbidden:
                    _snackbar.Add("No tiene permisos para realizar esta acción.", Severity.Warning);
                    break;
                    
                case HttpStatusCode.NotFound:
                    _snackbar.Add("Recurso no encontrado.", Severity.Error);
                    break;
                    
                default:
                    _snackbar.Add("Ha ocurrido un error. Por favor intente nuevamente.", Severity.Error);
                    break;
            }
        }
        
        return response;
    }
}
```

### Error Boundary

```razor
<ErrorBoundary>
    <ChildContent>
        <!-- Contenido normal de la página -->
        <DashboardComponent />
    </ChildContent>
    <ErrorContent>
        <MudPaper Class="pa-4 mud-error">
            <MudText Typo="Typo.h5">¡Ha ocurrido un error!</MudText>
            <MudText Typo="Typo.body1">
                Ha ocurrido un error inesperado al cargar la página.
                Por favor intente actualizar o contacte al administrador.
            </MudText>
            <MudButton Color="Color.Primary" OnClick="@(() => navigationManager.NavigateTo("/"))">
                Ir a Inicio
            </MudButton>
        </MudPaper>
    </ErrorContent>
</ErrorBoundary>
```

## Configuración de la Aplicación

En Program.cs se configura la aplicación:

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        
        // Configuración de HttpClient
        builder.Services.AddScoped(sp => new HttpClient { 
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
        });
        
        // Servicios de autenticación
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        
        // MudBlazor
        builder.Services.AddMudServices(config => {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
        });
        
        // Servicios de la aplicación
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IProductoService, ProductoService>();
        builder.Services.AddScoped<IMesaService, MesaService>();
        builder.Services.AddScoped<IComandaService, ComandaService>();
        builder.Services.AddScoped<IUserService, UserService>();
        
        // Gestión de estado
        builder.Services.AddSingleton<AppState>();
        
        // Almacenamiento local
        builder.Services.AddBlazoredLocalStorage();
        
        // Configuración de autenticación
        builder.Services.AddAuthorizationCore(options => {
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireRole("Admin"));
                
            options.AddPolicy("GestionMesas", policy => 
                policy.RequireRole("Admin", "Gerente"));
        });
        
        await builder.Build().RunAsync();
    }
}
```

## Despliegue

Para el despliegue, Blazor WebAssembly genera archivos estáticos que pueden alojarse en cualquier servidor:

```bash
dotnet publish -c Release
```

El resultado se puede desplegar en:
- Azure Static Web Apps
- GitHub Pages
- Servidor web como IIS o Nginx
- Servicios de hospedaje estático como Netlify o Vercel 