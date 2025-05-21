using System;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda
{
    /// <summary>
    /// Command para crear una nueva comanda
    /// </summary>
    public class CrearComandaCommand : IRequest<Result<ComandaDto>>
    {
        /// <summary>
        /// ID de la mesa para la comanda
        /// </summary>
        public Guid MesaId { get; set; }
        
        /// <summary>
        /// ID del cliente (opcional)
        /// </summary>
        public Guid? ClienteId { get; set; }
        
        /// <summary>
        /// Observaciones iniciales para la comanda
        /// </summary>
        public string? Observaciones { get; set; }
    }
} 