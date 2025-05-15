namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Tipos de acciones para personalizar un producto
    /// </summary>
    public enum AccionPersonalizacion
    {
        /// <summary>
        /// Agregar más cantidad de un ingrediente
        /// </summary>
        Agregar = 0,

        /// <summary>
        /// Quitar un ingrediente del producto
        /// </summary>
        Quitar = 1,

        /// <summary>
        /// Sustituir un ingrediente por otro
        /// </summary>
        Sustituir = 2
    }
} 