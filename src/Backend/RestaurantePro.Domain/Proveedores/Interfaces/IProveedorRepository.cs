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
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor encontrado o null si no existe</returns>
        Task<Proveedor> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un proveedor por su nombre
        /// </summary>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor encontrado o null si no existe</returns>
        Task<Proveedor> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores</returns>
        Task<IEnumerable<Proveedor>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene proveedores activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores activos</returns>
        Task<IEnumerable<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un nuevo proveedor
        /// </summary>
        /// <param name="proveedor">Proveedor a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarAsync(Proveedor proveedor, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un proveedor existente
        /// </summary>
        /// <param name="proveedor">Proveedor a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(Proveedor proveedor, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un proveedor
        /// </summary>
        /// <param name="id">ID del proveedor a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
    }
} 