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
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente?> ObtenerClientePorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra un nuevo cliente y le asigna una tarjeta de fidelización
        /// </summary>
        /// <param name="nombre">Nombre del cliente</param>
        /// <param name="apellidos">Apellidos del cliente</param>
        /// <param name="email">Email del cliente</param>
        /// <param name="telefono">Teléfono del cliente (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente registrado</returns>
        Task<Cliente> RegistrarNuevoClienteConTarjetaAsync(string nombre, string apellidos, string email, string? telefono = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza los datos de un cliente existente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="nombre">Nuevo nombre (null para no cambiar)</param>
        /// <param name="apellidos">Nuevos apellidos (null para no cambiar)</param>
        /// <param name="email">Nuevo email (null para no cambiar)</param>
        /// <param name="telefono">Nuevo teléfono (null para no cambiar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente actualizado o null si no se encuentra</returns>
        Task<Cliente?> ActualizarDatosClienteAsync(Guid clienteId, string? nombre = null, string? apellidos = null, string? email = null, string? telefono = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Asigna puntos a un cliente por una compra
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntos">Cantidad de puntos a asignar</param>
        /// <param name="comandaId">ID de la comanda que generó los puntos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se asignaron los puntos, False si no se encontró el cliente</returns>
        Task<bool> AsignarPuntosClienteAsync(Guid clienteId, int puntos, Guid comandaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcula un descuento basado en los puntos del cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoTotal">Monto total de la compra</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Monto de descuento calculado (0 si no aplica o no se encuentra el cliente)</returns>
        Task<decimal> CalcularDescuentoPuntosFidelizacionAsync(Guid clienteId, decimal montoTotal, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Aplica un canje de puntos por un descuento
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntosAUtilizar">Cantidad de puntos a utilizar</param>
        /// <param name="comandaId">ID de la comanda donde se aplica el descuento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Monto del descuento aplicado (0 si no se pudo aplicar)</returns>
        Task<decimal> CanjearPuntosPorDescuentoAsync(Guid clienteId, int puntosAUtilizar, Guid comandaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la política de clientes frecuentes para analizar y clasificar clientes
        /// </summary>
        /// <param name="clienteIds">Lista opcional de IDs de clientes para procesar (null para procesar todos)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Diccionario con los resultados de la política por cliente</returns>
        Task<Dictionary<Guid, SegmentoCliente>> EjecutarPoliticaClientesFrecuentesAsync(IEnumerable<Guid>? clienteIds = null, CancellationToken cancellationToken = default);
    }
} 