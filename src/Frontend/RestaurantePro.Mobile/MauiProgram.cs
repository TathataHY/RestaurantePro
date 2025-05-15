using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RestaurantePro.Mobile.Config;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Features.Comercial.Clientes.ViewModels;
using RestaurantePro.Mobile.Features.Comercial.Clientes.Views;

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

		// Registrar configuración
		builder.Services.AddSingleton<IAppSettings, AppSettings>();

		// Registrar servicios core
		RegisterCoreServices(builder.Services);

		// Registrar servicios de API por módulo
		RegisterApiClients(builder.Services);

		// Registrar vistas y viewmodels por módulo
		RegisterViewsAndViewModels(builder.Services);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void RegisterCoreServices(IServiceCollection services)
	{
		// Servicios base
		services.AddSingleton<INavigationService, NavigationService>();
		services.AddSingleton<IDialogService, DialogService>();
		services.AddSingleton<ITokenService, TokenService>();
		
		// Cliente API base
		services.AddSingleton<ApiClient>();
	}

	private static void RegisterApiClients(IServiceCollection services)
	{
		// Módulo Comercial
		services.AddSingleton<IClientesApiClient, ClientesApiClient>();
		
		// Módulo Operaciones
		services.AddSingleton<IComandasApiClient, ComandasApiClient>();
	}

	private static void RegisterViewsAndViewModels(IServiceCollection services)
	{
		// Registrar páginas principales
		services.AddTransient<MainPage>();
		
		// Módulo Comercial - Clientes
		services.AddTransient<ClientesListViewModel>();
		services.AddTransient<ClientesListPage>();
		services.AddTransient<ClienteDetailViewModel>();
		services.AddTransient<ClienteDetailPage>();
		services.AddTransient<ClienteAddViewModel>();
		services.AddTransient<ClienteAddPage>();
		
		// Otros módulos se agregarán aquí a medida que se desarrollen
	}
}
