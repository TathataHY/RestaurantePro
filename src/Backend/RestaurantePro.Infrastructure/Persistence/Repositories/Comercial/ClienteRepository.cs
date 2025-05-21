using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(RestauranteProDbContext context) : base(context)
        {
        }

        public async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.Nombre.NombreCompleto.Contains(nombre))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorSegmentoAsync(SegmentoCliente segmento, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.Segmento == segmento)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.EstaActivo == activos)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.TarjetaFidelizacionPrincipalId != null)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesMasFrecuentesAsync(int cantidad, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.EstaActivo)
                .OrderByDescending(c => c.CantidadVisitas)
                .Take(cantidad)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorPuntosMinimosAsync(int puntosMinimos, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.PuntosAcumulados >= puntosMinimos)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Cliente> Clientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Cliente>().AsQueryable();
            
            var total = await query.CountAsync(cancellationToken);
            
            var clientes = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (clientes, total);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorRangoFechasRegistroAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Cliente>()
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }
    }
} 