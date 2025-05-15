using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Reservaciones.Dtos;
using RestaurantePro.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Queries.ObtenerReservaciones
{
    public class ObtenerReservacionesQueryHandler : IRequestHandler<ObtenerReservacionesQuery, List<ReservacionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerReservacionesQueryHandler(
            IApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ReservacionDto>> Handle(ObtenerReservacionesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Reservaciones
                .Include(r => r.Mesa)
                .Include(r => r.Usuario)
                .AsQueryable();

            // Aplicar filtros
            if (request.MesaId.HasValue)
            {
                query = query.Where(r => r.MesaId == request.MesaId.Value);
            }

            if (!string.IsNullOrEmpty(request.NombreCliente))
            {
                query = query.Where(r => r.NombreCliente.Contains(request.NombreCliente));
            }

            if (!string.IsNullOrEmpty(request.TelefonoCliente))
            {
                query = query.Where(r => r.Telefono.Contains(request.TelefonoCliente));
            }

            if (!string.IsNullOrEmpty(request.EmailCliente))
            {
                query = query.Where(r => r.Email.Contains(request.EmailCliente));
            }

            if (request.FechaDesde.HasValue)
            {
                var fechaDesde = request.FechaDesde.Value.Date;
                query = query.Where(r => r.FechaReservacion >= fechaDesde);
            }

            if (request.FechaHasta.HasValue)
            {
                var fechaHasta = request.FechaHasta.Value.Date.AddDays(1).AddTicks(-1); // Hasta el final del día
                query = query.Where(r => r.FechaReservacion <= fechaHasta);
            }

            if (request.Estado.HasValue)
            {
                query = query.Where(r => r.Estado == request.Estado.Value);
            }

            if (!string.IsNullOrEmpty(request.UsuarioId))
            {
                query = query.Where(r => r.UsuarioId == request.UsuarioId);
            }

            // Ordenar por fecha de reservación (las más recientes primero)
            query = query.OrderByDescending(r => r.FechaReservacion);

            var reservaciones = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<ReservacionDto>>(reservaciones);
        }
    }
} 