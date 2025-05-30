using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CambiarEstadoMesa;

/// <summary>
/// Command para cambiar el estado de una mesa
/// </summary>
public class CambiarEstadoMesaCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// ID de la mesa a cambiar de estado
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Nuevo estado deseado para la mesa
    /// </summary>
    public EstadoMesa NuevoEstado { get; set; }

    /// <summary>
    /// Motivo del cambio de estado (requerido para FueraDeServicio)
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// ID del usuario que realiza el cambio (opcional)
    /// </summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Observaciones adicionales sobre el cambio de estado
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Factory method para cambiar a FueraDeServicio con motivo
    /// </summary>
    public static CambiarEstadoMesaCommand MarcarFueraDeServicio(Guid mesaId, string motivo, Guid? usuarioId = null, string? observaciones = null)
    {
        return new CambiarEstadoMesaCommand
        {
            MesaId = mesaId,
            NuevoEstado = EstadoMesa.FueraDeServicio,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Observaciones = observaciones
        };
    }

    /// <summary>
    /// Factory method para cambiar a cualquier estado sin motivo
    /// </summary>
    public static CambiarEstadoMesaCommand CambiarA(Guid mesaId, EstadoMesa nuevoEstado, Guid? usuarioId = null, string? observaciones = null)
    {
        return new CambiarEstadoMesaCommand
        {
            MesaId = mesaId,
            NuevoEstado = nuevoEstado,
            UsuarioId = usuarioId,
            Observaciones = observaciones
        };
    }
} 