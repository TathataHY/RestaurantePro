# Mapeo de Desarrollo Mobile - Versión 1: Conceptos Básicos
## RestaurantePro Mobile App (.NET MAUI)

---

## 📋 **INFORMACIÓN DEL DOCUMENTO**
- **Versión**: V1 - Conceptos Básicos
- **Fecha**: Diciembre 2024
- **Objetivo**: Establecer fundamentos básicos para el desarrollo de la aplicación móvil
- **Siguiente**: V2 - Conceptos Avanzados

---

## 🎯 **RESUMEN EJECUTIVO**

### **¿Qué es este documento?**
Este documento mapea los **conceptos básicos** necesarios para desarrollar la aplicación móvil de RestaurantePro usando .NET MAUI. La aplicación está **ultra-enfocada en operaciones diarias** del restaurante, no en administración.

### **¿Para qué sirve?**
- 📱 Establece los **fundamentos operativos** de la aplicación móvil
- 🏗️ Define la **arquitectura básica** MVVM para operaciones
- 🔧 Mapea **componentes esenciales** solo para flujos críticos
- 🎯 Identifica **casos de uso operativos** principales
- 📊 Establece **bases para pruebas** de flujos críticos

### **🔄 ALCANCE OPERATIVO (Solo Operaciones Críticas)**
```csharp
// ✅ QUE SÍ INCLUYE (Operaciones Diarias)
✅ Gestión de Mesas          // Estados, asignación, liberación
✅ Gestión de Comandas       // Crear, modificar, seguimiento
✅ Gestión de Preparaciones  // Estados de cocina, tiempos
✅ Facturación de Ventas     // Solo generar facturas al cliente
✅ Consulta de Menú          // Ver productos y disponibilidad
✅ Autenticación de Personal // Login del staff

// ❌ QUE NO INCLUYE (Va en Web Admin)
❌ Gestión de Personal       // Crear usuarios, roles, permisos
❌ Gestión de Productos      // Crear/editar menú, recetas
❌ Gestión de Proveedores    // Proveedores, contactos, órdenes
❌ Gestión de Inventario     // Movimientos, compras, reportes
❌ Reportes y Analytics      // Análisis de datos, métricas
❌ Configuración Sistema     // Settings, promociones, etc.
```

---

## 🏗️ **ARQUITECTURA MÓVIL - NIVEL BÁSICO**

### **1. Patrón MVVM con CommunityToolkit.Mvvm**

```csharp
// Estructura OPERATIVA del patrón MVVM (Solo operaciones críticas)
Mobile/
├── Models/                    # Modelos operativos únicamente
│   ├── Core/
│   │   ├── Producto.cs       # Solo consulta de productos
│   │   ├── AuthUser.cs       # Solo autenticación de personal
│   │   └── ApiResponse.cs    # Wrapper de respuestas API
│   ├── Operaciones/          # CORAZÓN de la aplicación
│   │   ├── Comanda.cs        # Modelo de comanda
│   │   ├── Mesa.cs           # Modelo de mesa
│   │   ├── ItemComanda.cs    # Modelo de item de comanda
│   │   └── Preparacion.cs    # Modelo de preparación
│   └── Comercial/
│       ├── ClienteBasico.cs  # Solo datos básicos para comandas
│       └── FacturaVenta.cs   # Solo generar facturas de venta
│
├── ViewModels/               # Lógica operativa únicamente
│   ├── Base/
│   │   ├── BaseViewModel.cs  # ViewModel base
│   │   └── BaseListViewModel.cs # ViewModel para listas
│   ├── Core/
│   │   ├── MenuViewModel.cs  # Solo consulta de menú
│   │   └── AuthViewModel.cs  # Solo login de personal
│   ├── Operaciones/          # ViewModels PRINCIPALES
│   │   ├── ComandasViewModel.cs
│   │   ├── MesasViewModel.cs
│   │   ├── ComandaDetalleViewModel.cs
│   │   └── PreparacionesViewModel.cs
│   └── Comercial/
│       └── FacturacionViewModel.cs  # Solo generar facturas
│
└── Views/                    # Interfaces operativas únicamente
    ├── Core/
    │   ├── MenuPage.xaml     # Solo consulta de menú
    │   └── LoginPage.xaml    # Solo login de personal
    ├── Operaciones/          # Páginas PRINCIPALES
    │   ├── ComandasPage.xaml
    │   ├── MesasPage.xaml
    │   ├── ComandaDetallePage.xaml
    │   └── PreparacionesPage.xaml
    └── Comercial/
        └── FacturacionPage.xaml  # Solo generar facturas
```

