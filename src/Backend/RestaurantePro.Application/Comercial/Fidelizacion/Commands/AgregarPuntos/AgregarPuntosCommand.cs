using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using MediatR;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AgregarPuntos;

/// <summary>
/// Comando para agregar puntos a una tarjeta de fidelización
/// </summary>
public class AgregarPuntosCommand : IRequest<Result<AgregarPuntosResponse>>
{
    public Guid TarjetaId { get; set; }
    public int Puntos { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal? MontoTransaccion { get; set; }
    public string? Referencia { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal? MontoCompra { get; set; }
    /// <summary>
    /// Versión de la entidad para control de concurrencia - comentada para tests con SQLite
    /// </summary>
    // public string? RowVersion { get; set; }
}

/// <summary>
/// Respuesta del comando de agregar puntos
/// </summary>
public class AgregarPuntosResponse
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int PuntosAgregados { get; set; }
    public int PuntosActuales { get; set; }
    public int PuntosDisponibles { get; set; }
    public string NivelFidelizacion { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    /// <summary>
    /// Versión de concurrencia (base64)
    /// </summary>
    public string? RowVersion { get; set; }
} 