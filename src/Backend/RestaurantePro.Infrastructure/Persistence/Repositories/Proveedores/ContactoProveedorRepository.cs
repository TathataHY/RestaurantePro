using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores
{
    /// <summary>
    /// Implementación del repositorio para contactos de proveedores
    /// </summary>
    public class ContactoProveedorRepository : Repository<ContactoProveedor>, IContactoProveedorRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public ContactoProveedorRepository(RestauranteProDbContext dbContext, ILogger<ContactoProveedorRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene un contacto de proveedor por su ID
        /// </summary>
        public override async Task<ContactoProveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los contactos de un proveedor
        /// </summary>
        public async Task<IEnumerable<ContactoProveedor>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.ProveedorId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca contactos por nombre o cargo
        /// </summary>
        public async Task<IEnumerable<ContactoProveedor>> BuscarPorNombreOCargoAsync(string termino, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodosAsync(cancellationToken);

            return await _dbSet
                .Where(c => c.Nombre.Contains(termino) || c.Cargo.Contains(termino))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca contactos por email
        /// </summary>
        public async Task<ContactoProveedor?> BuscarPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _dbSet
                .FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
        }

        /// <summary>
        /// Busca contactos por teléfono
        /// </summary>
        public async Task<IEnumerable<ContactoProveedor>> BuscarPorTelefonoAsync(string telefono, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return new List<ContactoProveedor>();

            return await _dbSet
                .Where(c => c.Telefono.Value.Contains(telefono))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega un nuevo contacto
        /// </summary>
        public override async Task AgregarAsync(ContactoProveedor entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Actualiza un contacto existente
        /// </summary>
        public override async Task ActualizarAsync(ContactoProveedor entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Elimina un contacto
        /// </summary>
        public override async Task EliminarAsync(ContactoProveedor entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
} 