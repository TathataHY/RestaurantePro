using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesDelDia
{
    /// <summary>
    /// Manejador para la consulta de obtener preparaciones del día
    /// </summary>
    public class ObtenerPreparacionesDelDiaQueryHandler : IRequestHandler<ObtenerPreparacionesDelDiaQuery, List<PreparacionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObtenerPreparacionesDelDiaQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IDateTimeService dateTimeService)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
        }

        /// <summary>
        /// Maneja la consulta para obtener las preparaciones del día
        /// </summary>
        public async Task<List<PreparacionDto>> Handle(
            ObtenerPreparacionesDelDiaQuery request,
            CancellationToken cancellationToken)
        {
            var fechaHoy = _dateTimeService.Now.Date;
            var fechaSiguiente = fechaHoy.AddDays(1);

            // Obtener las preparaciones creadas el día actual
            var preparaciones = await Task.FromResult(
                _context.PreparacionesDiarias
                .Where(p => p.FechaPreparacion.Date == fechaHoy)
                .OrderByDescending(p => p.FechaPreparacion)
                .ToList());

            // Mapear a DTOs
            var preparacionesDto = _mapper.Map<List<PreparacionDto>>(preparaciones);

            // Calcular horas para vencer para cada preparación
            foreach (var preparacionDto in preparacionesDto)
            {
                var ahora = _dateTimeService.Now;
                if (preparacionDto.FechaVencimiento <= ahora)
                {
                    preparacionDto.HorasParaVencer = 0;
                }
                else
                {
                    var tiempoRestante = preparacionDto.FechaVencimiento - ahora;
                    preparacionDto.HorasParaVencer = System.Math.Round(((TimeSpan)tiempoRestante).TotalHours, 1);
                }
            }

            return preparacionesDto;
        }
    }
} 