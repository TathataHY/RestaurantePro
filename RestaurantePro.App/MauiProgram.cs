using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RestaurantePro.App.Services;
using RestaurantePro.App.ViewModels;
using CommunityToolkit.Maui;
using RestaurantePro.App.Views;
using Microsoft.AspNetCore.SignalR.Client;

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

            // Registrar servicios como singleton
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthorizationService>();
            builder.Services.AddSingleton<SignalRService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IAuthorizationService, AuthorizationService>();
            builder.Services.AddSingleton<PushNotificationService>();

            // Registrar los ViewModels
            builder.Services.AddTransient<BaseViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<UsuarioViewModel>();
            builder.Services.AddTransient<ComandaViewModel>();
            builder.Services.AddTransient<ComandaDetailViewModel>();
            builder.Services.AddTransient<ComandaDetallesViewModel>();
            builder.Services.AddTransient<AddDetalleViewModel>();
            builder.Services.AddTransient<PlatoViewModel>();
            builder.Services.AddTransient<PlatoDetailViewModel>();
            builder.Services.AddTransient<MesaViewModel>();
            builder.Services.AddTransient<MesaDetailViewModel>();
            builder.Services.AddTransient<MenuViewModel>();
            builder.Services.AddTransient<ReporteVentaViewModel>();
            builder.Services.AddTransient<UsuarioDetailViewModel>();
            builder.Services.AddTransient<CocinaViewModel>();
            builder.Services.AddTransient<MeseroViewModel>();

            // Registrar las vistas
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<UsuarioPage>();
            builder.Services.AddTransient<ComandaPage>();
            builder.Services.AddTransient<ComandaDetallesPage>();
            builder.Services.AddTransient<AddDetallePage>();
            builder.Services.AddTransient<PlatoDetailPage>();
            builder.Services.AddTransient<UsuarioDetailPage>();
            builder.Services.AddTransient<MesaDetailPage>();
            builder.Services.AddTransient<MenuPage>();
            builder.Services.AddTransient<ReporteVentaPage>();
            builder.Services.AddTransient<CocinaPage>();
            builder.Services.AddTransient<MeseroPage>();

            // Configurar logging
#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Agregar después de los servicios existentes
            builder.Services.AddSingleton<HubConnection>(_ =>
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl("http://your-api-url/comandaHub")
                    .WithAutomaticReconnect()
                    .Build();

                return hubConnection;
            });

            var app = builder.Build();

            // Inicializar el servicio de localización de dependencias
            ServiceLocator.Init(app.Services);

            return app;
        }
    }
}
