using System;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Interfaz para el servicio de fidelización de clientes
    /// </summary>
    public interface IServicioFidelizacion
    {
        /// <summary>
        /// Calcula el descuento aplicable para un cliente según su nivel de fidelización
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Información del descuento aplicable</returns>
        Task<ResultadoDescuento> CalcularDescuentoAsync(Guid clienteId, decimal montoTotal);
        
        /// <summary>
        /// Acumula puntos para un cliente basado en el monto de su comanda
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Tarea asíncrona</returns>
        Task AcumularPuntosAsync(Guid clienteId, Guid comandaId, decimal montoTotal);
        
        /// <summary>
        /// Canjea puntos de un cliente
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="puntos">Cantidad de puntos a canjear</param>
        /// <param name="concepto">Motivo del canje</param>
        /// <returns>Tarea asíncrona</returns>
        Task CanjearPuntosAsync(Guid clienteId, int puntos, string concepto);
    }
} 