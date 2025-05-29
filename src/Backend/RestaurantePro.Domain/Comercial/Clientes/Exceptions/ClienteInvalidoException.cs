namespace RestaurantePro.Domain.Comercial.Clientes.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación inválida con un cliente.
/// </summary>
public class ClienteInvalidoException : BusinessRuleViolationException
{
    /// <summary>
    /// ID del cliente involucrado
    /// </summary>
    public Guid ClienteId { get; }

    /// <summary>
    /// Estado de activación del cliente
    /// </summary>
    public bool EstaActivo { get; }

    /// <summary>
    /// Constructor principal para cliente inválido
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="estaActivo">Estado de activación del cliente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razon">Razón por la cual la operación es inválida</param>
    public ClienteInvalidoException(
        Guid clienteId,
        bool estaActivo,
        string operacion,
        string razon)
        : base(
            "ClienteInvalido",
            "Cliente",
            $"No se puede realizar '{operacion}' con cliente (Activo: {estaActivo}): {razon}",
            "Comercial",
            clienteId)
    {
        ClienteId = clienteId;
        EstaActivo = estaActivo;
        
        WithData("EstaActivo", EstaActivo)
            .WithData("Operacion", operacion)
            .WithData("Razon", razon);
    }

    /// <summary>
    /// Crea excepción para cliente desactivado
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaClienteDesactivado(Guid clienteId, string operacion = "procesar")
    {
        return new ClienteInvalidoException(
            clienteId,
            false,
            operacion,
            "El cliente está desactivado y no puede realizar operaciones");
    }

    /// <summary>
    /// Crea excepción para tarjeta de fidelización vencida
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="fechaVencimiento">Fecha de vencimiento de la tarjeta</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaTarjetaVencida(
        Guid clienteId, 
        DateTime fechaVencimiento,
        string operacion = "usar puntos")
    {
        var diasVencida = (DateTime.Now - fechaVencimiento).Days;
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"La tarjeta de fidelización venció hace {diasVencida} días (venció {fechaVencimiento:dd/MM/yyyy})");
        
