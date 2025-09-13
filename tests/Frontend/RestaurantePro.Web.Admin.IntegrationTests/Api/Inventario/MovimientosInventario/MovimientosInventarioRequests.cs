namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.MovimientosInventario;

/// <summary>
/// Request para registrar un nuevo movimiento de inventario
/// </summary>
public class RegistrarMovimientoRequest
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario TipoMovimiento { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Guid? UsuarioId { get; set; }
    public DateTime? Fecha { get; set; }
}

/// <summary>
/// Request para actualizar un movimiento de inventario
/// </summary>
public class ActualizarMovimientoRequest
{
    public decimal? Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }
}
