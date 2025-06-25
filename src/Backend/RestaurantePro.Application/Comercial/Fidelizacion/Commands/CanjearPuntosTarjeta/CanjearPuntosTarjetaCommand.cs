namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntosTarjeta;

/// <summary>
/// Comando para canjear puntos de una tarjeta de fidelización
/// </summary>
public class CanjearPuntosTarjetaCommand : IRequest<Result<CanjearPuntosTarjetaResponse>>
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int PuntosACanjear { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public Guid UsuarioId { get; set; }
}

/// <summary>
/// Respuesta del comando de canjear puntos
/// </summary>
public class CanjearPuntosTarjetaResponse
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int PuntosCanjeados { get; set; }
    public int PuntosActuales { get; set; }
    public string Mensaje { get; set; } = string.Empty;
} 