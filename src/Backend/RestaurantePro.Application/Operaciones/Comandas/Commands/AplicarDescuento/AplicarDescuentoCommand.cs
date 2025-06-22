using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AplicarDescuento;

/// <summary>
/// Comando para aplicar un descuento a una comanda
/// Permite aplicar descuentos por porcentaje o monto fijo
/// </summary>
public record AplicarDescuentoCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda donde se aplicará el descuento
    /// </summary>
    public Guid ComandaId { get; init; }

    /// <summary>
    /// Porcentaje de descuento a aplicar (0-100)
    /// </summary>
    public decimal PorcentajeDescuento { get; init; }

    /// <summary>
    /// Motivo del descuento
    /// </summary>
    public string? Motivo { get; init; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public AplicarDescuentoCommand()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public AplicarDescuentoCommand(Guid comandaId, decimal porcentajeDescuento)
    {
        ComandaId = comandaId;
        PorcentajeDescuento = porcentajeDescuento;
    }

    /// <summary>
    /// Método para crear una nueva instancia con ComandaId actualizado
    /// </summary>
    public AplicarDescuentoCommand WithComandaId(Guid comandaId)
    {
        return this with { ComandaId = comandaId };
    }
} 