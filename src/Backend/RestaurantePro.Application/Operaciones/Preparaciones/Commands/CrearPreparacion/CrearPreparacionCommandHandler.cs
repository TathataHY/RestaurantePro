using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion
{
    /// <summary>
    /// Manejador para el comando de crear preparación
    /// </summary>
    public class CrearPreparacionCommandHandler : IRequestHandler<CrearPreparacionCommand, Result<PreparacionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Constructor
        /// </summary>
        public CrearPreparacionCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IDateTimeService dateTimeService)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
        }

        /// <summary>
        /// Maneja el comando para crear una nueva preparación
        /// </summary>
        public async Task<Result<PreparacionDto>> Handle(
            CrearPreparacionCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var preparacion = PreparacionDiaria.Crear(
                    request.ProductoId,
                    request.Cantidad,
                    request.ChefId ?? Guid.Empty,
                    request.FechaVencimiento ?? _dateTimeService.Now.AddHours(2), // Default 2 horas si no se especifica
                    request.Observaciones,
                    _dateTimeService.Now);

                await _context.Preparaciones.AddAsync(preparacion, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var preparacionDto = _mapper.Map<PreparacionDto>(preparacion);
                return Result.Success(preparacionDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<PreparacionDto>($"Error al crear preparación: {ex.Message}");
            }
        }
    }
} 