namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Interfaz para el servicio de fidelización de clientes
    /// </summary>
    public interface IServicioFidelizacion
    {
        /// <summary>
        /// Agrega puntos a la tarjeta de fidelización de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntos">Puntos a agregar</param>
        /// <param name="motivo">Motivo por el que se agregan los puntos</param>
        /// <returns>Resultado de la operación con los puntos totales</returns>
        Task<Result<int>> AgregarPuntosAsync(Guid clienteId, int puntos, string motivo);
        
        /// <summary>
        /// Calcula el descuento aplicable para un cliente según su nivel de fidelización
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoCompra">Monto de la compra</param>
        /// <returns>Resultado con el descuento aplicable</returns>
        Task<Result<decimal>> CalcularDescuentoAsync(Guid clienteId, decimal montoCompra);
        
        /// <summary>
        /// Canjea puntos de la tarjeta de fidelización por un beneficio
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntosACanjear">Puntos a canjear</param>
        /// <param name="beneficio">Descripción del beneficio</param>
        /// <returns>Resultado de la operación con los puntos restantes</returns>
        Task<Result<int>> CanjearPuntosAsync(Guid clienteId, int puntosACanjear, string beneficio);
        
        /// <summary>
        /// Acumula puntos para un cliente basado en el monto de su comanda
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Resultado de la operación con los puntos acumulados</returns>
        Task<Result<int>> AcumularPuntosAsync(Guid clienteId, Guid comandaId, decimal montoTotal);
        
        /// <summary>
        /// Calcula el descuento basado en los puntos a utilizar
        /// </summary>
        /// <param name="puntos">Puntos a utilizar</param>
        /// <param name="montoTotal">Monto total (opcional, solo para validaciones)</param>
        /// <returns>Monto del descuento calculado</returns>
        decimal CalcularDescuentoPorPuntos(int puntos, decimal montoTotal);
        
        /// <summary>
        /// Crea una nueva tarjeta de fidelización y la asocia al cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Resultado de la operación con la tarjeta creada</returns>
        Task<Result<TarjetaFidelizacion>> CrearTarjetaFidelizacionAsync(Guid clienteId);
    }
} 