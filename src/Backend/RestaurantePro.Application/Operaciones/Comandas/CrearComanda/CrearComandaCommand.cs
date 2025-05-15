using System;
using System.Collections.Generic;
using MediatR;
using RestaurantePro.Application.Common.Results;

namespace RestaurantePro.Application.Operaciones.Comandas.CrearComanda
{
    /// <summary>
    /// Comando para crear una nueva comanda
    /// </summary>
    public class CrearComandaCommand : IRequest<Result<Guid>>
    {
        /// <summary>
        /// ID de la mesa donde se crea la comanda
        /// </summary>
        public Guid MesaId { get; }
        
        /// <summary>
        /// ID del mesero que crea la comanda
        /// </summary>
        public Guid MeseroId { get; }
        
        /// <summary>
        /// ID del cliente (opcional)
        /// </summary>
        public Guid? ClienteId { get; }
        
        /// <summary>
        /// Observaciones adicionales para la comanda
        /// </summary>
        public string Observaciones { get; }
        
        /// <summary>
        /// Productos iniciales para agregar a la comanda
        /// </summary>
        public List<ProductoComandaDto> Productos { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public CrearComandaCommand(
            Guid mesaId, 
            Guid meseroId, 
            Guid? clienteId = null, 
            string observaciones = null,
            List<ProductoComandaDto> productos = null)
        {
            MesaId = mesaId;
            MeseroId = meseroId;
            ClienteId = clienteId;
            Observaciones = observaciones;
            Productos = productos ?? new List<ProductoComandaDto>();
        }
    }
    
    /// <summary>
    /// DTO para los productos al crear una comanda
    /// </summary>
    public class ProductoComandaDto
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Observaciones específicas para este producto
        /// </summary>
        public string Observaciones { get; set; }
    }
} 