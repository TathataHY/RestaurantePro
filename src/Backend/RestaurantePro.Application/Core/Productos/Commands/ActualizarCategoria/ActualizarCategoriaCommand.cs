using MediatR;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarCategoria;

/// <summary>
/// Command para actualizar una categoría de productos existente
/// Vertical Slice: ActualizarCategoria
/// </summary>
public class ActualizarCategoriaCommand : IRequest<Result<CategoriaProductoDto>>
{
    /// <summary>
    /// Identificador único de la categoría
    /// </summary>
    public Guid Id { get; set; }

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
    public int Orden { get; set; }

    /// <summary>
    /// Indica si la categoría está activa
    /// </summary>
    public bool Activa { get; set; }
}
