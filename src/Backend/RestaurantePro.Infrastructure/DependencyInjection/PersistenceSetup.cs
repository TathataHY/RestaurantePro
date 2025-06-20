using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Services;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Demo;
using RestaurantePro.Infrastructure.Persistence.SeedData.Testing;

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
        /// <param name="isTestEnvironment">Indica si es un entorno de prueba</param>
        /// <returns>Colección de servicios con los servicios de persistencia registrados</returns>
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isTestEnvironment = false)
        {
            // Registrar interceptores
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DomainEventInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();

            // Registrar contextos de base de datos por dominio
            if (!isTestEnvironment)
            {
                RegisterDbContexts(services, configuration);
            }

            // Registrar UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar repositorios por dominio
            RegisterRepositories(services);

            // Registrar servicios adicionales necesarios
            RegisterAdditionalServices(services);

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
            services.AddScoped<IProductoCategoriaRepository, ProductoCategoriaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            
            // Repositorios del dominio Comercial
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
            services.AddScoped<IHistorialPuntosRepository, HistorialPuntosRepository>();
            services.AddScoped<ITransaccionPuntosRepository, TransaccionPuntosRepository>();
            services.AddScoped<IPromocionRepository, PromocionRepository>();
            
            // Repositorios del dominio Operaciones
            services.AddScoped<IComandaRepository, ComandaRepository>();
            services.AddScoped<IReservacionRepository, ReservacionRepository>();
            
            services.AddScoped<IMesaRepository>(provider => 
                new MesaRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<MesaRepository>>(),
                    provider.GetRequiredService<IDateTimeService>()
                ));

            services.AddScoped<IPreparacionRepository, PreparacionRepository>();
            
            // Repositorios del dominio Inventario
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
            services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
            services.AddScoped<IInventarioIngredientesRepository, InventarioIngredientesRepository>();
            
            // Repositorios del dominio Proveedores
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<IContactoProveedorRepository, ContactoProveedorRepository>();
        }

        private static void RegisterAdditionalServices(IServiceCollection services)
        {
            // Registrar IApplicationDbContext usando RestauranteProDbContext
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // Registrar DbContext base para repositorios que lo necesitan directamente
            services.AddScoped<DbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // Registrar servicios base requeridos por los contextos
            services.AddScoped<ICurrentUserService, Services.CurrentUserService>();
            services.AddScoped<IDateTimeService, DateTimeService>();
            
            // Registrar domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Registrar HttpContextAccessor necesario para CurrentUserService
            services.AddHttpContextAccessor();

            // Registrar servicios stub para Application layer
            services.AddScoped<ICommunicationService, Services.CommunicationService>();
            services.AddScoped<INotificationService, Services.NotificationService>();
            services.AddScoped<IAuditService, Services.AuditService>();
            
            // Registrar servicios de Application layer que implementan en Infrastructure
            services.AddScoped<Application.Comercial.Facturacion.Interfaces.IFacturacionService, Services.FacturacionService>();
            services.AddScoped<Application.Comercial.Fidelizacion.Interfaces.IFidelizacionService, Services.FidelizacionService>();
            services.AddScoped<Application.Operaciones.Mesas.Interfaces.IMesaService, Services.MesaService>();
            
            // Registrar sistema de SeedData
            RegisterSeedDataServices(services);
        }
        
        private static void RegisterSeedDataServices(IServiceCollection services)
        {
            // La configuración de SeedData se bindea desde appsettings.json en Program.cs
            // Estos son valores por defecto que pueden ser sobrescritos
            
            // Registrar SeedDataRunner
            services.AddScoped<SeedDataRunner>();
            
            // SEEDERS CRÍTICOS
            services.AddScoped<ISeedData, RolesSeeder>();
            services.AddScoped<ISeedData, UnidadesMedidaSeeder>();
            services.AddScoped<ISeedData, PermisosSeeder>();
            services.AddScoped<ISeedData, ConfiguracionSeeder>();
            services.AddScoped<ISeedData, EstadosSeeder>();
            services.AddScoped<ISeedData, UsuarioAdminSeeder>();
            
            // SEEDERS DEMO  
            services.AddScoped<ISeedData, ProductoCategoriasSeeder>();
            services.AddScoped<ISeedData, ProductosSeeder>();
            services.AddScoped<ISeedData, IngredientesSeeder>();
            services.AddScoped<ISeedData, ProveedoresSeeder>();
            services.AddScoped<ISeedData, ClientesSeeder>();
            services.AddScoped<ISeedData, MesasSeeder>();
            services.AddScoped<ISeedData, EscenariosDemoSeeder>();
            
            // SEEDERS TESTING
            services.AddScoped<ISeedData, DatosPruebasUnitarias>();
            services.AddScoped<ISeedData, DatosPruebasIntegracion>();
            services.AddScoped<ISeedData, DatosRendimiento>();
        }
    }
} 