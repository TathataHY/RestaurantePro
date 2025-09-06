using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RestaurantePro.Mobile.Config;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Features.Authentication.Pages;
using RestaurantePro.Mobile.Views;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Features.Operations.Comandas.Pages;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Features.Operations.Productos.Pages;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Features.Operations.Mesas.Pages;
using RestaurantePro.Mobile.Core.Features.Inventory.Preparaciones.ViewModels;
using RestaurantePro.Mobile.Features.Inventory.Preparaciones.Pages;
using RestaurantePro.Mobile.Core.Features.Inventory.Reservaciones.ViewModels;
using RestaurantePro.Mobile.Features.Inventory.Reservaciones.Pages;
using RestaurantePro.Mobile.Core.Features.Commercial.Billing.ViewModels;
using RestaurantePro.Mobile.Features.Commercial.Billing.Pages;
using RestaurantePro.Mobile.Core.Features.Commercial.Customers.ViewModels;
using RestaurantePro.Mobile.Features.Commercial.Customers.Pages;
using RestaurantePro.Mobile.Core.Features.Commercial.Loyalty.ViewModels;
using RestaurantePro.Mobile.Features.Commercial.Loyalty.Pages;
using RestaurantePro.Mobile.Core.Features.Inventory.Ingredients.ViewModels;
using RestaurantePro.Mobile.Features.Inventory.Ingredients.Pages;
using RestaurantePro.Mobile.Core.Features.Categorias.ViewModels;
using RestaurantePro.Mobile.Features.Categorias.Pages;
using RestaurantePro.Mobile.Core.Features.Analytics.ViewModels;
using RestaurantePro.Mobile.Features.Analytics.Pages;
using RestaurantePro.Mobile.UI.Pages;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Views;
using RestaurantePro.Mobile.Core.Features.Onboarding.ViewModels;
using RestaurantePro.Mobile.ViewModels;
using System;
using System.Net.Http.Headers;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Services;
using System.Net.Http;
using CommunityToolkit.Maui;

namespace RestaurantePro.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Configurar HttpClient con URL base del backend y autenticación básica según perfil
		builder.Services.AddHttpClient<RestaurantePro.Mobile.Core.Services.Api.IApiService, RestaurantePro.Mobile.Core.Services.Api.ApiService>(client =>
		{
			// URL del backend por entorno/cliente
			client.BaseAddress = new Uri(ApiConfig.GetBaseUrl());
			client.Timeout = ApiConfig.RequestTimeout;
			
			// Configurar autenticación básica solo si el perfil lo requiere
			var basic = ApiConfig.GetEncodedBasicCredentials();
			if (!string.IsNullOrEmpty(basic))
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
			}
			// Evitar conexiones mantenidas si el hosting cierra abruptamente
			client.DefaultRequestHeaders.ConnectionClose = true;
		})
		.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			// Ignorar errores de certificado SSL en desarrollo (necesario para WSA)
#if DEBUG
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
#endif
		});

		// Configurar HttpClient para AuthService
		builder.Services.AddHttpClient<RestaurantePro.Mobile.Core.Services.Authentication.AuthService>(client =>
		{
			client.BaseAddress = new Uri(ApiConfig.GetBaseUrl());
			client.Timeout = ApiConfig.RequestTimeout;
			
			// Configurar autenticación básica solo si el perfil lo requiere
			var basic = ApiConfig.GetEncodedBasicCredentials();
			if (!string.IsNullOrEmpty(basic))
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
			}
			// Evitar conexiones mantenidas si el hosting cierra abruptamente
			client.DefaultRequestHeaders.ConnectionClose = true;
		})
		.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			// Ignorar errores de certificado SSL en desarrollo (necesario para WSA)
