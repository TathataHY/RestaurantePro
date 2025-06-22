using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.RemoverProducto;

/// <summary>
/// Comando para remover un producto de una comanda
/// Permite eliminar productos específicos de una comanda existente
/// </summary>
public class RemoverProductoCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// ID del item/producto a remover
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Cantidad a remover (si es 0, remueve todo el item)
    /// </summary>
    public int Cantidad { get; set; } = 0;

    /// <summary>
    /// Observaciones sobre la remoción
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public RemoverProductoCommand()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public RemoverProductoCommand(Guid comandaId, Guid itemId)
    {
        ComandaId = comandaId;
        ItemId = itemId;
    }
} 