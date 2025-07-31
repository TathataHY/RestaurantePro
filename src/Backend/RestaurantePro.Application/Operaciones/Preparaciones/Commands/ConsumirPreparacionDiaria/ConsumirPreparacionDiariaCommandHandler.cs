using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacionDiaria
{
    /// <summary>
    /// Manejador para el comando de consumir preparación diaria
    /// </summary>
    public class ConsumirPreparacionDiariaCommandHandler : IRequestHandler<ConsumirPreparacionDiariaCommand, Result<PreparacionDiariaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumirPreparacionDiariaCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsumirPreparacionDiariaCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ConsumirPreparacionDiariaCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Maneja el comando para consumir una cantidad específica de una preparación diaria
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> Handle(
            ConsumirPreparacionDiariaCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("🍽️ Consumiendo preparación diaria: {PreparacionId}, Cantidad: {Cantidad}", 
                request.Id, request.Cantidad);

            try
            {
                // Buscar la preparación diaria existente
                var preparacion = await _context.PreparacionesDiarias.FindAsync(new object[] { request.Id }, cancellationToken);
                if (preparacion == null)
                {
                    _logger.LogWarning("⚠️ Preparación diaria no encontrada: {PreparacionId}", request.Id);
                    return Result.Failure<PreparacionDiariaDto>("Preparación diaria no encontrada");
                }

                // Verificar que hay suficiente cantidad disponible
                if (preparacion.CantidadDisponible < request.Cantidad)
                {
                    _logger.LogWarning("⚠️ Cantidad insuficiente en preparación diaria: {PreparacionId}, Disponible: {Disponible}, Solicitado: {Solicitado}", 
                        request.Id, preparacion.CantidadDisponible, request.Cantidad);
                    return Result.Failure<PreparacionDiariaDto>($"Cantidad insuficiente. Disponible: {preparacion.CantidadDisponible}, Solicitado: {request.Cantidad}");
                }

                // Consumir la cantidad especificada
                preparacion.Consumir(request.Cantidad, request.Observaciones);

                // Guardar cambios en la base de datos
                await _context.SaveChangesAsync(cancellationToken);

                // Mapear a DTO
                var preparacionDto = _mapper.Map<PreparacionDiariaDto>(preparacion);

                _logger.LogInformation("✅ Preparación diaria consumida exitosamente: {PreparacionId}, Cantidad restante: {Restante}", 
                    preparacion.Id, preparacion.CantidadDisponible);

                return Result.Success(preparacionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consumir preparación diaria: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDiariaDto>($"Error al consumir preparación diaria: {ex.Message}");
            }
        }
    }
} 