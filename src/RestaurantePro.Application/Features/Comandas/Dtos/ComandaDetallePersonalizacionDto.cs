namespace RestaurantePro.Application.Features.Comandas.Dtos
{
    public class ComandaDetallePersonalizacionDto
    {
        public int Id { get; set; }
        public int IngredienteId { get; set; }
        public string NombreIngrediente { get; set; }
        public bool Agregar { get; set; }
        public bool Quitar { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioExtra { get; set; }
    }
} 