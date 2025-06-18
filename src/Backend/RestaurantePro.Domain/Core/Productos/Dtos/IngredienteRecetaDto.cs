namespace RestaurantePro.Domain.Core.Productos.Dtos
{
    public class IngredienteRecetaDto
    {
        public Guid IngredienteId { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
    }
} 