using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.EliminarPreparacionDiaria
{
    /// <summary>
    /// Manejador para el comando de eliminar preparación diaria
    /// </summary>
    public class EliminarPreparacionDiariaCommandHandler : IRequestHandler<EliminarPreparacionDiariaCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<EliminarPreparacionDiariaCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public EliminarPreparacionDiariaCommandHandler(
            IApplicationDbContext context,
            ILogger<EliminarPreparacionDiariaCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Maneja el comando para eliminar una preparación diaria
        /// </summary>
        public async Task<Result> Handle(
            EliminarPreparacionDiariaCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("🗑️ Eliminando preparación diaria: {PreparacionId}", request.Id);

            try
            {
                // Buscar la preparación diaria existente
                var preparacion = await _context.PreparacionesDiarias.FindAsync(new object[] { request.Id }, cancellationToken);
                if (preparacion == null)
                {
                    _logger.LogWarning("⚠️ Preparación diaria no encontrada: {PreparacionId}", request.Id);
                    return Result.Failure("Preparación diaria no encontrada");
                }

                // Verificar que no esté siendo utilizada (cantidad disponible = 0)
                if (preparacion.CantidadDisponible > 0)
                {
                    _logger.LogWarning("⚠️ No se puede eliminar preparación diaria con cantidad disponible: {PreparacionId}, Cantidad: {Cantidad}", 
                        request.Id, preparacion.CantidadDisponible);
                    return Result.Failure("No se puede eliminar una preparación diaria que aún tiene cantidad disponible");
                }

                // Eliminar de la base de datos
                _context.PreparacionesDiarias.Remove(preparacion);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("✅ Preparación diaria eliminada exitosamente: {PreparacionId}", preparacion.Id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar preparación diaria: {PreparacionId}", request.Id);
                return Result.Failure($"Error al eliminar preparación diaria: {ex.Message}");
            }
        }
    }
} 