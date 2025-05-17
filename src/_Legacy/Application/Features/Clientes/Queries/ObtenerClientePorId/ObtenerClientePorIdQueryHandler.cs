using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Clientes.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Clientes.Queries.ObtenerClientePorId
{
    public class ObtenerClientePorIdQueryHandler : IRequestHandler<ObtenerClientePorIdQuery, ClienteDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerClientePorIdQueryHandler(
            IApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ClienteDto> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes
                .Include(c => c.TarjetasFidelizacion)
                    .ThenInclude(t => t.Nivel)
                .Include(c => c.Reservaciones)
                .Include(c => c.Comandas)
                .Include(c => c.PromocionesUsadas)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente == null)
            {
                return null;
            }

            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            // Obtener la tarjeta activa si existe
            var tarjetaActiva = cliente.TarjetasFidelizacion
                .FirstOrDefault(t => t.Estado == Domain.Enums.EstadoTarjeta.Activa);

            if (tarjetaActiva != null)
            {
                clienteDto.NumeroTarjeta = tarjetaActiva.Codigo;
                clienteDto.EstadoTarjeta = tarjetaActiva.Estado.ToString();
                clienteDto.NivelFidelizacion = tarjetaActiva.Nivel.Nombre;
                clienteDto.PuntosDisponibles = tarjetaActiva.PuntosDisponibles;
            }
            else
            {
                // Si no hay tarjeta activa, buscar cualquier tarjeta (emitida, suspendida, etc.)
                var cualquierTarjeta = cliente.TarjetasFidelizacion.FirstOrDefault();
                if (cualquierTarjeta != null)
                {
                    clienteDto.NumeroTarjeta = cualquierTarjeta.Codigo;
                    clienteDto.EstadoTarjeta = cualquierTarjeta.Estado.ToString();
                    clienteDto.NivelFidelizacion = cualquierTarjeta.Nivel.Nombre;
                    clienteDto.PuntosDisponibles = cualquierTarjeta.PuntosDisponibles;
                }
            }

            return clienteDto;
        }
    }
} 