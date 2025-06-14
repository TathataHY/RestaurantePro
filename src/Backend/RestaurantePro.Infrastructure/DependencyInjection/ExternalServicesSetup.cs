using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.ExternalServices.Email;
using RestaurantePro.Infrastructure.ExternalServices.FileStorage;
using RestaurantePro.Infrastructure.ExternalServices.Payment;
using RestaurantePro.Infrastructure.ExternalServices.SMS;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración de servicios externos
    /// </summary>
    public static class ExternalServicesSetup
    {
        /// <summary>
        /// Registra los servicios externos en el contenedor de dependencias
        /// </summary>
        public static IServiceCollection AddExternalServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Registrar servicios de correo electrónico
            services.AddEmailServices(configuration);
            
            // Registrar servicios de almacenamiento de archivos
            services.AddFileStorageServices(configuration);
            
            // Registrar servicios de pago
            services.AddPaymentServices(configuration);
            
            // Registrar servicios de SMS
            services.AddSMSServices(configuration);
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de correo electrónico
        /// </summary>
        private static IServiceCollection AddEmailServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Registrar el servicio de correo electrónico principal
            services.AddScoped<IEmailService, EmailService>();
            
            // Registrar el servicio de SendGrid (comentado para implementación futura)
            // if (configuration.GetValue<bool>("Email:UseSendGrid"))
            // {
            //     services.AddScoped<IEmailService, SendGridService>();
            // }
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de almacenamiento de archivos
        /// </summary>
        private static IServiceCollection AddFileStorageServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Determinar qué proveedor de almacenamiento usar
            var storageProvider = configuration["FileStorage:Provider"]?.ToLower() ?? "local";
            
            switch (storageProvider)
            {
                case "azure":
                    services.AddScoped<IFileStorageService, AzureBlobService>();
                    break;
                    
                case "local":
                default:
                    services.AddScoped<IFileStorageService, LocalFileService>();
                    break;
            }
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de pago
        /// </summary>
        private static IServiceCollection AddPaymentServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Determinar qué proveedor de pagos usar
            var paymentProvider = configuration["Payment:Provider"]?.ToLower() ?? "none";
            
            switch (paymentProvider)
            {
                case "stripe":
                    services.AddScoped<IPaymentService, StripeService>();
                    break;
                    
                case "paypal":
                    services.AddScoped<IPaymentService, PayPalService>();
                    break;
                    
                case "none":
                default:
                    // Implementación simulada para desarrollo
                    services.AddScoped<IPaymentService, StripeService>();
                    break;
            }
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de SMS
        /// </summary>
        private static IServiceCollection AddSMSServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Registrar el servicio de SMS
            services.AddScoped<ISMSService, TwilioService>();
            
            return services;
        }
    }
} 