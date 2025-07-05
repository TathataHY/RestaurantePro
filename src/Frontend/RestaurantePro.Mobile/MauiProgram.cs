using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RestaurantePro.Mobile.Config;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Features.Authentication.Pages;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Features.Operations.Comandas.Pages;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Features.Operations.Productos.Pages;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Features.Operations.Mesas.Pages;
using RestaurantePro.Mobile.UI.Pages;

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

		// Configurar HttpClient con URL base del backend
		builder.Services.AddHttpClient<IApiService, ApiService>(client =>
		{
			// URL del backend RestaurantePro
			client.BaseAddress = new Uri("https://localhost:7071/"); // Ajustar según tu backend
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Configurar HttpClient para AuthService
		builder.Services.AddHttpClient<AuthService>(client =>
		{
			client.BaseAddress = new Uri("https://localhost:7071/"); // Ajustar según tu backend
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Registrar servicios fundamentales - V1
		RegisterCoreServicesV1(builder.Services);

		// Registrar páginas y ViewModels - V1
		RegisterViewsAndViewModelsV1(builder.Services);

		// ✅ Servicios Operativos
		builder.Services.AddScoped<RestaurantePro.Mobile.Core.Services.Mesas.IMesasService, RestaurantePro.Mobile.Core.Services.Mesas.MesasService>();
		builder.Services.AddScoped<RestaurantePro.Mobile.Core.Services.Comandas.IComandasService, RestaurantePro.Mobile.Core.Services.Comandas.ComandasService>();
		builder.Services.AddScoped<IProductosService, ProductosService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void RegisterCoreServicesV1(IServiceCollection services)
	{
		// Servicios fundamentales V1
		services.AddSingleton<INavigationService, NavigationService>();
		services.AddSingleton<IDialogService, DialogService>();
		services.AddSingleton<IAuthService, AuthService>();
		services.AddSingleton<IApiService, ApiService>();
		
		// Servicios de dominio V1
		services.AddSingleton<IMesasService, MesasService>();
		services.AddSingleton<IComandasService, ComandasService>();
		services.AddSingleton<IProductosService, ProductosService>();
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

		// ✅ ViewModels (ahora desde Mobile.Core)
		services.AddTransient<MesasViewModel>();
		services.AddTransient<MesaDetalleViewModel>();
		services.AddTransient<ComandasViewModel>();
		services.AddTransient<ProductosViewModel>();

		// ✅ Páginas
		services.AddTransient<LoginPage>();
		services.AddTransient<DashboardPage>();
		services.AddTransient<MesasPage>();
		services.AddTransient<MesaDetallePage>();
		services.AddTransient<ComandasPage>();
		services.AddTransient<ProductosPage>();
	}
}
