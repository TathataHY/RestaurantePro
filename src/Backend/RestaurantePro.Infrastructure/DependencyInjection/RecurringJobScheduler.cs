using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Operaciones;
using System.Threading;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    public static class RecurringJobScheduler
    {
        public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
        {
            var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate<NotificationCleanupJob>(
                "cleanup-notifications",
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Daily);

            recurringJobManager.AddOrUpdate<UserInactivityJob>(
                "check-user-inactivity",
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Daily(3)); 

            recurringJobManager.AddOrUpdate<InvoiceReminderJob>(
                "send-invoice-reminders",
                job => job.ExecuteAsync(CancellationToken.None), 
                Cron.Daily(9));

            recurringJobManager.AddOrUpdate<TableCleanupJob>(
                "cleanup-tables",
                job => job.ExecuteAsync(CancellationToken.None),
                "0 */2 * * *");

            recurringJobManager.AddOrUpdate<LowStockAlertJob>(
                "low-stock-alert",
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.HourInterval(4));

            recurringJobManager.AddOrUpdate<ExpirationCheckJob>(
                "check-expirations",
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Daily(2));

            return app;
        }
    }
} 