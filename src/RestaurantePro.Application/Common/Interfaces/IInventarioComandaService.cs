using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Application.Features.Inventario.Services;

namespace RestaurantePro.Application.Common.Interfaces
{
    public interface IInventarioComandaService
    {
        /// <summary>
        /// Actualiza el inventario en base a los productos incluidos en una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <returns>True si se procesó correctamente</returns>
        Task<bool> ActualizarInventarioPorComanda(int comandaId);

        /// <summary>
        /// Verifica la disponibilidad de ingredientes para un conjunto de productos
        /// </summary>
        /// <param name="detallesProductos">Lista de productos con sus cantidades</param>
        /// <returns>Diccionario con los ingredientes que faltan y sus cantidades</returns>
        Task<Dictionary<int, decimal>> VerificarDisponibilidadIngredientes(List<DetalleProductoDto> detallesProductos);
    }
} 