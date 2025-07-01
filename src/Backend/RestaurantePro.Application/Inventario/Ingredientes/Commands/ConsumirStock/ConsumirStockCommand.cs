using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;

/// <summary>
/// Comando para consumir stock de un ingrediente
/// </summary>
public class ConsumirStockCommand : IRequest<Result<IngredienteDto>>
{
    /// <summary>
    /// ID del ingrediente a consumir
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Cantidad a consumir
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Motivo del consumo
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del usuario que realiza el consumo
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Referencia externa (comanda, preparación, etc.)
    /// </summary>
    public string? ReferenciaExterna { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ConsumirStockCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ConsumirStockCommand(Guid ingredienteId, decimal cantidad, string motivo, Guid usuarioId)
    {
        IngredienteId = ingredienteId;
        Cantidad = cantidad;
        Motivo = motivo;
        UsuarioId = usuarioId;
    }
} 