namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepciones específicas del contexto de Operaciones de Restaurante
/// </summary>
public static class OperacionesExceptions
{
    /// <summary>
    /// Excepción para errores en la gestión de comandas
    /// </summary>
    public class ComandaOperationException : AppException
    {
        public Guid ComandaId { get; }
        public string Operation { get; }

        public ComandaOperationException(Guid comandaId, string operation, string message)
            : base($"Error en operación '{operation}' de comanda {comandaId}: {message}", 
                   "COMANDA_OPERATION_ERROR", 
                   ErrorSeverity.Medium)
        {
            ComandaId = comandaId;
            Operation = operation;
        }

        public static ComandaOperationException ForEstadoInvalido(Guid comandaId, string estadoActual, string operacion)
        {
            return new ComandaOperationException(comandaId, operacion, 
                $"No se puede realizar la operación. Estado actual: {estadoActual}");
        }

        public static ComandaOperationException ForItemsVacios(Guid comandaId)
        {
            return new ComandaOperationException(comandaId, "Finalizar", 
                "No se puede finalizar una comanda sin items");
        }
    }

    /// <summary>
    /// Excepción para errores en la gestión de mesas
    /// </summary>
    public class MesaOperationException : AppException
    {
        public Guid MesaId { get; }
        public string Operation { get; }

        public MesaOperationException(Guid mesaId, string operation, string message)
            : base($"Error en operación '{operation}' de mesa {mesaId}: {message}", 
                   "MESA_OPERATION_ERROR", 
                   ErrorSeverity.Medium)
        {
            MesaId = mesaId;
            Operation = operation;
        }

        public static MesaOperationException ForMesaOcupada(Guid mesaId, string operacion)
        {
            return new MesaOperationException(mesaId, operacion, 
                "La mesa está ocupada y no se puede realizar la operación");
        }

        public static MesaOperationException ForCapacidadInsuficiente(Guid mesaId, int capacidadRequerida, int capacidadDisponible)
        {
            return new MesaOperationException(mesaId, "Asignar", 
                $"Capacidad insuficiente. Requerida: {capacidadRequerida}, Disponible: {capacidadDisponible}");
        }
    }

    /// <summary>
    /// Excepción para errores en reservaciones
    /// </summary>
    public class ReservacionOperationException : AppException
    {
        public Guid ReservacionId { get; }
        public string Operation { get; }

        public ReservacionOperationException(Guid reservacionId, string operation, string message)
            : base($"Error en operación '{operation}' de reservación {reservacionId}: {message}", 
                   "RESERVACION_OPERATION_ERROR", 
                   ErrorSeverity.Medium)
        {
            ReservacionId = reservacionId;
            Operation = operation;
        }

        public static ReservacionOperationException ForFechaPasada(Guid reservacionId)
        {
            return new ReservacionOperationException(reservacionId, "Crear", 
                "No se puede crear una reservación para una fecha pasada");
        }

        public static ReservacionOperationException ForDisponibilidadInsuficiente(Guid reservacionId, DateTime fecha, int personasRequeridas)
        {
            return new ReservacionOperationException(reservacionId, "Crear", 
                $"No hay disponibilidad para {personasRequeridas} personas en la fecha {fecha:yyyy-MM-dd HH:mm}");
        }
    }
}

/// <summary>
/// Excepciones específicas del contexto de Inventario
/// </summary>
public static class InventarioExceptions
{
    /// <summary>
    /// Excepción para errores de stock
    /// </summary>
    public class StockOperationException : AppException
    {
        public Guid IngredienteId { get; }
        public string Operation { get; }
        public decimal CantidadSolicitada { get; }
        public decimal CantidadDisponible { get; }

        public StockOperationException(Guid ingredienteId, string operation, decimal cantidadSolicitada, decimal cantidadDisponible, string message)
            : base($"Error de stock en '{operation}' del ingrediente {ingredienteId}: {message}", 
                   "STOCK_OPERATION_ERROR", 
                   new { IngredienteId = ingredienteId, CantidadSolicitada = cantidadSolicitada, CantidadDisponible = cantidadDisponible },
                   ErrorSeverity.High)
        {
            IngredienteId = ingredienteId;
            Operation = operation;
            CantidadSolicitada = cantidadSolicitada;
            CantidadDisponible = cantidadDisponible;
        }

        public static StockOperationException ForStockInsuficiente(Guid ingredienteId, decimal cantidadSolicitada, decimal cantidadDisponible)
        {
            return new StockOperationException(ingredienteId, "Reservar", cantidadSolicitada, cantidadDisponible,
                $"Stock insuficiente. Solicitado: {cantidadSolicitada}, Disponible: {cantidadDisponible}");
        }

        public static StockOperationException ForStockNegativo(Guid ingredienteId, decimal cantidadFinal)
        {
            return new StockOperationException(ingredienteId, "Actualizar", 0, cantidadFinal,
                $"La operación resultaría en stock negativo: {cantidadFinal}");
        }
    }

    /// <summary>
    /// Excepción para errores en órdenes de compra
    /// </summary>
    public class OrdenCompraOperationException : AppException
    {
        public Guid OrdenCompraId { get; }
        public string Operation { get; }

