namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior para manejo automático de transacciones de base de datos
/// Asegura consistencia en operaciones que modifican múltiples entidades
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    // TODO: Descomentar cuando se implemente DbContext
    // private readonly DbContext _context;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        // TODO: Descomentar cuando se implemente DbContext
        // _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Solo aplicar transacciones a Commands que modifican datos
        var requiresTransaction = RequiresTransaction(requestName);
        
        if (!requiresTransaction)
        {
            return await next();
        }

        _logger.LogInformation("Iniciando transacción para {RequestName}", requestName);
        
        // TODO: Descomentar cuando se implemente DbContext
        // using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var response = await next();
            
            // TODO: Descomentar cuando se implemente DbContext
            // await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Transacción confirmada exitosamente para {RequestName}", requestName);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en transacción para {RequestName}. Realizando rollback.", requestName);
            
            // TODO: Descomentar cuando se implemente DbContext
            // await transaction.RollbackAsync(cancellationToken);
            
            throw;
        }
    }

    /// <summary>
    /// Determina si una operación requiere transacción de base de datos
    /// </summary>
    private static bool RequiresTransaction(string requestName)
    {
        // Operaciones que definitivamente requieren transacción
        var transactionalOperations = new[]
        {
            // Operaciones complejas de comandas
            "ProcesarPedidoCompleto", "FinalizarServicioCompleto", "FinalizarComanda",
            
            // Operaciones de facturación
            "CrearFactura", "AplicarDescuento", "ProcesarPago", "AnularFactura",
            
            // Operaciones de inventario que afectan múltiples entidades
            "ActualizarStock", "RegistrarMovimiento", "CrearOrdenCompra", "RecebirOrdenCompra",
            
            // Operaciones de fidelización
            "CanjearPuntos", "AcumularPuntos", "CrearTarjetaFidelizacion",
            
            // Operaciones de reservaciones complejas
            "CrearReservacion", "CambiarReservacion", "CancelarReservacion",
            
            // Operaciones de usuarios que afectan múltiples tablas
            "CrearUsuario", "ActualizarUsuario", "DesactivarUsuario",
            
            // Operaciones de clientes
            "CrearCliente", "DesactivarCliente",
            
            // Operaciones de proveedores
            "CrearProveedor", "ActualizarProveedor"
        };
        
        return transactionalOperations.Any(operation => 
            requestName.Contains(operation, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determina el nivel de aislamiento apropiado para la operación
    /// </summary>
    private static System.Data.IsolationLevel GetIsolationLevel(string requestName)
    {
        // Para operaciones críticas de inventario, usar Serializable
        if (requestName.Contains("Stock") || requestName.Contains("Inventario"))
        {
            return System.Data.IsolationLevel.Serializable;
        }
        
        // Para operaciones de facturación, usar RepeatableRead
        if (requestName.Contains("Factura") || requestName.Contains("Pago"))
        {
            return System.Data.IsolationLevel.RepeatableRead;
        }
        
        // Para la mayoría de operaciones, ReadCommitted es suficiente
        return System.Data.IsolationLevel.ReadCommitted;
    }
}

/// <summary>
/// Atributo para marcar Commands que requieren transacción explícitamente
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RequiresTransactionAttribute : Attribute
{
    public System.Data.IsolationLevel IsolationLevel { get; }

    public RequiresTransactionAttribute(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        IsolationLevel = isolationLevel;
    }
}

/// <summary>
/// Atributo para marcar Commands que NO requieren transacción
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NoTransactionAttribute : Attribute
{
} 