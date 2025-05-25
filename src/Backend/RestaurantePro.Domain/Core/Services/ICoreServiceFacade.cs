namespace RestaurantePro.Domain.Core.Services
{
    /// <summary>
    /// Fachada de servicios para el contexto Core
    /// Esta interfaz expone operaciones compuestas para ser utilizadas por la capa de Aplicación
    /// </summary>
    public interface ICoreServiceFacade
    {
        #region Productos
        
        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Producto encontrado o null si no existe</returns>
        Task<Productos.Entities.Producto?> ObtenerProductoPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra un nuevo producto
        /// </summary>
        /// <param name="nombre">Nombre del producto</param>
        /// <param name="descripcion">Descripción del producto</param>
        /// <param name="precio">Precio del producto</param>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="categoriaNombre">Nombre de la categoría (si no existe)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Producto registrado</returns>
        Task<Productos.Entities.Producto> RegistrarProductoAsync(
            string nombre, 
            string descripcion, 
            decimal precio, 
            Guid? categoriaId = null, 
            string? categoriaNombre = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="nombre">Nuevo nombre (null para no cambiar)</param>
        /// <param name="descripcion">Nueva descripción (null para no cambiar)</param>
        /// <param name="precio">Nuevo precio (null para no cambiar)</param>
        /// <param name="categoriaId">Nueva categoría (null para no cambiar)</param>
        /// <param name="activo">Estado activo (null para no cambiar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Producto actualizado o null si no existe</returns>
        Task<Productos.Entities.Producto?> ActualizarProductoAsync(
            Guid id, 
            string? nombre = null, 
            string? descripcion = null, 
            decimal? precio = null, 
            Guid? categoriaId = null, 
            bool? activo = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los productos por categoría
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="soloActivos">Indica si solo se deben obtener productos activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<List<Productos.Entities.Producto>> ObtenerProductosPorCategoriaAsync(
            Guid categoriaId, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza la categoría de un conjunto de productos
        /// </summary>
        /// <param name="productosIds">IDs de los productos a actualizar</param>
        /// <param name="categoriaId">ID de la nueva categoría</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de productos actualizados</returns>
        Task<int> ActualizarCategoriaProductosAsync(
            List<Guid> productosIds, 
            Guid categoriaId, 
            CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Recetas
        
        /// <summary>
        /// Registra o actualiza la receta de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="instrucciones">Instrucciones de preparación</param>
        /// <param name="tiempoPreparacion">Tiempo de preparación en minutos</param>
        /// <param name="ingredientes">Diccionario con ID de ingrediente como clave y cantidad como valor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Receta registrada o actualizada</returns>
        Task<Productos.Entities.Receta> RegistrarRecetaProductoAsync(
            Guid productoId, 
            string instrucciones, 
            int tiempoPreparacion, 
            Dictionary<Guid, decimal> ingredientes, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si hay suficiente stock de ingredientes para elaborar una cantidad específica de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad de producto a elaborar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si hay suficiente stock, False en caso contrario</returns>
        Task<bool> VerificarDisponibilidadProductoAsync(
            Guid productoId, 
            int cantidad, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los ingredientes que no tienen suficiente stock para elaborar una cantidad específica de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad de producto a elaborar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de IDs de ingredientes sin stock suficiente y la cantidad faltante</returns>
        Task<Dictionary<Guid, decimal>> ObtenerIngredientesFaltantesProductoAsync(
            Guid productoId, 
            int cantidad, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Calcula el costo total de los ingredientes necesarios para elaborar un producto según su receta
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Costo total de los ingredientes en la receta</returns>
        Task<decimal> CalcularCostoRecetaProductoAsync(
            Guid productoId,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Calcula la rentabilidad de un producto basado en su precio de venta y el costo de sus ingredientes
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Objeto con información detallada sobre la rentabilidad</returns>
        Task<Productos.ValueObjects.RentabilidadProducto> CalcularRentabilidadProductoAsync(
            Guid productoId,
            CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Usuarios
        
        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario</param>
        /// <param name="nombre">Nombre completo</param>
        /// <param name="email">Email</param>
        /// <param name="rolesIds">Lista de IDs de roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario registrado</returns>
        Task<Usuarios.Entities.Usuario> RegistrarUsuarioAsync(
            string nombreUsuario, 
            string nombre, 
            string email, 
            List<Guid>? rolesIds = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="nombre">Nuevo nombre (null para no cambiar)</param>
        /// <param name="email">Nuevo email (null para no cambiar)</param>
        /// <param name="activo">Estado activo (null para no cambiar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario actualizado o null si no existe</returns>
        Task<Usuarios.Entities.Usuario?> ActualizarUsuarioAsync(
            Guid usuarioId, 
            string? nombre = null, 
            string? email = null, 
            bool? activo = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="rolesIds">Lista de IDs de roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se asignaron los roles, False si no se encontró el usuario</returns>
        Task<bool> AsignarRolesUsuarioAsync(
            Guid usuarioId, 
            List<Guid> rolesIds, 
            CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Notificaciones
        
        /// <summary>
        /// Envía una notificación
        /// </summary>
        /// <param name="destinatarioId">ID del destinatario (null para notificación global)</param>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Mensaje de la notificación</param>
        /// <param name="datos">Datos adicionales (opcional)</param>
        /// <param name="prioridad">Prioridad de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Notificación enviada</returns>
        Task<Notificaciones.Entities.Notificacion> EnviarNotificacionAsync(
            Guid? destinatarioId, 
            string tipo, 
            string titulo, 
            string mensaje, 
            string datos = "", 
            int prioridad = 0, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="notificacionId">ID de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se marcó como leída, False si no se encontró</returns>
        Task<bool> MarcarNotificacionComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las notificaciones de un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="soloNoLeidas">Indica si solo se deben obtener notificaciones no leídas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de notificaciones del usuario</returns>
        Task<List<Notificaciones.Entities.Notificacion>> ObtenerNotificacionesUsuarioAsync(
            Guid usuarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default);
        
        #endregion
    }
} 