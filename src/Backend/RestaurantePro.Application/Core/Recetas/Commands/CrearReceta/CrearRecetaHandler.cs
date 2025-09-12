using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;

/// <summary>
/// Handler para crear una nueva receta
/// </summary>
public class CrearRecetaHandler : IRequestHandler<CrearRecetaCommand, Result<RecetaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearRecetaHandler> _logger;

    public CrearRecetaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CrearRecetaHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<RecetaDto>> Handle(
        CrearRecetaCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("➕ Creando nueva receta para producto: {ProductoId}", request.ProductoId);

            // 1. Verificar que el producto existe
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == request.ProductoId, cancellationToken);

            if (producto == null)
            {
                _logger.LogWarning("⚠️ Producto no encontrado: {ProductoId}", request.ProductoId);
                return Result.Failure<RecetaDto>($"Producto no encontrado: {request.ProductoId}");
            }

            // 2. Verificar que no existe ya una receta para este producto
            var recetaExistente = await _context.Recetas
                .FirstOrDefaultAsync(r => r.ProductoId == request.ProductoId && !r.RecetaEliminada, cancellationToken);

            if (recetaExistente != null)
            {
                _logger.LogWarning("⚠️ Ya existe una receta para el producto: {ProductoId}", request.ProductoId);
                return Result.Failure<RecetaDto>("Ya existe una receta para este producto");
            }

            // 3. Crear la receta
            var receta = Receta.Crear(
                request.ProductoId,
                request.Preparacion,
                request.TiempoPreparacionMinutos);

            _logger.LogInformation("📝 Receta creada: {Id}", receta.Id);

            // 4. Agregar ingredientes a la receta
            foreach (var ingredienteDto in request.Ingredientes)
            {
                // Verificar que el ingrediente existe
                var ingrediente = await _context.Ingredientes
                    .FirstOrDefaultAsync(i => i.Id == ingredienteDto.IngredienteId && i.EstaActivo, cancellationToken);

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

                _logger.LogInformation("🥘 Ingrediente agregado: {Nombre} ({Cantidad} {Unidad})",
                    ingrediente.Nombre, ingredienteDto.Cantidad, ingrediente.UnidadMedida);
            }

            // 5. Guardar la receta
            await _context.Recetas.AddAsync(receta, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("💾 Receta guardada exitosamente: {Id}", receta.Id);

            // 6. Mapear a DTO
            var recetaDto = _mapper.Map<RecetaDto>(receta);

            return Result.Success(recetaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al crear receta para producto: {ProductoId}", request.ProductoId);
            return Result.Failure<RecetaDto>("Error al crear la receta");
        }
    }
} 
