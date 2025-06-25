namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AgregarPuntos;

/// <summary>
/// Comando para agregar puntos a una tarjeta de fidelización
/// </summary>
public class AgregarPuntosCommand : IRequest<Result<AgregarPuntosResponse>>
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int Puntos { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal? MontoTransaccion { get; set; }
    public string? Referencia { get; set; }
    public Guid UsuarioId { get; set; }
}

/// <summary>
/// Respuesta del comando de agregar puntos
/// </summary>
public class AgregarPuntosResponse
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int PuntosAgregados { get; set; }
    public int PuntosActuales { get; set; }
    public string Mensaje { get; set; } = string.Empty;
} 