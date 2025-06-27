using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Commands.ActualizarMovimiento;

/// <summary>
/// Command para actualizar un movimiento de inventario existente
/// </summary>
public class ActualizarMovimientoCommand : IRequest<Result<MovimientoInventarioDto>>
{
    /// <summary>
    /// ID del movimiento a actualizar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nueva cantidad del movimiento
    /// </summary>
    public decimal? Cantidad { get; set; }

    /// <summary>
    /// Nuevo costo unitario
    /// </summary>
    public decimal? CostoUnitario { get; set; }

    /// <summary>
    /// Nuevo motivo del movimiento
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// Nuevas observaciones
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del usuario que realiza la actualización
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ActualizarMovimientoCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ActualizarMovimientoCommand(Guid id, Guid usuarioId)
    {
        Id = id;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Factory method para crear el command
    /// </summary>
    public static ActualizarMovimientoCommand Create(Guid id, Guid usuarioId)
    {
        return new ActualizarMovimientoCommand(id, usuarioId);
    }
} 