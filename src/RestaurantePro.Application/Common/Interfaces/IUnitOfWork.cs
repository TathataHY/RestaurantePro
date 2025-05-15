using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 