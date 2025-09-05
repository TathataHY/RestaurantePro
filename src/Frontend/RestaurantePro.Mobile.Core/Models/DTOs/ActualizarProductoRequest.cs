namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Request para actualizar un producto (alineado con ActualizarProductoCommand del backend)
/// </summary>
public class ActualizarProductoRequest
{
	public Guid Id { get; set; }
	public string Nombre { get; set; } = string.Empty;
	public string Descripcion { get; set; } = string.Empty;
	public decimal Precio { get; set; }
	public Guid CategoriaId { get; set; }
	public bool Activo { get; set; } = true;
}