        public OrdenCompraOperationException(Guid ordenCompraId, string operation, string message)
            : base($"Error en operación '{operation}' de orden de compra {ordenCompraId}: {message}", 
                   "ORDEN_COMPRA_OPERATION_ERROR", 
                   ErrorSeverity.Medium)
        {
            OrdenCompraId = ordenCompraId;
            Operation = operation;
        }

        public static OrdenCompraOperationException ForEstadoInvalido(Guid ordenCompraId, string estadoActual, string operacion)
        {
            return new OrdenCompraOperationException(ordenCompraId, operacion,
                $"No se puede realizar la operación. Estado actual: {estadoActual}");
        }
    }
}

/// <summary>
/// Excepciones específicas del contexto Comercial
/// </summary>
public static class ComercialExceptions
{
    /// <summary>
    /// Excepción para errores de facturación
    /// </summary>
    public class FacturacionOperationException : AppException
    {
        public Guid FacturaId { get; }
        public string Operation { get; }

        public FacturacionOperationException(Guid facturaId, string operation, string message)
            : base($"Error en operación '{operation}' de factura {facturaId}: {message}", 
                   "FACTURACION_OPERATION_ERROR", 
                   ErrorSeverity.High)
        {
            FacturaId = facturaId;
            Operation = operation;
        }

        public static FacturacionOperationException ForFacturaYaPagada(Guid facturaId)
        {
            return new FacturacionOperationException(facturaId, "Anular",
                "No se puede anular una factura que ya está pagada");
        }

        public static FacturacionOperationException ForDescuentoInvalido(Guid facturaId, decimal descuento, decimal total)
        {
            return new FacturacionOperationException(facturaId, "AplicarDescuento",
                $"El descuento {descuento:C} no puede ser mayor al total {total:C}");
        }
    }

    /// <summary>
    /// Excepción para errores de fidelización
    /// </summary>
    public class FidelizacionOperationException : AppException
    {
        public Guid ClienteId { get; }
        public string Operation { get; }

        public FidelizacionOperationException(Guid clienteId, string operation, string message)
            : base($"Error en operación '{operation}' de fidelización del cliente {clienteId}: {message}", 
                   "FIDELIZACION_OPERATION_ERROR", 
                   ErrorSeverity.Medium)
        {
            ClienteId = clienteId;
            Operation = operation;
        }

        public static FidelizacionOperationException ForPuntosInsuficientes(Guid clienteId, int puntosRequeridos, int puntosDisponibles)
        {
            return new FidelizacionOperationException(clienteId, "CanjearPuntos",
                $"Puntos insuficientes. Requeridos: {puntosRequeridos}, Disponibles: {puntosDisponibles}");
        }

        public static FidelizacionOperationException ForTarjetaInactiva(Guid clienteId, Guid tarjetaId)
        {
            return new FidelizacionOperationException(clienteId, "UsarTarjeta",
                $"La tarjeta de fidelización {tarjetaId} está inactiva");
        }
    }
}

/// <summary>
/// Excepciones de servicios externos e integración
/// </summary>
public static class IntegrationExceptions
{
    /// <summary>
    /// Excepción para errores de servicios de notificación
    /// </summary>
    public class NotificationServiceException : AppException
    {
        public string ServiceName { get; }
        public string NotificationType { get; }

        public NotificationServiceException(string serviceName, string notificationType, string message, Exception? innerException = null)
            : base($"Error en servicio de notificación '{serviceName}' para tipo '{notificationType}': {message}", 
                   innerException, 
                   "NOTIFICATION_SERVICE_ERROR", 
                   ErrorSeverity.Medium)
        {
            ServiceName = serviceName;
            NotificationType = notificationType;
        }

        public static NotificationServiceException ForEmailService(string message, Exception? innerException = null)
        {
            return new NotificationServiceException("EmailService", "Email", message, innerException);
        }

        public static NotificationServiceException ForSMSService(string message, Exception? innerException = null)
        {
            return new NotificationServiceException("SMSService", "SMS", message, innerException);
        }

        public static NotificationServiceException ForSignalRService(string message, Exception? innerException = null)
        {
            return new NotificationServiceException("SignalRService", "SignalR", message, innerException);
        }
    }

    /// <summary>
    /// Excepción para errores de trabajos en segundo plano
    /// </summary>
    public class BackgroundJobException : AppException
    {
        public string JobType { get; }
        public string JobId { get; }

        public BackgroundJobException(string jobType, string jobId, string message, Exception? innerException = null)
            : base($"Error en trabajo en segundo plano '{jobType}' (ID: {jobId}): {message}", 
                   innerException, 
                   "BACKGROUND_JOB_ERROR", 
                   ErrorSeverity.Medium)
        {
            JobType = jobType;
            JobId = jobId;
        }

        public static BackgroundJobException ForSchedulingError(string jobType, string message)
        {
            return new BackgroundJobException(jobType, Guid.NewGuid().ToString(), 
                $"Error al programar trabajo: {message}");
        }

        public static BackgroundJobException ForExecutionError(string jobType, string jobId, string message, Exception innerException)
        {
            return new BackgroundJobException(jobType, jobId, 
                $"Error durante la ejecución: {message}", innerException);
        }
    }
} 