namespace RestaurantePro.Domain.Comercial.Clientes.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación con un cliente inactivo.
/// </summary>
public class ClienteInactivoException : DomainException
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
            $"No se puede realizar la operación '{operacion}' porque el cliente está inactivo",
            "CLIENT_INACTIVE", 
            "Comercial")
    {
        ClienteId = clienteId;
        WithData("ClienteId", clienteId);
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
        var excepcion = new ClienteInactivoException(clienteId, "acumular puntos");
        excepcion.WithData("Puntos", puntos);
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para asociación de tarjeta con cliente inactivo
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <returns>Nueva instancia de ClienteInactivoException</returns>
    public static ClienteInactivoException ParaAsociacionTarjeta(Guid clienteId, Guid tarjetaId)
    {
        var excepcion = new ClienteInactivoException(clienteId, "asociar tarjeta de fidelización");
        excepcion.WithData("TarjetaId", tarjetaId);
        return excepcion;
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