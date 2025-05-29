using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;

/// <summary>
/// Comando para agregar un producto a una comanda existente
/// Permite modificar comandas que están en estado "Creada" o "EnProceso"
/// </summary>
public class AgregarItemComandaCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda a la cual agregar el item
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// ID del producto a agregar
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Nombre del producto (para auditoría y display)
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del producto a agregar
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del producto al momento del pedido
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Observaciones específicas para este item
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Lista de personalizaciones para el producto
    /// </summary>
    public List<PersonalizacionCreateDto> Personalizaciones { get; set; } = new();

    /// <summary>
    /// ID del usuario que está agregando el item
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación del comando
    /// </summary>
    public AgregarItemComandaCommand() { }

    /// <summary>
    /// Constructor de conveniencia para casos básicos
    /// </summary>
    public AgregarItemComandaCommand(
        Guid comandaId,
        Guid productoId,
        string nombreProducto,
        int cantidad,
        decimal precioUnitario,
        Guid usuarioId)
    {
        ComandaId = comandaId;
        ProductoId = productoId;
        NombreProducto = nombreProducto;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        UsuarioId = usuarioId;
    }
} 