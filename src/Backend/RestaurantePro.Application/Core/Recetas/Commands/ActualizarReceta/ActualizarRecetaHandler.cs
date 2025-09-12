using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Application.Core.Recetas.Commands.ActualizarReceta;

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

            // 2. Actualizar datos básicos de la receta (solo si no se van a actualizar ingredientes)
            if (request.Ingredientes == null || !request.Ingredientes.Any())
            {
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
            }

            // 3. Actualizar ingredientes si se proporcionan
            if (request.Ingredientes != null && request.Ingredientes.Any())
            {
                // Verificar que todos los ingredientes existen antes de hacer cambios
                var ingredientesIds = request.Ingredientes.Select(i => i.IngredienteId).ToList();
                var ingredientesExistentes = await _context.Ingredientes
                    .Where(i => ingredientesIds.Contains(i.Id) && !i.EstaEliminado)
                    .ToListAsync(cancellationToken);

                if (ingredientesExistentes.Count != ingredientesIds.Count)
                {
                    var ingredientesNoEncontrados = ingredientesIds.Except(ingredientesExistentes.Select(i => i.Id)).ToList();
                    _logger.LogWarning("⚠️ Ingredientes no encontrados: {IngredientesIds}", string.Join(", ", ingredientesNoEncontrados));
                    return Result.Failure<RecetaDto>($"Ingredientes no encontrados: {string.Join(", ", ingredientesNoEncontrados)}");
                }

                // Crear nueva receta con los ingredientes actualizados
                var nuevaReceta = Receta.Crear(
                    receta.ProductoId,
                    request.Preparacion ?? receta.Preparacion,
                    request.TiempoPreparacionMinutos > 0 ? request.TiempoPreparacionMinutos : receta.TiempoPreparacionMinutos);

                // Establecer el ID original usando reflexión
                var idProperty = typeof(Receta).BaseType?.GetProperty("Id");
                if (idProperty != null)
                {
                    idProperty.SetValue(nuevaReceta, receta.Id);
                }

                // Agregar ingredientes a la nueva receta
                foreach (var ingredienteDto in request.Ingredientes)
                {
                    var ingrediente = ingredientesExistentes.First(i => i.Id == ingredienteDto.IngredienteId);

                    nuevaReceta.AgregarIngrediente(
                        ingredienteDto.IngredienteId,
                        ingrediente.Nombre,
                        ingredienteDto.Cantidad,
                        ingrediente.UnidadMedida,
                        ingredienteDto.EsOpcional);

                    _logger.LogInformation("🥘 Ingrediente actualizado: {Nombre} ({Cantidad} {Unidad})",
                        ingrediente.Nombre, ingredienteDto.Cantidad, ingrediente.UnidadMedida);
                }

                // Reemplazar la receta existente
                _context.Recetas.Remove(receta);
                _context.Recetas.Add(nuevaReceta);
                
                // Actualizar la variable receta para el mapeo
                receta = nuevaReceta;
            }

            // 4. Guardar cambios
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
