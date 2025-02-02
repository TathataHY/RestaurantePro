using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities.Base;
using RestaurantePro.Core.Specifications.Base;

namespace RestaurantePro.Core.Interfaces.Repositories
{
    public interface IAsyncRepository<T> where T : BaseEntity

    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IReadOnlyList<T>> GetAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
    }
}