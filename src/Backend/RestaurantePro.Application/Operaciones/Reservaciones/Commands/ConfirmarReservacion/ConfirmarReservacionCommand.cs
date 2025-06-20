namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;

/// <summary>
/// Command para confirmar una reservación
/// </summary>
public class ConfirmarReservacionCommand : IRequest<Result<ReservacionDto>>
{
    public Guid Id { get; set; }
    public Guid? MesaId { get; set; }
    public string? Observaciones { get; set; }
    public Guid? UsuarioId { get; set; }
    
    // Propiedades adicionales que el validator espera
    public Guid ReservacionId { get; set; }
    public string? CodigoReservacion { get; set; }
    public string MetodoConfirmacion { get; set; } = string.Empty;
    public string? ConfirmadoPor { get; set; }
    public string? NotasConfirmacion { get; set; }
    public Dictionary<string, object>? DatosAdicionales { get; set; }
} 