using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración de dependencias para SignalR
    /// Sigue las mejores prácticas de arquitectura limpia y DDD
    /// </summary>
    public static class SignalRSetup
    {
        /// <summary>
        /// Agrega todos los servicios relacionados con SignalR al contenedor de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios configurada</returns>
        public static IServiceCollection AddSignalRServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar opciones de SignalR
            var signalRSettings = new SignalRSettings();
            configuration.GetSection("SignalR").Bind(signalRSettings);
            
            // Registrar configuración como singleton para acceso global
            services.AddSingleton(signalRSettings);

            // Configurar opciones de SignalR
            services.Configure<HubOptions>(options =>
            {
                // Configurar opciones globales de hubs
                options.EnableDetailedErrors = signalRSettings.EnableDetailedErrors;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(signalRSettings.ClientTimeoutSeconds);
                options.KeepAliveInterval = TimeSpan.FromSeconds(signalRSettings.KeepAliveIntervalSeconds);
                options.MaximumReceiveMessageSize = signalRSettings.MaximumReceiveMessageSize;
                options.StreamBufferCapacity = signalRSettings.StreamBufferCapacity;
            });

            RegisterSignalRServices(services);
            ConfigureSignalRLogging(services);

            return services;
        }

        /// <summary>
        /// Registra los servicios de SignalR
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        private static void RegisterSignalRServices(IServiceCollection services)
        {
            // Registrar HubConnectionManager como singleton
            services.AddSingleton<HubConnectionManager>();

            // Registrar SignalRService como scoped
            services.AddScoped<ISignalRService, SignalRService>();

            // Registrar servicios de monitoreo de conexiones
            services.AddScoped<IConnectionMonitorService, SignalRConnectionMonitorService>();

            // Registrar servicios de métricas
            services.AddScoped<ISignalRMetricsService, SignalRMetricsService>();
        }

        /// <summary>
        /// Configura el logging específico para SignalR
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        private static void ConfigureSignalRLogging(IServiceCollection services)
        {
            // Configurar logging específico para SignalR
            services.AddLogging(builder =>
            {
                builder.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Information);
                builder.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Information);
            });
        }

        /// <summary>
        /// Configura SignalR para el pipeline de la aplicación
        /// </summary>
        /// <param name="app">Aplicación web</param>
        /// <param name="configuration">Configuración</param>
        public static void UseSignalR(this IApplicationBuilder app, IConfiguration configuration)
        {
            var signalRSettings = configuration.GetSection("SignalR").Get<SignalRSettings>();
            
            if (signalRSettings?.Enabled == true)
            {
                // Configurar middleware de SignalR
                ConfigureSignalRMiddleware(app);
                
                // Configurar health checks para SignalR
                ConfigureSignalRHealthChecks(app);
            }
        }

        /// <summary>
        /// Configura el middleware específico para SignalR
        /// </summary>
        /// <param name="app">Aplicación web</param>
        private static void ConfigureSignalRMiddleware(IApplicationBuilder app)
        {
            // Middleware para logging de conexiones SignalR
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/hubs"))
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<object>>();
                    logger.LogInformation("SignalR connection attempt: {Path}", context.Request.Path);
                }
                
                await next();
            });
        }

        /// <summary>
        /// Configura health checks para SignalR
        /// </summary>
        /// <param name="app">Aplicación web</param>
        private static void ConfigureSignalRHealthChecks(IApplicationBuilder app)
        {
            // Health check para verificar que SignalR esté funcionando
            // Esto se puede expandir para verificar conectividad con Redis backplane si se usa
        }
    }

    /// <summary>
    /// Servicio de monitoreo de conexiones SignalR
    /// </summary>
    public class SignalRConnectionMonitorService : IConnectionMonitorService
    {
        private readonly HubConnectionManager _connectionManager;
        private readonly ILogger<SignalRConnectionMonitorService> _logger;

        public SignalRConnectionMonitorService(
            HubConnectionManager connectionManager,
            ILogger<SignalRConnectionMonitorService> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task<int> GetActiveConnectionsCountAsync()
        {
            var stats = await _connectionManager.ObtenerEstadisticasConexionesAsync();
            return stats.TotalConexionesActivas;
        }

        public async Task<List<Guid>> GetConnectedUsersAsync()
        {
            // Necesitamos obtener los usuarios conectados de otra manera
            // ya que ConexionesEstadisticas no tiene esta lista
            return new List<Guid>(); // Implementación simplificada
        }

        public async Task<bool> IsUserConnectedAsync(Guid userId)
        {
            return await _connectionManager.UsuarioEstaConectadoAsync(userId);
        }
    }

    /// <summary>
    /// Servicio de métricas para SignalR
    /// </summary>
    public class SignalRMetricsService : ISignalRMetricsService
    {
        private readonly HubConnectionManager _connectionManager;
        private readonly ILogger<SignalRMetricsService> _logger;

        public SignalRMetricsService(
            HubConnectionManager connectionManager,
            ILogger<SignalRMetricsService> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task<SignalRMetrics> GetMetricsAsync()
        {
            var stats = await _connectionManager.ObtenerEstadisticasConexionesAsync();
            
            return new SignalRMetrics
            {
                TotalConnections = stats.TotalConexionesActivas,
                ConnectedUsers = stats.TotalUsuariosConectados,
                ActiveGroups = stats.TotalGruposActivos,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Interfaz para el servicio de monitoreo de conexiones
    /// </summary>
    public interface IConnectionMonitorService
    {
        Task<int> GetActiveConnectionsCountAsync();
        Task<List<Guid>> GetConnectedUsersAsync();
        Task<bool> IsUserConnectedAsync(Guid userId);
    }

    /// <summary>
    /// Interfaz para el servicio de métricas de SignalR
    /// </summary>
    public interface ISignalRMetricsService
    {
        Task<SignalRMetrics> GetMetricsAsync();
    }

    /// <summary>
    /// Modelo de métricas de SignalR
    /// </summary>
    public class SignalRMetrics
    {
        public int TotalConnections { get; set; }
        public int ConnectedUsers { get; set; }
        public int ActiveGroups { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 