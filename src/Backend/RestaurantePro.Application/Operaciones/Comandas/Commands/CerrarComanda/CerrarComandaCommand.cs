using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;

/// <summary>
/// Comando para cerrar una comanda y generar la factura
/// Marca la comanda como cerrada y genera la factura correspondiente
/// </summary>
public record CerrarComandaCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda que se va a cerrar
    /// </summary>
    public Guid ComandaId { get; init; }

    /// <summary>
    /// Método de pago utilizado
    /// </summary>
    public string MetodoPago { get; init; } = string.Empty;

    /// <summary>
    /// Monto pagado (puede ser diferente al total si hay propina)
    /// </summary>
    public decimal MontoPagado { get; set; }

    /// <summary>
    /// Propina dejada por el cliente
    /// </summary>
    public decimal Propina { get; set; } = 0;

    /// <summary>
    /// Observaciones finales sobre el cierre de la comanda
    /// </summary>
    public string? Observaciones { get; init; }

    /// <summary>
    /// ID del usuario que cierra la comanda
    /// </summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public CerrarComandaCommand()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public CerrarComandaCommand(Guid comandaId, string metodoPago)
    {
        ComandaId = comandaId;
        MetodoPago = metodoPago;
    }

    /// <summary>
    /// Método para crear una nueva instancia con ComandaId actualizado
    /// </summary>
    public CerrarComandaCommand WithComandaId(Guid comandaId)
    {
        return this with { ComandaId = comandaId };
    }
} 