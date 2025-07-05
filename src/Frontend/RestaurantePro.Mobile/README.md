# RestaurantePro.Mobile - Aplicación Móvil Operativa

Aplicación móvil multiplataforma desarrollada con .NET MAUI, enfocada **exclusivamente en operaciones diarias críticas** del restaurante.

## 🎯 **Alcance Operativo**

La aplicación móvil maneja **solo 7 funcionalidades operativas críticas**:
- 🔐 Autenticación y autorización
- 🏠 Gestión de mesas
- 📝 Gestión de comandas
- 🍳 Preparaciones por demanda
- 🍽️ Preparaciones diarias
- 📊 Consulta de menú
- 🔔 Notificaciones

## 📁 **Estructura Final de Carpetas**

```
RestaurantePro.Mobile/
├── 📱 Features/                    # Funcionalidades operativas críticas
│   ├── 🔐 Authentication/         # Sistema de autenticación
│   │   ├── Pages/                 # LoginPage.xaml
│   │   ├── ViewModels/            # LoginViewModel.cs
│   │   └── Services/              # AuthService.cs
│   │
│   ├── 🏠 Operations/             # Operaciones diarias críticas
│   │   ├── Tables/                # Gestión de mesas
│   │   │   ├── Pages/             # TablesPage.xaml, TableDetailPage.xaml
│   │   │   ├── ViewModels/        # TablesViewModel.cs
│   │   │   └── Services/          # TablesService.cs
│   │   │
│   │   ├── Orders/                # Gestión de comandas
│   │   │   ├── Pages/             # OrdersPage.xaml, NewOrderPage.xaml
│   │   │   ├── ViewModels/        # OrdersViewModel.cs
│   │   │   └── Services/          # OrdersService.cs
│   │   │
│   │   ├── Preparations/          # Preparaciones por demanda
│   │   │   ├── Pages/             # PreparationsPage.xaml
│   │   │   ├── ViewModels/        # PreparationsViewModel.cs
│   │   │   └── Services/          # PreparationsService.cs
│   │   │
│   │   ├── DailyPreparations/     # 🆕 Preparaciones diarias
│   │   │   ├── Pages/             # DailyPreparationsPage.xaml
│   │   │   ├── ViewModels/        # DailyPreparationsViewModel.cs
│   │   │   └── Services/          # DailyPreparationsService.cs
│   │   │
│   │   └── Reservations/          # Gestión de reservas
│   │       ├── Pages/             # ReservationsPage.xaml
│   │       ├── ViewModels/        # ReservationsViewModel.cs
│   │       └── Services/          # ReservationsService.cs
│   │
│   ├── 💰 Commercial/             # Operaciones comerciales
│   │   └── Billing/               # Facturación y cobros
│   │       ├── Pages/             # BillingPage.xaml
│   │       ├── ViewModels/        # BillingViewModel.cs
│   │       └── Services/          # BillingService.cs
│   │
│   ├── 📊 Catalog/                # Consulta de información
│   │   ├── Products/              # Productos del menú
│   │   │   ├── Pages/             # ProductsPage.xaml
│   │   │   ├── ViewModels/        # ProductsViewModel.cs
│   │   │   └── Services/          # ProductsService.cs
│   │   │
│   │   └── Categories/            # Categorías de productos
│   │       ├── Pages/             # CategoriesPage.xaml
│   │       ├── ViewModels/        # CategoriesViewModel.cs
│   │       └── Services/          # CategoriesService.cs
│   │
│   └── 🔔 Notifications/          # Sistema de notificaciones
│       ├── Pages/                 # NotificationsPage.xaml
│       ├── ViewModels/            # NotificationsViewModel.cs
│       └── Services/              # NotificationsService.cs
│
├── 🧩 Shared/                     # Componentes compartidos
│   ├── Components/                # Componentes reutilizables
│   │   ├── TableCard.xaml         # Tarjeta de mesa
│   │   ├── OrderCard.xaml         # Tarjeta de comanda
│   │   └── ProductCard.xaml       # Tarjeta de producto
│   │
│   ├── Converters/               # Convertidores XAML
│   │   ├── BoolToColorConverter.cs
│   │   └── StatusToIconConverter.cs
│   │
│   ├── Controls/                 # Controles personalizados
│   │   ├── LoadingButton.xaml
│   │   └── StatusBadge.xaml
│   │
│   ├── Styles/                   # Estilos y temas
│   │   ├── GlobalStyles.xaml
│   │   └── ColorScheme.xaml
│   │
│   └── Resources/                # Recursos compartidos
│       ├── Fonts/                # Fuentes personalizadas
│       ├── Images/               # Imágenes comunes
│       └── Icons/                # Iconos del sistema
│
├── 🏗️ Core/                      # Infraestructura y servicios base
│   ├── Services/                 # Servicios principales
│   │   ├── Api/                  # Cliente API
│   │   │   ├── ApiClient.cs      # Cliente HTTP base
│   │   │   └── ApiEndpoints.cs   # Endpoints del backend
│   │   │
│   │   ├── Authentication/       # Autenticación
│   │   │   ├── IAuthService.cs
│   │   │   └── AuthService.cs
│   │   │
│   │   ├── Navigation/           # Navegación
│   │   │   ├── INavigationService.cs
│   │   │   └── NavigationService.cs
│   │   │
│   │   ├── Dialog/               # Diálogos
│   │   │   ├── IDialogService.cs
│   │   │   └── DialogService.cs
│   │   │
│   │   ├── Cache/                # Cache local
│   │   │   ├── ICacheService.cs
│   │   │   └── CacheService.cs
│   │   │
│   │   ├── Offline/              # Sincronización offline
│   │   │   ├── IOfflineService.cs
│   │   │   └── OfflineService.cs
│   │   │
│   │   └── Notifications/        # Notificaciones push
│   │       ├── INotificationService.cs
│   │       └── NotificationService.cs
│   │
│   ├── Models/                   # Modelos de datos
│   │   ├── DTOs/                 # Objetos de transferencia
│   │   │   ├── MesaDto.cs
│   │   │   ├── ComandaDto.cs
│   │   │   └── ProductoDto.cs
│   │   │
│   │   ├── ViewModels/           # ViewModels base
│   │   │   ├── BaseViewModel.cs
│   │   │   └── BasePageViewModel.cs
│   │   │
│   │   └── Entities/             # Entidades locales
│   │       ├── LocalMesa.cs
│   │       └── LocalComanda.cs
│   │
│   ├── Extensions/               # Métodos de extensión
│   │   ├── StringExtensions.cs
│   │   └── DateTimeExtensions.cs
│   │
│   ├── Helpers/                  # Clases de ayuda
│   │   ├── ApiHelper.cs
│   │   └── ValidationHelper.cs
│   │
│   └── Constants/                # Constantes globales
│       ├── ApiConstants.cs
│       └── AppConstants.cs
│
├── 🎨 UI/                        # Componentes de interfaz
│   ├── Pages/                    # Páginas principales
│   │   ├── MainPage.xaml         # Página principal
│   │   └── DashboardPage.xaml    # Dashboard operativo
│   │
│   ├── Views/                    # Vistas reutilizables
│   │   ├── HeaderView.xaml       # Encabezado común
│   │   └── FooterView.xaml       # Pie de página
│   │
│   ├── Popups/                   # Popups y modales
│   │   ├── ConfirmationPopup.xaml
│   │   └── LoadingPopup.xaml
│   │
│   └── Templates/                # Plantillas de datos
│       ├── TableTemplate.xaml
│       └── OrderTemplate.xaml
│
├── 📱 Platforms/                 # Código específico por plataforma
│   ├── Android/                  # Configuración Android
│   │   ├── MainActivity.cs
│   │   └── AndroidManifest.xml
│   │
│   ├── iOS/                      # Configuración iOS
│   │   ├── AppDelegate.cs
│   │   └── Info.plist
│   │
│   └── Windows/                  # Configuración Windows
│       ├── App.xaml
│       └── Package.appxmanifest
│
├── 🔧 Config/                    # Configuración de la aplicación
│   ├── AppSettings.cs            # Configuración general
│   ├── ApiConfig.cs              # Configuración de API
│   └── ThemeConfig.cs            # Configuración de temas
│
├── App.xaml                      # Aplicación principal
├── App.xaml.cs                   # Lógica de aplicación
├── AppShell.xaml                 # Shell de navegación
├── AppShell.xaml.cs              # Lógica del shell
├── MauiProgram.cs                # Configuración MAUI
└── RestaurantePro.Mobile.csproj  # Archivo de proyecto
```

## 🏗️ **Arquitectura MVVM**

Cada funcionalidad sigue el patrón MVVM:

- **Pages/**: Páginas XAML con interfaz de usuario
- **ViewModels/**: Lógica de presentación y binding
- **Services/**: Lógica de negocio y comunicación con API

## 🔗 **Integración con Backend**

Consume **11 controladores** del backend de 21 total:
- `AuthController`, `MesasController`, `ComandasController`
- `PreparacionesController`, `PreparacionesDiariasController`
- `ReservacionesController`, `FacturacionController`
- `ProductosController`, `CategoriasController`
- `AnalyticsController`, `NotificacionesController`

## 🎯 **Características Principales**

- **Multiplataforma**: Android, iOS, Windows
- **Tiempo real**: SignalR para notificaciones
- **Offline**: Sincronización automática
- **Moderna**: .NET MAUI 8.0 + MVVM
- **Probada**: XUnit + Moq para testing
- **Escalable**: Clean Architecture

---

*Esta estructura está optimizada para operaciones diarias críticas del restaurante, evitando funcionalidades administrativas que corresponden a otras aplicaciones.* 