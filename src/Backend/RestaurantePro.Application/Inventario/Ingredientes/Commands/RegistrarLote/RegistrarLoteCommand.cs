using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;

/// <summary>
/// Comando para registrar un lote con fecha de vencimiento para un ingrediente
/// </summary>
public class RegistrarLoteCommand : IRequest<Result<IngredienteDto>>
{
    /// <summary>
    /// ID del ingrediente al que se registrará el lote
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Número de lote
    /// </summary>
    public string NumeroLote { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del lote
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Fecha de vencimiento del lote
    /// </summary>
    public DateTime FechaVencimiento { get; set; }

    /// <summary>
    /// Precio unitario del lote
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Observaciones del lote
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del usuario que registra el lote
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// ID del proveedor (opcional)
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public RegistrarLoteCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public RegistrarLoteCommand(Guid ingredienteId, string numeroLote, decimal cantidad, DateTime fechaVencimiento, decimal precioUnitario, Guid usuarioId)
    {
        IngredienteId = ingredienteId;
        NumeroLote = numeroLote;
        Cantidad = cantidad;
        FechaVencimiento = fechaVencimiento;
        PrecioUnitario = precioUnitario;
        UsuarioId = usuarioId;
    }
}