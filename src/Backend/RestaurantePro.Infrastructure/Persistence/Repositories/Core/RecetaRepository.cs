using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    /// <summary>
    /// Repositorio para la entidad Receta
    /// </summary>
    public class RecetaRepository : Repository<Receta>, IRecetaRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public RecetaRepository(RestauranteProDbContext dbContext, ILogger<RecetaRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene una receta por el ID del producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Receta del producto o null si no existe</returns>
        public async Task<Receta?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.ProductoId == productoId, cancellationToken);
        }

        /// <summary>
        /// Obtiene los ingredientes de una receta por su ID
        /// </summary>
        /// <param name="recetaId">ID de la receta</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de ingredientes de la receta</returns>
        public async Task<IEnumerable<IngredienteReceta>> ObtenerRecetaIngredientesAsync(Guid recetaId, CancellationToken cancellationToken = default)
        {
            var receta = await _dbSet
                .FirstOrDefaultAsync(r => r.Id == recetaId, cancellationToken);
                
            return receta?.Ingredientes ?? new List<IngredienteReceta>();
        }

        /// <summary>
        /// Obtiene una receta por ID, incluyendo sus ingredientes
        /// </summary>
        public override async Task<Receta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            
            // Nota: Como los ingredientes son un value object dentro de la receta,
            // EF Core los cargará automáticamente como parte de la entidad principal
        }
    }
} 