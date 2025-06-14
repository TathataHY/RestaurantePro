using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Infrastructure.ExternalServices.Email;
using RestaurantePro.Infrastructure.ExternalServices.Email.Models;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extensiones para registrar servicios de infraestructura en la inyección de dependencias
    /// </summary>
    public static class InfrastructureServiceCollectionExtensions
    {
        /// <summary>
        /// Agrega los servicios de infraestructura a la colección de servicios
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>La colección de servicios con los servicios de infraestructura agregados</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar contexto de base de datos
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));

            // Registrar Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar repositorios
            services.AddRepositories();

            // Registrar servicios de comunicación
            services.AddCommunicationServices();

            // Registrar servicio de fecha y hora
            var dateTimeService = new DateTimeService();
            services.AddSingleton<IDateTimeService>(dateTimeService);

            return services;
        }

        /// <summary>
        /// Agrega los servicios de identidad a la colección de servicios
        /// </summary>
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Aquí se registrarían los servicios de identidad
            // Por ahora solo un placeholder para evitar errores
            return services;
        }

        /// <summary>
        /// Agrega los servicios externos a la colección de servicios
        /// </summary>
        public static IServiceCollection AddExternalServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuración de servicios de email
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // Aquí se agregarían otros servicios externos como
            // - APIs de terceros
            // - Servicios de almacenamiento en la nube
            // - Pasarelas de pago
            // - Servicios de mensajería SMS
            // - Etc.

            return services;
        }

        /// <summary>
        /// Registra todos los repositorios en la colección de servicios
        /// </summary>
        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Repositorio genérico
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Repositorios específicos de contexto
            
            // Comercial
            services.AddScoped<IClienteRepository, ClienteRepository>();
            
            // Core
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            
            // Inventario
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            
            // Operaciones - Comandas
            services.AddScoped<IComandaRepository, ComandaRepository>();
            
            // Operaciones - Reservaciones
            services.AddScoped<IMesaRepository, MesaRepository>();
            services.AddScoped<IReservacionRepository, ReservacionRepository>();
            
            // Proveedores
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            
            // Repositorios de Operaciones
            services.AddScoped<IPreparacionRepository, PreparacionRepository>();
            
            return services;
        }

        /// <summary>
        /// Registra servicios de comunicación (Email, SMS)
        /// </summary>
        private static IServiceCollection AddCommunicationServices(this IServiceCollection services)
        {
            // Servicios de comunicación básicos serán registrados en AddExternalServices
            // Aquí se registrarían otros servicios de comunicación interna si son necesarios
            
            return services;
        }
    }
} 