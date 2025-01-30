using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RestaurantePro.App.Services;
using RestaurantePro.App.ViewModels;
using CommunityToolkit.Maui;
using RestaurantePro.App.Views;

namespace RestaurantePro.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // Inicializar el MAUI Community Toolkit
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Registrar DatabaseService como un singleton
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthorizationService>();

            // Registrar los ViewModels
            builder.Services.AddTransient<BaseViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<UsuarioViewModel>();
            builder.Services.AddTransient<ComandaViewModel>();
            builder.Services.AddTransient<ComandaDetailViewModel>();
            builder.Services.AddTransient<AddDetalleViewModel>();
            builder.Services.AddTransient<PlatoViewModel>();
            builder.Services.AddTransient<PlatoDetailViewModel>();
            builder.Services.AddTransient<MesaViewModel>();
            builder.Services.AddTransient<MesaDetailViewModel>();
            builder.Services.AddTransient<MenuViewModel>();
            builder.Services.AddTransient<ReporteVentaViewModel>();
            builder.Services.AddTransient<UsuarioDetailViewModel>();

            // Registrar las vistas
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<UsuarioPage>();
            builder.Services.AddTransient<ComandaPage>();
            builder.Services.AddTransient<AddDetallePage>();
            builder.Services.AddTransient<PlatoPage>();
            builder.Services.AddTransient<PlatoDetailPage>();
            builder.Services.AddTransient<MesaPage>();
            builder.Services.AddTransient<MesaDetailPage>();
            builder.Services.AddTransient<MenuPage>();
            builder.Services.AddTransient<ReporteVentaPage>();
            builder.Services.AddTransient<UsuarioDetailPage>();

            // Configurar logging
#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Inicializar el servicio de localización de dependencias
            ServiceLocator.Init(app.Services);

            return app;
        }
    }
}
