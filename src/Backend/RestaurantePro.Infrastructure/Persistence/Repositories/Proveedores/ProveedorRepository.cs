using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores
{
    public class ProveedorRepository : Repository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(RestauranteProDbContext context) : base(context)
        {
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, CancellationToken cancellationToken = default)
        {
            if (incluirContactos)
            {
                return await _context.Set<Proveedor>()
                    .Include(p => p.Contactos)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            }
            
            return await _context.Set<Proveedor>()
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Proveedor?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .FirstOrDefaultAsync(p => p.Nombre == nombre, cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorRFCAsync(string rfc, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.RFC == rfc)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, CancellationToken cancellationToken = default)
        {
            if (incluirContactos)
            {
                return await _context.Set<Proveedor>()
                    .Include(p => p.Contactos)
                    .ToListAsync(cancellationToken);
            }
            
            return await _context.Set<Proveedor>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>()
                .Where(p => p.EstaActivo);
                
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.Ciudad.Contains(ciudad))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorIngredienteAsync(Guid ingredienteId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            // Asumimos que existe una relación entre Proveedor e Ingrediente a través de una tabla intermedia
            // Esto es una implementación simplificada
            var query = _context.Set<Proveedor>()
                .Where(p => p.IngredientesProveidos.Any(i => i.IngredienteId == ingredienteId));
                
            if (soloActivos)
            {
                query = query.Where(p => p.EstaActivo);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<ContactoProveedor?> ObtenerContactoPorIdAsync(Guid contactoId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ContactoProveedor>()
                .FindAsync(new object[] { contactoId }, cancellationToken);
        }

        public async Task<IEnumerable<ContactoProveedor>> ObtenerContactosPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ContactoProveedor>()
                .Where(c => c.ProveedorId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> BuscarAsync(string termino, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.Nombre.Contains(termino) || 
                           p.Ciudad.Contains(termino) ||
                           p.Email.Contains(termino) ||
                           p.Telefono.Contains(termino))
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Proveedor> Proveedores, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirContactos = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>().AsQueryable();
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            var proveedores = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (proveedores, total);
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 