### **2. Componentes Básicos MAUI**

#### **A. Servicios Fundamentales**
```csharp
// Servicios OPERATIVOS únicamente
Services/
├── Core/
│   ├── IApiService.cs           # Comunicación con API
│   ├── IAuthService.cs          # Autenticación de personal
│   ├── INavigationService.cs    # Navegación
│   └── IDialogService.cs        # Diálogos y alertas
├── Platform/
│   ├── IConnectivityService.cs  # Conectividad
│   ├── IStorageService.cs       # Almacenamiento local mínimo
│   └── INotificationService.cs  # Recibir notificaciones
└── Operations/                  # Servicios PRINCIPALES
    ├── IMesaService.cs          # Gestión de mesas
    ├── IComandaService.cs       # Gestión de comandas
    ├── IPreparacionService.cs   # Estados de preparación
    ├── IFacturacionService.cs   # Solo generar facturas
    ├── IMenuService.cs          # Solo consulta de menú
    └── IClienteBasicoService.cs # Datos básicos para comandas
```

#### **B. Páginas Principales**
```csharp
// Páginas OPERATIVAS únicamente (Solo lo esencial)
Pages/
├── Shared/
│   ├── SplashPage.xaml         # Pantalla de carga
│   ├── LoadingPage.xaml        # Pantalla de carga
│   └── ErrorPage.xaml          # Pantalla de error
├── Auth/
│   └── LoginPage.xaml          # Solo inicio de sesión de personal
├── Operations/                 # Páginas PRINCIPALES
│   ├── MesasPage.xaml         # Gestión de mesas
│   ├── ComandasPage.xaml      # Gestión de comandas
│   ├── PreparacionesPage.xaml # Estados de preparación
│   └── FacturacionPage.xaml   # Solo generar facturas
└── Support/
    └── MenuPage.xaml          # Solo consulta de menú
```

---

## 🔧 **COMPONENTES ESENCIALES**

### **1. Controles Básicos MAUI**

#### **A. Navegación**
```xml
<!-- Shell Navigation - OPERACIONES únicamente -->
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/20De nada ¿De qué trataba el vídeo? Lo hiciste tú ahora algo que compartiste 09/xaml"
       x:Class="RestaurantePro.Mobile.AppShell">
    
    <!-- Tabs OPERATIVAS únicamente -->
    <TabBar>
        <ShellContent Title="Mesas" 
                      Icon="mesa_icon.png" 
                      ContentTemplate="{DataTemplate local:MesasPage}" />
        <ShellContent Title="Comandas" 
                      Icon="comanda_icon.png" 
                      ContentTemplate="{DataTemplate local:ComandasPage}" />
        <ShellContent Title="Cocina" 
                      Icon="cocina_icon.png" 
                      ContentTemplate="{DataTemplate local:PreparacionesPage}" />
        <ShellContent Title="Facturar" 
                      Icon="factura_icon.png" 
                      ContentTemplate="{DataTemplate local:FacturacionPage}" />
        <ShellContent Title="Menú" 
                      Icon="menu_icon.png" 
                      ContentTemplate="{DataTemplate local:MenuPage}" />
    </TabBar>
</Shell>
```

