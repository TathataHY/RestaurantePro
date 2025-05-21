using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    public class MesaRepository : Repository<Mesa>, IMesaRepository
    {
        public MesaRepository(RestauranteProDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Mesa>> ObtenerTodasAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<Mesa> ObtenerPorIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id) ?? throw new KeyNotFoundException($"Mesa con ID {id} no encontrada");
        }

        public async Task<Mesa> ObtenerPorNumeroAsync(int numero)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Numero == numero) ?? 
                   throw new KeyNotFoundException($"Mesa número {numero} no encontrada");
        }

        public async Task<IEnumerable<Mesa>> BuscarPorUbicacionAsync(string ubicacion)
        {
            return await _dbSet
                .Where(m => m.Ubicacion.Contains(ubicacion))
                .ToListAsync();
        }

        public async Task AgregarAsync(Mesa mesa)
        {
            await _dbSet.AddAsync(mesa);
        }

        public Task ActualizarAsync(Mesa mesa)
        {
            _context.Entry(mesa).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task EliminarAsync(Guid id)
        {
            var mesa = await ObtenerPorIdAsync(id);
            _dbSet.Remove(mesa);
        }

        public async Task<int> ObtenerTotalComensalesActualesAsync()
        {
            return await _dbSet
                .Where(m => m.Estado == EstadoMesa.Ocupada)
                .SumAsync(m => m.Capacidad);
        }

        public async Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync()
        {
            return await _dbSet
                .Where(m => m.Estado == EstadoMesa.Disponible)
                .ToListAsync();
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 