namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Request para crear un producto (alineado con CrearProductoCommand del backend)
/// </summary>
public class CrearProductoRequest
{
	public string Nombre { get; set; } = string.Empty;
	public string Descripcion { get; set; } = string.Empty;
	public decimal Precio { get; set; }
	public Guid CategoriaId { get; set; }
	public bool Activo { get; set; } = true;
}


