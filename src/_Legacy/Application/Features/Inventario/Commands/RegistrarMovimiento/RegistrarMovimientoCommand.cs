using MediatR;
using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Application.Features.Inventario.Commands.RegistrarMovimiento
{
    /// <summary>
    /// Comando para registrar un movimiento de inventario individual
    /// </summary>
    public class RegistrarMovimientoCommand : IRequest<int>
    {
        /// <summary>
        /// ID del inventario asociado
        /// </summary>
        public int InventarioId { get; set; }
        
        /// <summary>
        /// Tipo de movimiento a registrar
        /// </summary>
        public TipoMovimiento TipoMovimiento { get; set; }
        
        /// <summary>
        /// Cantidad involucrada en el movimiento
        /// </summary>
        public decimal Cantidad { get; set; }
        
        /// <summary>
        /// Descripción del movimiento
        /// </summary>
        public string Descripcion { get; set; }
        
        /// <summary>
        /// Referencia externa (opcional)
        /// </summary>
        public string Referencia { get; set; }
        
        /// <summary>
        /// ID de la comanda asociada (opcional)
        /// </summary>
        public int? ComandaId { get; set; }
        
        /// <summary>
        /// ID de la orden de compra asociada (opcional)
        /// </summary>
        public int? OrdenCompraId { get; set; }
    }
} 