        excepcion.WithData("FechaVencimiento", fechaVencimiento)
                 .WithData("DiasVencida", diasVencida)
                 .WithData("TipoProblema", "TarjetaVencida");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para puntos insuficientes
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="puntosActuales">Puntos actuales del cliente</param>
    /// <param name="puntosRequeridos">Puntos requeridos para la operación</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaPuntosInsuficientes(
        Guid clienteId, 
        int puntosActuales, 
        int puntosRequeridos,
        string operacion = "canjear puntos")
    {
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"Puntos insuficientes: tiene {puntosActuales}, necesita {puntosRequeridos}");
            
        excepcion.WithData("PuntosActuales", puntosActuales)
                 .WithData("PuntosRequeridos", puntosRequeridos)
                 .WithData("Deficit", puntosRequeridos - puntosActuales)
                 .WithData("TipoProblema", "PuntosInsuficientes");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para cliente sin tarjeta de fidelización
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaClienteSinTarjeta(Guid clienteId, string operacion = "usar fidelización")
    {
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            "El cliente no tiene una tarjeta de fidelización activa");
            
        excepcion.WithData("TipoProblema", "SinTarjeta");
        
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para límite de crédito excedido
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="limiteCredito">Límite de crédito del cliente</param>
    /// <param name="saldoPendiente">Saldo pendiente actual</param>
    /// <param name="montoSolicitud">Monto de la nueva solicitud</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaLimiteCreditoExcedido(
        Guid clienteId, 
        decimal limiteCredito, 
        decimal saldoPendiente,
        decimal montoSolicitud,
        string operacion = "procesar venta a crédito")
    {
        var disponible = limiteCredito - saldoPendiente;
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"Límite de crédito excedido: disponible ${disponible:F2}, solicitado ${montoSolicitud:F2}");
            
        excepcion.WithData("LimiteCredito", limiteCredito)
                 .WithData("SaldoPendiente", saldoPendiente)
                 .WithData("MontoSolicitud", montoSolicitud)
                 .WithData("Disponible", disponible)
                 .WithData("Exceso", montoSolicitud - disponible)
                 .WithData("TipoProblema", "LimiteCreditoExcedido");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para facturación pendiente vencida
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="facturasPendientes">Número de facturas pendientes</param>
    /// <param name="montoVencido">Monto total vencido</param>
    /// <param name="diasVencimiento">Días de vencimiento promedio</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaFacturacionVencida(
        Guid clienteId, 
        int facturasPendientes, 
        decimal montoVencido,
        int diasVencimiento,
        string operacion = "nueva compra")
    {
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"Tiene {facturasPendientes} facturas vencidas por ${montoVencido:F2} (vencidas desde hace {diasVencimiento} días)");
            
        excepcion.WithData("FacturasPendientes", facturasPendientes)
                 .WithData("MontoVencido", montoVencido)
                 .WithData("DiasVencimiento", diasVencimiento)
                 .WithData("TipoProblema", "FacturacionVencida");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para información de contacto incompleta
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="camposFaltantes">Lista de campos faltantes</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaInformacionIncompleta(
        Guid clienteId, 
        string[] camposFaltantes,
        string operacion = "procesar operación")
    {
        var campos = string.Join(", ", camposFaltantes);
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"Información de contacto incompleta. Campos requeridos: {campos}");
            
        excepcion.WithData("CamposFaltantes", camposFaltantes)
                 .WithData("TipoProblema", "InformacionIncompleta");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para cliente menor de edad
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="edad">Edad actual del cliente</param>
    /// <param name="edadMinima">Edad mínima requerida</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaClienteMenorEdad(
        Guid clienteId, 
        int edad, 
        int edadMinima,
        string operacion = "comprar productos restringidos")
    {
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"Cliente menor de edad: {edad} años (mínimo requerido: {edadMinima} años)");
            
        excepcion.WithData("EdadCliente", edad)
                 .WithData("EdadMinima", edadMinima)
                 .WithData("TipoProblema", "MenorEdad");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para cliente bloqueado por fraude
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="razonBloqueo">Razón específica del bloqueo</param>
    /// <param name="fechaBloqueo">Fecha del bloqueo</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaClienteBloqueado(
        Guid clienteId, 
        string razonBloqueo, 
        DateTime fechaBloqueo,
        string operacion = "realizar transacción")
    {
        var diasBloqueado = (DateTime.Now - fechaBloqueo).Days;
        var excepcion = new ClienteInvalidoException(
            clienteId,
            false,
            operacion,
            $"Cliente bloqueado por fraude: {razonBloqueo} (bloqueado hace {diasBloqueado} días)");
            
        excepcion.WithData("RazonBloqueo", razonBloqueo)
                 .WithData("FechaBloqueo", fechaBloqueo)
                 .WithData("DiasBloqueado", diasBloqueado)
                 .WithData("TipoProblema", "ClienteBloqueado");
                 
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para promoción no elegible
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="promocionId">ID de la promoción</param>
    /// <param name="criteriosNoMet">Criterios no cumplidos</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ClienteInvalidoException</returns>
    public static ClienteInvalidoException ParaPromocionNoElegible(
        Guid clienteId, 
        Guid promocionId, 
        string[] criteriosNoMet,
        string operacion = "aplicar promoción")
    {
        var criterios = string.Join(", ", criteriosNoMet);
        var excepcion = new ClienteInvalidoException(
            clienteId,
            true,
            operacion,
            $"No elegible para la promoción. Criterios no cumplidos: {criterios}");
            
        excepcion.WithData("PromocionId", promocionId)
                 .WithData("CriteriosNoMet", criteriosNoMet)
                 .WithData("TipoProblema", "PromocionNoElegible");
                 
        return excepcion;
    }
} 