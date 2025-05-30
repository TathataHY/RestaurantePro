using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

public class MovimientoInventarioDto : BaseDto
{
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public TipoMovimientoInventario TipoMovimiento { get; set; }
    public string TipoMovimientoTexto => TipoMovimiento.ToString();
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal => Cantidad * CostoUnitario;
    public decimal StockAnterior { get; set; }
    public decimal StockResultante { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Guid UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    
    // Información adicional
    public string? NumeroDocumento { get; set; }
    public Guid? ProveedorId { get; set; }
    public string? NombreProveedor { get; set; }
    
    // Propiedades calculadas para UI
    public string ColorTipo { get; set; } = string.Empty;
    public string IconoTipo { get; set; } = string.Empty;
    public string FechaTexto { get; set; } = string.Empty;
    public string CantidadTexto { get; set; } = string.Empty;
} 