#### **B. Listas y Colecciones**
```xml
<!-- CollectionView básica para listas -->
<CollectionView ItemsSource="{Binding Productos}"
                SelectionMode="Single"
                SelectedItem="{Binding SelectedProducto}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid Padding="10">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                
                <Label Grid.Column="0" 
                       Text="{Binding Nombre}" 
                       Style="{StaticResource LabelMedium}" />
                <Label Grid.Column="1" 
                       Text="{Binding Precio, StringFormat='{0:C}'}" 
                       Style="{StaticResource LabelSmall}" />
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### **2. Comunicación con API**

#### **A. ApiService Básico**
```csharp
public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse<bool>> DeleteAsync(string endpoint);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public ApiService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            await AddAuthHeader();
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(json);
                return result;
            }
            
            return ApiResponse<T>.ErrorResponse(
                new List<string> { "Error en la comunicación con el servidor" },
                "Error de conexión", 
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse(
                new List<string> { ex.Message },
                "Error inesperado", 
                500);
        }
    }

    private async Task AddAuthHeader()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
```

---

## 📱 **CASOS DE USO PRINCIPALES**

### **1. Flujo de Autenticación**
```csharp
// ViewModel básico de autenticación
public partial class AuthViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await ShowErrorAsync("Por favor complete todos los campos");
            return;
        }

        IsLoading = true;
        
        try
        {
            var result = await _authService.LoginAsync(Email, Password);
            
            if (result.Succeeded)
            {
                await _navigationService.NavigateToAsync("//dashboard");
            }
            else
            {
                await ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### **2. Gestión de Mesas**
```csharp
// ViewModel básico de mesas
public partial class MesasViewModel : BaseViewModel
{
    private readonly IMesaService _mesaService;
    
    [ObservableProperty]
    private ObservableCollection<Mesa> mesas;

    [ObservableProperty]
    private Mesa selectedMesa;

    [RelayCommand]
    private async Task LoadMesasAsync()
    {
        IsBusy = true;
        
        try
        {
            var result = await _mesaService.GetMesasAsync();
            
            if (result.Succeeded)
            {
                Mesas = new ObservableCollection<Mesa>(result.Data);
            }
            else
            {
                await ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AsignarMesaAsync(Mesa mesa)
    {
        if (mesa == null) return;

        var result = await _mesaService.AsignarMesaAsync(mesa.Id);
        
        if (result.Succeeded)
        {
            await LoadMesasAsync(); // Refrescar lista
        }
        else
        {
            await ShowErrorAsync(result.Error);
        }
    }
}
```

### **3. Gestión de Comandas**
```csharp
// ViewModel básico de comandas
public partial class ComandasViewModel : BaseViewModel
{
    private readonly IComandaService _comandaService;
    
    [ObservableProperty]
    private ObservableCollection<Comanda> comandas;

    [ObservableProperty]
    private Comanda selectedComanda;

    [RelayCommand]
    private async Task LoadComandasAsync()
    {
        IsBusy = true;
        
        try
        {
            var result = await _comandaService.GetComandasAsync();
            
            if (result.Succeeded)
            {
                Comandas = new ObservableCollection<Comanda>(result.Data);
            }
            else
            {
                await ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CrearComandaAsync()
    {
        await Shell.Current.GoToAsync("comandadetalle");
    }
}
```

---

## 🧪 **ESTRATEGIA DE PRUEBAS - NIVEL BÁSICO**

### **1. Tipos de Pruebas Esenciales**

#### **A. Pruebas Unitarias de ViewModels**
```csharp
// Ejemplo de prueba unitaria básica
[TestClass]
public class AuthViewModelTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<INavigationService> _mockNavigationService;
    private AuthViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new AuthViewModel(_mockAuthService.Object, _mockNavigationService.Object);
    }

    [TestMethod]
    public async Task LoginAsync_WithValidCredentials_ShouldNavigateToDashboard()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(Result<AuthResponse>.Success(new AuthResponse()));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync("//dashboard"), Times.Once);
    }

    [TestMethod]
    public async Task LoginAsync_WithInvalidCredentials_ShouldShowError()
    {
        // Arrange
        _viewModel.Email = "invalid@example.com";
        _viewModel.Password = "wrongpassword";
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(Result<AuthResponse>.Failure("Credenciales inválidas"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        Assert.IsTrue(_viewModel.HasError);
        Assert.AreEqual("Credenciales inválidas", _viewModel.ErrorMessage);
    }
}
```

#### **B. Pruebas de Integración de Servicios**
```csharp
[TestClass]
public class ApiServiceTests
{
    private ApiService _apiService;
    private Mock<IAuthService> _mockAuthService;
    private HttpClient _httpClient;

    [TestInitialize]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _httpClient = new HttpClient();
        _apiService = new ApiService(_httpClient, _mockAuthService.Object);
    }

    [TestMethod]
    public async Task GetAsync_WithValidEndpoint_ShouldReturnData()
    {
        // Arrange
        _mockAuthService.Setup(x => x.GetTokenAsync())
                       .ReturnsAsync("valid_token");

        // Act
        var result = await _apiService.GetAsync<List<Producto>>("api/core/productos");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Data);
    }
}
```

### **2. Configuración de Pruebas**

#### **A. Configuración MSTest**
```xml
<!-- Archivo de configuración de pruebas -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.1.1" />
    <PackageReference Include="MSTest.TestFramework" Version="3.1.1" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="Moq" Version="4.20.69" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\RestaurantePro.Mobile\RestaurantePro.Mobile.csproj" />
  </ItemGroup>

</Project>
```

---

## 🔧 **CONFIGURACIÓN DE PROYECTO**

### **1. Estructura de Carpetas**
```
RestaurantePro.Mobile/
├── Platforms/              # Configuración específica de plataforma
│   ├── Android/
│   ├── iOS/
│   └── Windows/
├── Resources/              # Recursos compartidos
│   ├── Images/
│   ├── Fonts/
│   └── Styles/
├── Models/                 # Modelos de datos
├── ViewModels/             # ViewModels MVVM
├── Views/                  # Páginas XAML
├── Services/               # Servicios de negocio
├── Controls/               # Controles personalizados
├── Converters/             # Convertidores de datos
└── Behaviors/              # Comportamientos personalizados
```

### **2. Dependencias Básicas**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <RootNamespace>RestaurantePro.Mobile</RootNamespace>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- MAUI Workload -->
    <PackageReference Include="Microsoft.Maui.Controls" Version="8.0.3" />
    <PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="8.0.3" />
    
    <!-- MVVM Toolkit -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    
    <!-- HTTP Client -->
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
    
    <!-- JSON Serialization -->
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
    
    <!-- Logging -->
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="8.0.0" />
    
    <!-- SQLite (para almacenamiento local) -->
    <PackageReference Include="sqlite-net-pcl" Version="1.8.116" />
    
    <!-- Connectivity -->
    <PackageReference Include="Microsoft.Maui.Essentials" Version="8.0.3" />
  </ItemGroup>

</Project>
```

---

## 🎯 **OBJETIVOS DE LA VERSIÓN 1**

### **✅ Que SÍ incluye esta versión:**
- [x] **Estructura básica** del proyecto MAUI
- [x] **Patrón MVVM** con CommunityToolkit.Mvvm
- [x] **Servicios fundamentales** (API, Auth, Navigation)
- [x] **Páginas principales** (Login, Dashboard, Mesas, Comandas)
- [x] **Comunicación básica** con el backend
- [x] **Pruebas unitarias** esenciales
- [x] **Configuración de proyecto** básica

### **❌ Que NO incluye esta versión:**
- [ ] **Arquitectura avanzada** (Dependency Injection avanzado)
- [ ] **Patrones complejos** (Repository, CQRS en mobile)
- [ ] **Sincronización offline** completa
- [ ] **Pruebas UI automatizadas**
- [ ] **Optimizaciones de performance**
- [ ] **Funcionalidades avanzadas** (SignalR, Push notifications)

---

## 🚀 **SIGUIENTE PASO**

### **Próximo Documento: V2 - Conceptos Avanzados**
- **Dependency Injection avanzado**
- **Patrones de arquitectura complejos**
- **Sincronización offline**
- **Performance y optimizaciones**
- **Integración con servicios externos**

---

*Este documento establece las bases sólidas para el desarrollo de la aplicación móvil de RestaurantePro. Una vez implementados estos conceptos básicos, se puede avanzar a la versión 2 con funcionalidades más avanzadas.* 

## 🎯 **ALCANCE MÓVIL V1** - Operaciones Críticas

### **📋 Funcionalidades Operativas (10 funcionalidades principales)**

1. **🔐 Autenticación y Autorización**
   - Login seguro con JWT
   - Gestión de permisos por rol
   - Logout y renovación de tokens

2. **🏠 Gestión de Mesas**
   - Visualización de mesas disponibles/ocupadas
   - Asignación de mesas a meseros
   - Cambio de estado de mesas
   - **🆕 Página de detalle de mesa** (MesaDetallePage)

3. **📝 Gestión de Comandas**
   - Creación de nuevas comandas
   - Selección de productos del menú
   - Gestión de estados (pendiente, preparando, lista)
   - **🆕 Página de detalle de comanda** (ComandaDetallePage) - **PENDIENTE**

4. **🍽️ Gestión de Productos/Menú**
   - Visualización de productos disponibles
   - Filtrado por categorías
   - Información de precios y disponibilidad
   - **🆕 Página de detalle de producto** (ProductoDetallePage) - **PENDIENTE**

5. **🔧 Infraestructura de Testing**
   - Tests unitarios para todos los ViewModels
   - Cobertura completa de servicios
   - Framework de pruebas robusto

6. **🏗️ Arquitectura Correcta**
   - ViewModels en Mobile.Core para facilitar testing
   - Eliminación de duplicaciones
   - Separación clara de responsabilidades

7. **📊 Consulta de Menú**
   - Visualización de productos disponibles
   - Filtrado por categorías
   - Información de precios y disponibilidad

8. **🍳 Preparaciones por Demanda**
   - Cola de preparaciones de cocina
   - Marcar preparaciones como iniciadas/completadas
   - Notificaciones de preparaciones listas

9. **🆕 🍽️ Preparaciones Diarias**
   - **Planificación matutina:** Crear preparaciones del día
   - **Gestión de cantidades:** Definir cantidades por producto
   - **Disponibilidad tiempo real:** Ver stock preparado disponible
   - **Consumo automático:** Descuento automático al servir

10. **🔔 Notificaciones**
    - Notificaciones push para estados críticos
    - Alertas de preparaciones listas
    - Comunicación tiempo real con SignalR

### **🚨 IDENTIFICACIÓN DE BRECHAS V1**

#### **A. Páginas de Detalle Faltantes**
- ✅ **MesaDetallePage** - Implementada
- ❌ **ComandaDetallePage** - **FALTA IMPLEMENTAR**
- ❌ **ProductoDetallePage** - **FALTA IMPLEMENTAR**

#### **B. ViewModels sin Tests**
- ✅ **LoginViewModel** - 24 tests completos
- ❌ **MesasViewModel** - **SIN TESTS**
- ❌ **ComandasViewModel** - **SIN TESTS**
- ❌ **ProductosViewModel** - **SIN TESTS**
- ❌ **MesaDetalleViewModel** - **SIN TESTS**

#### **C. Problemas de Arquitectura**
- ⚠️ **ViewModels en ubicación incorrecta** (Mobile en lugar de Mobile.Core)
- ⚠️ **LoginViewModel duplicado** (existe en ambos proyectos)
- ⚠️ **Navegación incompleta** (faltan 2 páginas de detalle)

## 📁 **ESTRUCTURA FINAL DE CARPETAS**

```
RestaurantePro.Mobile/
├── 📱 Features/                    # Funcionalidades operativas críticas
│   ├── 🔐 Authentication/         # Sistema de autenticación
│   │   ├── Pages/                 # LoginPage.xaml
│   │   ├── ViewModels/            # ⚠️ MOVER A MOBILE.CORE
│   │   └── Services/              # AuthService.cs
│   │
│   ├── 🏠 Operations/             # Operaciones diarias críticas
│   │   ├── Tables/                # Gestión de mesas
│   │   │   ├── Pages/             # TablesPage.xaml, TableDetailPage.xaml ✅
│   │   │   ├── ViewModels/        # ⚠️ MOVER A MOBILE.CORE
│   │   │   └── Services/          # TablesService.cs
│   │   │
│   │   ├── Orders/                # Gestión de comandas
│   │   │   ├── Pages/             # OrdersPage.xaml, ❌ NewOrderPage.xaml
│   │   │   ├── ViewModels/        # ⚠️ MOVER A MOBILE.CORE
│   │   │   └── Services/          # OrdersService.cs
│   │   │
│   │   ├── Products/              # Gestión de productos
│   │   │   ├── Pages/             # ProductsPage.xaml, ❌ ProductDetailPage.xaml
│   │   │   ├── ViewModels/        # ⚠️ MOVER A MOBILE.CORE
│   │   │   └── Services/          # ProductsService.cs
│   │   │
│   │   ├── Preparations/          # Preparaciones por demanda
│   │   │   ├── Pages/             # ❌ PreparationsPage.xaml
│   │   │   ├── ViewModels/        # ❌ PreparationsViewModel.cs
│   │   │   └── Services/          # ❌ PreparationsService.cs
│   │   │
│   │   ├── DailyPreparations/     # 🆕 Preparaciones diarias
│   │   │   ├── Pages/             # ❌ DailyPreparationsPage.xaml
│   │   │   ├── ViewModels/        # ❌ DailyPreparationsViewModel.cs
│   │   │   └── Services/          # ❌ DailyPreparationsService.cs
│   │   │
│   │   └── Reservations/          # Gestión de reservas
│   │       ├── Pages/             # ❌ ReservationsPage.xaml
│   │       ├── ViewModels/        # ❌ ReservationsViewModel.cs
│   │       └── Services/          # ❌ ReservationsService.cs
│   │
│   ├── 💰 Commercial/             # Operaciones comerciales
│   │   └── Billing/               # Facturación y cobros
│   │       ├── Pages/             # ❌ BillingPage.xaml
│   │       ├── ViewModels/        # ❌ BillingViewModel.cs
│   │       └── Services/          # ❌ BillingService.cs
│   │
│   ├── 📊 Catalog/                # Consulta de información
│   │   ├── Products/              # Productos del menú
│   │   │   ├── Pages/             # ProductsPage.xaml ✅
│   │   │   ├── ViewModels/        # ProductsViewModel.cs ✅
│   │   │   └── Services/          # ProductsService.cs ✅
│   │   │
│   │   └── Categories/            # Categorías de productos
│   │       ├── Pages/             # ❌ CategoriesPage.xaml
│   │       ├── ViewModels/        # ❌ CategoriesViewModel.cs
│   │       └── Services/          # ❌ CategoriesService.cs
│   │
│   └── 🔔 Notifications/          # Sistema de notificaciones
│       ├── Pages/                 # ❌ NotificationsPage.xaml
│       ├── ViewModels/            # ❌ NotificationsViewModel.cs
│       └── Services/              # ❌ NotificationsService.cs
│
├── 🧩 Shared/                     # Componentes compartidos
│   ├── Components/                # Componentes reutilizables
│   ├── Converters/               # Convertidores XAML ✅
│   ├── Controls/                 # Controles personalizados
│   ├── Styles/                   # Estilos y temas
│   └── Resources/                # Recursos compartidos
│
├── 🏗️ Core/                      # Infraestructura y servicios base
│   ├── Services/                 # Servicios principales ✅
│   │   ├── Api/                  # Cliente API ✅
│   │   ├── Authentication/       # Autenticación ✅
│   │   ├── Navigation/           # Navegación ✅
│   │   ├── Dialog/               # Diálogos ✅
│   │   ├── Cache/                # Cache local mínimo
│   │   ├── Offline/              # Sincronización offline
│   │   └── Notifications/        # Notificaciones push
│   │
│   ├── Models/                   # Modelos de datos ✅
│   │   ├── DTOs/                 # Objetos de transferencia ✅
│   │   ├── ViewModels/           # ViewModels base ✅
│   │   └── Entities/             # Entidades locales
│   │
│   ├── Extensions/               # Métodos de extensión
│   ├── Helpers/                  # Clases de ayuda
│   └── Constants/                # Constantes globales
│
├── 🎨 UI/                        # Componentes de interfaz
│   ├── Pages/                    # Páginas principales
│   ├── Views/                    # Vistas reutilizables
│   ├── Popups/                   # Popups y modales
│   └── Templates/                # Plantillas de datos
│
├── 📱 Platforms/                 # Código específico por plataforma
│   ├── Android/
│   ├── iOS/
│   └── Windows/
│
├── 🔧 Config/                    # Configuración de la aplicación
│   ├── AppSettings.cs
│   ├── ApiConfig.cs
│   └── ThemeConfig.cs
│
├── App.xaml                      # Aplicación principal
├── AppShell.xaml                 # Shell de navegación
├── MauiProgram.cs                # Configuración MAUI
└── RestaurantePro.Mobile.csproj  # Archivo de proyecto
```

### **🎯 Características de la Estructura**

**✅ Organización por funcionalidad:**
- `Features/` contiene todas las funcionalidades operativas
- Cada feature tiene su propia carpeta con Pages, ViewModels, Services

**⚠️ Problemas identificados:**
- ViewModels en Mobile en lugar de Mobile.Core
- Páginas de detalle faltantes
- Tests unitarios incompletos

**✅ Separación clara de responsabilidades:**
- `Core/` - Infraestructura y servicios base
- `Shared/` - Componentes reutilizables
- `UI/` - Componentes de interfaz
- `Platforms/` - Código específico por plataforma

**✅ Escalabilidad:**
- Fácil agregar nuevas funcionalidades
- Estructura consistente en todos los módulos
- Separación clara entre operaciones y consultas

### **🎯 Decisión Final**

**Recomiendo completar el V1 con las correcciones identificadas** porque:
1. **Corrige problemas de arquitectura críticos**
2. **Establece base sólida para testing**
3. **Implementa navegación completa**
4. **Mantiene enfoque en operaciones críticas**
5. **Prepara terreno sólido para V2**

---

## 🚀 **PLAN DE ACCIÓN PARA COMPLETAR V1**

### **📅 FASE 1: CORRECCIÓN DE ARQUITECTURA (1-2 días)**
1. **Mover ViewModels a Mobile.Core**
2. **Eliminar LoginViewModel duplicado**
3. **Actualizar referencias y DI**

### **📅 FASE 2: IMPLEMENTAR PÁGINAS DE DETALLE (2-3 días)**
1. **ComandaDetallePage + ComandaDetalleViewModel**
2. **ProductoDetallePage + ProductoDetalleViewModel**
3. **Configurar navegación**

### **📅 FASE 3: COMPLETAR TESTS (3-4 días)**
1. **Tests para todos los ViewModels**
2. **~70 tests adicionales**
3. **Validar cobertura completa**

### **📅 FASE 4: VALIDACIÓN FINAL (1 día)**
1. **Probar navegación completa**
2. **Validar todos los flujos**
3. **Optimizar UX**

### **🏆 RESULTADO ESPERADO**
- **V1 robusto con ~140 tests**
- **Navegación completa implementada**
- **Arquitectura correcta establecida**
- **Base sólida para V2** 