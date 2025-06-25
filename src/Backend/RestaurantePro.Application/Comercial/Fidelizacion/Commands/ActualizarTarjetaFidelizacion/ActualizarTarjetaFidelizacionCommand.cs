using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActualizarTarjetaFidelizacion;

/// <summary>
/// Comando para actualizar una tarjeta de fidelización existente
/// </summary>
public class ActualizarTarjetaFidelizacionCommand : IRequest<Result<TarjetaFidelizacionDto>>
{
    /// <summary>
    /// ID de la tarjeta a actualizar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo nivel de fidelización
    /// </summary>
    public NivelFidelizacion Nivel { get; set; }

    /// <summary>
    /// Nuevo multiplicador de puntos
    /// </summary>
    public decimal MultiplicadorPuntos { get; set; }

    /// <summary>
    /// Nuevo límite mensual de puntos (opcional)
    /// </summary>
    public int? LimiteMensual { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del usuario que realiza la actualización
    /// </summary>
    public Guid UsuarioId { get; set; }
} 