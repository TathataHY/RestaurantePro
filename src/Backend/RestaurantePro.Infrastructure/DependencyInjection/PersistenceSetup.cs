using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;

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
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar interceptores
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DomainEventInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();

            // Configuración de la base de datos
            services.AddDbContext<RestauranteProDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    });

                // Agregar interceptores
                options.AddInterceptors(
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<SoftDeleteInterceptor>());
            });

            // Registrar interfaces de aplicación
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // Registrar Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar repositorios
            RegisterRepositories(services);

            return services;
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Repositorio base genérico
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Repositorios específicos de Core
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IRecetaRepository, RecetaRepository>();
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            
            // Repositorios específicos de Comercial
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
            
            // Repositorios específicos de Operaciones
            services.AddScoped<IMesaRepository, MesaRepository>();
            services.AddScoped<IComandaRepository, ComandaRepository>();
            services.AddScoped<IReservacionRepository, ReservacionRepository>();
            services.AddScoped<IPreparacionRepository, PreparacionRepository>();
            
            // Repositorios específicos de Inventario
            services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
            services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
            
            // Repositorios específicos de Proveedores
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<IContactoProveedorRepository, ContactoProveedorRepository>();
            
            // TODO: Implementar servicios de caché, logging, etc.
        }
    }
} 