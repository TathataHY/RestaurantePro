namespace RestaurantePro.Domain.Proveedores.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de proveedores
    /// </summary>
    public interface IProveedorRepository : IRepository<Proveedor>
    {
        /// <summary>
        /// Obtiene un proveedor por su ID
        /// </summary>
        /// <param name="id">ID del proveedor</param>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor encontrado o null si no existe</returns>
        Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un proveedor por su nombre
        /// </summary>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor encontrado o null si no existe</returns>
        Task<Proveedor?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores por RFC
        /// </summary>
        /// <param name="rfc">RFC del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores que coinciden con el RFC</returns>
        Task<IEnumerable<Proveedor>> ObtenerPorRFCAsync(string rfc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un proveedor por su RUT
        /// </summary>
        /// <param name="rut">RUT del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor encontrado o null si no existe</returns>
        Task<Proveedor?> ObtenerPorRutAsync(string rut, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores</returns>
        Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores activos
        /// </summary>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores activos</returns>
        Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores por ciudad o región
        /// </summary>
        /// <param name="ciudad">Ciudad o región</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores de la ciudad o región especificada</returns>
        Task<IEnumerable<Proveedor>> ObtenerPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores que proveen un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="soloActivos">Indica si se deben obtener solo proveedores activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores que ofrecen el ingrediente</returns>
        Task<IEnumerable<Proveedor>> ObtenerPorIngredienteAsync(Guid ingredienteId, bool soloActivos = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores por tipo de producto o servicio
        /// </summary>
        /// <param name="tipoProducto">Tipo de producto o servicio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores que ofrecen el tipo de producto</returns>
        Task<IEnumerable<Proveedor>> ObtenerPorTipoProductoAsync(string tipoProducto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un contacto de proveedor por su ID
        /// </summary>
        /// <param name="contactoId">ID del contacto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Contacto encontrado o null si no existe</returns>
        Task<ContactoProveedor?> ObtenerContactoPorIdAsync(Guid contactoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene contactos de un proveedor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de contactos del proveedor</returns>
        Task<IEnumerable<ContactoProveedor>> ObtenerContactosPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca proveedores por término (nombre, ciudad, email, etc.)
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores que coinciden con el término</returns>
        Task<IEnumerable<Proveedor>> BuscarAsync(string termino, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con proveedores y total de elementos</returns>
        Task<(IEnumerable<Proveedor> Proveedores, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirContactos = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene estadísticas de proveedores
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Estadísticas de proveedores</returns>
        Task<EstadisticasProveedores> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);
    }
}