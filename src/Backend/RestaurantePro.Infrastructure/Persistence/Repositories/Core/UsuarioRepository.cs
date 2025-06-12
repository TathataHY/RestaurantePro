using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        private readonly RestauranteProDbContext _dbContext;

        public UsuarioRepository(RestauranteProDbContext dbContext, ILogger<UsuarioRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public async Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su ID de Identity
        /// </summary>
        public async Task<Usuario?> ObtenerPorIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (soloActivos)
                query = query.Where(u => u.Estado == EstadoUsuario.Activo);
            
            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        public async Task<IEnumerable<Usuario>> ObtenerPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.Roles.Contains(rol))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega un nuevo usuario
        /// </summary>
        public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(usuario, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        public async Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(usuario).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Verifica si existe un usuario con el nombre de usuario especificado
        /// </summary>
        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);
        }

        /// <summary>
        /// Verifica si existe un usuario con el email especificado
        /// </summary>
        public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }
    }
} 