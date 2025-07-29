using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacionDiaria
{
    /// <summary>
    /// Manejador para el comando de actualizar preparación diaria
    /// </summary>
    public class ActualizarPreparacionDiariaCommandHandler : IRequestHandler<ActualizarPreparacionDiariaCommand, Result<PreparacionDiariaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ActualizarPreparacionDiariaCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActualizarPreparacionDiariaCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ActualizarPreparacionDiariaCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Maneja el comando para actualizar una preparación diaria existente
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> Handle(
            ActualizarPreparacionDiariaCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("✏️ Actualizando preparación diaria: {PreparacionId}", request.Id);

            try
            {
                // Buscar la preparación diaria existente
                var preparacion = await _context.PreparacionesDiarias.FindAsync(new object[] { request.Id }, cancellationToken);
                if (preparacion == null)
                {
                    _logger.LogWarning("⚠️ Preparación diaria no encontrada: {PreparacionId}", request.Id);
                    return Result.Failure<PreparacionDiariaDto>("Preparación diaria no encontrada");
                }

                // Verificar que el producto existe
                var producto = await _context.Productos.FindAsync(new object[] { request.ProductoId }, cancellationToken);
                if (producto == null)
                {
                    _logger.LogWarning("⚠️ Producto no encontrado: {ProductoId}", request.ProductoId);
                    return Result.Failure<PreparacionDiariaDto>($"Producto no encontrado: {request.ProductoId}");
                }

                // Actualizar los campos de la preparación diaria
                preparacion.Actualizar(
                    request.ProductoId,
                    request.CantidadPreparada,
                    request.CantidadDisponible,
                    request.ChefId,
                    request.FechaVencimiento,
                    request.Observaciones);

                // Guardar cambios en la base de datos
                await _context.SaveChangesAsync(cancellationToken);

                // Mapear a DTO
                var preparacionDto = _mapper.Map<PreparacionDiariaDto>(preparacion);

                _logger.LogInformation("✅ Preparación diaria actualizada exitosamente: {PreparacionId}", preparacion.Id);

                return Result.Success(preparacionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar preparación diaria: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDiariaDto>($"Error al actualizar preparación diaria: {ex.Message}");
            }
        }
    }
} 