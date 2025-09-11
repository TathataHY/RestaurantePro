using MediatR;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Core.Productos.Commands.CrearCategoria;

/// <summary>
/// Command para crear una nueva categoría de productos
/// Vertical Slice: CrearCategoria
/// </summary>
public class CrearCategoriaCommand : IRequest<Result<CategoriaProductoDto>>
{
    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Color asociado a la categoría para UI
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icono asociado a la categoría
    /// </summary>
    public string? Icono { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int Orden { get; set; } = 0;

    /// <summary>
    /// Indica si la categoría está activa
    /// </summary>
    public bool Activa { get; set; } = true;
}
