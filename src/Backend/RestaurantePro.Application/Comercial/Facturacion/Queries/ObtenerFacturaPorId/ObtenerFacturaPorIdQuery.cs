using MediatR;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Domain.Common;

namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;

/// <summary>
/// Query para obtener una factura específica por su ID con detalles completos
/// Incluye información de cliente, detalles, descuentos, pagos y auditoría
/// </summary>
public class ObtenerFacturaPorIdQuery : IRequest<Result<FacturaDetalladaDto>>
{
    /// <summary>
    /// ID único de la factura a consultar
    /// </summary>
    public Guid FacturaId { get; set; }

    /// <summary>
    /// Indica si incluir detalles de los productos en la respuesta
    /// </summary>
    public bool IncluirDetallesProductos { get; set; } = true;

    /// <summary>
    /// Indica si incluir información completa del cliente
    /// </summary>
    public bool IncluirInformacionCliente { get; set; } = true;

    /// <summary>
    /// Indica si incluir historial de descuentos aplicados
    /// </summary>
    public bool IncluirDescuentos { get; set; } = true;

    /// <summary>
    /// Indica si incluir historial de pagos realizados
    /// </summary>
    public bool IncluirPagos { get; set; } = true;

    /// <summary>
    /// Indica si incluir auditoría de movimientos de la factura
    /// </summary>
    public bool IncluirAuditoria { get; set; } = false;

    /// <summary>
    /// Indica si incluir información de las comandas asociadas
    /// </summary>
    public bool IncluirComandas { get; set; } = true;

    /// <summary>
    /// Indica si incluir documentos adjuntos relacionados
    /// </summary>
    public bool IncluirDocumentosAdjuntos { get; set; } = false;

    /// <summary>
    /// Indica si incluir información tributaria detallada
    /// </summary>
    public bool IncluirInformacionTributaria { get; set; } = false;

    /// <summary>
    /// Indica si incluir métricas de rentabilidad
    /// </summary>
    public bool IncluirMetricasRentabilidad { get; set; } = false;

    /// <summary>
    /// Formato de respuesta: Completo, Resumido, Basico
    /// </summary>
    public string FormatoRespuesta { get; set; } = "Completo";

    /// <summary>
    /// ID del usuario que consulta (para permisos y auditoría)
    /// </summary>
    public Guid? UsuarioConsultaId { get; set; }

    /// <summary>
    /// Indica si validar permisos de acceso del usuario
    /// </summary>
    public bool ValidarPermisos { get; set; } = true;

    /// <summary>
    /// Motivo de la consulta (para auditoría)
    /// </summary>
    public string? MotivoConsulta { get; set; }

    // Factory Methods para diferentes tipos de consulta

