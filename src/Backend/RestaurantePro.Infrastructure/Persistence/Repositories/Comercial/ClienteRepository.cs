using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        private readonly RestauranteProDbContext _dbContext;

        public ClienteRepository(RestauranteProDbContext context, ILogger<ClienteRepository> logger) 
            : base(context, logger)
        {
            _dbContext = context;
        }

        public new async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.Nombre.NombreCompleto.Contains(nombre))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorSegmentoAsync(SegmentoCliente segmento, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.Segmento == segmento)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.EstaActivo == activos)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.TarjetaFidelizacionPrincipalId != null)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesMasFrecuentesAsync(int cantidad, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.EstaActivo)
                .OrderByDescending(c => c.CantidadVisitas)
                .Take(cantidad)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorPuntosMinimosAsync(int puntosMinimos, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.PuntosAcumulados >= puntosMinimos)
                .ToListAsync(cancellationToken);
        }

        public new async Task<(IEnumerable<Cliente> Clientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<Cliente>().AsQueryable();
            
            var total = await query.CountAsync(cancellationToken);
            
            var clientes = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (clientes, total);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorRangoFechasRegistroAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> VerificarExistenciaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesActivosConVisitasAsync(int cantidadMinimaVisitas, int cantidadDias, CancellationToken cancellationToken = default)
        {
            var fechaLimite = DateTime.Now.AddDays(-cantidadDias);
            
            return await _dbContext.Set<Cliente>()
                .Where(c => c.EstaActivo && c.CantidadVisitas >= cantidadMinimaVisitas)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesConHistorialVisitasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            // Aquí asumo que tienes una relación entre Cliente y alguna entidad de Visitas
            // Si no existe tal relación, esto tendría que adaptarse
            return await _dbContext.Set<Cliente>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosConHistorialVisitasAsync(int diasHistorial, CancellationToken cancellationToken = default)
        {
            var fechaLimite = DateTime.Now.AddDays(-diasHistorial);
            
            return await _dbContext.Set<Cliente>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Factura>> ObtenerFacturasRecientesAsync(Guid clienteId, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Factura>()
                .Where(f => f.ClienteId == clienteId && f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public new Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(cliente).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task GuardarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            if (cliente.Id == Guid.Empty)
            {
                _dbContext.Set<Cliente>().Add(cliente);
            }
            else
            {
                _dbContext.Entry(cliente).State = EntityState.Modified;
            }

            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public override Task<Cliente> AgregarAsync(Cliente entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<Cliente>().Add(entity);
            _dbContext.SaveChangesAsync(cancellationToken);
            return Task.FromResult(entity);
        }

        public override Task<IEnumerable<Cliente>> AgregarRangoAsync(IEnumerable<Cliente> entities, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<Cliente>().AddRange(entities);
            _dbContext.SaveChangesAsync(cancellationToken);
            return Task.FromResult(entities);
        }

        public Task<IEnumerable<Cliente>> BuscarAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().Where(predicado).ToList();
            return Task.FromResult<IEnumerable<Cliente>>(result);
        }

        public Task<bool> ExisteAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().Any(predicado);
            return Task.FromResult(result);
        }

        public Task<int> ContarAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().Count(predicado);
            return Task.FromResult(result);
        }

        public Task<Cliente?> PrimeroODefaultAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorSpecAsync(ISpecification<Cliente> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).ToListAsync(cancellationToken);
        }

        public async Task<int> ContarPorSpecAsync(ISpecification<Cliente> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).CountAsync(cancellationToken);
        }

        public async Task<Cliente?> PrimeroODefaultPorSpecAsync(ISpecification<Cliente> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
        }

        private IQueryable<Cliente> ApplySpecification(ISpecification<Cliente> spec)
        {
            return SpecificationEvaluator<Cliente>.GetQuery(_dbContext.Set<Cliente>().AsQueryable(), spec);
        }
    }
} 