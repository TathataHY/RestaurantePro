namespace RestaurantePro.Domain.Proveedores.Interfaces
{
    /// <summary>
    /// Repositorio para la gestión de proveedores
    /// </summary>
    public interface IProveedorRepository : IRepository<Proveedor>
    {
        /// <summary>
        /// Obtiene un proveedor por su ID
        /// </summary>
        /// <param name="id">ID del proveedor</param>
        /// <returns>El proveedor si existe, null en caso contrario</returns>
        Task<Proveedor> ObtenerPorIdAsync(Guid id);
        
        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        /// <param name="soloActivos">Indica si sólo se deben retornar los proveedores activos</param>
        /// <returns>Lista de proveedores</returns>
        Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool soloActivos = true);
        
        /// <summary>
        /// Obtiene proveedores por nombre
        /// </summary>
        /// <param name="nombre">Nombre o parte del nombre a buscar</param>
        /// <param name="soloActivos">Indica si sólo se deben retornar los proveedores activos</param>
        /// <returns>Lista de proveedores que coinciden con el criterio</returns>
        Task<IEnumerable<Proveedor>> BuscarPorNombreAsync(string nombre, bool soloActivos = true);
        
        /// <summary>
        /// Verifica si existe un proveedor con el RFC especificado
        /// </summary>
        /// <param name="rfc">RFC a verificar</param>
        /// <param name="excluyendoId">ID de proveedor a excluir de la búsqueda (para actualizaciones)</param>
        /// <returns>True si existe un proveedor con ese RFC, False en caso contrario</returns>
        Task<bool> ExisteRFCAsync(string rfc, Guid? excluyendoId = null);
        
        /// <summary>
        /// Agrega un nuevo proveedor
        /// </summary>
        /// <param name="proveedor">Proveedor a agregar</param>
        Task AgregarAsync(Entities.Proveedor proveedor);
        
        /// <summary>
        /// Actualiza un proveedor existente
        /// </summary>
        /// <param name="proveedor">Proveedor con los cambios aplicados</param>
        Task ActualizarAsync(Entities.Proveedor proveedor);
        
        /// <summary>
        /// Elimina un proveedor (normalmente una eliminación lógica cambiando su estado a inactivo)
        /// </summary>
        /// <param name="id">ID del proveedor a eliminar</param>
        Task EliminarAsync(Guid id);
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task GuardarCambiosAsync();
    }
} 