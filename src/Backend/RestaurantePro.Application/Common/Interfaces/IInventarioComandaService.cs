using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Application.Features.Inventario.Services;

namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Servicio para gestionar la relación entre inventario y comandas
    /// </summary>
    public interface IInventarioComandaService
    {
        /// <summary>
        /// Actualiza el inventario en base a los productos incluidos en una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se procesó correctamente</returns>
        Task<bool> ActualizarInventarioPorComanda(Guid comandaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica la disponibilidad de ingredientes para un conjunto de productos
        /// </summary>
        /// <param name="detallesProductos">Lista de productos con sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Diccionario con los ingredientes que faltan y sus cantidades</returns>
        Task<Dictionary<Guid, decimal>> VerificarDisponibilidadIngredientes(List<DetalleProductoDto> detallesProductos, CancellationToken cancellationToken = default);
    }
} 