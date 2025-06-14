using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.ExternalServices.Email;
using RestaurantePro.Infrastructure.ExternalServices.Email.Models;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración de servicios externos
    /// </summary>
    public static class ExternalServicesSetup
    {
        /// <summary>
        /// Agrega servicios externos a la colección de servicios
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios con los servicios externos registrados</returns>
        public static IServiceCollection AddExternalServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuración de Email
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Servicios de Email
            ConfigurarServiciosEmail(services, configuration);

            // TODO: Agregar configuración de servicios de pago (PayPal, Stripe) - Implementación futura
            
            // TODO: Agregar configuración de servicios SMS (Twilio) - Implementación futura

            // TODO: Agregar configuración de servicios de almacenamiento de archivos - Implementación futura
            
            return services;
        }

        private static void ConfigurarServiciosEmail(IServiceCollection services, IConfiguration configuration)
        {
            // Por ahora solo usamos la implementación SMTP estándar
            // La implementación SendGrid está comentada en el código para implementación futura
            services.AddScoped<IEmailService, EmailService>();
            
            /*
            // Código para seleccionar el proveedor según la configuración - Implementación futura
            var emailProvider = configuration.GetValue<string>("EmailSettings:Provider")?.ToLower() ?? "smtp";

            switch (emailProvider)
            {
                case "sendgrid":
                    services.AddScoped<IEmailService, SendGridService>();
                    break;
                case "smtp":
                default:
                    services.AddScoped<IEmailService, EmailService>();
                    break;
            }
            */
        }
    }
} 