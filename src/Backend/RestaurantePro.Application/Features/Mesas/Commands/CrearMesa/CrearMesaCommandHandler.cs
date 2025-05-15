using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Mesas.Commands.CrearMesa
{
    public class CrearMesaCommandHandler : IRequestHandler<CrearMesaCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearMesaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearMesaCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el número de mesa no exista
            var existeNumero = await _context.Mesas
                .AnyAsync(m => m.Numero == request.Numero, cancellationToken);

            if (existeNumero)
                throw new Exception($"Ya existe una mesa con el número {request.Numero}");

            var mesa = new Mesa
            {
                Numero = request.Numero,
                Capacidad = request.Capacidad,
                Ubicacion = request.Ubicacion,
                Activa = request.Activa,
                Estado = request.Estado,
                QrCode = GenerarQrCode(request.Numero),
                FechaCreacion = DateTime.Now
            };

            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync(cancellationToken);

            return mesa.Id;
        }

        private string GenerarQrCode(int numeroMesa)
        {
            // Aquí se implementaría la generación del código QR
            // Para simplificar, retornamos un identificador básico
            return $"MESA-{numeroMesa}-{Guid.NewGuid()}";
        }
    }
} 