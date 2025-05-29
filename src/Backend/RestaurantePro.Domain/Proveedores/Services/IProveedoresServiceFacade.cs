namespace RestaurantePro.Domain.Proveedores.Services
{
    /// <summary>
    /// Fachada de servicios para el contexto de Proveedores
    /// Esta interfaz expone operaciones compuestas para ser utilizadas por la capa de Aplicación
    /// </summary>
    public interface IProveedoresServiceFacade
    {
        /// <summary>
        /// Registra un nuevo proveedor
        /// </summary>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="rut">RUT del proveedor</param>
        /// <param name="direccion">Dirección del proveedor</param>
        /// <param name="telefono">Teléfono del proveedor</param>
        /// <param name="email">Email del proveedor</param>
        /// <param name="sitioWeb">Sitio web del proveedor (opcional)</param>
        /// <param name="notas">Notas adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor registrado</returns>
        Task<Proveedor> RegistrarProveedorAsync(
            string nombre, 
            string rut, 
            string direccion, 
            string telefono, 
            string email, 
            string? sitioWeb = null, 
            string? notas = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra un nuevo proveedor con información avanzada usando ProveedorBuilder
        /// </summary>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="nombreContacto">Nombre del contacto principal</param>
        /// <param name="email">Email del proveedor</param>
        /// <param name="telefono">Teléfono del proveedor</param>
        /// <param name="direccion">Dirección completa</param>
        /// <param name="ciudad">Ciudad</param>
        /// <param name="codigoPostal">Código postal</param>
        /// <param name="pais">País (default: México)</param>
        /// <param name="rfc">RFC del proveedor</param>
        /// <param name="informacionBancaria">Información bancaria (opcional)</param>
        /// <param name="diasCredito">Días de crédito (default: 30)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Result con el proveedor registrado o errores de validación</returns>
        Task<Result<Proveedor>> RegistrarProveedorAvanzadoAsync(
            string nombre,
            string nombreContacto,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string codigoPostal,
            string pais = "México",
            string? rfc = null,
            string? informacionBancaria = null,
            int diasCredito = 30,
            string? observaciones = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza los datos de un proveedor existente
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="direccion">Nueva dirección (null para no cambiar)</param>
        /// <param name="telefono">Nuevo teléfono (null para no cambiar)</param>
        /// <param name="email">Nuevo email (null para no cambiar)</param>
        /// <param name="sitioWeb">Nuevo sitio web (null para no cambiar)</param>
        /// <param name="notas">Nuevas notas (null para no cambiar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor actualizado o null si no se encontró</returns>
        Task<Proveedor?> ActualizarDatosProveedorAsync(
            Guid proveedorId, 
            string? direccion = null, 
            string? telefono = null, 
            string? email = null, 
            string? sitioWeb = null, 
            string? notas = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un contacto a un proveedor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre del contacto</param>
        /// <param name="cargo">Cargo del contacto</param>
        /// <param name="telefono">Teléfono del contacto</param>
        /// <param name="email">Email del contacto</param>
        /// <param name="esPrincipal">Indica si es el contacto principal</param>
        /// <param name="notas">Notas adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Proveedor actualizado o null si no se encontró</returns>
        Task<Proveedor?> AgregarContactoProveedorAsync(
            Guid proveedorId, 
            string nombre, 
            string cargo, 
            string telefono, 
            string email, 
            bool esPrincipal = false, 
            string? notas = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Desactiva un proveedor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="motivo">Motivo de la desactivación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se desactivó correctamente, False si no se encontró el proveedor</returns>
        Task<bool> DesactivarProveedorAsync(
            Guid proveedorId, 
            string motivo, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Activa un proveedor previamente desactivado
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se activó correctamente, False si no se encontró el proveedor</returns>
        Task<bool> ActivarProveedorAsync(
            Guid proveedorId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene proveedores por tipo de producto o servicio
        /// </summary>
        /// <param name="tipoProducto">Tipo de producto o servicio</param>
        /// <param name="soloActivos">Indica si se deben obtener solo proveedores activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de proveedores que ofrecen el tipo de producto o servicio</returns>
        Task<IEnumerable<Proveedor>> ObtenerProveedoresPorTipoProductoAsync(
            string tipoProducto, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default);
    }
} 