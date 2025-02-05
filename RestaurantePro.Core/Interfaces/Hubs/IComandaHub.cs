using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using System.Threading.Tasks;

namespace RestaurantePro.Core.Interfaces.Hubs
{
    public interface IComandaHub
    {
        Task NotifyComandaCreatedWithDetails(ComandaDto comanda);
        Task NotifyComandaCreatedById(int comandaId);
        Task NotifyComandaStatusChanged(int comandaId, EstadoComanda newStatus);
        Task NotifyComandaUpdated(int comandaId);
        Task NotifyRole(string role, string message);
        Task NotifyAll(string message);
        Task NotifyUser(string userId, string message);
    }
} 