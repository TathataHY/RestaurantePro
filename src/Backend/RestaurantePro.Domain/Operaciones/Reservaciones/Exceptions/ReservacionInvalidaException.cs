namespace RestaurantePro.Domain.Operaciones.Reservaciones.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación inválida con una reservación.
/// </summary>
public class ReservacionInvalidaException : BusinessRuleViolationException
{
    /// <summary>
    /// ID de la reservación involucrada
    /// </summary>
    public Guid ReservacionId { get; }

    /// <summary>
    /// Estado actual de la reservación
    /// </summary>
    public EstadoReservacion EstadoActual { get; }

    /// <summary>
    /// Constructor principal para reservación inválida
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="estadoActual">Estado actual de la reservación</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razon">Razón por la cual la operación es inválida</param>
    public ReservacionInvalidaException(
        Guid reservacionId,
        EstadoReservacion estadoActual,
        string operacion,
        string razon)
        : base(
            "ReservacionInvalida",
            "Reservacion",
            $"No se puede realizar '{operacion}' en reservación con estado '{estadoActual}': {razon}",
            "Operaciones",
            reservacionId)
    {
        ReservacionId = reservacionId;
        EstadoActual = estadoActual;
        
        WithData("EstadoActual", EstadoActual.ToString())
            .WithData("Operacion", operacion)
            .WithData("Razon", razon);
    }

    /// <summary>
    /// Crea excepción para reservación ya confirmada
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ReservacionInvalidaException</returns>
    public static ReservacionInvalidaException ParaReservacionConfirmada(Guid reservacionId, string operacion = "modificar")
    {
        return new ReservacionInvalidaException(
            reservacionId,
            EstadoReservacion.Confirmada,
            operacion,
            "La reservación ya ha sido confirmada");
    }

    /// <summary>
    /// Crea excepción para reservación cancelada
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ReservacionInvalidaException</returns>
    public static ReservacionInvalidaException ParaReservacionCancelada(Guid reservacionId, string operacion = "confirmar")
    {
        return new ReservacionInvalidaException(
            reservacionId,
            EstadoReservacion.Cancelada,
            operacion,
            "La reservación ha sido cancelada");
    }

    /// <summary>
    /// Crea excepción para horario inválido
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="fechaHora">Fecha y hora solicitada</param>
    /// <param name="razon">Razón específica del rechazo</param>
    /// <returns>Nueva instancia de ReservacionInvalidaException</returns>
    public static ReservacionInvalidaException ParaHorarioInvalido(
        Guid reservacionId, 
        DateTime fechaHora, 
        string razon)
    {
        return new ReservacionInvalidaException(
            reservacionId,
            EstadoReservacion.Pendiente,
            "establecer horario",
            $"Horario inválido: {razon}")
            .WithData("FechaHoraSolicitada", fechaHora)
            .WithData("RazonRechazo", razon) as ReservacionInvalidaException;
    }

    /// <summary>
    /// Crea excepción para capacidad insuficiente
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="personasSolicitadas">Número de personas solicitadas</param>
    /// <param name="capacidadDisponible">Capacidad máxima disponible</param>
    /// <returns>Nueva instancia de ReservacionInvalidaException</returns>
    public static ReservacionInvalidaException ParaCapacidadInsuficiente(
        Guid reservacionId, 
        int personasSolicitadas, 
        int capacidadDisponible)
    {
        return new ReservacionInvalidaException(
            reservacionId,
            EstadoReservacion.Pendiente,
            "asignar mesa",
            $"Capacidad insuficiente: se solicitaron {personasSolicitadas} personas pero solo hay capacidad para {capacidadDisponible}")
            .WithData("PersonasSolicitadas", personasSolicitadas)
            .WithData("CapacidadDisponible", capacidadDisponible) as ReservacionInvalidaException;
    }

    /// <summary>
    /// Crea excepción para mesa no disponible
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="mesaId">ID de la mesa solicitada</param>
    /// <param name="fechaHora">Fecha y hora de la reservación</param>
    /// <returns>Nueva instancia de ReservacionInvalidaException</returns>
    public static ReservacionInvalidaException ParaMesaNoDisponible(
        Guid reservacionId, 
        Guid mesaId, 
        DateTime fechaHora)
    {
        return new ReservacionInvalidaException(
            reservacionId,
            EstadoReservacion.Pendiente,
            "asignar mesa específica",
            "La mesa solicitada no está disponible en el horario especificado")
            .WithData("MesaId", mesaId)
            .WithData("FechaHora", fechaHora) as ReservacionInvalidaException;
    }

    /// <summary>
    /// Crea excepción para reservación expirada
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="fechaLimite">Fecha límite que se pasó</param>
    /// <returns>Nueva instancia de ReservacionInvalidaException</returns>
    public static ReservacionInvalidaException ParaReservacionExpirada(Guid reservacionId, DateTime fechaLimite)
    {
        return new ReservacionInvalidaException(
            reservacionId,
            EstadoReservacion.Pendiente,
            "confirmar",
            "La reservación ha expirado y no puede ser confirmada")
            .WithData("FechaLimite", fechaLimite)
            .WithData("FechaActual", DateTime.UtcNow) as ReservacionInvalidaException;
    }
} 