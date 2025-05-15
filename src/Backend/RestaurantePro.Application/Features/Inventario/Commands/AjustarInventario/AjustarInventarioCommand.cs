using MediatR;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Inventario.Commands.AjustarInventario
{
    /// <summary>
    /// Comando para ajustar manualmente el inventario
    /// </summary>
    public class AjustarInventarioCommand : IRequest<int>
    {
        /// <summary>
        /// ID del inventario a ajustar
        /// </summary>
        public int InventarioId { get; set; }
        
        /// <summary>
        /// ID del ingrediente (opcional si se provee InventarioId)
        /// </summary>
        public int? IngredienteId { get; set; }
        
        /// <summary>
        /// Tipo de movimiento (entrada, salida, merma, ajuste)
        /// </summary>
        public TipoMovimiento TipoMovimiento { get; set; }
        
        /// <summary>
        /// Cantidad a ajustar
        /// </summary>
        public decimal Cantidad { get; set; }
        
        /// <summary>
        /// Motivo o descripción del ajuste
        /// </summary>
        public string Motivo { get; set; }
        
        /// <summary>
        /// Referencia o documento asociado al ajuste
        /// </summary>
        public string Referencia { get; set; }
        
        /// <summary>
        /// Costo unitario (opcional, para entradas de inventario)
        /// </summary>
        public decimal? CostoUnitario { get; set; }
    }
} 