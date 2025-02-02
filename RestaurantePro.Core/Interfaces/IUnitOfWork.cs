using RestaurantePro.Core.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace RestaurantePro.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IComandaRepository Comandas { get; }

        IMesaRepository Mesas { get; }
        IPlatoRepository Platos { get; }
        Task<int> CompleteAsync();
    }
}