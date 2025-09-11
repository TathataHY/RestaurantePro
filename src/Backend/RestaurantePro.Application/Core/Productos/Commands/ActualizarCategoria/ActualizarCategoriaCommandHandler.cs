using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarCategoria;

/// <summary>
/// Handler para actualizar una categoría de productos existente
/// </summary>
public class ActualizarCategoriaCommandHandler : IRequestHandler<ActualizarCategoriaCommand, Result<CategoriaProductoDto>>
{
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly ILogger<ActualizarCategoriaCommandHandler> _logger;

    public ActualizarCategoriaCommandHandler(
        IProductoCategoriaRepository categoriaRepository,
        ILogger<ActualizarCategoriaCommandHandler> logger)
    {
        _categoriaRepository = categoriaRepository;
        _logger = logger;
    }

    public async Task<Result<CategoriaProductoDto>> Handle(ActualizarCategoriaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando actualización de categoría con ID: {Id}", request.Id);

            // Validar datos de entrada
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                validationErrors.Add("El nombre de la categoría es requerido");
            }

            if (request.Orden < 0)
            {
                validationErrors.Add("El orden debe ser mayor o igual a 0");
            }

            if (validationErrors.Any())
            {
                _logger.LogWarning("Errores de validación en actualización de categoría: {Errors}", string.Join(", ", validationErrors));
                return Result.Failure<CategoriaProductoDto>(string.Join("; ", validationErrors));
            }

            // Buscar la categoría existente (incluyendo inactivas para poder actualizarlas)
            var categoria = await _categoriaRepository.ObtenerPorIdIncluyendoInactivasAsync(request.Id, cancellationToken);

            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada con ID: {Id}", request.Id);
                return Result.Failure<CategoriaProductoDto>("Categoría no encontrada");
            }

            // Verificar si ya existe otra categoría con el mismo nombre (excluyendo la actual)
            var categoriasExistentes = await _categoriaRepository.ObtenerTodasAsync(cancellationToken);
            var categoriaConMismoNombre = categoriasExistentes
                .FirstOrDefault(c => c.Nombre.ToLower() == request.Nombre.ToLower() && c.Id != request.Id);

            if (categoriaConMismoNombre != null)
            {
                _logger.LogWarning("Ya existe otra categoría con el nombre: {Nombre}", request.Nombre);
                return Result.Failure<CategoriaProductoDto>($"Ya existe otra categoría con el nombre '{request.Nombre}'");
            }

            // Actualizar la categoría
            _logger.LogInformation("Actualizando categoría - Icono recibido: '{Icono}', Color recibido: '{Color}'", request.Icono, request.Color);
            _logger.LogInformation("Valores antes de actualizar - request.Icono: '{Icono}', request.Color: '{Color}'", request.Icono, request.Color);
            categoria.Actualizar(
                request.Nombre,
                request.Descripcion ?? string.Empty,
                request.Orden,
                request.Color ?? "#FF5722",
                request.Icono ?? "🍽️");
            
            _logger.LogInformation("Categoría actualizada - Icono después de actualizar: '{Icono}', Color después de actualizar: '{Color}'", categoria.Icono, categoria.Color);

            // Actualizar el estado activo si es necesario
            if (request.Activa && !categoria.EstaActivo)
            {
                categoria.Activar();
            }
            else if (!request.Activa && categoria.EstaActivo)
            {
                categoria.Desactivar();
            }

            await _categoriaRepository.ActualizarAsync(categoria, cancellationToken);

            _logger.LogInformation("Categoría actualizada exitosamente con ID: {Id}", categoria.Id);

            // Para el DTO, asumimos que no hay productos asociados por ahora
            // En una implementación completa, se podría inyectar IProductoRepository
            var cantidadProductos = 0;
            var productosDisponibles = 0;

            // Mapear a DTO
            var dto = new CategoriaProductoDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Color = categoria.Color,
                Icono = categoria.Icono,
                Orden = categoria.Orden,
                Activa = categoria.EstaActivo,
                CantidadProductos = cantidadProductos,
                ProductosDisponibles = productosDisponibles,
                FechaCreacion = categoria.FechaCreacion
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la categoría con ID: {Id}", request.Id);
            return Result.Failure<CategoriaProductoDto>("Error interno al actualizar la categoría");
        }
    }
}
