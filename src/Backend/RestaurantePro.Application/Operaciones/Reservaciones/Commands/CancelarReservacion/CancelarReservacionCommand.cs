namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;

/// <summary>
/// 📅 Command para cancelar una reservación
/// Maneja cancelaciones por parte del cliente o del restaurante
/// </summary>
public class CancelarReservacionCommand : IRequest<Result<ReservacionDto>>
{
    public Guid ReservacionId { get; init; }
    public Guid UsuarioId { get; init; }
    public MotivoCancelacion Motivo { get; init; }
    public string? MotivoDetalle { get; init; }
    public bool NotificarCliente { get; init; } = true;
    public bool AplicarPenalizacion { get; init; } = false;
    public bool LiberarMesaInmediatamente { get; init; } = true;
    public DateTime? FechaCancelacion { get; init; }

    /// <summary>
    /// Motivo de cancelación como string para el validator
    /// </summary>
    public string MotivoTexto => MotivoDetalle ?? Motivo.ToString();

    /// <summary>
    /// Usuario que realiza la cancelación
    /// </summary>
    public string CanceladoPor => UsuarioId.ToString();

    /// <summary>
    /// Factory method para cancelación por cliente
    /// </summary>
    public static CancelarReservacionCommand CancelacionCliente(
        Guid reservacionId,
        Guid usuarioId,
        string? motivo = null)
    {
        return new CancelarReservacionCommand
        {
            ReservacionId = reservacionId,
            UsuarioId = usuarioId,
            Motivo = MotivoCancelacion.ClienteSolicita,
            MotivoDetalle = motivo ?? "Cancelación solicitada por el cliente",
            NotificarCliente = true,
            AplicarPenalizacion = false,
            LiberarMesaInmediatamente = true,
            FechaCancelacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method para cancelación por no presentarse
    /// </summary>
    public static CancelarReservacionCommand NoShow(
        Guid reservacionId,
        Guid usuarioId,
        string? observaciones = null)
    {
        return new CancelarReservacionCommand
        {
            ReservacionId = reservacionId,
            UsuarioId = usuarioId,
            Motivo = MotivoCancelacion.NoSePresento,
            MotivoDetalle = $"Cliente no se presentó. {observaciones}",
            NotificarCliente = false, // Ya pasó el tiempo
            AplicarPenalizacion = true,
            LiberarMesaInmediatamente = true,
            FechaCancelacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method para cancelación por el restaurante
    /// </summary>
    public static CancelarReservacionCommand CancelacionRestaurante(
        Guid reservacionId,
        Guid usuarioId,
        MotivoCancelacion motivo,
        string detalleMotivo)
    {
        return new CancelarReservacionCommand
        {
            ReservacionId = reservacionId,
            UsuarioId = usuarioId,
            Motivo = motivo,
            MotivoDetalle = detalleMotivo,
            NotificarCliente = true,
            AplicarPenalizacion = false,
            LiberarMesaInmediatamente = true,
            FechaCancelacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method para cancelación de emergencia
    /// </summary>
    public static CancelarReservacionCommand CancelacionEmergencia(
        Guid reservacionId,
        Guid usuarioId,
        string detalleEmergencia)
    {
        return new CancelarReservacionCommand
        {
            ReservacionId = reservacionId,
            UsuarioId = usuarioId,
            Motivo = MotivoCancelacion.Emergencia,
            MotivoDetalle = $"EMERGENCIA: {detalleEmergencia}",
            NotificarCliente = true,
            AplicarPenalizacion = false,
            LiberarMesaInmediatamente = true,
            FechaCancelacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method para cancelación tardía (con penalización)
    /// </summary>
    public static CancelarReservacionCommand CancelacionTardia(
        Guid reservacionId,
        Guid usuarioId,
        string? motivo = null)
    {
        return new CancelarReservacionCommand
        {
            ReservacionId = reservacionId,
            UsuarioId = usuarioId,
            Motivo = MotivoCancelacion.CancelacionTardia,
            MotivoDetalle = motivo ?? "Cancelación realizada con menos de 2 horas de anticipación",
            NotificarCliente = true,
            AplicarPenalizacion = true,
            LiberarMesaInmediatamente = true,
            FechaCancelacion = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 📋 Motivos posibles para cancelar una reservación
/// </summary>
public enum MotivoCancelacion
{
    ClienteSolicita = 1,          // Cliente solicita cancelación
    NoSePresento = 2,             // Cliente no se presentó (no-show)
    CancelacionTardia = 3,        // Cancelación con poca anticipación
    Emergencia = 4,               // Emergencia del restaurante
    ProblemasMesa = 5,            // Problemas con la mesa asignada
    ProblemasPersonal = 6,        // Falta de personal
    EventoPrivado = 7,            // Mesa necesaria para evento privado
    MantenimientoUrgente = 8,     // Mantenimiento urgente del área
    CapacidadReducida = 9,        // Reducción de capacidad temporal
    SolicitudEspecial = 10        // Cancelación por solicitud especial
} 