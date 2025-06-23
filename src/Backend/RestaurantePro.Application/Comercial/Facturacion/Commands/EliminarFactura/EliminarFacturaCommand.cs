namespace RestaurantePro.Application.Comercial.Facturacion.Commands.EliminarFactura;

/// <summary>
/// Comando para anular una factura existente
/// Incluye validaciones de seguridad y auditoría
/// </summary>
public class EliminarFacturaCommand : IRequest<Result<bool>>
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
    /// ID del usuario que autoriza la anulación
    /// </summary>
    public Guid UsuarioAutorizaId { get; set; }

    /// <summary>
    /// Observaciones adicionales para el proceso de anulación
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public EliminarFacturaCommand()
    {
    }

    /// <summary>
    /// Constructor para anulación básica
    /// </summary>
    public EliminarFacturaCommand(Guid facturaId, string motivo, Guid usuarioAutorizaId)
    {
        FacturaId = facturaId;
        Motivo = motivo;
        UsuarioAutorizaId = usuarioAutorizaId;
    }

    /// <summary>
    /// Factory method para anulación normal
    /// </summary>
    public static EliminarFacturaCommand CrearAnulacionNormal(
        Guid facturaId, 
        string motivo, 
        Guid usuarioAutorizaId)
    {
        return new EliminarFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId
        };
    }

    /// <summary>
    /// Validación básica del comando
    /// </summary>
    public bool EsValido()
    {
        return FacturaId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(Motivo) &&
               UsuarioAutorizaId != Guid.Empty;
    }

    /// <summary>
    /// Obtiene resumen del comando para logging
    /// </summary>
    public string ObtenerResumen()
    {
        return $"AnularFactura: {FacturaId} - Motivo: {Motivo} - Usuario: {UsuarioAutorizaId}";
    }
} 