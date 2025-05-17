using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Pagos.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Pagos.Queries.ObtenerPagoPorId
{
    public class ObtenerPagoPorIdQueryHandler : IRequestHandler<ObtenerPagoPorIdQuery, PagoDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerPagoPorIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagoDto> Handle(ObtenerPagoPorIdQuery request, CancellationToken cancellationToken)
        {
            var pago = await _context.Pagos
                .Include(p => p.Comanda)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (pago == null)
                return null;

            return _mapper.Map<PagoDto>(pago);
        }
    }
} 