namespace RestaurantePro.Domain.Comercial.Promociones.Services
{
    /// <summary>
    /// Servicio de dominio para gestionar promociones
    /// </summary>
    public interface IServicioPromociones
    {
        /// <summary>
        /// Obtiene todas las promociones válidas para un cliente y monto específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoTotal">Monto total de la compra</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de promociones válidas</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesValidasParaClienteAsync(
            Guid clienteId,
            decimal montoTotal,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Obtiene todas las promociones aplicables a un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="categoriaId">ID de la categoría (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de promociones aplicables</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesParaProductoAsync(
            Guid productoId,
            Guid? categoriaId = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Calcula el descuento total aplicando las promociones seleccionadas
        /// </summary>
        /// <param name="promocionesIds">IDs de las promociones a aplicar</param>
        /// <param name="montoOriginal">Monto original sin descuentos</param>
        /// <param name="productosIds">IDs de los productos en la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Monto total del descuento</returns>
        Task<decimal> CalcularDescuentoTotalAsync(
            IEnumerable<Guid> promocionesIds,
            decimal montoOriginal,
            IEnumerable<Guid> productosIds,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Registra el uso de una promoción por un cliente
        /// </summary>
        /// <param name="promocionId">ID de la promoción</param>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="montoAplicado">Monto del descuento aplicado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se registró correctamente</returns>
        Task<bool> RegistrarUsoPromocionAsync(
            Guid promocionId,
            Guid clienteId,
            Guid comandaId,
            decimal montoAplicado,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Valida si las promociones seleccionadas son compatibles entre sí
        /// </summary>
        /// <param name="promocionesIds">IDs de las promociones a validar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si son compatibles</returns>
        Task<bool> ValidarCompatibilidadPromocionesAsync(
            IEnumerable<Guid> promocionesIds,
            CancellationToken cancellationToken = default);
    }
} 