#if DEBUG
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
#endif
		});

		// Registrar servicios fundamentales - V1
		RegisterCoreServicesV1(builder.Services);

		// Registrar páginas y ViewModels - V1
		RegisterViewsAndViewModelsV1(builder.Services);

		// COMENTADO: Las rutas se registran ahora en AppShell.xaml.cs para evitar duplicados
		// RegisterNavigationRoutesV1();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void RegisterCoreServicesV1(IServiceCollection services)
	{
		// Servicios fundamentales V1 - Solo desde Core
		services.AddSingleton<INavigationService, RestaurantePro.Mobile.Services.NavigationService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Dialog.IDialogService, RestaurantePro.Mobile.Services.DialogService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Platform.ISecureStorageService, RestaurantePro.Mobile.Core.Services.Platform.SecureStorageService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Realtime.IComandaRealtimeService, RestaurantePro.Mobile.Services.ComandaRealtimeService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Notifications.INotificationService, RestaurantePro.Mobile.Services.NotificationService>();
		
		// Servicios V4 - Sistema de Temas
		services.AddSingleton<RestaurantePro.Mobile.Services.ThemeService>();
		
		// Servicios V4 - Optimización de Performance
		services.AddSingleton<RestaurantePro.Mobile.Services.ImageOptimizationService>();
		services.AddSingleton<RestaurantePro.Mobile.Services.AnimationOptimizationService>();
		services.AddSingleton<RestaurantePro.Mobile.Services.PerformanceService>();
		services.AddSingleton<RestaurantePro.Mobile.Services.CacheService>();
		services.AddSingleton<RestaurantePro.Mobile.Services.LazyLoadingService>();
		
		// Servicios V4 - Accesibilidad
		services.AddSingleton<RestaurantePro.Mobile.Services.AccessibilityService>();
		
		// Servicios V4 - Internacionalización
		services.AddSingleton<RestaurantePro.Mobile.Services.LocalizationService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService, RestaurantePro.Mobile.Core.Services.Authentication.AuthService>();
		
		// Servicios de dominio V1 - Solo desde Core
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Mesas.IMesasService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Mesas.MesasService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Comandas.IComandasService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Comandas.ComandasService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Productos.IProductosService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Productos.ProductosService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		
		// Servicios de Inventario V1
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Inventory.IPreparacionesService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Inventory.PreparacionesService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		
		// Servicio de Preparaciones Diarias V1
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.IDailyPreparationsService>(sp =>
			new RestaurantePro.Mobile.Core.Services.DailyPreparationsService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>(),
				sp.GetRequiredService<ILogger<RestaurantePro.Mobile.Core.Services.DailyPreparationsService>>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Inventory.IReservacionesService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Inventory.ReservacionesService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Inventory.IIngredientesService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Inventory.IngredientesService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		
		// Servicios Comerciales V1
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Commercial.IFacturasService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Commercial.FacturasService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Commercial.IClientesService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Commercial.ClientesService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Commercial.ITarjetasFidelizacionService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Commercial.TarjetasFidelizacionService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		
		// Servicios de Categorías y Analytics V1
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Categorias.ICategoriasService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Categorias.CategoriasService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Analytics.IAnalyticsService>(sp =>
			new RestaurantePro.Mobile.Core.Services.Analytics.AnalyticsService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>()));
		
		// Servicios V4 - Modernización Visual
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Preferences.IPreferencesService, RestaurantePro.Mobile.Core.Services.Preferences.PreferencesService>();
	}

	private static void RegisterViewsAndViewModelsV1(IServiceCollection services)
	{
		// Páginas básicas V1
		services.AddTransient<MainPage>();
		
		// Authentication Feature - V1 Fundamental
		services.AddTransient<LoginViewModel>();
		services.AddTransient<LoginPage>();
		
		// Dashboard - V1 Fundamental
		services.AddTransient<DashboardPage>();
		
		// Modern Dashboard - V4 Modernización Visual
		services.AddTransient<ModernDashboardViewModel>();
		services.AddTransient<ModernDashboardPage>();
		
		// Dashboard Service
		services.AddTransient<IDashboardService>(sp =>
			new DashboardService(
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Api.IApiService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Analytics.IAnalyticsService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Mesas.IMesasService>(),
				sp.GetRequiredService<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService>(),
				sp.GetRequiredService<ILogger<DashboardService>>()));

		// ViewModels desde Mobile.Core - Operaciones
		services.AddTransient<MesasViewModel>();
		services.AddTransient<MesaDetalleViewModel>();
		services.AddTransient<ComandasViewModel>();
		services.AddTransient<ComandaDetalleViewModel>();
		services.AddTransient<CrearComandaViewModel>();
		services.AddTransient<ProductosViewModel>();
		services.AddTransient<ProductoDetalleViewModel>();
		services.AddTransient<ProductoEditorViewModel>();
		services.AddTransient<RestaurantePro.Mobile.Core.Features.Operations.Cocina.ViewModels.ModernCocinaViewModel>();

		// ViewModels desde Mobile.Core - Inventario
		services.AddTransient<PreparacionesViewModel>();
		services.AddTransient<ReservacionesViewModel>();
		services.AddTransient<IngredientesViewModel>();
		
		// ViewModel de Preparaciones Diarias V1
		services.AddTransient<DailyPreparationsViewModel>();

		// ViewModels desde Mobile.Core - Comercial
		services.AddTransient<FacturasViewModel>();
		services.AddTransient<ClientesViewModel>();
		services.AddTransient<TarjetasFidelizacionViewModel>();

		// ViewModels desde Mobile.Core - Categorías y Analytics
		services.AddTransient<CategoriasViewModel>();
		services.AddTransient<AnalyticsViewModel>();

		// Páginas - Operaciones
		services.AddTransient<MesasPage>();
		services.AddTransient<MesaDetallePage>();
		services.AddTransient<ComandaDetallePage>();
		services.AddTransient<CrearComandaPage>();
		services.AddTransient<ProductosPage>();
		services.AddTransient<ProductoDetallePage>();
		services.AddTransient<ProductoEditorPage>();

		// Páginas - Inventario
		services.AddTransient<PreparacionesPage>();
		services.AddTransient<ReservacionesPage>();
		services.AddTransient<IngredientesPage>();
		
		// Página de Preparaciones Diarias V1
		services.AddTransient<DailyPreparationsPage>();

		// Páginas - Comercial
		services.AddTransient<FacturasPage>();
		services.AddTransient<ClientesPage>();
		services.AddTransient<TarjetasFidelizacionPage>();

		// Páginas - Categorías y Analytics
		services.AddTransient<CategoriasPage>();
		services.AddTransient<AnalyticsPage>();
		
		// ViewModels y Páginas V4 - Modernización Visual
		services.AddTransient<RestaurantePro.Mobile.Core.Features.Onboarding.ViewModels.OnboardingViewModel>();
		services.AddTransient<OnboardingPage>();
		services.AddTransient<SplashPage>();
		
		// Páginas Modernas V4 - Operaciones
		services.AddTransient<ModernMesasPage>();
		services.AddTransient<ModernComandasPage>();
		services.AddTransient<ModernProductosPage>();
		services.AddTransient<RestaurantePro.Mobile.Features.Operations.Cocina.Pages.ModernCocinaPage>();
		services.AddTransient<ModernIngredientesPage>();
		services.AddTransient<ModernReservacionesPage>();
		services.AddTransient<ModernFacturasPage>();
		services.AddTransient<ModernClientesPage>();
		
		// Páginas V4 - Configuración
		services.AddTransient<ConfiguracionPage>();
		services.AddTransient<RestaurantePro.Mobile.ViewModels.ConfiguracionViewModel>();
		
		// Páginas V4 - Performance Monitor
		services.AddTransient<PerformanceMonitorViewModel>();
		services.AddTransient<PerformanceMonitorPage>();
	}

	// COMENTADO: Todas las rutas se registran ahora en AppShell.xaml.cs para evitar duplicados
	/*
	private static void RegisterNavigationRoutesV1()
	{
		// Rutas principales
		Routing.RegisterRoute("login", typeof(SimpleLoginPage));
		Routing.RegisterRoute("dashboard", typeof(ModernDashboardPage));
		
		// Rutas de operaciones
		Routing.RegisterRoute("mesas", typeof(MesasPage));
		// Routing.RegisterRoute("mesadetalle", typeof(MesaDetallePage)); // Duplicada - ya registrada en AppShell.xaml.cs
		Routing.RegisterRoute("comandas", typeof(ComandasPage));
		Routing.RegisterRoute("comandadetalle", typeof(ComandaDetallePage));
		Routing.RegisterRoute("productos", typeof(ProductosPage));
		Routing.RegisterRoute("productodetalle", typeof(ProductoDetallePage));
		
		// Rutas de inventario
		Routing.RegisterRoute("preparaciones", typeof(PreparacionesPage));
		Routing.RegisterRoute("reservaciones", typeof(ReservacionesPage));
		Routing.RegisterRoute("ingredientes", typeof(IngredientesPage));
		
		// Ruta de Preparaciones Diarias V1
		Routing.RegisterRoute("dailypreparations", typeof(DailyPreparationsPage));
		
		// Rutas comerciales
		Routing.RegisterRoute("facturas", typeof(FacturasPage));
		Routing.RegisterRoute("clientes", typeof(ClientesPage));
		Routing.RegisterRoute("tarjetasfidelizacion", typeof(TarjetasFidelizacionPage));
		
		// Rutas de categorías y analytics
		Routing.RegisterRoute("categorias", typeof(CategoriasPage));
		Routing.RegisterRoute("analytics", typeof(AnalyticsPage));
		
		// Rutas V4 - Modernización Visual
		Routing.RegisterRoute("onboarding", typeof(OnboardingPage));
		Routing.RegisterRoute("splash", typeof(SplashPage));
		
		// Rutas Modernas V4 - Operaciones
		Routing.RegisterRoute("modernmesas", typeof(ModernMesasPage));
		Routing.RegisterRoute("moderncomandas", typeof(ModernComandasPage));
		Routing.RegisterRoute("modernproductos", typeof(ModernProductosPage));
		Routing.RegisterRoute("modernpreparaciones", typeof(ModernPreparacionesPage));
		Routing.RegisterRoute("moderningredientes", typeof(ModernIngredientesPage));
		Routing.RegisterRoute("modernreservaciones", typeof(ModernReservacionesPage));
		Routing.RegisterRoute("modernfacturas", typeof(ModernFacturasPage));
		Routing.RegisterRoute("modernclientes", typeof(ModernClientesPage));
		Routing.RegisterRoute("configuracion", typeof(ConfiguracionPage));
		Routing.RegisterRoute("performancemonitor", typeof(PerformanceMonitorPage));
	}
	*/
}
