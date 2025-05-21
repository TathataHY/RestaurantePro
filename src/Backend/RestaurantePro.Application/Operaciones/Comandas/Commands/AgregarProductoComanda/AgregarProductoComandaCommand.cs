using System;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProductoComanda
{
    /// <summary>
    /// Comando para agregar un producto a una comanda existente
    /// </summary>
    public class AgregarProductoComandaCommand : IRequest<Result<ComandaDto>>
    {
        /// <summary>
        /// ID de la comanda a la que se agregará el producto
        /// </summary>
        public Guid ComandaId { get; set; }
        
        /// <summary>
        /// ID del producto a agregar
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad del producto
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        public decimal PrecioUnitario { get; set; }
        
        /// <summary>
        /// Observaciones específicas para este producto
        /// </summary>
        public string? Observaciones { get; set; }
    }
} 