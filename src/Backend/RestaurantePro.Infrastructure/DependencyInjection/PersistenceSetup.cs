using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Interfaces;
using RestaurantePro.Domain.Comercial.Interfaces;
using RestaurantePro.Domain.Operaciones.Interfaces;
using RestaurantePro.Domain.Inventario.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Application.Common.Interfaces.Repositories;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración de los servicios de persistencia
    /// </summary>
    public static class PersistenceSetup
    {
        /// <summary>
        /// Registra los servicios de persistencia en el contenedor de inyección de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios con los servicios de persistencia registrados</returns>
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar interceptores
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DomainEventInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();

            // Registrar contextos de base de datos por dominio
            RegisterDbContexts(services, configuration);

            // Registrar UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar repositorios por dominio
            RegisterRepositories(services);

            return services;
        }

        private static void RegisterDbContexts(IServiceCollection services, IConfiguration configuration)
        {
            var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

            // Contexto principal
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseSqlServer(
                    defaultConnectionString,
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));

            // Core
            services.AddDbContext<CoreDbContext>(options =>
                options.UseSqlServer(
                    defaultConnectionString,
                    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistoryCore", "Core")));

            // Comercial
            services.AddDbContext<ComercialDbContext>(options =>
                options.UseSqlServer(
                    defaultConnectionString,
                    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistoryComercial", "Comercial")));

            // Operaciones
            services.AddDbContext<OperacionesDbContext>(options =>
                options.UseSqlServer(
                    defaultConnectionString,
                    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistoryOperaciones", "Operaciones")));

            // Inventario
            services.AddDbContext<InventarioDbContext>(options =>
                options.UseSqlServer(
                    defaultConnectionString,
                    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistoryInventario", "Inventario")));

            // Proveedores
            services.AddDbContext<ProveedoresDbContext>(options =>
                options.UseSqlServer(
                    defaultConnectionString,
                    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistoryProveedores", "Proveedores")));
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Repositorios del dominio Core
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            services.AddScoped<IRecetaRepository, RecetaRepository>();
            
            // Repositorios del dominio Comercial
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
            
            // Repositorios del dominio Operaciones
            services.AddScoped<IComandaRepository, ComandaRepository>();
            services.AddScoped<IReservacionRepository, ReservacionRepository>();
            services.AddScoped<IMesaRepository, MesaRepository>();
            services.AddScoped<IPreparacionDiariaRepository, PreparacionDiariaRepository>();
            
            // Repositorios del dominio Inventario
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
            services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
            
            // Repositorios del dominio Proveedores
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<IContactoProveedorRepository, ContactoProveedorRepository>();
        }
    }
} 