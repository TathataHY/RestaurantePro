using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using System.Threading.Tasks;

namespace RestaurantePro.Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string message);
        Task NotifyComandaCreatedAsync(int comandaId);
        Task NotifyComandaUpdatedAsync(int comandaId);
        Task NotifyAsync(string message);
        Task NotifyRoleAsync(string role, string message);
        Task NotifyUserAsync(string username, string message);
        Task NotifyComandaCreatedAsync(ComandaDto comanda);
        Task NotifyComandaStatusChangedAsync(int comandaId, EstadoComanda newStatus);
    }
} 