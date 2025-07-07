using Microsoft.Extensions.Logging;
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
		builder.Services.AddHttpClient<RestaurantePro.Mobile.Core.Services.Api.IApiService, RestaurantePro.Mobile.Core.Services.Api.ApiService>(client =>
		{
			// URL del backend RestaurantePro
			client.BaseAddress = new Uri("https://localhost:7071/"); // Ajustar según tu backend
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Configurar HttpClient para AuthService
		builder.Services.AddHttpClient<RestaurantePro.Mobile.Core.Services.Authentication.AuthService>(client =>
		{
			client.BaseAddress = new Uri("https://localhost:7071/"); // Ajustar según tu backend
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Registrar servicios fundamentales - V1
		RegisterCoreServicesV1(builder.Services);

		// Registrar páginas y ViewModels - V1
		RegisterViewsAndViewModelsV1(builder.Services);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void RegisterCoreServicesV1(IServiceCollection services)
	{
		// Servicios fundamentales V1 - Solo desde Core
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Navigation.INavigationService, RestaurantePro.Mobile.Core.Services.Navigation.NavigationService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Dialog.IDialogService, RestaurantePro.Mobile.Core.Services.Dialog.DialogService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService, RestaurantePro.Mobile.Core.Services.Authentication.AuthService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Api.IApiService, RestaurantePro.Mobile.Core.Services.Api.ApiService>();
		
		// Servicios de dominio V1 - Solo desde Core
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Mesas.IMesasService, RestaurantePro.Mobile.Core.Services.Mesas.MesasService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Comandas.IComandasService, RestaurantePro.Mobile.Core.Services.Comandas.ComandasService>();
		services.AddSingleton<RestaurantePro.Mobile.Core.Services.Productos.IProductosService, RestaurantePro.Mobile.Core.Services.Productos.ProductosService>();
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

		// ViewModels desde Mobile.Core
		services.AddTransient<MesasViewModel>();
		services.AddTransient<MesaDetalleViewModel>();
		services.AddTransient<ComandasViewModel>();
		services.AddTransient<ComandaDetalleViewModel>();
		services.AddTransient<ProductosViewModel>();
		services.AddTransient<ProductoDetalleViewModel>();

		// Páginas
		services.AddTransient<MesasPage>();
		services.AddTransient<MesaDetallePage>();
		services.AddTransient<ComandasPage>();
		services.AddTransient<ComandaDetallePage>();
		services.AddTransient<ProductosPage>();
		services.AddTransient<ProductoDetallePage>();
	}
}
