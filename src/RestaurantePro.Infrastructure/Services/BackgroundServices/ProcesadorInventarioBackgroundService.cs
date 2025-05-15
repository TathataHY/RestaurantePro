using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Services.BackgroundServices
{
    public class ProcesadorInventarioBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcesadorInventarioBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public ProcesadorInventarioBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ProcesadorInventarioBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Procesador de inventario iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarComandasPendientes(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar comandas para inventario");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Procesador de inventario detenido");
        }

        private async Task ProcesarComandasPendientes(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Buscando comandas pendientes para procesar inventario");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var inventarioService = scope.ServiceProvider.GetRequiredService<IInventarioComandaService>();

            // Buscar comandas completadas o entregadas que no tengan procesado el inventario
            var comandasPendientes = await dbContext.Comandas
                .Where(c => (c.Estado == EstadoComanda.Completada || c.Estado == EstadoComanda.Entregada) 
                       && !c.InventarioProcesado)
                .ToListAsync(stoppingToken);

            _logger.LogInformation($"Se encontraron {comandasPendientes.Count} comandas pendientes de procesar");

            foreach (var comanda in comandasPendientes)
            {
                try
                {
                    _logger.LogInformation($"Procesando inventario para comanda {comanda.NumeroComanda}");
                    await inventarioService.ActualizarInventarioPorComanda(comanda.Id);
                    _logger.LogInformation($"Comanda {comanda.NumeroComanda} procesada correctamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al procesar inventario para comanda {comanda.NumeroComanda}");
                    // Continuar con la siguiente comanda a pesar del error
                }
            }
        }
    }
} 