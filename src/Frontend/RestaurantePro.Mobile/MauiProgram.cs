using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RestaurantePro.Mobile.Config;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Features.Authentication.Pages;
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
using System;
using System.Net.Http.Headers;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile;

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

		// Configurar HttpClient con URL base del backend y autenticación básica
		builder.Services.AddHttpClient<RestaurantePro.Mobile.Core.Services.Api.IApiService, RestaurantePro.Mobile.Core.Services.Api.ApiService>(client =>
		{
			// URL del backend RestaurantePro (cambiar a tu URL de hosting)
			client.BaseAddress = new Uri(ApiConfig.BaseUrl);
			client.Timeout = ApiConfig.RequestTimeout;
			
			// Configurar autenticación básica para el hosting
			var credentials = ApiConfig.HostingCredentials.GetEncodedCredentials();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
		});

		// Configurar HttpClient para AuthService
		builder.Services.AddHttpClient<RestaurantePro.Mobile.Core.Services.Authentication.AuthService>(client =>
		{
			client.BaseAddress = new Uri(ApiConfig.BaseUrl);
			client.Timeout = ApiConfig.RequestTimeout;
			
			// Configurar autenticación básica para el hosting
			var credentials = ApiConfig.HostingCredentials.GetEncodedCredentials();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
		});

		// Registrar servicios fundamentales - V1
		RegisterCoreServicesV1(builder.Services);

		// Registrar páginas y ViewModels - V1
		RegisterViewsAndViewModelsV1(builder.Services);

		// Registrar rutas de navegación - V1
		RegisterNavigationRoutesV1();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void RegisterCoreServicesV1(IServiceCollection services)
	{
		// Servicios fundamentales V1 - Solo desde Core
		services.AddSingleton<INavigationService, MauiNavigationService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Dialog.IDialogService, RestaurantePro.Mobile.Core.Services.Dialog.DialogService>();
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

		// ViewModels desde Mobile.Core - Operaciones
		services.AddTransient<MesasViewModel>();
		services.AddTransient<MesaDetalleViewModel>();
		services.AddTransient<ComandasViewModel>();
		services.AddTransient<ComandaDetalleViewModel>();
		services.AddTransient<ProductosViewModel>();
		services.AddTransient<ProductoDetalleViewModel>();

		// ViewModels desde Mobile.Core - Inventario
		services.AddTransient<PreparacionesViewModel>();
		services.AddTransient<ReservacionesViewModel>();
		services.AddTransient<IngredientesViewModel>();

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
		services.AddTransient<ComandasPage>();
		services.AddTransient<ComandaDetallePage>();
		services.AddTransient<ProductosPage>();
		services.AddTransient<ProductoDetallePage>();

		// Páginas - Inventario
		services.AddTransient<PreparacionesPage>();
		services.AddTransient<ReservacionesPage>();
		services.AddTransient<IngredientesPage>();

		// Páginas - Comercial
		services.AddTransient<FacturasPage>();
		services.AddTransient<ClientesPage>();
		services.AddTransient<TarjetasFidelizacionPage>();

		// Páginas - Categorías y Analytics
		services.AddTransient<CategoriasPage>();
		services.AddTransient<AnalyticsPage>();
	}

	private static void RegisterNavigationRoutesV1()
	{
		// Rutas principales
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("dashboard", typeof(DashboardPage));
		
		// Rutas de operaciones
		Routing.RegisterRoute("mesas", typeof(MesasPage));
		Routing.RegisterRoute("mesadetalle", typeof(MesaDetallePage));
		Routing.RegisterRoute("comandas", typeof(ComandasPage));
		Routing.RegisterRoute("comandadetalle", typeof(ComandaDetallePage));
		Routing.RegisterRoute("productos", typeof(ProductosPage));
		Routing.RegisterRoute("productodetalle", typeof(ProductoDetallePage));
		
		// Rutas de inventario
		Routing.RegisterRoute("preparaciones", typeof(PreparacionesPage));
		Routing.RegisterRoute("reservaciones", typeof(ReservacionesPage));
		Routing.RegisterRoute("ingredientes", typeof(IngredientesPage));
		
		// Rutas comerciales
		Routing.RegisterRoute("facturas", typeof(FacturasPage));
		Routing.RegisterRoute("clientes", typeof(ClientesPage));
		Routing.RegisterRoute("tarjetasfidelizacion", typeof(TarjetasFidelizacionPage));
		
		// Rutas de categorías y analytics
		Routing.RegisterRoute("categorias", typeof(CategoriasPage));
		Routing.RegisterRoute("analytics", typeof(AnalyticsPage));
	}
}
