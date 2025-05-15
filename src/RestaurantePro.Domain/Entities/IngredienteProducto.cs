namespace RestaurantePro.Domain.Entities
{
    public class IngredienteProducto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int IngredienteId { get; set; }
        public decimal Cantidad { get; set; }
        public bool Opcional { get; set; }
        
        // Relaciones
        public virtual Producto Producto { get; set; }
        public virtual Ingrediente Ingrediente { get; set; }
    }
} 