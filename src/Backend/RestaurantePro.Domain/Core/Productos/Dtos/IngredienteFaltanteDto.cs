namespace RestaurantePro.Domain.Core.Productos.Dtos;

public class IngredienteFaltanteDto
{
    public Guid IngredienteId { get; }
    public string Nombre { get; }
    public decimal CantidadFaltante { get; }
    public string UnidadMedida { get; }

    public IngredienteFaltanteDto(Guid ingredienteId, string nombre, decimal cantidadFaltante, string unidadMedida)
    {
        IngredienteId = ingredienteId;
        Nombre = nombre;
        CantidadFaltante = cantidadFaltante;
        UnidadMedida = unidadMedida;
    }
} 