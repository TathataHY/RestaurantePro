namespace RestaurantePro.Domain.Comercial.Clientes.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación con un cliente inactivo.
/// </summary>
public class ClienteInactivoException : BusinessRuleViolationException
{
    /// <summary>
    /// ID del cliente inactivo
    /// </summary>
    public Guid ClienteId { get; }

    /// <summary>
    /// Constructor para cliente inactivo
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    public ClienteInactivoException(Guid clienteId, string operacion)
        : base(
            "ClienteInactivo",
            "Cliente", 
            $"No se puede realizar la operación '{operacion}' porque el cliente está inactivo",
            "Comercial",
            clienteId)
    {
        ClienteId = clienteId;
        WithData("Operacion", operacion);
    }

    /// <summary>
    /// Constructor simplificado
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    public ClienteInactivoException(Guid clienteId)
        : this(clienteId, "operación general")
    {
    }

    /// <summary>
    /// Crea excepción para acumulación de puntos con cliente inactivo
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="puntos">Puntos que se intentaron acumular</param>
    /// <returns>Nueva instancia de ClienteInactivoException</returns>
    public static ClienteInactivoException ParaAcumulacionPuntos(Guid clienteId, int puntos)
    {
        return new ClienteInactivoException(clienteId, "acumular puntos")
            .WithData("PuntosIntentoAcumular", puntos) as ClienteInactivoException;
    }

    /// <summary>
    /// Crea excepción para asociación de tarjeta con cliente inactivo
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <returns>Nueva instancia de ClienteInactivoException</returns>
    public static ClienteInactivoException ParaAsociacionTarjeta(Guid clienteId, Guid tarjetaId)
    {
        return new ClienteInactivoException(clienteId, "asociar tarjeta de fidelización")
            .WithData("TarjetaId", tarjetaId) as ClienteInactivoException;
    }

    /// <summary>
    /// Crea excepción para registro de visita con cliente inactivo
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Nueva instancia de ClienteInactivoException</returns>
    public static ClienteInactivoException ParaRegistroVisita(Guid clienteId)
    {
        return new ClienteInactivoException(clienteId, "registrar visita");
    }
} 