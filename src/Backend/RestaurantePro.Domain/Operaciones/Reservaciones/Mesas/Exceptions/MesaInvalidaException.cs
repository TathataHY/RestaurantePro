namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación inválida con una mesa.
/// </summary>
public class MesaInvalidaException : BusinessRuleViolationException
{
    /// <summary>
    /// ID de la mesa involucrada
    /// </summary>
    public Guid MesaId { get; }

    /// <summary>
    /// Estado actual de la mesa
    /// </summary>
    public EstadoMesa EstadoActual { get; }

    /// <summary>
    /// Constructor principal para mesa inválida
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="estadoActual">Estado actual de la mesa</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razon">Razón por la cual la operación es inválida</param>
    public MesaInvalidaException(
        Guid mesaId,
        EstadoMesa estadoActual,
        string operacion,
        string razon)
        : base(
            "MesaInvalida",
            "Mesa",
            $"No se puede realizar '{operacion}' en mesa con estado '{estadoActual}': {razon}",
            "Operaciones",
            mesaId)
    {
        MesaId = mesaId;
        EstadoActual = estadoActual;
        
        WithData("EstadoActual", EstadoActual.ToString())
            .WithData("Operacion", operacion)
            .WithData("Razon", razon);
    }

    /// <summary>
    /// Crea excepción para mesa ya ocupada
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaMesaOcupada(Guid mesaId, string operacion = "asignar")
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.Ocupada,
            operacion,
            "La mesa ya está ocupada");
    }

    /// <summary>
    /// Crea excepción para mesa reservada
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaMesaReservada(Guid mesaId, string operacion = "asignar")
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.Reservada,
            operacion,
            "La mesa está reservada para otro cliente");
    }

    /// <summary>
    /// Crea excepción para mesa fuera de servicio
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razonMantenimiento">Razón específica del mantenimiento</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaMesaFueraDeServicio(
        Guid mesaId, 
        string operacion = "asignar",
        string razonMantenimiento = "mantenimiento general")
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.FueraDeServicio,
            operacion,
            $"La mesa está fuera de servicio: {razonMantenimiento}")
            .WithData("RazonMantenimiento", razonMantenimiento) as MesaInvalidaException;
    }

    /// <summary>
    /// Crea excepción para capacidad insuficiente
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="personasSolicitadas">Número de personas solicitadas</param>
    /// <param name="capacidadMesa">Capacidad real de la mesa</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaCapacidadInsuficiente(
        Guid mesaId, 
        int personasSolicitadas, 
        int capacidadMesa)
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.Disponible,
            "asignar por capacidad",
            $"La mesa tiene capacidad para {capacidadMesa} personas pero se solicitaron {personasSolicitadas}")
            .WithData("PersonasSolicitadas", personasSolicitadas)
            .WithData("CapacidadMesa", capacidadMesa) as MesaInvalidaException;
    }

    /// <summary>
    /// Crea excepción para número de mesa duplicado
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="numeroMesa">Número de mesa duplicado</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaNumeroMesaDuplicado(Guid mesaId, int numeroMesa)
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.Disponible,
            "asignar número",
            $"Ya existe otra mesa con el número {numeroMesa}")
            .WithData("NumeroMesaDuplicado", numeroMesa) as MesaInvalidaException;
    }

    /// <summary>
    /// Crea excepción para ubicación inválida
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="ubicacion">Ubicación inválida</param>
    /// <param name="razon">Razón específica del rechazo</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaUbicacionInvalida(
        Guid mesaId, 
        string ubicacion, 
        string razon)
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.Disponible,
            "establecer ubicación",
            $"Ubicación '{ubicacion}' no válida: {razon}")
            .WithData("UbicacionInvalida", ubicacion)
            .WithData("RazonRechazo", razon) as MesaInvalidaException;
    }

    /// <summary>
    /// Crea excepción para operación de limpieza requerida
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de MesaInvalidaException</returns>
    public static MesaInvalidaException ParaLimpiezaRequerida(Guid mesaId, string operacion = "asignar")
    {
        return new MesaInvalidaException(
            mesaId,
            EstadoMesa.Disponible,
            operacion,
            "La mesa requiere limpieza antes de poder ser asignada")
            .WithData("RequiereLimpieza", true) as MesaInvalidaException;
    }
} 