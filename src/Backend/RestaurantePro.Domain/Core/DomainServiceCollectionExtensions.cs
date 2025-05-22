using Microsoft.Extensions.DependencyInjection;
using System;
using RestaurantePro.Domain.Core.Services;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Comercial.Policies;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Core.Productos.Policies;
using RestaurantePro.Domain.Core.Productos.Specifications;

namespace RestaurantePro.Domain.Core
{
    /// <summary>
    /// Extensiones para configurar servicios de dominio
    /// </summary>
    public static class DomainServiceCollectionExtensions
    {
        /// <summary>
        /// Registra todos los servicios y políticas de dominio
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios de dominio agregados</returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Registrar eventos de dominio (con suscripciones y registro)
            services.AddDomainEventServicesComplete();
            
            // Registrar servicios compartidos
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            // Registrar especificaciones reutilizables
            services.AddTransient<Comercial.Clientes.Specifications.ClienteFrecuenteSpecification>();
            services.AddTransient<Inventario.Ingredientes.Specifications.IngredienteRotacionAltaSpecification>();
            services.AddTransient<Core.Productos.Specifications.ProductoDisponibleSpecification>();
            
            // Registrar interfaces de fachada para la capa de aplicación
            services.AddScoped<Comercial.Services.IComercialServiceFacade, Comercial.Services.ComercialServiceFacade>();
            services.AddScoped<Operaciones.Services.IOperacionesServiceFacade, Operaciones.Services.OperacionesServiceFacade>();
            services.AddScoped<Inventario.Services.IInventarioServiceFacade, Inventario.Services.InventarioServiceFacade>();
            services.AddScoped<Proveedores.Services.IProveedoresServiceFacade, Proveedores.Services.ProveedoresServiceFacade>();
            
            return services;
        }
        
        /// <summary>
        /// Registra servicios de dominio para pruebas
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios de dominio para pruebas</returns>
        public static IServiceCollection AddDomainServicesForTests(this IServiceCollection services)
        {
            // Registrar eventos de dominio pero con implementación nula del registro
            services.AddDomainEventServices();
            
            // Usar directamente el MockDateTimeService de Core/SharedKernel/Services
            services.AddSingleton<IDateTimeService>(new MockDateTimeService(DateTime.Now));
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            // Registrar especificaciones reutilizables para pruebas
            services.AddTransient<Comercial.Clientes.Specifications.ClienteFrecuenteSpecification>();
            services.AddTransient<Inventario.Ingredientes.Specifications.IngredienteRotacionAltaSpecification>();
            services.AddTransient<Core.Productos.Specifications.ProductoDisponibleSpecification>();
            
            // Registrar mocks de interfaces de fachada para pruebas
            // Aquí se pueden usar implementaciones simuladas para pruebas
            
            return services;
        }
    }
} 