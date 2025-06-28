using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarReceta;

/// <summary>
/// Handler para actualizar una receta existente
/// </summary>
public class ActualizarRecetaHandler : IRequestHandler<ActualizarRecetaCommand, Result<RecetaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarRecetaHandler> _logger;

    public ActualizarRecetaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ActualizarRecetaHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<RecetaDto>> Handle(
        ActualizarRecetaCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("✏️ Actualizando receta: {Id}", request.Id);

            // 1. Obtener la receta existente
            var receta = await _context.Recetas
                .Include(r => r.Ingredientes)
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.RecetaEliminada, cancellationToken);

            if (receta == null)
            {
                _logger.LogWarning("⚠️ Receta no encontrada: {Id}", request.Id);
                return Result.Failure<RecetaDto>("Receta no encontrada");
            }

            // 2. Actualizar datos básicos de la receta
            if (!string.IsNullOrWhiteSpace(request.Preparacion))
            {
                receta.ActualizarPreparacion(request.Preparacion);
                _logger.LogInformation("📝 Preparación actualizada");
            }

            if (request.TiempoPreparacionMinutos > 0)
            {
                receta.ActualizarTiempoPreparacion(request.TiempoPreparacionMinutos);
                _logger.LogInformation("⏱️ Tiempo de preparación actualizado: {Tiempo} minutos", request.TiempoPreparacionMinutos);
            }

            // 3. Actualizar ingredientes si se proporcionan
            if (request.Ingredientes != null && request.Ingredientes.Any())
            {
                // Limpiar ingredientes existentes
                foreach (var ingredienteExistente in receta.Ingredientes.ToList())
                {
                    receta.EliminarIngrediente(ingredienteExistente.IngredienteId);
                }

                // Agregar nuevos ingredientes
                foreach (var ingredienteDto in request.Ingredientes)
                {
                    // Verificar que el ingrediente existe
                    var ingrediente = await _context.Ingredientes
                        .FirstOrDefaultAsync(i => i.Id == ingredienteDto.IngredienteId && !i.EstaEliminado, cancellationToken);

                    if (ingrediente == null)
                    {
                        _logger.LogWarning("⚠️ Ingrediente no encontrado: {IngredienteId}", ingredienteDto.IngredienteId);
                        return Result.Failure<RecetaDto>($"Ingrediente no encontrado: {ingredienteDto.IngredienteId}");
                    }

                    // Agregar ingrediente a la receta
                    receta.AgregarIngrediente(
                        ingredienteDto.IngredienteId,
                        ingrediente.Nombre,
                        ingredienteDto.Cantidad,
                        ingrediente.UnidadMedida,
                        ingredienteDto.EsOpcional);

                    _logger.LogInformation("🥘 Ingrediente actualizado: {Nombre} ({Cantidad} {Unidad})",
                        ingrediente.Nombre, ingredienteDto.Cantidad, ingrediente.UnidadMedida);
                }
            }

            // 4. Guardar cambios
            _context.Recetas.Update(receta);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("💾 Receta actualizada exitosamente: {Id}", request.Id);

            // 5. Mapear a DTO
            var recetaDto = _mapper.Map<RecetaDto>(receta);

            return Result.Success(recetaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar receta: {Id}", request.Id);
            return Result.Failure<RecetaDto>("Error al actualizar la receta");
        }
    }
} 