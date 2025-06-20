using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(DbContext dbContext, ILogger<ClienteRepository> logger)
            : base(dbContext, logger)
        {
        }

        public override async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(c => !c.EstaEliminado).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los clientes no eliminados
        /// Override para aplicar filtro de eliminación lógica
        /// </summary>
        public override async Task<IEnumerable<Cliente>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(c => !c.EstaEliminado).ToListAsync(cancellationToken);
        }

        public async Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(c => !c.EstaEliminado).FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            // Simplificado para compatibilidad con In-Memory
            var clientes = await _dbSet.ToListAsync(cancellationToken);
            return clientes.Where(c => (c.Nombre.Nombre + " " + c.Nombre.Apellido).Contains(nombre, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorSegmentoAsync(SegmentoCliente segmento, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Segmento == segmento && !c.EstaEliminado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorEstadoActivoAsync(bool activo, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.EstaActivo == activo && !c.EstaEliminado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.TarjetaFidelizacionPrincipalId != null)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesMasFrecuentesAsync(int cantidad, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(c => c.CantidadVisitas)
                .Take(cantidad)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorPuntosMinimosAsync(int puntosMinimos, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.PuntosAcumulados >= puntosMinimos)
                .ToListAsync(cancellationToken);
        }

        async Task<(IEnumerable<Cliente> Clientes, int Total)> IClienteRepository.ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken)
        {
            var (items, total) = await base.ObtenerPaginadoAsync(pagina, elementosPorPagina, cancellationToken);
            return (items, total);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorRangoFechasRegistroAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> VerificarExistenciaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<Cliente>> ObtenerClientesActivosConVisitasAsync(int cantidadMinimaVisitas, int cantidadDias, CancellationToken cancellationToken = default)
        {
            // La lógica simplificada se basa en CantidadVisitas. No se usa cantidadDias porque no hay fechas de visita.
            return await _dbSet
                .Where(c => c.EstaActivo && c.CantidadVisitas >= cantidadMinimaVisitas)
                .ToListAsync(cancellationToken);
        }

        public Task<IEnumerable<Cliente>> ObtenerClientesConHistorialVisitasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("El método {MethodName} no se puede implementar de forma fiable sin una entidad 'Visita' y devolverá una lista vacía.", nameof(ObtenerClientesConHistorialVisitasAsync));
            return Task.FromResult(Enumerable.Empty<Cliente>());
        }

        public Task<IEnumerable<Cliente>> ObtenerTodosConHistorialVisitasAsync(int diasHistorial, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("El método {MethodName} no se puede implementar de forma fiable sin una entidad 'Visita' y devolverá una lista vacía.", nameof(ObtenerTodosConHistorialVisitasAsync));
            return Task.FromResult(Enumerable.Empty<Cliente>());
        }

        public async Task<IEnumerable<Factura>> ObtenerFacturasRecientesAsync(Guid clienteId, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Factura>()
                .Where(f => f.ClienteId == clienteId && f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin)
                .ToListAsync(cancellationToken);
        }
        
        public async Task GuardarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            var existe = await _dbSet.AnyAsync(e => e.Id == cliente.Id, cancellationToken);
            if (!existe)
            {
                await _dbSet.AddAsync(cliente, cancellationToken);
            }
            else
            {
                _dbContext.Entry(cliente).State = EntityState.Modified;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<List<Cliente>> ObtenerClientesConComprasRecientes(DateTime fechaDesde, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
} 