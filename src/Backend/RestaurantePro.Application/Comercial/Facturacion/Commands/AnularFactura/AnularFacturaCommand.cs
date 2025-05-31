using MediatR;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Domain.Common;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;

/// <summary>
/// Comando para anular una factura existente con motivos empresariales
/// Incluye validaciones, auditoría y notificaciones automáticas
/// </summary>
public class AnularFacturaCommand : IRequest<Result<FacturaDto>>
{
    /// <summary>
    /// ID único de la factura a anular
    /// </summary>
    public Guid FacturaId { get; set; }

    /// <summary>
    /// Motivo de la anulación (requerido para auditoría)
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del motivo de anulación
    /// </summary>
    public string? DescripcionDetallada { get; set; }

    /// <summary>
    /// ID del usuario que autoriza la anulación
    /// </summary>
    public Guid UsuarioAutorizaId { get; set; }

    /// <summary>
    /// Código de autorización para anulaciones especiales
    /// </summary>
    public string? CodigoAutorizacion { get; set; }

    /// <summary>
    /// Tipo de anulación: Normal, Emergencia, Administrativa, Devolución
    /// </summary>
    public string TipoAnulacion { get; set; } = "Normal";

    /// <summary>
    /// Indica si se debe generar nota de crédito automáticamente
    /// </summary>
    public bool GenerarNotaCredito { get; set; } = false;

    /// <summary>
    /// Indica si se debe notificar al cliente sobre la anulación
    /// </summary>
    public bool NotificarCliente { get; set; } = true;

    /// <summary>
    /// Indica si se debe procesar devolución automática del pago
    /// </summary>
    public bool ProcesarDevolucionPago { get; set; } = false;

    /// <summary>
    /// Método de devolución: Efectivo, Tarjeta, Transferencia, SaldoFavor
    /// </summary>
    public string? MetodoDevolucion { get; set; }

    /// <summary>
    /// Referencia externa para la devolución (número de transacción, etc.)
    /// </summary>
    public string? ReferenciaDevolucion { get; set; }

    /// <summary>
    /// Fecha límite para procesar la devolución
    /// </summary>
    public DateTime? FechaLimiteDevolucion { get; set; }

    /// <summary>
    /// Indica si se debe revertir movimientos de inventario
    /// </summary>
    public bool RevertirInventario { get; set; } = true;

    /// <summary>
    /// Indica si se debe cancelar puntos de fidelización otorgados
    /// </summary>
    public bool CancelarPuntosFidelizacion { get; set; } = true;

    /// <summary>
    /// Observaciones adicionales para el proceso de anulación
    /// </summary>
    public string? ObservacionesAdicionales { get; set; }

    /// <summary>
    /// Documentos adjuntos relacionados con la anulación
    /// </summary>
    public List<string> DocumentosAdjuntos { get; set; } = new();

    /// <summary>
    /// Prioridad de procesamiento: Baja=1, Normal=2, Alta=3, Crítica=4
    /// </summary>
    public int Prioridad { get; set; } = 2;

    /// <summary>
    /// Indica si requiere aprobación adicional de gerencia
    /// </summary>
    public bool RequiereAprobacionGerencia { get; set; } = false;

    /// <summary>
    /// ID del gerente que debe aprobar (si aplica)
    /// </summary>
    public Guid? GerenteAprobadorId { get; set; }

    /// <summary>
    /// Fecha y hora programada para la anulación (si aplica)
    /// </summary>
    public DateTime? FechaProgramadaAnulacion { get; set; }

    // Factory Methods para diferentes tipos de anulación

    /// <summary>
    /// Crea comando para anulación normal por error administrativo
    /// </summary>
    public static AnularFacturaCommand AnulacionNormal(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId, 
        bool notificarCliente = true)
    {
        return new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            TipoAnulacion = "Normal",
            NotificarCliente = notificarCliente,
            RevertirInventario = true,
            Prioridad = 2
        };
    }

    /// <summary>
    /// Crea comando para anulación de emergencia (problemas sistema, etc.)
    /// </summary>
    public static AnularFacturaCommand AnulacionEmergencia(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId, 
        string codigoAutorizacion)
    {
        return new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            CodigoAutorizacion = codigoAutorizacion,
            TipoAnulacion = "Emergencia",
            NotificarCliente = true,
            RevertirInventario = true,
            RequiereAprobacionGerencia = true,
            Prioridad = 4
        };
    }

    /// <summary>
    /// Crea comando para anulación con devolución de dinero
    /// </summary>
    public static AnularFacturaCommand AnulacionConDevolucion(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId,
        string metodoDevolucion,
        bool generarNotaCredito = true)
    {
        return new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            TipoAnulacion = "Devolución",
            ProcesarDevolucionPago = true,
            MetodoDevolucion = metodoDevolucion,
            GenerarNotaCredito = generarNotaCredito,
            NotificarCliente = true,
            RevertirInventario = true,
            CancelarPuntosFidelizacion = true,
            FechaLimiteDevolucion = DateTime.UtcNow.AddDays(30),
            Prioridad = 3
        };
    }

    /// <summary>
    /// Crea comando para anulación administrativa (corrección contable)
    /// </summary>
    public static AnularFacturaCommand AnulacionAdministrativa(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId,
        bool revertirInventario = false)
    {
        return new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            TipoAnulacion = "Administrativa",
            NotificarCliente = false,
            RevertirInventario = revertirInventario,
            CancelarPuntosFidelizacion = false,
            RequiereAprobacionGerencia = true,
            Prioridad = 1
        };
    }

    /// <summary>
    /// Crea comando para anulación por solicitud del cliente
    /// </summary>
    public static AnularFacturaCommand AnulacionSolicitudCliente(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId,
        string? referenciaDevolucion = null)
    {
        return new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            DescripcionDetallada = "Anulación solicitada por el cliente",
            UsuarioAutorizaId = usuarioAutorizaId,
            TipoAnulacion = "SolicitudCliente",
            ProcesarDevolucionPago = true,
            MetodoDevolucion = "SaldoFavor",
            ReferenciaDevolucion = referenciaDevolucion,
            GenerarNotaCredito = true,
            NotificarCliente = true,
            RevertirInventario = true,
            CancelarPuntosFidelizacion = true,
            Prioridad = 2
        };
    }

    /// <summary>
    /// Crea comando para anulación programada (para procesar después)
    /// </summary>
    public static AnularFacturaCommand AnulacionProgramada(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId,
        DateTime fechaProgramada)
    {
        return new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            TipoAnulacion = "Programada",
            FechaProgramadaAnulacion = fechaProgramada,
            NotificarCliente = true,
            RevertirInventario = true,
            Prioridad = 1
        };
    }

    /// <summary>
    /// Validación básica del comando
    /// </summary>
    public bool EsValido()
    {
        return FacturaId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(Motivo) &&
               UsuarioAutorizaId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(TipoAnulacion) &&
               Prioridad >= 1 && Prioridad <= 4;
    }

    /// <summary>
    /// Obtiene resumen del comando para logging
    /// </summary>
    public string ObtenerResumen()
    {
        return $"AnularFactura: {FacturaId} - Tipo: {TipoAnulacion} - Motivo: {Motivo} - Usuario: {UsuarioAutorizaId}";
    }
} 