using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Services;
using Hangfire;

namespace RestaurantePro.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundJobService(ILogger<BackgroundJobService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task ProcessPendingComandas()
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var pendingComandas = await unitOfWork.Comandas.GetPendientesAsync();
            foreach (var comanda in pendingComandas)
            {
                await notificationService.NotifyRoleAsync("Cocinero", 
                    $"Comanda #{comanda.Id} pendiente desde hace {DateTime.UtcNow.Subtract(comanda.FechaHora).Minutes} minutos");
            }
        }

        public string Enqueue<T>(Action<T> methodCall)
        {
            return BackgroundJob.Enqueue<T>(x => methodCall(x));
        }

        public string Schedule<T>(Action<T> methodCall, TimeSpan delay)
        {
            return BackgroundJob.Schedule<T>(x => methodCall(x), delay);
        }

        public async Task<bool> DeleteAsync(string jobId)
        {
            return await Task.FromResult(BackgroundJob.Delete(jobId));
        }
    }
} 