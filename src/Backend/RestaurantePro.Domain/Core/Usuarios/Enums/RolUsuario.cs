namespace RestaurantePro.Domain.Core.Usuarios.Enums
{
    /// <summary>
    /// Roles disponibles en el sistema
    /// </summary>
    public enum RolUsuario
    {
        /// <summary>
        /// Administrador del sistema con acceso total
        /// </summary>
        Administrador = 1,
        
        /// <summary>
        /// Gerente del restaurante
        /// </summary>
        Gerente = 2,
        
        /// <summary>
        /// Cajero del restaurante
        /// </summary>
        Cajero = 3,
        
        /// <summary>
        /// Mesero del restaurante
        /// </summary>
        Mesero = 4,
        
        /// <summary>
        /// Cocinero del restaurante
        /// </summary>
        Cocinero = 5,
        
        /// <summary>
        /// Responsable del inventario y almacén
        /// </summary>
        EncargadoInventario = 6,
        
        /// <summary>
        /// Alias para EncargadoInventario - Gerente de inventario
        /// </summary>
        GerenteInventario = EncargadoInventario,
        
        /// <summary>
        /// Empleado genérico del restaurante
        /// </summary>
        Empleado = 7
    }
} 