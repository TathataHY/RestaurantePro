using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    /// <summary>
    /// Adaptador para usar TestDbContext como RestauranteProDbContext en pruebas
    /// </summary>
    public class TestDbContextAdapter : RestauranteProDbContext
    {
        private readonly TestDbContext _testDbContext;

        public TestDbContextAdapter(TestDbContext testDbContext)
            : base(new DbContextOptions<RestauranteProDbContext>(), new Mock<ILogger<RestauranteProDbContext>>().Object)
        {
            _testDbContext = testDbContext;
        }

        public override DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _testDbContext.Set<TEntity>();
        }

        public override int SaveChanges()
        {
            return _testDbContext.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _testDbContext.SaveChangesAsync(cancellationToken);
        }
    }
} 