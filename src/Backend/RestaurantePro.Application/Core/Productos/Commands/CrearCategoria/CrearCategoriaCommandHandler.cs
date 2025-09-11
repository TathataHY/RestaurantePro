using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;

namespace RestaurantePro.Application.Core.Productos.Commands.CrearCategoria;

/// <summary>
/// Handler para crear una nueva categoría de productos
/// </summary>
public class CrearCategoriaCommandHandler : IRequestHandler<CrearCategoriaCommand, Result<CategoriaProductoDto>>
{
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CrearCategoriaCommandHandler> _logger;

    public CrearCategoriaCommandHandler(
        IProductoCategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork,
        ILogger<CrearCategoriaCommandHandler> logger)
    {
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoriaProductoDto>> Handle(CrearCategoriaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de categoría: {Nombre}", request.Nombre);

            // Usar UnitOfWork para asegurar que los cambios se guarden correctamente
            return await _unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                // Verificar si ya existe una categoría con el mismo nombre
                var categoriasExistentes = await _categoriaRepository.ObtenerTodasAsync(cancellationToken);
                _logger.LogInformation("Categorías existentes encontradas: {Count}", categoriasExistentes.Count());
                
                var categoriaExistente = categoriasExistentes
                    .FirstOrDefault(c => c.Nombre.ToLower() == request.Nombre.ToLower());

                if (categoriaExistente != null)
                {
                    _logger.LogWarning("Ya existe una categoría con el nombre: {Nombre} (ID: {Id})", request.Nombre, categoriaExistente.Id);
                    return Result.Failure<CategoriaProductoDto>("Ya existe una categoría con este nombre");
                }
                
                _logger.LogInformation("No se encontró categoría duplicada, procediendo con la creación");

                // Crear la nueva categoría
                var categoria = ProductoCategoria.Crear(
                    request.Nombre,
                    request.Descripcion ?? string.Empty,
                    request.Orden,
                    request.Color ?? "#FF5722",
                    request.Icono ?? "🍽️");

                await _categoriaRepository.AgregarAsync(categoria, cancellationToken);

                _logger.LogInformation("Categoría creada exitosamente con ID: {Id}", categoria.Id);

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
                    CantidadProductos = 0,
                    ProductosDisponibles = 0,
                    FechaCreacion = categoria.FechaCreacion
                };

                return Result.Success(dto);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la categoría: {Nombre}", request.Nombre);
            return Result.Failure<CategoriaProductoDto>("Error interno al crear la categoría");
        }
    }
}
