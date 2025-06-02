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
        /// <param name="incluirCategorias">Indica si se deben incluir las categorías del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor encontrado o null si no existe</returns>
        Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, bool incluirCategorias = true, CancellationToken cancellationToken = default);

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
        /// <param name="incluirCategorias">Indica si se deben incluir las categorías de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores</returns>
        Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores activos
        /// </summary>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos de los proveedores</param>
        /// <param name="incluirCategorias">Indica si se deben incluir las categorías de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores activos</returns>
        Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default);

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
        /// <param name="incluirCategorias">Indica si se deben incluir las categorías de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con proveedores y total de elementos</returns>
        Task<(IEnumerable<Proveedor> Proveedores, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            bool incluirContactos = false, 
            bool incluirCategorias = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores paginados con filtros avanzados
        /// </summary>
        /// <param name="pagina">Número de página (base 1)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="termino">Término de búsqueda</param>
        /// <param name="categoria">Categoría de proveedor</param>
        /// <param name="soloActivos">Incluir solo proveedores activos</param>
        /// <param name="incluirInactivos">Incluir proveedores inactivos</param>
        /// <param name="campoOrden">Campo para ordenar</param>
        /// <param name="ordenAscendente">Dirección del orden</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores paginados</returns>
        Task<IEnumerable<Proveedor>> ObtenerProveedoresPaginadosAsync(
            int pagina, 
            int elementosPorPagina, 
            string? termino = null,
            Enums.CategoriaProveedor? categoria = null,
            bool soloActivos = true,
            bool incluirInactivos = false,
            string? campoOrden = null,
            bool ordenAscendente = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta el total de proveedores con filtros
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <param name="categoria">Categoría de proveedor</param>
        /// <param name="soloActivos">Incluir solo proveedores activos</param>
        /// <param name="incluirInactivos">Incluir proveedores inactivos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Total de proveedores que cumplen los criterios</returns>
        Task<int> ContarProveedoresAsync(
            string? termino = null,
            Enums.CategoriaProveedor? categoria = null,
            bool soloActivos = true,
            bool incluirInactivos = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene estadísticas de proveedores
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Estadísticas de proveedores</returns>
        Task<Results.ResultadoEstadisticasProveedores> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores por categoría
        /// </summary>
        /// <param name="categoria">Categoría de proveedor</param>
        /// <param name="soloProveedoresPrincipales">Si es true, sólo devuelve los proveedores marcados como principales</param>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores de la categoría especificada</returns>
        Task<IEnumerable<Proveedor>> ObtenerPorCategoriaAsync(
            Enums.CategoriaProveedor categoria, 
            bool soloProveedoresPrincipales = false, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el proveedor principal para una categoría específica
        /// </summary>
        /// <param name="categoria">Categoría de proveedor</param>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>El proveedor principal para la categoría o null si no hay ninguno marcado como principal</returns>
        Task<Proveedor?> ObtenerProveedorPrincipalPorCategoriaAsync(
            Enums.CategoriaProveedor categoria, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene proveedores por múltiples categorías (que cumplan con todas las categorías especificadas)
        /// </summary>
        /// <param name="categorias">Lista de categorías requeridas</param>
        /// <param name="incluirContactos">Indica si se deben incluir los contactos de los proveedores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores que tienen todas las categorías especificadas</returns>
        Task<IEnumerable<Proveedor>> ObtenerPorMultiplesCategoriaAsync(
            IEnumerable<Enums.CategoriaProveedor> categorias, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default);
    }
}