    /// <summary>
    /// Consulta básica solo con información esencial de la factura
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaBasica(Guid facturaId, Guid? usuarioId = null)
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Basico",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = false,
            IncluirDescuentos = false,
            IncluirPagos = false,
            IncluirAuditoria = false,
            IncluirComandas = false,
            IncluirDocumentosAdjuntos = false,
            IncluirInformacionTributaria = false,
            IncluirMetricasRentabilidad = false,
            ValidarPermisos = false
        };
    }

    /// <summary>
    /// Consulta resumida con información comercial principal
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaResumida(Guid facturaId, Guid usuarioId)
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Resumido",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = true,
            IncluirDescuentos = true,
            IncluirPagos = true,
            IncluirAuditoria = false,
            IncluirComandas = false,
            IncluirDocumentosAdjuntos = false,
            IncluirInformacionTributaria = false,
            IncluirMetricasRentabilidad = false,
            ValidarPermisos = true,
            MotivoConsulta = "Consulta comercial"
        };
    }

    /// <summary>
    /// Consulta completa con toda la información disponible
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaCompleta(Guid facturaId, Guid usuarioId, string motivo = "Consulta administrativa")
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Completo",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = true,
            IncluirDescuentos = true,
            IncluirPagos = true,
            IncluirAuditoria = true,
            IncluirComandas = true,
            IncluirDocumentosAdjuntos = true,
            IncluirInformacionTributaria = true,
            IncluirMetricasRentabilidad = true,
            ValidarPermisos = true,
            MotivoConsulta = motivo
        };
    }

    /// <summary>
    /// Consulta para auditoría con énfasis en trazabilidad
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaAuditoria(Guid facturaId, Guid usuarioId)
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Completo",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = true,
            IncluirDescuentos = true,
            IncluirPagos = true,
            IncluirAuditoria = true,
            IncluirComandas = true,
            IncluirDocumentosAdjuntos = true,
            IncluirInformacionTributaria = true,
            IncluirMetricasRentabilidad = false,
            ValidarPermisos = true,
            MotivoConsulta = "Auditoría y control"
        };
    }

    /// <summary>
    /// Consulta para análisis financiero y rentabilidad
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaFinanciera(Guid facturaId, Guid usuarioId)
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Completo",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = false,
            IncluirDescuentos = true,
            IncluirPagos = true,
            IncluirAuditoria = false,
            IncluirComandas = false,
            IncluirDocumentosAdjuntos = false,
            IncluirInformacionTributaria = true,
            IncluirMetricasRentabilidad = true,
            ValidarPermisos = true,
            MotivoConsulta = "Análisis financiero"
        };
    }

    /// <summary>
    /// Consulta para atención al cliente
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaAtencionCliente(Guid facturaId, Guid usuarioId)
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Resumido",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = true,
            IncluirDescuentos = true,
            IncluirPagos = true,
            IncluirAuditoria = false,
            IncluirComandas = true,
            IncluirDocumentosAdjuntos = false,
            IncluirInformacionTributaria = false,
            IncluirMetricasRentabilidad = false,
            ValidarPermisos = true,
            MotivoConsulta = "Atención al cliente"
        };
    }

    /// <summary>
    /// Consulta para impresión de factura
    /// </summary>
    public static ObtenerFacturaPorIdQuery ConsultaImpresion(Guid facturaId, Guid? usuarioId = null)
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = facturaId,
            UsuarioConsultaId = usuarioId,
            FormatoRespuesta = "Resumido",
            IncluirDetallesProductos = true,
            IncluirInformacionCliente = true,
            IncluirDescuentos = true,
            IncluirPagos = false,
            IncluirAuditoria = false,
            IncluirComandas = false,
            IncluirDocumentosAdjuntos = false,
            IncluirInformacionTributaria = true,
            IncluirMetricasRentabilidad = false,
            ValidarPermisos = false,
            MotivoConsulta = "Impresión de factura"
        };
    }

    /// <summary>
    /// Validación básica de la query
    /// </summary>
    public bool EsValida()
    {
        return FacturaId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(FormatoRespuesta) &&
               new[] { "Basico", "Resumido", "Completo" }.Contains(FormatoRespuesta, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene resumen de la consulta para logging
    /// </summary>
    public string ObtenerResumen()
    {
        return $"ObtenerFacturaPorId: {FacturaId} - Formato: {FormatoRespuesta} - Usuario: {UsuarioConsultaId} - Motivo: {MotivoConsulta ?? "No especificado"}";
    }

    /// <summary>
    /// Determina el nivel de detalle requerido
    /// </summary>
    public int ObtenerNivelDetalle()
    {
        return FormatoRespuesta.ToLower() switch
        {
            "basico" => 1,
            "resumido" => 2,
            "completo" => 3,
            _ => 2
        };
    }

    /// <summary>
    /// Obtiene lista de elementos a incluir en la consulta
    /// </summary>
    public List<string> ObtenerElementosAIncluir()
    {
        var elementos = new List<string>();

        if (IncluirDetallesProductos) elementos.Add("DetallesProductos");
        if (IncluirInformacionCliente) elementos.Add("InformacionCliente");
        if (IncluirDescuentos) elementos.Add("Descuentos");
        if (IncluirPagos) elementos.Add("Pagos");
        if (IncluirAuditoria) elementos.Add("Auditoria");
        if (IncluirComandas) elementos.Add("Comandas");
        if (IncluirDocumentosAdjuntos) elementos.Add("DocumentosAdjuntos");
        if (IncluirInformacionTributaria) elementos.Add("InformacionTributaria");
        if (IncluirMetricasRentabilidad) elementos.Add("MetricasRentabilidad");

        return elementos;
    }
} 