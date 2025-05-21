using System;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId
{
    /// <summary>
    /// Consulta para obtener una comanda por su ID
    /// </summary>
    public class ObtenerComandaPorIdQuery : IRequest<Result<ComandaDto>>
    {
        /// <summary>
        /// ID de la comanda a obtener
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Indica si se deben incluir los items de la comanda
        /// </summary>
        public bool IncluirItems { get; set; } = true;
    }
} 