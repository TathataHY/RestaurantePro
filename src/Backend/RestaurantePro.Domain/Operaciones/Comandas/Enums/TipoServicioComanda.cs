namespace RestaurantePro.Domain.Operaciones.Comandas.Enums
{
    /// <summary>
    /// Tipo de servicio para una comanda
    /// </summary>
    public enum TipoServicioComanda
    {
        /// <summary>
        /// Servicio en mesa dentro del restaurante
        /// </summary>
        Local = 0,
        
        /// <summary>
        /// Servicio para llevar (el cliente recoge)
        /// </summary>
        ParaLlevar = 1,
        
        /// <summary>
        /// Servicio a domicilio (entrega)
        /// </summary>
        Domicilio = 2
    }
} 