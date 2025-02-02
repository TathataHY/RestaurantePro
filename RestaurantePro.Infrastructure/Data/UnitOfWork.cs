using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Data;
using RestaurantePro.Infrastructure.Repositories;

namespace RestaurantePro.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RestauranteContext _context;

        private IComandaRepository _comandaRepository;
        private IMesaRepository _mesaRepository;
        private IPlatoRepository _platoRepository;

        public UnitOfWork(RestauranteContext context)
        {
            _context = context;
        }

        public IComandaRepository Comandas =>
            _comandaRepository ??= new ComandaRepository(_context);

        public IMesaRepository Mesas =>
            _mesaRepository ??= new MesaRepository(_context);

        public IPlatoRepository Platos =>
            _platoRepository ??= new PlatoRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}