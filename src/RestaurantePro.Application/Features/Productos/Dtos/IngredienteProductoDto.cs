namespace RestaurantePro.Application.Features.Productos.Dtos
{
    public class IngredienteProductoDto
    {
        public int Id { get; set; }
        public int IngredienteId { get; set; }
        public string NombreIngrediente { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public bool Opcional { get; set; }
    }
} 