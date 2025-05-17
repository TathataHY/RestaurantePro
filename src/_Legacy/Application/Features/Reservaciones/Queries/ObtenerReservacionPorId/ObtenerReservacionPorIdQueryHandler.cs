using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Reservaciones.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Queries.ObtenerReservacionPorId
{
    public class ObtenerReservacionPorIdQueryHandler : IRequestHandler<ObtenerReservacionPorIdQuery, ReservacionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerReservacionPorIdQueryHandler(
            IApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReservacionDto> Handle(ObtenerReservacionPorIdQuery request, CancellationToken cancellationToken)
        {
            var reservacion = await _context.Reservaciones
                .Include(r => r.Mesa)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservacion == null)
            {
                return null;
            }

            return _mapper.Map<ReservacionDto>(reservacion);
        }
    }
} 