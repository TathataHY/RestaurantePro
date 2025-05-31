namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Fachada de servicios para el contexto de Comercial
    /// Esta interfaz expone operaciones compuestas para ser utilizadas por la capa de Aplicación
    /// </summary>
    public interface IComercialServiceFacade
    {
        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el cliente encontrado o null si no existe</returns>
        Task<Result<Cliente?>> ObtenerClientePorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra un nuevo cliente y le asigna una tarjeta de fidelización
        /// </summary>
        /// <param name="nombre">Nombre del cliente</param>
        /// <param name="apellidos">Apellidos del cliente</param>
        /// <param name="email">Email del cliente</param>
        /// <param name="telefono">Teléfono del cliente (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el cliente registrado</returns>
        Task<Result<Cliente>> RegistrarNuevoClienteConTarjetaAsync(string nombre, string apellidos, string email, string? telefono = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza los datos de un cliente existente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="nombre">Nuevo nombre (null para no cambiar)</param>
        /// <param name="apellidos">Nuevos apellidos (null para no cambiar)</param>
        /// <param name="email">Nuevo email (null para no cambiar)</param>
        /// <param name="telefono">Nuevo teléfono (null para no cambiar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el cliente actualizado</returns>
        Task<Result<Cliente?>> ActualizarDatosClienteAsync(Guid clienteId, string? nombre = null, string? apellidos = null, string? email = null, string? telefono = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Asigna puntos a un cliente por una compra/comanda
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntos">Cantidad de puntos a asignar</param>
        /// <param name="comandaId">ID de la comanda asociada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado indicando si la operación fue exitosa</returns>
        Task<Result<bool>> AsignarPuntosClienteAsync(Guid clienteId, int puntos, Guid comandaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcula el descuento aplicable para un cliente según su nivel de fidelización
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoTotal">Monto total de la compra</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el monto del descuento aplicable</returns>
        Task<Result<decimal>> CalcularDescuentoPuntosFidelizacionAsync(Guid clienteId, decimal montoTotal, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Canjea puntos de fidelización por un descuento
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntosAUtilizar">Cantidad de puntos a utilizar</param>
        /// <param name="comandaId">ID de la comanda donde aplicar el descuento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el monto del descuento obtenido</returns>
        Task<Result<decimal>> CanjearPuntosPorDescuentoAsync(Guid clienteId, int puntosAUtilizar, Guid comandaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la política de clientes frecuentes para determinar segmentos
        /// </summary>
        /// <param name="clienteIds">IDs específicos de clientes a procesar (null para procesar todos)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el diccionario de clientes y sus segmentos actualizados</returns>
        Task<Result<Dictionary<Guid, SegmentoCliente>>> EjecutarPoliticaClientesFrecuentesAsync(IEnumerable<Guid>? clienteIds = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Procesa un cliente inactivo para reactivación con validaciones de negocio complejas.
        /// Incluye verificaciones de historial y elegibilidad.
        /// </summary>
        /// <param name="clienteId">ID del cliente a reactivar</param>
        /// <param name="motivoReactivacion">Motivo de la reactivación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result> ProcesarReactivacionClienteAsync(Guid clienteId, string motivoReactivacion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transfiere puntos entre clientes con validaciones exhaustivas.
        /// Incluye verificación de eligibilidad y límites de transferencia.
        /// </summary>
        /// <param name="clienteOrigenId">ID del cliente que transfiere puntos</param>
        /// <param name="clienteDestinoId">ID del cliente que recibe puntos</param>
        /// <param name="puntos">Cantidad de puntos a transferir</param>
        /// <param name="motivo">Motivo de la transferencia</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación con detalles de la transferencia</returns>
        Task<Result<TransferenciaResult>> TransferirPuntosAsync(
            Guid clienteOrigenId,
            Guid clienteDestinoId,
            int puntos,
            string motivo,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida múltiples clientes en lote y retorna un resumen de resultados.
        /// </summary>
        /// <param name="clienteIds">IDs de clientes a validar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con resumen de validaciones</returns>
        Task<Result<ValidacionLoteResult>> ValidarClientesEnLoteAsync(IEnumerable<Guid> clienteIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Acumula puntos de fidelización para un cliente basado en el monto de compra.
        /// Calcula automáticamente los puntos según las reglas de negocio configuradas.
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoCompra">Monto total de la compra</param>
        /// <param name="comandaId">ID de la comanda asociada (opcional)</param>
        /// <param name="concepto">Concepto de la acumulación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la cantidad de puntos acumulados</returns>
        Task<Result<int>> AcumularPuntosPorCompraAsync(
            Guid clienteId, 
            decimal montoCompra, 
            Guid? comandaId = null, 
            string concepto = "Compra", 
            CancellationToken cancellationToken = default);
    }
} 