namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;

/// <summary>
/// 🎯 Command para canjear puntos de fidelización por descuento
/// Usa ComercialServiceFacade para operaciones complejas de negocio
/// </summary>
public class CanjearPuntosCommand : IRequest<Result<CanjearPuntosResult>>
{
    public Guid ClienteId { get; init; }
    public int PuntosAUtilizar { get; init; }
    public Guid? ComandaId { get; init; }
    public string? Motivo { get; init; }

    /// <summary>
    /// Factory method para crear el command con validaciones básicas
    /// </summary>
    public static CanjearPuntosCommand Crear(Guid clienteId, int puntosAUtilizar, Guid? comandaId = null, string? motivo = null)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId no puede estar vacío", nameof(clienteId));
        
        if (puntosAUtilizar <= 0)
            throw new ArgumentException("PuntosAUtilizar debe ser mayor a 0", nameof(puntosAUtilizar));

        return new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = puntosAUtilizar,
            ComandaId = comandaId,
            Motivo = motivo ?? $"Canje de {puntosAUtilizar} puntos por descuento"
        };
    }
}

/// <summary>
/// 🎯 Resultado del canje de puntos
/// </summary>
public class CanjearPuntosResult
{
    public Guid ClienteId { get; set; }
    public int PuntosUtilizados { get; set; }
    public decimal MontoDescuento { get; set; }
    public int PuntosRestantes { get; set; }
    public Guid? ComandaId { get; set; }
    public DateTime FechaCanje { get; set; }
    public string Motivo { get; set; } = string.Empty;
} 