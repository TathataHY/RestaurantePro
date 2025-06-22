using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProducto;

/// <summary>
/// Comando para agregar un producto a una comanda existente
/// Permite agregar productos con cantidad y observaciones específicas
/// </summary>
public record AgregarProductoCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda donde se agregará el producto
    /// </summary>
    public Guid ComandaId { get; init; }

    /// <summary>
    /// ID del producto a agregar
    /// </summary>
    public Guid ProductoId { get; init; }

    /// <summary>
    /// Cantidad del producto a agregar
    /// </summary>
    public int Cantidad { get; init; }

    /// <summary>
    /// Observaciones específicas para este producto en la comanda
    /// </summary>
    public string? Observaciones { get; init; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public AgregarProductoCommand()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public AgregarProductoCommand(Guid comandaId, Guid productoId, int cantidad)
    {
        ComandaId = comandaId;
        ProductoId = productoId;
        Cantidad = cantidad;
    }

    /// <summary>
    /// Método para crear una nueva instancia con ComandaId actualizado
    /// </summary>
    public AgregarProductoCommand WithComandaId(Guid comandaId)
    {
        return this with { ComandaId = comandaId };
    }
} 