using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Data;
using RestaurantePro.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RestauranteContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILogger<ComandaRepository> _comandaLogger;
        private readonly ILogger<MesaRepository> _mesaLogger;
        private readonly ILogger<PlatoRepository> _platoLogger;
        
        private IComandaRepository _comandaRepository;
        private IMesaRepository _mesaRepository;
        private IPlatoRepository _platoRepository;

        public UnitOfWork(
            RestauranteContext context,
            ILogger<UnitOfWork> logger,
            ILogger<ComandaRepository> comandaLogger,
            ILogger<MesaRepository> mesaLogger,
            ILogger<PlatoRepository> platoLogger)
        {
            _context = context;
            _logger = logger;
            _comandaLogger = comandaLogger;
            _mesaLogger = mesaLogger;
            _platoLogger = platoLogger;
        }

        public IComandaRepository Comandas => 
            _comandaRepository ??= new ComandaRepository(_context, _comandaLogger);

        public IMesaRepository Mesas => 
            _mesaRepository ??= new MesaRepository(_context, _mesaLogger);

        public IPlatoRepository Platos => 
            _platoRepository ??= new PlatoRepository(_context, _platoLogger);

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