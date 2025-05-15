namespace RestaurantePro.Domain.Enums
{
    public enum EstadoInventario
    {
        /// <summary>
        /// Stock normal, dentro de los niveles adecuados
        /// </summary>
        Normal = 0,
        
        /// <summary>
        /// Stock bajo, necesita reposición pronto
        /// </summary>
        Bajo = 1,
        
        /// <summary>
        /// Stock crítico, necesita reposición inmediata
        /// </summary>
        Critico = 2,
        
        /// <summary>
        /// Stock agotado, no disponible
        /// </summary>
        Agotado = 3,
        
        /// <summary>
        /// Stock sobre el nivel óptimo
        /// </summary>
        Exceso = 4,
        
        /// <summary>
        /// Ingrediente pendiente de recibir
        /// </summary>
        PendienteRecepcion = 5,
        
        /// <summary>
        /// Ingrediente próximo a caducar
        /// </summary>
        ProximoCaducar = 6
    }
} 