namespace RestaurantePro.Application.Operaciones.Mesas.Commands.TransferirMesa;

/// <summary>
/// Command para transferir una comanda de una mesa a otra
/// Permite mover comandas activas entre mesas disponibles
/// </summary>
public class TransferirMesaCommand : IRequest<Result<TransferirMesaDto>>
{
    /// <summary>
    /// ID de la comanda a transferir
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// ID de la mesa de origen
    /// </summary>
    public Guid MesaOrigenId { get; set; }

    /// <summary>
    /// ID de la mesa de destino
    /// </summary>
    public Guid MesaDestinoId { get; set; }

    /// <summary>
    /// Motivo de la transferencia
    /// </summary>
    public string MotivoTransferencia { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que autoriza la transferencia
    /// </summary>
    public Guid? AutorizadoPor { get; set; }

    /// <summary>
    /// Notas adicionales sobre la transferencia
    /// </summary>
    public string? NotasTransferencia { get; set; }

    /// <summary>
    /// Indica si se debe notificar al mesero
    /// </summary>
    public bool NotificarMesero { get; set; } = true;

    /// <summary>
    /// Indica si se debe mantener el estado actual de la comanda
    /// </summary>
    public bool MantenerEstado { get; set; } = true;

    /// <summary>
    /// Datos adicionales de la transferencia
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public TransferirMesaCommand(Guid comandaId, Guid mesaOrigenId, Guid mesaDestinoId, string motivoTransferencia)
    {
        ComandaId = comandaId;
        MesaOrigenId = mesaOrigenId;
        MesaDestinoId = mesaDestinoId;
        MotivoTransferencia = motivoTransferencia;
    }

    public TransferirMesaCommand() { }
}

/// <summary>
/// DTO de respuesta para la transferencia de mesa
/// </summary>
public class TransferirMesaDto
{
    public Guid ComandaId { get; set; }
    public Guid MesaAnteriorId { get; set; }
    public Guid MesaNuevaId { get; set; }
    public string MotivoTransferencia { get; set; } = string.Empty;
    public DateTime FechaTransferencia { get; set; }
    public Guid? AutorizadoPor { get; set; }
    public string? NotasTransferencia { get; set; }
    public bool TransferenciaExitosa { get; set; }
} 