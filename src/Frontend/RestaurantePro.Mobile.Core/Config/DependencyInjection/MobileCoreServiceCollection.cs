using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Categorias;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Inventory;

namespace RestaurantePro.Mobile.Core.Config.DependencyInjection;

/// <summary>
/// Extensiones para configurar servicios de Mobile.Core
/// </summary>
public static class MobileCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios de Mobile.Core
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>La colección de servicios con los servicios de Mobile.Core agregados</returns>
    public static IServiceCollection AddMobileCoreServices(this IServiceCollection services)
    {
        // Servicios de infraestructura
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IDialogService, DialogService>();

        // Servicios de operaciones
        services.AddScoped<IMesasService, MesasService>();
        services.AddScoped<IComandasService, ComandasService>();

        // Servicios de catálogo
        services.AddScoped<IProductosService, ProductosService>();
        services.AddScoped<ICategoriasService, CategoriasService>();

        // Servicios comerciales
        services.AddScoped<IFacturasService, FacturasService>();
        services.AddScoped<IClientesService, ClientesService>();
        services.AddScoped<ITarjetasFidelizacionService, TarjetasFidelizacionService>();

        // Servicios de inventario
        services.AddScoped<IIngredientesService, IngredientesService>();
        services.AddScoped<IPreparacionesService, PreparacionesService>();
        services.AddScoped<IReservacionesService, ReservacionesService>();

        // Servicios de analytics
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }
} 