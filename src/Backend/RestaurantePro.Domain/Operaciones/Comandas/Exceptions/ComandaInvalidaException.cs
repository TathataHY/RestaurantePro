namespace RestaurantePro.Domain.Operaciones.Comandas.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación inválida con una comanda.
/// </summary>
public class ComandaInvalidaException : BusinessRuleViolationException
{
    /// <summary>
    /// ID de la comanda involucrada
    /// </summary>
    public Guid ComandaId { get; }

    /// <summary>
    /// Estado actual de la comanda
    /// </summary>
    public EstadoComanda EstadoActual { get; }

    /// <summary>
    /// Constructor principal para comanda inválida
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="estadoActual">Estado actual de la comanda</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razon">Razón por la cual la operación es inválida</param>
    public ComandaInvalidaException(
        Guid comandaId,
        EstadoComanda estadoActual,
        string operacion,
        string razon)
        : base(
            "ComandaInvalida",
            "Comanda",
            $"No se puede realizar '{operacion}' en comanda con estado '{estadoActual}': {razon}",
            "Operaciones",
            comandaId)
    {
        ComandaId = comandaId;
        EstadoActual = estadoActual;
        
        WithData("EstadoActual", EstadoActual.ToString())
            .WithData("Operacion", operacion)
            .WithData("Razon", razon);
    }

    /// <summary>
    /// Crea excepción para comanda ya finalizada
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ComandaInvalidaException</returns>
    public static ComandaInvalidaException ParaComandaFinalizada(Guid comandaId, string operacion = "modificar")
    {
        return new ComandaInvalidaException(
            comandaId,
            EstadoComanda.Finalizada,
            operacion,
            "La comanda ya ha sido finalizada");
    }

    /// <summary>
    /// Crea excepción para comanda cancelada
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ComandaInvalidaException</returns>
    public static ComandaInvalidaException ParaComandaCancelada(Guid comandaId, string operacion = "procesar")
    {
        return new ComandaInvalidaException(
            comandaId,
            EstadoComanda.Cancelada,
            operacion,
            "La comanda ha sido cancelada");
    }

    /// <summary>
    /// Crea excepción para comanda sin items
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <returns>Nueva instancia de ComandaInvalidaException</returns>
    public static ComandaInvalidaException ParaComandaSinItems(Guid comandaId)
    {
        return new ComandaInvalidaException(
            comandaId,
            EstadoComanda.EnProceso, // Estado típico - corregido
            "finalizar",
            "La comanda no tiene items para procesar")
            .WithData("ItemsCount", 0) as ComandaInvalidaException;
    }

    /// <summary>
    /// Crea excepción para descuento inválido
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="descuentoSolicitado">Descuento solicitado</param>
    /// <param name="descuentoMaximo">Descuento máximo permitido</param>
    /// <returns>Nueva instancia de ComandaInvalidaException</returns>
    public static ComandaInvalidaException ParaDescuentoInvalido(
        Guid comandaId, 
        decimal descuentoSolicitado, 
        decimal descuentoMaximo)
    {
        return new ComandaInvalidaException(
            comandaId,
            EstadoComanda.Creada, // Estado típico - corregido
            "aplicar descuento",
            $"El descuento solicitado ({descuentoSolicitado:P}) excede el máximo permitido ({descuentoMaximo:P})")
            .WithData("DescuentoSolicitado", descuentoSolicitado)
            .WithData("DescuentoMaximo", descuentoMaximo) as ComandaInvalidaException;
    }

    /// <summary>
    /// Crea excepción para mesa ocupada
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="mesaId">ID de la mesa</param>
    /// <returns>Nueva instancia de ComandaInvalidaException</returns>
    public static ComandaInvalidaException ParaMesaOcupada(Guid comandaId, Guid mesaId)
    {
        return new ComandaInvalidaException(
            comandaId,
            EstadoComanda.Creada, // Estado típico - corregido
            "asignar mesa",
            "La mesa seleccionada ya está ocupada")
            .WithData("MesaId", mesaId) as ComandaInvalidaException;
    }
} 