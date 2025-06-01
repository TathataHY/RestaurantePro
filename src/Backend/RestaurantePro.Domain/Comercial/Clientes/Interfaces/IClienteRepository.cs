namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de clientes
    /// </summary>
    public interface IClienteRepository : IRepository<Cliente>
    {
        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        new Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un cliente por su email
        /// </summary>
        /// <param name="email">Email del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes por nombre (búsqueda parcial)
        /// </summary>
        /// <param name="nombre">Nombre completo o parcial</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes que coinciden con el criterio</returns>
        Task<IEnumerable<Cliente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes por segmento
        /// </summary>
        /// <param name="segmento">Segmento de cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes del segmento especificado</returns>
        Task<IEnumerable<Cliente>> ObtenerPorSegmentoAsync(SegmentoCliente segmento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes activos o inactivos
        /// </summary>
        /// <param name="activos">Indica si se deben obtener clientes activos o inactivos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes según el criterio</returns>
        Task<IEnumerable<Cliente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con tarjeta de fidelización
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes con tarjeta de fidelización</returns>
        Task<IEnumerable<Cliente>> ObtenerConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los clientes más frecuentes (por número de visitas)
        /// </summary>
        /// <param name="cantidad">Cantidad de clientes a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes más frecuentes</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesMasFrecuentesAsync(int cantidad, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con una cantidad mínima de puntos acumulados
        /// </summary>
        /// <param name="puntosMinimos">Cantidad mínima de puntos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes con los puntos especificados o más</returns>
        Task<IEnumerable<Cliente>> ObtenerPorPuntosMinimosAsync(int puntosMinimos, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con clientes y total de elementos</returns>
        new Task<(IEnumerable<Cliente> Clientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes registrados en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes registrados en el rango de fechas</returns>
        Task<IEnumerable<Cliente>> ObtenerPorRangoFechasRegistroAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si existe un cliente con el ID especificado
        /// </summary>
        /// <param name="id">ID del cliente a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe, False en caso contrario</returns>
        Task<bool> VerificarExistenciaAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes por sus IDs
        /// </summary>
        /// <param name="ids">Lista de IDs de clientes</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes correspondientes a los IDs proporcionados</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes activos con sus historiales de visitas
        /// </summary>
        /// <param name="cantidadMinimaVisitas">Cantidad mínima de visitas para incluir al cliente</param>
        /// <param name="cantidadDias">Días hacia atrás para considerar las visitas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes activos con historial de visitas</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesActivosConVisitasAsync(int cantidadMinimaVisitas, int cantidadDias, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con su historial de visitas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio para considerar las visitas</param>
        /// <param name="fechaFin">Fecha de fin para considerar las visitas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes con su historial de visitas</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesConHistorialVisitasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los clientes con su historial de visitas
        /// </summary>
        /// <param name="diasHistorial">Días hacia atrás para considerar las visitas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de todos los clientes con su historial de visitas</returns>
        Task<IEnumerable<Cliente>> ObtenerTodosConHistorialVisitasAsync(int diasHistorial, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene facturas recientes de un cliente en un rango de fechas
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de facturas del cliente en el rango de fechas especificado</returns>
        Task<IEnumerable<Factura>> ObtenerFacturasRecientesAsync(Guid clienteId, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un cliente
        /// </summary>
        /// <param name="cliente">Cliente a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea completada cuando finaliza la operación</returns>
        new Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda un cliente
        /// </summary>
        /// <param name="cliente">Cliente a guardar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea completada cuando finaliza la operación</returns>
        Task GuardarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    }
}

