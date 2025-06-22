namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

public class AgregarProductoDto
{
    /// <summary>
    /// ID del producto a agregar
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad del producto
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Observaciones específicas para este producto (opcional)
    /// </summary>
    public string? Observaciones { get; set; }
} 