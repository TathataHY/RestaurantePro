using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Entities;

namespace RestaurantePro.Core.Interfaces.Data
{
    public interface IRestauranteContext
    {
        DbSet<Comanda> Comandas { get; set; }
        DbSet<Mesa> Mesas { get; set; }
        DbSet<Plato> Platos { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
} 